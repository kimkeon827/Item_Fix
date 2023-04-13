using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;  //위의 전처리기 있을 때만 실행 버전에 넣어라. 
#endif

[RequireComponent(typeof(Rigidbody))]   //필수적인 컴포넌트가 있을때 자동으로 넣는 유니티 속성
[RequireComponent(typeof(Animator))]

public class Enemy : MonoBehaviour, IHealth, IBattle
{
    // 웨이포인트 관련 변수 -------------------------------------------------
    /// <summary>
    /// 적이 순찰할 웨이포인트들
    /// </summary>
    public Waypoints waypoints; 

    /// <summary>
    /// 지금 적이 이동할 목표 지점(웨이포인트)의 트랜스폼
    /// </summary>
    Transform waypointTarget;   
    
    // 이동 관련 변수 -------------------------------------------------------
    /// <summary>
    /// 적의 이동 속도
    /// </summary>
    public float moveSpeed = 3.0f;

    // 추적 관련 변수 -------------------------------------------------
    /// <summary>
    /// 시야 범위
    /// </summary>
    public float sightRange = 10.0f;    

    public float closeSightRange = 2.5f;    

    /// <summary>
    /// 시야각의 절반
    /// </summary>
    public float sightHalfAngle = 50.0f;
    
    /// <summary>
    /// 추적할 플레이어의 트랜스폼
    /// </summary>
    Transform chaseTarget;

    // 상태 관련 변수 -------------------------------------------------
    EnemyState state = EnemyState.Patrol; //적 현재 상태
    public float waitTime = 2.0f;   //목적지에 도달했을 때 기다리는 시간
    float waitTimer;    //남아있는 기다리는 시간

    // 컴포넌트 캐싱용 변수 -------------------------------------------------
    Animator anim;
    NavMeshAgent agent;
    SphereCollider bodyCollider;    
    Rigidbody rigid;  
    ParticleSystem dieEffect;       // 죽을 때 표시될 이팩트

    // 추가 데이터 타입 -------------------------------------------------
    /// <summary>
    /// 적의 상태를 나타내기 위한 enum
    /// </summary>
    protected enum EnemyState  
    {
        Wait = 0,   // 대기 상태
        Patrol,     // 순찰 상태
        Chase,      // 추적 상태
        Attack,     // 공격 상태
        Dead        // 사망 상태
    }

    // 전투용 데이터 -------------------------------------------------
    public float attackPower = 10.0f;      // 공격력
    public float defencePower = 3.0f;      // 방어력
    public float maxHP = 100.0f;           // 최대 HP
    float hp = 100.0f;                     // 현재 HP

    float attackSpeed = 1.0f;              // 1초마다 공격
    float attackCoolTime = 1.0f;           // 쿨타임이 0 미만이 되면 공격
    IBattle attackTarget;

    // 아이템 드랍용 데이터 -------------------------------------------------
    [System.Serializable]
    public struct ItemDropInfo          // 드랍 아이템 정보
    {
        public ItemIDCode id;           // 아이템 종류
        [Range(0.0f,1.0f)] 
        public float dropPercentage;    // 아이템 드랍 확률
    }

    /// <summary>
    /// 이 몬스터가 드랍할 아이템의 종류
    /// </summary>
    public ItemDropInfo[] dropItems;

    // 델리게이트 -------------------------------------------------

    /// <summary>
    /// HP가 변경될 때 실행될 델리게이트
    /// </summary>
    public Action<float> onHealthChange { get; set; }

    /// <summary>
    /// 적이 죽을 때 실행될 델리게이트
    /// </summary>
    public Action onDie { get; set; }
    
    /// <summary>
    /// 상태별 업데이터 함수를 가질 델리게이트
    /// </summary>
    Action stateUpdate;
    // -------------------------------------------------

    // 프로퍼티 -------------------------------------------------
    public float AttackPower => attackPower;

    public float DefencePower => defencePower;

    public float HP
    {
        get => hp;
        set
        {
            if (hp != value) // 살아있고 HP가 변경되었을 때만 실행
            {
                hp = value;

                if ( State != EnemyState.Dead && hp < 0)  
                {                    
                    Die();
                }
                hp = Mathf.Clamp(hp, 0.0f, maxHP);

                onHealthChange?.Invoke(hp / maxHP);
            }
        }
    }

    public float MaxHP => maxHP;

    /// <summary>
    /// 이동할 목적지(웨이포인트)를 나타내는 프로퍼티
    /// </summary>
    protected Transform WaypointTarget
    {
        get => waypointTarget;
        set
        {
            waypointTarget = value;            
        }
    }

