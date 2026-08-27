using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Modifier
{
    public class RegularReflection : IReflectionBehavior
    {
        public Vector3 CalculateDirection(Vector3 incomingDirection, Vector3 surfaceNormal)
        {
            return Vector3.Reflect(incomingDirection, surfaceNormal);
        }
        public IReflectionBehavior Clone() => new RegularReflection();
    }
}
