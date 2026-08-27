using Shield_Shot.InputSystem.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class ShieldOrbitController : MonoBehaviour
    {
        #region Inspector
        [Header("Orbit Settings")]
        [SerializeField] private Transform orbitCenter;
        [SerializeField] private float orbitRadius = 1.5f;

        [Header("Angle Limit")]
        [SerializeField, Range(10f, 180f)] private float halfAngleLimit = 80f;
        [SerializeField] private float fixedForwardAngle = 90f;

        [Header("Feel Settings")]
        [Tooltip("이 픽셀 거리만큼 노브를 당기면 halfAngleLimit까지 꽉 찬 것으로 취급한다. DynamicJoystickUI의 조이스틱 반경과 값을 맞춰줘야 한다.")]
        [SerializeField] private float maxJoystickRadius = 150f;
        [Tooltip("노브를 끝까지 당겼을 때(최대 편향) 초당 회전 각도")]
        [SerializeField] private float maxAngularSpeed = 90f;
        [SerializeField] private float keyboardSensitivity = 50f; // AD 키 속도 조절용

        [Header("Movement Settings")]
        [SerializeField] private float rotationLerpSpeed = 10f;
        #endregion

        #region 내부 상태
        private float currentAngle = 0f;
        private float targetAngle = 0f;
        private Vector2 prevDrag = Vector2.zero;
        #endregion

        public float CurrentAngle => currentAngle;

        #region Unity 생명주기
        private void Update()
        {
            if (orbitCenter == null) return;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                float keyboardInputX = 0f;

                if (keyboard.aKey.isPressed) keyboardInputX -= 1f;
                if (keyboard.dKey.isPressed) keyboardInputX += 1f;

                if (Mathf.Abs(keyboardInputX) > 0.001f)
                {
                    // A는 왼쪽(-1), D는 오른쪽(+1)이므로 방향에 맞춰 targetAngle을 변경한다.
                    float angleDelta = -keyboardInputX * keyboardSensitivity * Time.deltaTime;
                    targetAngle = ClampAngle(targetAngle + angleDelta);
                }
            }

            currentAngle = rotationLerpSpeed <= 0f
                ? targetAngle
                : Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationLerpSpeed * 10f * Time.deltaTime);

            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
            transform.position = orbitCenter.position + offset;

            Vector3 toCenter = (orbitCenter.position - transform.position).normalized;
            if (toCenter != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(toCenter, Vector3.up);
        }
        #endregion

        #region 공개 API
        public void SetupOrbitCenter(Transform center)
        {
            orbitCenter = center;
            if (orbitCenter == null) return;

            fixedForwardAngle = Mathf.Atan2(orbitCenter.forward.z, orbitCenter.forward.x) * Mathf.Rad2Deg;
            targetAngle = fixedForwardAngle;
            currentAngle = fixedForwardAngle;
            Debug.Log($"[ShieldOrbit] 중심 설정 완료: {orbitCenter.name}");
        }

        public void OnOnInputBegan(InputContext ctx) => ResetDragOrigin();
        public void OnOnInputStay(InputContext ctx) => UpdateOrbitFromDrag(ctx.dragVector);

        public void UpdateOrbitFromDrag(Vector2 screenDrag)
        {
            // screenDrag = 터치 시작점(앵커) 대비 절대 오프셋(조이스틱 노브 변위).
            // 편향량(0~1)을 "각속도"로 써서, 끝까지 당길수록 더 빠르게 계속 회전하도록 한다.
            float normalizedX = Mathf.Clamp(-screenDrag.x / maxJoystickRadius, -1f, 1f);
            float angularSpeed = normalizedX * maxAngularSpeed;
            targetAngle = ClampAngle(targetAngle + angularSpeed * Time.deltaTime);
        }

        public void ResetDragOrigin()
        {
            // 조이스틱 방식은 델타를 누적하지 않으므로 특별히 리셋할 상태가 없다.
            // 손을 뗐을 때 방패를 정면으로 되돌리고 싶다면 아래 주석을 해제한다.
            // targetAngle = fixedForwardAngle;
        }

        public void SetOrbitAngleDirectly(float angle)
        {
            targetAngle = ClampAngle(angle);
        }
        #endregion

        private float ClampAngle(float angle)
        {
            float relative = Mathf.Clamp(Mathf.DeltaAngle(fixedForwardAngle, angle),
                                         -halfAngleLimit, halfAngleLimit);
            return fixedForwardAngle + relative;
        }

        #region 디버그 기즈모
        private void OnDrawGizmosSelected()
        {
            if (orbitCenter == null) return;
            Gizmos.color = Color.gray;
            DrawArc(orbitCenter.position, orbitRadius, 0f, 360f, 64);
            float fwd = Mathf.Atan2(orbitCenter.forward.z, orbitCenter.forward.x) * Mathf.Rad2Deg;
            Gizmos.color = Color.green;
            DrawArc(orbitCenter.position, orbitRadius, fwd - halfAngleLimit, fwd + halfAngleLimit, 32);
        }

        private static void DrawArc(Vector3 center, float radius, float fromDeg, float toDeg, int segments)
        {
            float step = (toDeg - fromDeg) / segments;
            for (int i = 0; i < segments; i++)
            {
                float a1 = (fromDeg + step * i) * Mathf.Deg2Rad;
                float a2 = (fromDeg + step * (i + 1)) * Mathf.Deg2Rad;
                Gizmos.DrawLine(
                    center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius,
                    center + new Vector3(Mathf.Cos(a2), 0f, Mathf.Sin(a2)) * radius);
            }
        }
        #endregion
    }
}