    /// <summary>
    /// 적의 상태를 나타내는 프로퍼티
    /// </summary>
    protected EnemyState State
    {
        get => state;
        set
        {            
            if(state != value)
            {
                state = value;  //새로운 상태로 변경
                switch (state) //새로운 상태(새로운 상태로 들어가면서 해야 할 일 처리)
                {
                    case EnemyState.Wait:
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                        waitTimer = waitTime;       //타이머 초기화
                        anim.SetTrigger("Stop");    //Idle 애니메이션 재생
                        stateUpdate = Update_Wait;  //FixedUpdate에서 실행될 델리게이트 변경
                        break;                    
                    case EnemyState.Patrol:
                        agent.isStopped = false;
                        agent.SetDestination(WaypointTarget.position);
                        anim.SetTrigger("Move");    //Run 애니메이션 재생
                        stateUpdate = Update_Patrol;//FixedUpdate에서 실행될 델리게이트 변경
                        break;                    
                    case EnemyState.Chase:
                        agent.isStopped = false;                        
                        anim.SetTrigger("Move");        // 이동 애니메이션 재생
                        stateUpdate = Update_Chase;     //FixedUpdate에서 실행될 델리게이트 변경
                        break;
                    case EnemyState.Attack:
                        agent.isStopped = true;         // 이동 정지
                        agent.velocity = Vector3.zero;
                        anim.SetTrigger("Stop");        // 애니메이션 변경
                        attackCoolTime = attackSpeed;   // 공격 쿨타임 초기화
                        stateUpdate = Update_Attack;    // FixedUpdate에서 실행될 델리게이트 변경
                        break;                    
                    case EnemyState.Dead:
                        agent.isStopped = true;             //길찾기 정지
                        agent.velocity = Vector3.zero;
                        anim.SetTrigger("Die");             //사망 애니메이션 재생
                        StartCoroutine(DeadRepresent());    // 시간이 지나면 서서히 가라앉는 연출 실행
                        stateUpdate = Update_Dead;          // FixedUpdate에서 실행될 델리게이트 변경
                        break;
                    default:
                        break;
                }
            }
        }
    }

        
    /// <summary>
    /// 남은 대기 시간을 나타내는 프로퍼티
    /// </summary>
    protected float WaitTimer
    {
        get => waitTimer;
        set
        {
            waitTimer = value;
            if( waypoints != null && waitTimer < 0.0f)    //웨이포인트가 있으면 ,남은 시간이 다 되면
            {
                State = EnemyState.Patrol;  //Patrol 상태로 전환
            }
        }
    }
    // -------------------------------------------------

    private void Awake()
    {
        //컴포넌트 찾기
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        bodyCollider = GetComponent<SphereCollider>();
        rigid = GetComponent<Rigidbody>();
        dieEffect = GetComponentInChildren<ParticleSystem>();

        Enemy_AttackArea attackArea = GetComponentInChildren<Enemy_AttackArea>();
        attackArea.onPlayerIn += (target) =>
        {
            //if (State == EnemyState.Chase)
            //{
                attackTarget = target;
                State = EnemyState.Attack;
            //}
        };

        attackArea.onPlayerOut += (target) =>
        {
            if (attackTarget == target)
            {
                attackTarget = null;
                if (State != EnemyState.Dead)
                {
                    State = EnemyState.Chase;
                }
            }
        };

    }

    private void Start()
    {
        agent.speed = moveSpeed;

        //웨이포인트가 없을 때를 대비한 코드
        if (waypoints != null)
        {
            WaypointTarget = waypoints.Current;
        }
        else
        {
            WaypointTarget = transform;
        }

        //값 초기화 작업
        State = EnemyState.Wait;    //기본 상태 설정(wait)
        anim.ResetTrigger("Stop");  //트리거 쌓이는 현상을 리셋해 방지

        //HP 테스트 코드
        onHealthChange += Test_HP_Change;
        onDie += Test_Die;
    }

    private void FixedUpdate()
    {
        //플레이어 추적 
        if (State !=EnemyState.Dead && State != EnemyState.Attack && SearchPlayer())
        {
            State = EnemyState.Chase;
        }
               
        //상태 업데이트
        stateUpdate();
    }

    //Patrol 상태일 때 실행될 업데이트 함수
    void Update_Patrol()
    {
        //도착 확인
        //agent.pathPending : 경로 계산이 진행중인지 확인.true면 아직 경로 계산 중 - 플레이어 추적때 멀리 나가면 돌아올때 
        //agent.remainingDistance : 도착지점까지 남아있는 거리
        //agent.stoppingDistance : 도착지점에 도착했다고 인정되는 거리
        if ( !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance )   //경로계산이 완료, 도착지점까지 이동하지 않았다.
        {
            WaypointTarget = waypoints.MoveNext();  //다음 웨이포인트 지점을 MoveTarget으로 설정
            State = EnemyState.Wait;    //대기 상태로 변경
        }
    }

    
    //wait 상태일 때 실행될 업데이트 함수
    void Update_Wait()
    {
        WaitTimer -= Time.fixedDeltaTime;   //시간 지속적으로 감소
    }

