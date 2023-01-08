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

public class Enemy : MonoBehaviour, IBattle, IHealth
{
    //웨이포인트 관련 변수...................................
    public Waypoints waypoints; //순찰에 필요한 웨이포인트들
    Transform waypointTarget;   //적이 이동할 웨이포인트의 트랜스폼
    //Vector3 lookDir;    //적이 이동하는 방향(바라보는 방향)

    public float moveSpeed = 3.0f;  //적 이동속도

    //추적 관련 변수(적 추적 기능 추가)...........................
    public float sightRange = 10.0f;    //시야범위 10.0f
    public float closeSightRange = 2.5f;    //근접 시야 범위 추가

    public float sightHalfAngle = 50.0f;   //시야각 반 50도 
    Transform chaseTarget;  //추적할 플레이어의 트랜스폼

    //상태 관련 변수 ............................................
    EnemyState state = EnemyState.Patrol; //적 현재 상태
    public float waitTime = 2.0f;   //목적지에 도달했을 때 기다리는 시간 설정
    float waitTimer;    //남아있는 기다리는 시간
    

    //컴포넌트 캐싱용 변수.......................
    Animator anim;
    NavMeshAgent agent;

    SphereCollider bodyCollider;    //적 콜라이더 찾기
    Rigidbody rigid;  //적 리지드 바디 찾기
    ParticleSystem dieEffect;       // 죽을 때 표시될 이팩트

    //추가 데이터 타입..............................
    protected enum EnemyState //적 상태 대기,순찰 
    {
        Wait = 0,   //대기 상태
        Patrol, //순찰 상태
        Chase,   //추적 상태
        Attack, //공격 상태
        Dead    //사망 상태
    }

    //전투용 데이터..........................................
    public float attackPower = 10.0f;      // 공격력
    public float defencePower = 3.0f;      // 방어력
    public float maxHP = 100.0f;    // 최대 HP
    float hp = 100.0f;              // 현재 HP

    float attackSpeed = 1.0f;       // 1초마다 공격
    float attackCoolTime = 1.0f;    // 쿨타임이 0 미만이 되면 공격
    IBattle attackTarget;

    //델리게이트.................................................................

    /// <summary>
    /// HP가 변경될 때 실행될 델리게이트
    /// </summary>
    public Action<float> onHealthChange { get; set; }

    /// <summary>
    /// 적이 죽을 때 실행될 델리게이트
    /// </summary>
    public Action onDie { get; set; }
    
    Action stateUpdate; //상태별 업데이터 함수를 가질 델리게이트


    //프로퍼티...............................................................

    /// <summary>
    /// 
    /// </summary>
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

                if ( State !=EnemyState.Dead && hp < 0)  //상태 dead, hp가 0이 되는 순간 die처리
                {
                    
                    Die();
                }

                hp = Mathf.Clamp(hp, 0.0f, maxHP);

