using UnityEngine;

namespace Assets._Project._Scripts.GameplayCore.Weapon.Core
{
    public interface IWeaponFireExecutor
    {
        void FireFromSkill(Transform spawnPoint, Vector3 centerDirection, float chargeRatio, bool isCritical);
    }
}
