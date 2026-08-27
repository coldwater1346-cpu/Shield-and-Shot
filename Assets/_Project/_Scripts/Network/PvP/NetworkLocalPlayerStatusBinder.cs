using Fusion;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(PlayerStatus))]
    public sealed class NetworkLocalPlayerStatusBinder : NetworkBehaviour
    {
        private PlayerStatus _playerStatus;

        private void Awake()
        {
            _playerStatus = GetComponent<PlayerStatus>();
        }

        public override void Spawned()
        {
            if (_playerStatus == null)
            {
                return;
            }

            if (Object.HasInputAuthority)
            {
                LocalPlayerStatusContext.Register(_playerStatus);
            }
            else
            {
                LocalPlayerStatusContext.Unregister(_playerStatus);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_playerStatus != null)
            {
                LocalPlayerStatusContext.Unregister(_playerStatus);
            }
        }
    }
}
