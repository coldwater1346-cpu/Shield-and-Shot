using Fusion;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.InputSystem;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(WeaponBase))]
    public sealed class NetworkLocalWeaponInputBinder : NetworkBehaviour
    {
        [SerializeField] private PlayerInputReceiver _inputReceiver;

        private WeaponBase _weapon;

        private void Awake()
        {
            _weapon = GetComponent<WeaponBase>();
        }

        public override void Spawned()
        {
            if (!Object.HasInputAuthority)
            {
                return;
            }

            if (_weapon == null)
            {
                Debug.LogError("[NetworkLocalWeaponInputBinder] WeaponBase is missing.");
                return;
            }

            if (_inputReceiver == null)
            {
                _inputReceiver = FindFirstObjectByType<PlayerInputReceiver>();
            }

            if (_inputReceiver == null)
            {
                Debug.LogError("[NetworkLocalWeaponInputBinder] PlayerInputReceiver is missing in scene.");
                return;
            }

            _weapon.Initialize();
            _inputReceiver.SetCurrentWeapon(_weapon);
        }
    }
}
