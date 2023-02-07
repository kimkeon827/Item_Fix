using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float moveSpeed = 3.0f;                       //이동 속도
    public float turnSpeed = 3.0f;                     // 회전 속도
    Vector3 inputDir = Vector3.zero;                    // 입력으로 지정된 바라보는 방향 
    Quaternion targetRotation = Quaternion.identity;    // 회전 목표
    InputActions input; 
    Animator anim;                                      //애니메이터 컴포넌트 캐싱용
    Rigidbody rb;                                       // 리지드바디

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
    }

    private void OnDisable()
    {
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
}
