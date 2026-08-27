using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Stage;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shield_Shot.GameplayCore.Monster
{
    public class ClickDamage : MonoBehaviour
    {
        [SerializeField] private float _damage = 10f;
        [SerializeField] private StageManager _stageManager;

        private void Update()
        {
            // 스페이스바 → 스테이지 시작
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                _stageManager?.StartStage();

            // 마우스 클릭 → 몬스터 데미지
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var health = hit.collider.GetComponent<HealthComponent>();
                if (health != null)
                    health.TakeDamage(_damage);
            }
        }
    }
}