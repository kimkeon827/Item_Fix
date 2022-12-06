using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


// �̱���
// 1. ������ ���� �� �ϳ�
// 2. Ŭ������ ��ü(Instance)�� ������ �ϳ��� �����ϴ� ������ ����
// 3. �����͸� Ȯ���� �� �ִ�.
// 4. static �ɹ��� �̿��ؼ� ��ü�� ���� ���� �� �� �ֵ��� ���ش�.


// Singleton Ŭ������ ���׸� Ÿ���� Ŭ�����̴�.(���鶧 Ÿ��(T)�� �ϳ� �޾ƾ� �Ѵ�.)
// where ���Ͽ� �ִ� ������ �������Ѿ� �Ѵ�.(T�� ������Ʈ�� ��ӹ��� Ÿ���̾�� �Ѵ�.)
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
                Debug.LogWarning($"{typeof(T)} �̱����� �̹� �����Ǿ���.");
#endif
                return null;
            }

            if (_instance == null)
            {
                // �ѹ��� ���� ���� ����.
                T obj = FindObjectOfType<T>();              // ���� Ÿ���� ������Ʈ�� ���ӿ� �ִ��� ã�ƺ���
                if (obj == null)
                {
                    GameObject gameObj = new GameObject();  // �ٸ� ��ü�� ������ ���� �����.
                    gameObj.name = $"{typeof(T).Name}";
                    obj = gameObj.AddComponent<T>();
                }

                _instance = obj;                            // ã�ų� ���� ���� ��ü�� �ν��Ͻ��� �����Ѵ�.
                DontDestroyOnLoad(_instance.gameObject);    // ���� ��������� ���� ������Ʈ�� �������� �ʰ� �ϴ� �ڵ�
            }
#if PRINT_DEBUG_INFO
            Debug.Log($"Singleton({_instance.gameObject.name}) : Get");
#endif
            return _instance;   // ������ null�� �ƴ� ���� ���ϵȴ�.
        }
    }

    /// <summary>
    /// ������Ʈ�� ���� �Ϸ�� ���Ŀ� ȣ��(���� �̱��� ������Ʈ�� ������ ��ġ�� ��Ȳ�� �� ó���� ���� �ۼ�)
    /// </summary>
    private void Awake()
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : Awake");
#endif
        if (_instance == null)
        {
            // ó�� ���� �Ϸ�� �̱��� ���� ������Ʈ
            _instance = this as T;                      // _instance�� �� ��ũ��Ʈ�� ��ü ����
            DontDestroyOnLoad(_instance.gameObject);    // ���� ��������� ���� ������Ʈ�� �������� �ʰ� �ϴ� �ڵ�
        }
        else
        {
            // ù��° ���Ŀ� ������� �̱��� ���� ������Ʈ
            if (_instance != this)
            {
                Destroy(this.gameObject);       // ���� �ƴ� ���� ������ ������Ʈ�� �̹� ������ �ڽ��� �ٷ� ����
            }
        }
    }

    private void OnEnable()
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : OnEnable");
#endif
        SceneManager.sceneLoaded += OnSceneLoaded;  // �� �ε尡 �Ϸ�Ǹ� Initialize �Լ� ����
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
        Initialize();   // ���� �ε� �Ǹ� �ʱ�ȭ �Լ� ���� ����
    }

    /// <summary>
    /// ���� �޴����� ���� ��������ų� ���� �ε� �Ǿ��� �� ����� �ʱ�ȭ �Լ�
    /// </summary>
    protected virtual void Initialize()
    {
#if PRINT_DEBUG_INFO
        Debug.Log($"Singleton({this.gameObject.name}) : Initialize");
#endif
    }
}

// static Ű����
// ���� �������� �̹� �޸𸮿� ��ġ�� �����ǰ� �ϴ� ������ Ű����
// Ÿ���̸��� ���ؼ��� �ɹ��� ������ �����ϴ�.
// ��� ��ü(instance)�� ���� ���� ������.

// as Ű����
// ����) a as b;  // a�� bŸ������ ĳ������ �õ��� �� �����ϸ� null �ƴϸ� bŸ������ �����ؼ� ó�� 
