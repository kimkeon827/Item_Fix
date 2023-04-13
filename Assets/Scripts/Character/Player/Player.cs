using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IBattle
{
    public float moveSpeed = 3.0f;                      //이동 속도
    public float turnSpeed = 3.0f;                      // 회전 속도
    public float attackPower = 10.0f;                   // 공격력
    public float defencePower = 3.0f;                   // 방어력
    bool isAlive = true;
    public float maxHP = 100.0f;
    float hp = 100.0f;
    Vector3 inputDir = Vector3.zero;                    // 입력으로 지정된 바라보는 방향 
    Quaternion targetRotation = Quaternion.identity;    // 회전 목표
    InputActions input; 
    Animator anim;                                      //애니메이터 컴포넌트 캐싱용
    Rigidbody rb;                                       // 리지드바디



    public float AttackPower => attackPower;
    public float DefencePower => defencePower;

    public float MaxHP => maxHP;
    public bool IsAlive => isAlive;

    public Action<float> onHealthChange { get; set; }

    public float HP
    {
        get => hp;
        set
        {
            if (isAlive && hp != value)
            {
                hp = value;

                if (hp < 0)
                {
                    Die();
                }

                hp = Mathf.Clamp(hp, 0.0f, maxHP);

                onHealthChange?.Invoke(hp / maxHP);
            }
        }
    }

    private void Awake()
    {
        input = new InputActions();
        //컴포넌트 찾아오기
        anim = GetComponent<Animator>();        
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;
        input.Player.Attack.performed += OnAttack;
        input.Player.Attack.canceled += OnAttack;
    }

    private void OnDisable()
    {
        input.Player.Attack.canceled -= OnAttack;
        input.Player.Attack.performed -= OnAttack;
        input.Player.Move.canceled -= OnMove;
        input.Player.Move.performed -= OnMove;
        input.Player.Disable();
    }

    private void Update()
    {
        transform.Translate(moveSpeed * Time.deltaTime * inputDir, Space.World);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

  

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        inputDir.x = input.x;
        inputDir.y = 0.0f;
        inputDir.z = input.y;    

        if (!context.canceled)
        {
            targetRotation = Quaternion.LookRotation(inputDir);
        }
    }

    void Turn ()
    {
        Quaternion newRotation = Quaternion.LookRotation(inputDir);

        rb.MoveRotation(newRotation);
    }

    /// <summary>
    /// 스페이스 키 했을 때 실행
    /// </summary>
    /// <param name="_"></param>
    private void OnAttack(InputAction.CallbackContext _)
    {
        
        anim.ResetTrigger("Attack");    //트리거 쌓임 방지
        anim.SetTrigger("Attack");  //트리거 실행

        //anim.SetTrigger("Stop");    //트리거off가 안되서 임시로 넣은 스탑 
    }

    public void Attack(IBattle target)
    {
        target?.Defence(AttackPower);
    }

    public void Defence(float damage)
    {
        if (isAlive)
        {
            anim.SetTrigger("Hit");
            HP -= (damage - DefencePower);
        }
    }

    public void Die()
    {
        isAlive = false;
        
    }
}
