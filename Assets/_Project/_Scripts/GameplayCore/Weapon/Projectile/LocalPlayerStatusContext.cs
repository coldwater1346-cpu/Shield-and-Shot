using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public static class LocalPlayerStatusContext
    {
        public static PlayerStatus Current { get; private set; }

        public static void Register(PlayerStatus playerStatus)
        {
            if (playerStatus == null)
            {
                return;
            }

            if (Current != null && Current != playerStatus)
            {
                Debug.LogWarning($"[LocalPlayerStatusContext] Replacing local PlayerStatus from {Current.name} to {playerStatus.name}.");
            }

            Current = playerStatus;
        }

        public static void Unregister(PlayerStatus playerStatus)
        {
            if (Current == playerStatus)
            {
                Current = null;
            }
        }

        public static bool TryGet(out PlayerStatus playerStatus)
        {
            playerStatus = Current;
            return playerStatus != null;
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}
