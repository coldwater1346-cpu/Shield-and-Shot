using System;
using UnityEngine;

// 충돌 대상 종류
public enum CollisionTargetType
{
    None,
    Monster,
    Wall,
    Shield,     // 플레이어 방패에 몬스터 투사체가 맞을 경우
    Player,     // 몬스터 투사체가 플레이어에 닿을 경우
}

// 충돌 감지 결과를 담는 데이터 구조체
public struct CollisionResult
{
    // 충돌 대상 종류
    public CollisionTargetType targetType;
    // 충돌한 GameObject
    public GameObject hitObject;
    // 충돌 지점 (월드 좌표)
    public Vector3 hitPoint;
    // 충돌 표면의 법선 벡터. ReflectModifier에서 반사 방향 계산에 사용
    public Vector3 hitNormal;
}
namespace Shield_Shot.GameplayCore.Projectile
{
    public class CollisionDetector : MonoBehaviour
    {
        #region Inspector
        [Header("레이어 설정")]
        [Tooltip("몬스터 레이어. 인스펙터에서 프로젝트 레이어에 맞게 설정.")]
        [SerializeField] private LayerMask monsterLayer;
        [Tooltip("벽(장애물) 레이어.")]
        [SerializeField] private LayerMask wallLayer;
        [Tooltip("방패 레이어.")]
        [SerializeField] private LayerMask shieldLayer;
        [Tooltip("플레이어 레이어.")]
        [SerializeField] private LayerMask playerLayer;
        [Header("충돌 방식")]
        [Tooltip("true = Trigger 방식 / false = Collision 방식. 프로젝트 물리 설정에 맞게 선택.")]
        [SerializeField] private bool useTrigger = true;
        #endregion

        #region 이벤트
        // 몬스터와 충돌 시 발행
        public event Action<CollisionResult> OnMonsterHit;
        // 벽과 충돌 시 발행
        public event Action<CollisionResult> OnWallHit;
        // 방패와 충돌 시 발행
        public event Action<CollisionResult> OnShieldHit;
        // 플레이어와 충돌 시 발행 (몬스터 투사체용)
        public event Action<CollisionResult> OnPlayerHit;
        #endregion

        #region 활성화 제어
        private bool isActive = true;
        // 충돌 감지 활성화 여부
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }
        #endregion

        // Trigger 방식
        private void OnTriggerEnter(Collider other)
        {
            if (!useTrigger || !isActive) return;
            // Trigger는 법선벡터를 직접 제공하지 않으므로 추정
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitNormal = (transform.position - other.transform.position).normalized;
            hitNormal = new Vector3(hitNormal.x, 0f, hitNormal.z).normalized; // Y 제거
            ProcessCollision(other.gameObject, hitPoint, hitNormal);
        }

        // Collision 방식
        private void OnCollisionEnter(Collision other)
        {
            if (useTrigger || !isActive) return;
            ContactPoint contact = other.GetContact(0);
            Vector3 hitPoint = contact.point;
            Vector3 hitNormal = contact.normal;
            hitNormal = new Vector3(hitNormal.x, 0f, hitNormal.z).normalized; // Y 제거
            ProcessCollision(other.gameObject, hitPoint, hitNormal);
        }

        #region 공통 충돌 처리
        // 충돌한 오브젝트의 레이어를 식별하고 해당 이벤트 발행
        private void ProcessCollision(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal)
        {
            CollisionTargetType targetType = IdentifyTarget(hitObject);
            if (targetType == CollisionTargetType.None) return;
            CollisionResult result = new CollisionResult
            {
                targetType = targetType,
                hitObject = hitObject,
                hitPoint = hitPoint,
                hitNormal = hitNormal
            };
            switch (targetType)
            {
                case CollisionTargetType.Monster:
                    OnMonsterHit?.Invoke(result);
                    break;
                case CollisionTargetType.Wall:
                    OnWallHit?.Invoke(result);
                    break;
                case CollisionTargetType.Shield:
                    OnShieldHit?.Invoke(result);
                    break;
                case CollisionTargetType.Player:
                    OnPlayerHit?.Invoke(result);
                    break;
            }
        }

        // 충돌 오브젝트의 레이어로 대상 종류 식별
        private CollisionTargetType IdentifyTarget(GameObject target)
        {
            int layer = 1 << target.layer;
            if ((monsterLayer.value & layer) != 0) return CollisionTargetType.Monster;
            if ((wallLayer.value & layer) != 0) return CollisionTargetType.Wall;
            if ((shieldLayer.value & layer) != 0) return CollisionTargetType.Shield;
            if ((playerLayer.value & layer) != 0) return CollisionTargetType.Player;
            return CollisionTargetType.None;
        }
        #endregion

        #region 외부 API
        // 이벤트 구독 전체 해제. 오브젝트 풀 반환 전 호출
        public void ResetState()
        {
            isActive = true;
            OnMonsterHit = null;
            OnWallHit = null;
            OnShieldHit = null;
            OnPlayerHit = null;
        }
        #endregion
    }
}