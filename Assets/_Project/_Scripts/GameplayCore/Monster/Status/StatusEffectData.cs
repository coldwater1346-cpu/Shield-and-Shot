using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Status
{
    public readonly struct StatusEffectData
    {
        public readonly string StatusID;
        public readonly StatusEffectType Type;
        public readonly float Duration;
        public readonly float TickInterval;
        public readonly float DamagePerTick;
        public readonly float SlowMultiplier;
        public readonly Object Source;

        public readonly bool ShowDamagePopup;
        public readonly VFXType TickVfxType;
        public readonly float VfxAutoReleaseTime;

        public StatusEffectData(
            string statusID,
            StatusEffectType type,
            float duration,
            float tickInterval = 0f,
            float damagePerTick = 0f,
            float slowMultiplier = 1f,
            Object source = null,
            bool showDamagePopup = true,
            VFXType tickVfxType = VFXType.None,
            float vfxAutoReleaseTime = 1.5f)
        {
            StatusID = string.IsNullOrWhiteSpace(statusID)
                ? type.ToString()
                : statusID;

            Type = type;
            Duration = Mathf.Max(0f, duration);
            TickInterval = Mathf.Max(0f, tickInterval);
            DamagePerTick = Mathf.Max(0f, damagePerTick);
            SlowMultiplier = Mathf.Clamp01(slowMultiplier);
            Source = source;

            ShowDamagePopup = showDamagePopup;
            TickVfxType = tickVfxType;
            VfxAutoReleaseTime = Mathf.Max(0f, vfxAutoReleaseTime);
        }

        public bool IsDamageOverTime => DamagePerTick > 0f && TickInterval > 0f;

        public float DamagePerSecond
        {
            get
            {
                if (!IsDamageOverTime)
                {
                    return 0f;
                }

                return DamagePerTick / TickInterval;
            }
        }
    }
}