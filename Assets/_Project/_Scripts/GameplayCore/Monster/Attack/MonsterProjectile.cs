//using Shield_Shot.GameplayCore.Common;
//using Shield_Shot.GameplayCore.Weapon.Projectile;
//using UnityEngine;

//public class MonsterProjectile : MonoBehaviour
//{
//    [SerializeField] private float _damage = 5f;
//    [SerializeField] private float _lifeTimer = 5f;

//    public float Damage => _damage;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            var damageable = other.GetComponent<PlayerStatus>();
//            damageable?.TakeDamage(Damage);
//            Destroy(gameObject);
//        }
//    }

//    private void Update()
//    {
//        _lifeTimer -= Time.deltaTime;
//        if (_lifeTimer <= 0f)
//        {
//            Destroy(gameObject);
//        }
//    }


//}
using Shield_Shot.GameplayCore.Common;   // ITakeDamage
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

public class MonsterProjectile : MonoBehaviour
{
    [SerializeField] private float _damage = 5f;
    [SerializeField] private float _lifeTimer = 5f;

    [Header("피격 대상 레이어")]
    [SerializeField] private LayerMask _playerLayer;   // 정방향 피격
    [SerializeField] private LayerMask _enemyLayer;    // 반사 후 피격

    public float Damage => _damage;

    // 반사 여부 — 반사 처리하는 쪽에서 SetReflected() 호출
    public bool IsReflected { get; private set; }
    public void SetReflected(bool reflected = true) => IsReflected = reflected;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsReflected)
        {
            // 정방향: 플레이어 피격
            if (InLayer(other.gameObject, _playerLayer))
            {
                other.GetComponent<PlayerStatus>()?.TakeDamage(Damage);
                Destroy(gameObject);
            }
        }
        else
        {
            // 반사 후: 몬스터 피격
            if (InLayer(other.gameObject, _enemyLayer))
            {
                var dmg = other.GetComponentInParent<ITakeDamage>();
                if (dmg != null)
                {
                    dmg.TakeDamage(Damage);

                    if (DamagePopupManager.Instance != null)
                    {
                        DamagePopupManager.Instance.Show(other.transform.position, Damage);
                    }

                    Destroy(gameObject);
                }
            }
        }
    }

    private void Update()
    {
        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f) Destroy(gameObject);
    }

    // 해당 오브젝트의 레이어가 mask에 포함되는지
    private static bool InLayer(GameObject go, LayerMask mask)
        => (mask.value & (1 << go.layer)) != 0;
}