using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                if (_instance == null)
                {
                    Debug.LogError($"[Singleton] {typeof(T).Name} 인스턴스를 씬에서 찾을 수 없습니다.");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        // 씬이 파괴될 때 static 참조를 비워주어야 메모리 누수(Leak)가 안 생깁니다.
        if (_instance == this)
        {
            _instance = null;
        }
    }
}