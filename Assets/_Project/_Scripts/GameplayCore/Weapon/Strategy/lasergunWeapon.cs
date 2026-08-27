using System.Collections.Generic;
using UnityEngine;
using Shield_Shot.InputSystem.Data;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using Shield_Shot.GameplayCore.Render;

namespace Shield_Shot.GameplayCore.Weapon
{
    public class LasergunWeapon : WeaponBase
    {
        public override WeaponType Type => WeaponType.Laser;

        [SerializeField] private Transform fireTipPoint;
        protected override Transform EffectSpawnPoint => fireTipPoint;

        [Header("Laser Beam Settings")]
        [Tooltip("비워두면(0) AimLineRenderer.MaxLineDistance 값을 그대로 사용 (시각/데미지 거리 동기화)")]
        [SerializeField] private float _laserRangeOverride = 0f;

        [Tooltip("적 데미지 판정 대상 레이어")]
        [SerializeField] private LayerMask _laserHitMask;

        [Tooltip("비워두면(레이어 0) AimLineRenderer.WallLayerMask를 그대로 사용")]
        [SerializeField] private LayerMask _wallLayerOverride;

        [Tooltip("레이저 경로 두께 (SphereCast 반경)")]
        [SerializeField] private float _beamThickness = 0.15f;

        [Header("Laser Damage Settings")]
        [Tooltip("틱당 데미지 레벨 (DefaultDamageSO와 동일한 레벨보너스 공식 사용)")]
        [SerializeField] private int _damageLevel = 1;

        [Tooltip("데미지 적용 주기 (초)")]
        [SerializeField] private float _tickInterval = 0.2f;

        [Tooltip("-1이면 AimLineRenderer.DefaultBounceCount를 그대로 사용, 0 이상이면 이 값으로 고정")]
        [SerializeField, Min(-1)] private int _reflectCountOverride = -1;

        [Header("Rainbow Color Settings")]
        [SerializeField] private float _rainbowCycleDuration = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _saturation = 1f;
        [SerializeField, Range(0f, 1f)] private float _value = 1f;
        [SerializeField, Range(0f, 1f)] private float _gradientHueShift = 0.15f;

        [Header("Sound")]
        [SerializeField] private AudioClip startClip;
        [SerializeField] private AudioClip loopClip;
        [SerializeField] private AudioClip endClip;
        [SerializeField] private float _volume = 1f;

        [SerializeField] private AudioSource oneShotSource;
        [SerializeField] private AudioSource loopSource;

        private float _tickTimer;
        private Material _laserMaterialInstance;
        private bool _laserVisualStarted;
        private bool _laserSoundStarted;

        private readonly List<BeamSegment> _beamSegments = new List<BeamSegment>();
        [SerializeField, Min(1)] private int _maxHitsPerSegment = 16;
        private RaycastHit[] _hitBuffer;

        private struct BeamSegment
        {
            public Vector3 Origin;
            public Vector3 Dir;
            public float Dist;

            public BeamSegment(Vector3 origin, Vector3 dir, float dist)
            {
                Origin = origin;
                Dir = dir;
                Dist = dist;
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            aimLineRenderer?.Hide();
            _hitBuffer ??= new RaycastHit[_maxHitsPerSegment];
        }

        public override void HandleInputStay(InputContext ctx)
        {
            if (!gameObject.activeInHierarchy) return;

            aimController?.UpdateAimDirection(ctx.dragVector);
            ApplyLocalRotation();
            chargeController?.Tick(Time.deltaTime);

            FireLaserTick();
        }

        public override void HandleInputUp(InputContext ctx)
        {
            aimLineRenderer?.Hide();
            StopLaserSound();
            Deactivate();
        }

        private void FireLaserTick()
        {
            if (fireTipPoint == null || aimLineRenderer == null) return;

            if (!_laserVisualStarted)
            {
                aimLineRenderer.Show(this);
                _laserVisualStarted = true;
            }

            if (!_laserSoundStarted)
            {
                _laserSoundStarted = true;
                PlayLaserStartSound();
            }

            aimLineRenderer.UpdateLine(AimDirection, 1f, fireTipPoint.position);

            ApplyRainbowColor();
            RecalculateDamageSegments();

            _tickTimer += Time.deltaTime;
            if (_tickTimer >= _tickInterval)
            {
                _tickTimer = 0f;
                ApplyDamageAlongAllSegments();
            }
        }

        private void PlayLaserStartSound()
        {
            if (loopSource != null) loopSource.Stop();

            if (oneShotSource != null && startClip != null)
                oneShotSource.PlayOneShot(startClip, _volume);

            if (loopSource != null && loopClip != null)
            {
                loopSource.clip = loopClip;
                loopSource.loop = true;
                loopSource.volume = _volume;

                double delay = startClip != null ? startClip.length : 0f;
                loopSource.PlayScheduled(AudioSettings.dspTime + delay);
            }
        }

        private void StopLaserSound()
        {
            if (!_laserSoundStarted) return;

            _laserSoundStarted = false;

            if (loopSource != null)
                loopSource.Stop();

            if (oneShotSource != null && endClip != null)
                oneShotSource.PlayOneShot(endClip, _volume);
        }

        #region AimLineRenderer 값 동기화 헬퍼
        /// <summary>Override가 0보다 크면 그 값을, 아니면 AimLineRenderer.MaxLineDistance를 사용.</summary>
        private float GetEffectiveLaserRange()
        {
            if (_laserRangeOverride > 0f) return _laserRangeOverride;
            return aimLineRenderer != null ? aimLineRenderer.MaxLineDistance : 30f;
        }

