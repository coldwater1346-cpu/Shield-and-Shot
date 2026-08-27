using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    [CreateAssetMenu(fileName = "NewShieldData", menuName = "ShieldShot/Weapon/ShieldData", order = 1)]
    public class ShieldDataSO : ScriptableObject
    {
        [Header("Base Settings")]
        [Tooltip("방패 프리팹 (실제 소환할 오브젝트)")]
        [SerializeField] private GameObject _shieldPrefab;

        [Tooltip("방패 최대 게이지 수치")]
        [SerializeField] private float _maxGauge = 100f;

        [Header("Reflect / Block Settings")]
        [Tooltip("반사/차단 성공 시 재생할 파티클 프리팹")]
        [SerializeField] private VFXType _vfxType;

        [Tooltip("파티클이 재생된 후 자동 파괴될 시간 (초)")]
        [SerializeField] private float _vfxDestroyTime = 1.5f;

        [Tooltip("반사 시 타겟이 될 몬스터 레이어")]
        [SerializeField] private LayerMask _monsterLayer;

        [Tooltip("반사 시 투사체 속도 보너스 배율")]
        [SerializeField] private float _speedMultiplier = 1f;

        #region Properties
        public GameObject ShieldPrefab => _shieldPrefab;
        public float MaxGauge => _maxGauge;
        public VFXType VfxType => _vfxType;
        public float VfxDestroyTime => _vfxDestroyTime;
        public LayerMask MonsterLayer => _monsterLayer;
        public float SpeedMultiplier => _speedMultiplier;
        #endregion
    }
}