                onHealthChange?.Invoke(hp / maxHP);


            }
        }
    }

    public float MaxHP => maxHP;

    //이동할 웨이포인트를 나타내는 프로퍼티...........................................
    protected Transform WaypointTarget
    {
        get => waypointTarget;
        set
        {
            waypointTarget = value;
            //lookDir = (moveTarget.position - transform.position).normalized;    //lookDir도 함께 갱신
            //agent.SetDestination(moveTarget.position);
        }
    }

    //적의 상태를 나타내는 프로퍼티..........................
    protected EnemyState State
    {
        get => state;
        set
        {
            //switch (state)//이전 상태(상태를 나가면서 해야 할일 처리)
            //{
            //    case EnemyState.Wait:
            //        break;
            //    case EnemyState.Patrol:
            //        break;
            //    default:
            //        break;
            //}
            if(state != value)
            {
                state = value;  //새로운 상태로 변경
                switch (state) //새로운 상태(새로운 상태로 들어가면서 해야 할 일 처리)
                {
                    //적 대기 상태 정의
                    case EnemyState.Wait:
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                        waitTimer = waitTime;   //타이머 초기화
                        anim.SetTrigger("Stop");    //Idle 애니메이션 재생
                        stateUpdate = Update_Wait;  //FixedUpdate에서 실행될 델리게이트 변경
                        break;

                    //적 이동 상태 정의 
                    case EnemyState.Patrol:
                        agent.isStopped = false;
                        agent.SetDestination(WaypointTarget.position);
                        anim.SetTrigger("Move");    //Run 애니메이션 재생
                        stateUpdate = Update_Patrol;//FixedUpdate에서 실행될 델리게이트 변경
                        break;

                    //적 추적 상태 정의
                    case EnemyState.Chase:
                        agent.isStopped = false;
                        //agent.SetDestination(chaseTarget.position); 계산량이 많아 제거
                        anim.SetTrigger("Move");
                        stateUpdate = Update_Chase;//FixedUpdate에서 실행될 델리게이트 변경
                        break;

                    case EnemyState.Attack:
                        agent.isStopped = true;         // 이동 정지
                        agent.velocity = Vector3.zero;
                        anim.SetTrigger("Stop");        // 애니메이션 변경
                        attackCoolTime = attackSpeed;   // 공격 쿨타임 초기화
                        stateUpdate = Update_Attack;    // FixedUpdate에서 실행될 델리게이트 변경
                        break;

                    //적 사망 상태 정의
                    case EnemyState.Dead:
                        agent.isStopped = true; //길찾기 정지
                        agent.velocity = Vector3.zero;
                        anim.SetTrigger("Die"); //사망 애니메이션 재생
                        StartCoroutine(DeadRepresent());    // 시간이 지나면 서서히 가라앉는 연출 실행
                        stateUpdate = Update_Dead;
                        break;

                    default:
                        break;
                }
            }
        }
    }


    //남은 대기 시간을 나타내는 프로퍼티...................
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
    //.................................................................

    private void Awake()
    {
        //컴포넌트 찾기
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        dieEffect = GetComponentInChildren<ParticleSystem>();
        rigid = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<SphereCollider>();

        //적 attack area
        Enemy_AttackArea attackArea = GetComponentInChildren<Enemy_AttackArea>();
        attackArea.onPlayerIn += (target) =>
        {
            if ( State == EnemyState.Chase )     // 추적 상태이면 
            {
                attackTarget = target;
                State = EnemyState.Attack;      // 공격 상태로 변경
            }
        };
         
        attackArea.onPlayerOut += (target) =>
        {
            if ( attackTarget == target )        // 공격하던 대상이 범위를 벗어나면
            {
                attackTarget = null;            // 공격 대상을 비우기
                if (State != EnemyState.Dead)
                {
                    State = EnemyState.Chase;       // 플레이어가 공격 범위에서 벗어나면 다시 추적 상태로
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
        transform.rotation =                // 공격 대상 바라보게 만들기
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

    //적 감지+추적 함수.............................
    bool SearchPlayer()
    {
        bool result = false;    //에러 제거용
        chaseTarget = null; //안보일때는 null

        //Overlap = 겹친 영역 안에 있는 물체 감지   
        //특정 범위 안에 플레이어 감지
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange, LayerMask.GetMask("Player"));  
        if( colliders.Length > 0)
        {
            //Player가 sightRange 안에 있다.
            //Debug.Log("Player가 시야 범위에 있다"); //시야 체크용 디버그


            //특정 시야각 안에 있는지 확인하는 방법 
            //Vector3.Angle(a,b)  //a벡터와 b벡터 사이각을 계산
            //sightHalfAngle;
            //Vector3 playerPos = colliders[0].transform.position;
            //float angle = Vector3.Angle(transform.forward, playerPos - transform.position);
            //if( sightHalfAngle > angle) 
            Vector3 playerPos = colliders[0].transform.position;    //플레이어의 위치
            Vector3 toPlayerDir = playerPos - transform.position;   //플레이어로 가는 방향

            if (toPlayerDir.sqrMagnitude < closeSightRange * closeSightRange)  // 근접 시야 범위 안에 있는지 확인
            {
                // 근접 시야 범위 안에 player가 있다.

                chaseTarget = colliders[0].transform;   // 추적할 플레이어 저장
                result = true;
            }
            else
            {
                if (IsInSightAngle(toPlayerDir))    // 시야각 안에 플레이어가 있는지 확인
                {
                    // 시야각 안에 player가 있다.

                    // 시야가 다른 물체로 인해 막혔는지 확인
                    if (!IsSightBlocked(toPlayerDir))
                    {
                        // 시야가 다른 몰체로 인해 막히지 않았다.

                        chaseTarget = colliders[0].transform;   // 추적할 플레이어 저장
                        result = true;
                    }
                }
            }
        }
        //LayerMask.GetMask("Player","Water"));   // 2^6(64) 리턴, getmask = 레이어 여러개를 한번에 받을수 있음    > 2^6+2^4 = 80
        //LayerMask.NameToLayer("Player"); //6 리턴

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
  //      target?.Defence(AttackPower);
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
        Destroy(dieEffect.gameObject);  // 이팩트 삭제

        yield return new WaitForSeconds(1.5f);  // 1.5초 뒤에

        // 삭제 처리
        SpawnManager._instance.enemyCount--;    //죽은 적 숫자만큼 카운트 다운**

        Destroy(this.gameObject);       // 적도 삭제

        //SpawnManager._instance.isSpawn[int.Parse(transform.parent.name) - 1] = false;   //몬스터가 죽은 자리를 false 처리로 해당 자리에 스폰하기


        //yield return new WaitForSeconds(5f);  // 5초 뒤에 적 다시 생성( 몬스터 리젠 가능한가)
        //Instantiate(this.gameObject, transform.position  );   
        //Instantiate(dieEffect.gameObject);
        //Instantiate(hpBar.gameObject);



    }

    public void Test()
    {
        SearchPlayer();
        //Debug.Log(this.gameObject.layer);
        //this.gameObject.layer = 0b_0000_0000_0000_0000_0000_0000_0000_1101;
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
