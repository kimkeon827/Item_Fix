using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour,IBattle
{
    public float attackPower = 100.0f;  // ���ݷ�
    public float maxHP = 100.0f;        // �ִ� HP
    float hp = 100.0f;                  // ���� HP

    public float maxMP = 100.0f;        // �ִ� MP
    float mp = 100.0f;                  // ���� MP
    
    public float moveSpeed = 3.0f;      // �̵��ӵ�

    Vector3 inputDir = Vector3.zero;    // �Է����� ������ �ٶ󺸴� ����
    InputActions inputActions;           // ��ǲ�׼� �ν��Ͻ�

    private Rigidbody rb;
    
    // ������Ƽ ------------------------------------
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

    // ��������Ʈ ----------------------------------------
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
