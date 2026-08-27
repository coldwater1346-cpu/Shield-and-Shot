using UnityEngine;
using Shield_Shot.InputSystem;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class ShieldSystemBinder : MonoBehaviour
    {
        [SerializeField] private GestureAnalyzer defendGestureAnalyzer;
        [SerializeField] private ShieldOrbitController shieldOrbitController;

        public void InitializeBinding(GestureAnalyzer analyzer, ShieldOrbitController controller)
        {
            UnbindEvents();

            shieldOrbitController = controller;
            defendGestureAnalyzer = analyzer;

            if (defendGestureAnalyzer == null)
            {
                Debug.Log("[ShieldBinder] GestureAnalyzer null → 바인딩 스킵.");
                return;
            }

            BindEvents();
        }

        // Awake 자동 바인딩 제거
        // → 프리팹에 인스펙터로 연결되어 있어도 InitializeBinding 호출 전까지 바인딩 안 됨
        // → 싱글플레이: WeaponManager가 InitializeBinding 호출
        // → PvP: NetworkShieldSpawnSetup이 null로 호출하여 차단

        private void OnDestroy() => UnbindEvents();

        private void BindEvents()
        {
            if (defendGestureAnalyzer == null || shieldOrbitController == null) return;
            defendGestureAnalyzer.OnInputBegan += shieldOrbitController.OnOnInputBegan;
            defendGestureAnalyzer.OnInputStay += shieldOrbitController.OnOnInputStay;
            Debug.Log("[ShieldBinder] 바인딩 완료.");
        }

        private void UnbindEvents()
        {
            if (defendGestureAnalyzer == null || shieldOrbitController == null) return;
            defendGestureAnalyzer.OnInputBegan -= shieldOrbitController.OnOnInputBegan;
            defendGestureAnalyzer.OnInputStay -= shieldOrbitController.OnOnInputStay;
        }
    }
}