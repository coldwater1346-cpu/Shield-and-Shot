using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Aim
{
    public class WeaponAimController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("이 크기 이하의 드래그는 무시하여 떨림 방지")]
        [SerializeField] private float minDragMagnitudeSqr = 0.01f;

        [Tooltip("활처럼 당겨서 발사: 드래그 반대 방향으로 조준")]
        [SerializeField] private bool invertDragDirection = true;

        private Vector3 aimDirection = Vector3.right;

        public Vector3 AimDirection => aimDirection;

        public void UpdateAimDirection(Vector3 dragVector)
        {
            if (dragVector.sqrMagnitude > minDragMagnitudeSqr)
            {
                aimDirection = invertDragDirection
                    ? (-dragVector).normalized
                    : dragVector.normalized;
            }
        }

        public void Reset()
        {
            aimDirection = Vector3.right;
        }
    }

    public static class AimDirectionUtility
    {
        public static Vector3 ToWorldXZ(Vector3 direction)
        {
            Vector3 worldDirection = Mathf.Approximately(direction.z, 0f)
                ? new Vector3(direction.x, 0f, direction.y)
                : new Vector3(direction.x, 0f, direction.z);

            return worldDirection.sqrMagnitude > 0.0001f
                ? worldDirection.normalized
                : Vector3.zero;
        }
    }
}
