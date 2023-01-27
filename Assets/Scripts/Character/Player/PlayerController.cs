using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3.0f;                       //이동 속도
    public float rotateSpeed = 3.0f;                     // 회전 속도
    Vector3 inputDir = Vector3.zero;                    //벡터 2로 입력받은걸 벡터 3로 바꾸기 
    Quaternion targetRotation = Quaternion.identity;    // 회전 목표
    TestPlayerInputActions inputActions;  
    Animator anim;                                      //애니메이터 컴포넌트 캐싱용


    private void Awake()
    {
        //컴포넌트 만들어졌을때 인풋 액션 인스턴스 생성
        inputActions = new TestPlayerInputActions();
        //컴포넌트 찾아오기
        anim = GetComponent<Animator>();        
    }

    private void OnEnable()
    {
        //인풋 액션에서 액션맵 활성화
        inputActions.Player.Enable();
        //액션과 함수 연결
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Attack.performed += OnAttack;        
    }

    private void OnDisable()
    {
        //액션과 함수 연결 해제
        inputActions.Player.Attack.performed -= OnAttack;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Move.performed -= OnMove;
        //인풋 액션에서 액션맵 비활성화
        inputActions.Player.Disable();
    }

    private void Update()
    {
        transform.Translate(moveSpeed * Time.deltaTime * inputDir);
        // 플레이어 이동을 moveSpeed, 이전 프레임에 걸린시간, inputDir을 통해 얻은 방향만큼 움직인다.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        // 플레이어 회전을 현재 플레이어가 보는 방향, 목표방향, 회전속도를 이전 프레임만큼 곱해서 방향을 만든다.        
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        inputDir.x = input.x;
        inputDir.y = 0.0f;
        inputDir.z = input.y;   
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
