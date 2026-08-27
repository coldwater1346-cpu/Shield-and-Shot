using Fusion;
using Shield_Shot.GameplayCore.Network.Match;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class NetworkProjectileIdentity : NetworkBehaviour
    {
        [Networked] public PlayerRef Owner { get; private set; }
        [Networked] public int OwnerSideValue { get; private set; }

        public PlayerSide OwnerSide => (PlayerSide)OwnerSideValue;

        public float SpawnTime { get; private set; }

        public bool IsOwnedBy(PlayerRef playerRef) => Owner == playerRef;
        public bool IsSameSide(PlayerSide side) => OwnerSide == side;

        public void Initialize(PlayerRef owner, PlayerSide ownerSide)
        {
            Owner = owner;
            OwnerSideValue = (int)ownerSide;
        }

        public override void Spawned()
        {
            SpawnTime = Time.time;
            Debug.Log($"[NetworkProjectileIdentity] Spawned. Owner: {Owner}, Side: {OwnerSide}");
        }
    }
}