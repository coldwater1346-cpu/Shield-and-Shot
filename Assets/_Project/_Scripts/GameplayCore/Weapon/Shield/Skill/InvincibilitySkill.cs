using System.Collections;
using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class InvincibilitySkill : ShieldSkillBase
    {
        [Header("Invincibility Settings")]
        [Tooltip("무적 지속 시간 (초)")]
        [SerializeField] private float _invincibleDuration = 3f;

        [Header("VFX Settings")]
        [Tooltip("무적 상태 표시 VFX 타입 (VFXPoolManager에 등록된 일반 오브젝트도 가능)")]
        [SerializeField] private VFXType _vfxType = VFXType.Invincibility;

        [Header("Sound")]
        [SerializeField] private AudioClip _invincivilitySfx;
        [SerializeField] private float _volume = 1f;

        private PlayerStatus _playerStatus;
        private GameObject _activeVfxInstance;

        private void Awake()
        {
            _playerStatus = FindFirstObjectByType<PlayerStatus>();
            if (_playerStatus == null)
                Debug.LogWarning("[InvincibilitySkill] PlayerStatus를 찾을 수 없음. Activate 시 재탐색.");
        }

        public override void Activate()
        {
            if (_playerStatus == null)
                _playerStatus = FindFirstObjectByType<PlayerStatus>();

            if (_playerStatus == null)
            {
                Debug.LogError("[InvincibilitySkill] PlayerStatus 없음. 무적 적용 실패.");
                return;
            }

            _playerStatus.StartInvincibility(_invincibleDuration);

            SoundManager.Instance.PlaySFX(_invincivilitySfx, _volume);

            // 플레이어 위치에서 VFX 재생 (자동으로 _invincibleDuration 후 풀로 반환)
            if (_vfxType != VFXType.None && VFXPoolManager.Instance != null)
            {
                _activeVfxInstance = VFXPoolManager.Instance.SpawnVFX(
                    _vfxType,
                    _playerStatus.transform.position,
                    Quaternion.identity,
                    _invincibleDuration
                );

                // VFX가 플레이어를 따라다니도록 부모 설정
                if (_activeVfxInstance != null)
                    _activeVfxInstance.transform.SetParent(_playerStatus.transform, true);
            }

            Debug.Log($"[InvincibilitySkill] 무적 발동! 지속시간: {_invincibleDuration}초");
        }
    }
}