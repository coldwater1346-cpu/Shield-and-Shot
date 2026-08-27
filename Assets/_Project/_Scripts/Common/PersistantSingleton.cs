using UnityEngine;

public class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        // 부모(Singleton)의 중복 검사 및 _instance 할당 로직을 먼저 실행합니다.
        base.Awake();

        // 만약 내가 살아남은 진짜 인스턴스라면, 파괴되지 않도록 설정합니다.
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
