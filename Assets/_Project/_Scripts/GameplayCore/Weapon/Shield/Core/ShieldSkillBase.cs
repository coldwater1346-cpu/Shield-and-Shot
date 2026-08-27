using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public abstract class ShieldSkillBase : MonoBehaviour
    {
        public abstract void Activate();

        public virtual void OnGaugeReady() { }

        public virtual void OnGaugeReset() { }
    }
}