    //Chase 상태일 때 실행될 업데이트 함수
    private void Update_Chase()
    {
        //추적 대상 확인
        if(chaseTarget != null) 
        {
            agent.SetDestination(chaseTarget.position); //계속 타겟 위치 추적 
        }
        else
        {
            State = EnemyState.Wait;    //없으면 대기 > 웨이포인트로 반복
        }
    }

    /// <summary>
    /// Update 상태일 때 실행될 업데이트 함수
    /// </summary>
    private void Update_Attack()
    {
        attackCoolTime -= Time.deltaTime;   // 쿨타임 감소
        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(attackTarget.transform.position - transform.position),
                0.1f);
       
        if (attackCoolTime < 0)            // 쿨타임 체크
        {
            // 쿨타임 다 됬으면 공격
            anim.SetTrigger("Attack");      // 공격 애니메이션 재생
            Attack(attackTarget);           // 공격 처리                      
        }
    }

    //사망 상태일 때 실행될 업데이트 함수
    private void Update_Dead()
    {

    }

    /// <summary>
    /// 플레이어를 감지하는 함수
    /// </summary>
    /// <returns>적이 플레이어를 감지하면 true. 아니면 false</returns>
    bool SearchPlayer()
    {
        bool result = false;
        chaseTarget = null;

        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange, LayerMask.GetMask("Player"));
        if(colliders.Length > 0 )
        {
            Vector3 playerPos = colliders[0].transform.position;
            Vector3 toPlayerDir = playerPos - transform.position;

            if( toPlayerDir.sqrMagnitude < closeSightRange * closeSightRange)
            {
                chaseTarget = colliders[0].transform;
                result = true;
            }
            else
            {
                if(IsInSightAngle(toPlayerDir))
                {
                    if(!IsSightBlocked(toPlayerDir))
                    {
                        chaseTarget = colliders[0].transform;
                        result = true;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 대상이 시야각안에 들어와 있는지 확인하는 함수
    /// </summary>
    /// <param name="toTargetDir">대상으로 가는 방향 벡터</param>
    /// <returns>true면 대상이 시야각안에 있다. false면 없다.</returns>
    bool IsInSightAngle(Vector3 toTargetDir)
    {
        float angle = Vector3.Angle(transform.forward, toTargetDir);    // forward 벡터와 플레어어로 가는 방향 벡터의 사이각 구하기
        return (sightHalfAngle > angle);
    }

    /// <summary>
    /// 플레이어를 바라보는 시야가 막혔는지 확인하는 함수
    /// </summary>
    /// <param name="toTargetDir">대상으로 가는 방향 벡터</param>
    /// <returns>true면 시야가 막혀있다. false면 아니다.</returns>
    bool IsSightBlocked(Vector3 toTargetDir)
    {
        bool result = true;
        // 레이 만들기 : 시작점 = 적의 위치 + 적의 눈높이, 방향 = 적에서 플레이어로 가는 방향
        Ray ray = new(transform.position + transform.up * 0.5f, toTargetDir);
        if (Physics.Raycast(ray, out RaycastHit hit, sightRange))
        {
            // 레이에 부딪친 컬라이더가 있다.
            if (hit.collider.CompareTag("Player"))
            {
                // 그 컬라이더가 플레이어이다.
                result = false;
            }
        }
        return result;
    }

    /// <summary>
    /// 공격용 함수
    /// </summary>
    /// <param name="target">공격할 대상</param>
    public void Attack(IBattle target)
    {
        target?.Defence(AttackPower);
        attackCoolTime = attackSpeed;   // 쿨타임 초기화
    }

    /// <summary>
    /// 방어용 함수
    /// </summary>
    /// <param name="damage">현재 입은 데미지</param>
    public void Defence(float damage)   
    {
        if( State != EnemyState.Dead )    //죽지 않을 때만 맞기 가능
        {
            anim.SetTrigger("Hit");
            HP -= (damage - DefencePower);
        }
        
    }

    /// <summary>
    /// 죽었을 때 실행될 함수
    /// </summary>
    public void Die()
    {
        State = EnemyState.Dead;
        onDie?.Invoke();

        MakeDropItem();
    }

    /// <summary>
    /// 아이템 드랍 함수
    /// </summary>
    void MakeDropItem()
    {
        float percentage = UnityEngine.Random.Range(0.0f, 1.0f);    // 드랍할 아이템을 결정하기 위한 랜덤 숫자 가져오기
        int index = 0;  // 드랍할 (내가 가지고 있는) 아이템의 인덱스
        float max = 0;  // 가장 드랍할 확률이 높은 아이템을 찾기 위한 임시값
        for (int i = 0; i < dropItems.Length; i++)
        {
            if( max < dropItems[i].dropPercentage)
            {
                max = dropItems[i].dropPercentage;  // 가장 드랍 확률이 높은 아이템 찾기
                index = i;                          // index의 디폴트 값은 가장 드랍 확률이 높은 아이템
            }
        }

        float checkPercentage = 0.0f;               // 아이템의 드랍 확률을 누적하는 임시 값
        for(int i = 0; i < dropItems.Length; i++)
        {
            checkPercentage += dropItems[i].dropPercentage; // checkPercentage를 단계별로 계속 누적 시킴

            // checkPercentage와 percentage 비교 (랜덤 숫자가 누적된 확률보다 낮은지 확인, 낮으면 해당 아이템 생성)
            if ( percentage <= checkPercentage)
            {
                index = i;  // 생성할 아이템 결정
                break;      // for문 종료
            }
        }

        GameObject obj = ItemFactory.MakeItem(dropItems[index].id, transform.position, true);   // 선택된 아이템 생성
    }

    /// <summary>
    /// 사망 연출용 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadRepresent()
    {
        // 바닥에 이팩트 처리
        dieEffect.Play();                   // 사망시 파티클 이팩트 재생
        dieEffect.transform.parent = null;  // 부모자식 관계 끊기
        Enemy_HP_Bar hpBar = GetComponentInChildren<Enemy_HP_Bar>();
        Destroy(hpBar.gameObject);          // HP바 제거

        yield return new WaitForSeconds(1.5f);  // 슬라임 사망 애니메이션이 1.33초라 그 이후에 떨어짐

        // 바닥 아래로 떨어트리기
        agent.enabled = false;          // 네브메시 에이전트 컴포넌트를 끄기
        bodyCollider.enabled = false;   // 컬라이더 컴포넌트 끄기
        rigid.isKinematic = false;      // 키네마틱 끄기
        rigid.drag = 10.0f;             // 마찰력은 천천히 떨어질 정도로
        
        yield return new WaitForSeconds(1.5f);  // 1.5초 뒤에

        // 삭제 처리
        Destroy(dieEffect.gameObject);  // 이펙트 삭제
        Destroy(this.gameObject);       // 적도 삭제        
    }

    

    void Test_HP_Change(float ratio)
    {
        Debug.Log($"{gameObject.name}의 HP가 {HP}로 변경되었습니다.");
    }

    void Test_Die()
    {
        Debug.Log($"{gameObject.name}이 죽었습니다.");
    }

    private void OnDrawGizmos() //OnDrawGizmos + Selected 추가로 쓰면 선택한 몬스터의 시야범위만 볼 수 있음
    {
#if UNITY_EDITOR
        Handles.color = Color.green;    //기본 시야 색
        //Gizmos.DrawWireSphere(transform.position, sightRange);
        Handles.DrawWireDisc(transform.position, transform.up, sightRange); //시야 범위를 디스트 평면원 형태로

        if (SearchPlayer()) //플레이어 감지 
        {
            Handles.color = Color.red;  //감지 시야 색 
        }

        Vector3 forward = transform.forward * sightRange;   //시야 범위 벡터 
        Handles.DrawDottedLine(transform.position, transform.position + forward, 2.0f);    // 시야 반경 원 


        Quaternion q1 = Quaternion.AngleAxis(-sightHalfAngle, transform.up);    //up벡터를 축으로 반시계 반앵글만큼 회전
        Quaternion q2 = Quaternion.AngleAxis(sightHalfAngle, transform.up);     //up벡터를 축으로 시계 반앵글만큼 회전


        //시야 범위 표시용 선긋기 기즈모
        //Handles.DrawLine(transform.position, transform.position + halfAngle만큼 회전된 forward * sightRange);  
        Handles.DrawLine(transform.position, transform.position + q1 * forward);    //중심선 반시계 회전시켜 그리기
        Handles.DrawLine(transform.position, transform.position + q2 * forward);    //중심선 시계 회전시켜 그리기

        Handles.DrawWireArc(transform.position, transform.up, q1 * forward, sightHalfAngle * 2, sightRange, 5.0f);  //부채꼴 호 강조

        // 근접 시야 처리
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, transform.up, closeSightRange);

#endif
    }
}
