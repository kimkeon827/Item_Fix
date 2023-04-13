using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static bool isShutDown = false;
    private static T _instance = null;
    public static T Inst
    {
        get
        {
            if (isShutDown)
            {
#if PRINT_DEBUG_INFO
                Debug.LogWarning($"{typeof(T)} 싱글톤은 이미 삭제되었음.");
#endif
                return null;
            }

            if (_instance == null)
            {
                T obj = FindObjectOfType<T>();
                if (obj == null)
                {
                    GameObject gameObj = new GameObject();
                    gameObj.name = $"{typeof(T).Name}";
                    obj = gameObj.AddComponent<T>();
                }

                _instance = obj;
                DontDestroyOnLoad(_instance.gameObject);
            }
#if PRINT_DEBUG_INFO
            Debug.Log($"Singleton({_instance.gameObject.name}) : Get")};
#endif
            return _instance;
        }
    }

    private void Awake()
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : Awake");
#endif
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(_instance.gameObject);
        }
        else
        {
            if( _instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void OnEnable()
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : OnEnable");
#endif
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : OnDisable");
#endif
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationQuit()
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : Quit");
#endif
        isShutDown = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : SceneLoaded");
#endif
        Initialize();
    }

    protected virtual void Initialize()
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : Initialize");
#endif
    }
}
