using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestBase : MonoBehaviour
{
    TestPlayerInputActions inputActions;

    protected virtual void Awake()
    {
        inputActions = new TestPlayerInputActions();
    }

    protected virtual void OnEnable()
    {
        inputActions.Test.Enable();
        inputActions.Test.Test1.performed += Test1;
        inputActions.Test.Test2.performed += Test2;
        inputActions.Test.Test3.performed += Test3;
        
    }
    protected virtual void OnDisable()
    {
        
        inputActions.Test.Test3.performed -= Test3;
        inputActions.Test.Test2.performed -= Test2;
        inputActions.Test.Test1.performed -= Test1;
        inputActions.Test.Disable();
    }

    protected virtual void Test1(InputAction.CallbackContext _)
    {
    }

    protected virtual void Test2(InputAction.CallbackContext _)
    {
    }

    protected virtual void Test3(InputAction.CallbackContext _)
    {
    }

}
