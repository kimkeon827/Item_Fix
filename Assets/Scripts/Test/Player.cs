using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour,IBattle
{
    public float attackPower = 100.0f;  // 공격력
    public float maxHP = 100.0f;        // 최대 HP
    float hp = 100.0f;                  // 현재 HP

    public float maxMP = 100.0f;        // 최대 MP
    float mp = 100.0f;                  // 현재 MP
    
    public float moveSpeed = 3.0f;      // 이동속도

    Vector3 inputDir = Vector3.zero;    // 입력으로 지정된 바라보는 방향
    InputActions inputActions;           // 인풋액션 인스턴스

    private Rigidbody rb;
    
    // 프로퍼티 ------------------------------------
    public float AttackPower => attackPower;

    public float HP
    {
        get => hp;
        set
        {
            hp = Mathf.Clamp(hp, 0.0f, maxHP);
        }
    }

    public float MaxHP => maxHP;
    public float MP
    {
        get => mp;
        set
        {
            mp = Mathf.Clamp(value, 0.0f, maxMP);
        }
    }

    public float MaxMP => maxMP;

    // 델리게이트 ----------------------------------------
    public Action<float> onHealthChange { get; set; }

    public Action<float> onManaChange { get; set; }
    // -----------------------------------------------------

    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Disable();
    }
    private void Update()
    {
        transform.Translate(moveSpeed * Time.deltaTime * inputDir);
    }

    private void Start()
    {
        hp = maxHP;
        rb = GetComponent<Rigidbody>();
    }

    public void Attack(IBattle target)
    {
        
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        Debug.Log(input);
        inputDir.x = input.x;
        inputDir.y = 0.0f;
        inputDir.z = input.y;
    }
}
