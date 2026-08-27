using System.Collections;
using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using Unity.VisualScripting;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class FieldClearSkill : ShieldSkillBase
    {
        [Header("Field Clear Settings")]
        [Tooltip("제거할 투사체 레이어 이름")]
        [SerializeField] private string _enemyProjectileLayerName = "MonsterProjectile";

        [Tooltip("방패 중심 충격파 VFX 타입")]
        [SerializeField] private VFXType _vfxSkill = VFXType.FieldClear;

        [Tooltip("투사체 소멸 VFX 타입")]
        [SerializeField] private VFXType _vfxDestroy;

        [Tooltip("방패에서 퍼져나가는 충격파 속도 (m/s)")]
        [SerializeField] private float _waveSpeed = 30f;

        [Header("Sound")]
        [SerializeField] private AudioClip _clearSfx;
        [SerializeField] private float _volume = 1f;

        public override void Activate()
        {
            if (VFXPoolManager.Instance != null)
                VFXPoolManager.Instance.SpawnVFX(_vfxSkill, transform.position, Quaternion.Euler(90f, 0f, 0f), 3f);
            SoundManager.Instance.PlaySFX(_clearSfx, _volume);

            Debug.Log("[FieldClearSkill] 방패 중심 파동 전개! 순차적 탄환 소멸 프로토콜 가동.");

#pragma warning disable CS0618
            ProjectileBase[] allProjectiles = FindObjectsOfType<ProjectileBase>();
#pragma warning restore CS0618

            int enemyLayer = LayerMask.NameToLayer(_enemyProjectileLayerName);
            Vector3 originPosition = transform.position;


            foreach (var projectile in allProjectiles)
            {
                if (projectile == null || projectile.gameObject == null) continue;
                if (projectile.gameObject.layer != enemyLayer) continue;

                float delay = Vector3.Distance(originPosition, projectile.transform.position) / _waveSpeed;
                StartCoroutine(Co_ReleaseWithDelay(projectile, delay));
            }
        }

        private IEnumerator Co_ReleaseWithDelay(ProjectileBase projectile, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (projectile == null || projectile.gameObject == null || !projectile.gameObject.activeSelf)
                yield break;

            if (_vfxDestroy != VFXType.None && VFXPoolManager.Instance != null)
                VFXPoolManager.Instance.SpawnVFX(_vfxDestroy, projectile.transform.position,
                    projectile.transform.rotation, 1.5f);

            projectile.gameObject.SetActive(false);

            if (ProjectileManager.Instance == null)
                Destroy(projectile.gameObject);
        }
    }
}