        /// <summary>Override 레이어가 설정되어 있으면 그 값을, 아니면 AimLineRenderer.WallLayerMask를 사용.</summary>
        private LayerMask GetEffectiveWallLayer()
        {
            if (_wallLayerOverride.value != 0) return _wallLayerOverride;
            return aimLineRenderer != null ? aimLineRenderer.WallLayerMask : _wallLayerOverride;
        }

        /// <summary>Override가 0 이상이면 그 값을, -1이면 AimLineRenderer.DefaultBounceCount를 사용.</summary>
        private int GetEffectiveReflectCount()
        {
            if (_reflectCountOverride >= 0) return _reflectCountOverride;
            return aimLineRenderer != null ? aimLineRenderer.DefaultBounceCount : 0;
        }
        #endregion

        private void ApplyRainbowColor()
        {
            var lineRenderer = aimLineRenderer.GetComponent<LineRenderer>();
            if (lineRenderer == null) return;

            float hue = Mathf.Repeat(Time.time / _rainbowCycleDuration, 1f);
            Color startColor = Color.HSVToRGB(hue, _saturation, _value);
            float endHue = Mathf.Repeat(hue + _gradientHueShift, 1f);
            Color endColor = Color.HSVToRGB(endHue, _saturation, _value);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
            lineRenderer.colorGradient = gradient;
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;

            if (_laserMaterialInstance == null || lineRenderer.sharedMaterial != _laserMaterialInstance)
            {
                _laserMaterialInstance = new Material(lineRenderer.sharedMaterial);
                lineRenderer.material = _laserMaterialInstance;
            }

            if (_laserMaterialInstance.HasProperty("_BaseColor"))
                _laserMaterialInstance.SetColor("_BaseColor", startColor);
            else if (_laserMaterialInstance.HasProperty("_Color"))
                _laserMaterialInstance.SetColor("_Color", startColor);
        }

        /// <summary>
        /// AimLineRenderer와 동일하게 벽에서만 반사, 적은 관통하는 경로를 데미지 판정용으로 재계산.
        /// </summary>
        private void RecalculateDamageSegments()
        {
            _beamSegments.Clear();

            Vector3 direction = AimDirection.sqrMagnitude > 0.0001f
                ? AimDirection.normalized
                : transform.forward;

            Vector3 worldDir = new Vector3(direction.x, 0f, direction.y).normalized;
            if (worldDir.sqrMagnitude < 0.0001f)
                worldDir = transform.forward;

            Vector3 currentOrigin = fireTipPoint.position;
            Vector3 currentDir = worldDir;
            float remainingRange = GetEffectiveLaserRange();
            int reflectsLeft = GetEffectiveReflectCount();
            LayerMask wallLayer = GetEffectiveWallLayer();

            while (remainingRange > 0.01f)
            {
                bool didHitWall = Physics.SphereCast(currentOrigin, _beamThickness, currentDir,
                    out RaycastHit hit, remainingRange, wallLayer);

                if (!didHitWall)
                {
                    _beamSegments.Add(new BeamSegment(currentOrigin, currentDir, remainingRange));
                    break;
                }

                _beamSegments.Add(new BeamSegment(currentOrigin, currentDir, hit.distance));

                if (reflectsLeft <= 0)
                    break;

                Vector3 reflectedDir = Vector3.Reflect(currentDir, hit.normal);
                reflectedDir.y = 0f;
                reflectedDir = reflectedDir.sqrMagnitude > 0.0001f ? reflectedDir.normalized : currentDir;

                remainingRange -= hit.distance;
                currentOrigin = hit.point + reflectedDir * 0.02f;
                currentDir = reflectedDir;
                reflectsLeft--;
            }
        }

        private float CalculateTickDamage()
        {
            var statCalc = projectileLauncher != null ? projectileLauncher.StatCalculator : null;
            if (statCalc == null) return 5f;

            float baseDamage = statCalc.Calculate(0f).Damage;
            float levelBonus = (_damageLevel - 1) * 5f;
            return baseDamage + levelBonus;
        }

        private void ApplyDamageAlongAllSegments()
        {
            float tickDamage = CalculateTickDamage();
            int totalHitCount = 0;

            foreach (var segment in _beamSegments)
            {
                int hitCount = Physics.SphereCastNonAlloc(
                    segment.Origin, _beamThickness, segment.Dir, _hitBuffer, segment.Dist, _laserHitMask);

                for (int i = 0; i < hitCount; i++)
                {
                    RaycastHit hit = _hitBuffer[i];

                    var monster = hit.collider.GetComponent<Monster.Core.MonsterBase>()
                               ?? hit.collider.GetComponentInParent<Monster.Core.MonsterBase>();

                    if (monster == null || monster.Health == null) continue;

                    monster.Health.TakeDamage(tickDamage);

                    if (DamagePopupManager.Instance != null)
                        DamagePopupManager.Instance.Show(hit.point, tickDamage, false);

                    totalHitCount++;
                }
            }

            if (totalHitCount > 0)
                Debug.Log($"[LasergunWeapon] 레이저 틱 데미지 {tickDamage:F1} 적용. 대상 수: {totalHitCount}");
        }

        public override void Deactivate()
        {
            base.Deactivate();
            chargeController?.Reset();
            aimLineRenderer?.Hide();
            StopLaserSound();
            _tickTimer = 0f;
            _laserVisualStarted = false;
            _beamSegments.Clear();
        }

        private void OnDestroy()
        {
            if (_laserMaterialInstance != null)
                Destroy(_laserMaterialInstance);
        }
    }
}