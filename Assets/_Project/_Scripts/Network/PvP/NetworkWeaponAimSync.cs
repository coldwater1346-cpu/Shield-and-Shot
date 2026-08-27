using Fusion;
using Shield_Shot.GameplayCore.Weapon.Aim;
using Shield_Shot.GameplayCore.Weapon.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(WeaponBase))]
    [RequireComponent(typeof(WeaponRotationController))]
    public sealed class NetworkWeaponAimSync : NetworkBehaviour
    {
        [Header("Smoothing")]
        [SerializeField] private float _remoteLerpSpeed = 20f;
        [SerializeField] private float _localLerpSpeed = 40f;

        [Networked] private Vector3 NetworkAimDirection { get; set; }

        private WeaponBase _weapon;
        private WeaponRotationController _rotationController;

        private Vector3 _smoothedLocalDirection = Vector3.forward;
        private Vector3 _smoothedRemoteDirection = Vector3.forward;

        private void Awake()
        {
            _weapon = GetComponent<WeaponBase>();
            _rotationController = GetComponent<WeaponRotationController>();
        }

        public override void Spawned()
        {
            if (_weapon != null)
            {
                _weapon.SetLocalRotationEnabled(false);
            }

            _smoothedLocalDirection = Vector3.right;
            _smoothedRemoteDirection = Vector3.right;
        }

        public override void FixedUpdateNetwork()
        {
            if (_weapon == null)
            {
                return;
            }

            if (!Object.HasInputAuthority)
            {
                return;
            }

            Vector3 aimDirection = _weapon.AimDirection;

            Vector3 normalizedDirection = NormalizeAimDirection(aimDirection);

            if (normalizedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            if (Object.HasStateAuthority)
            {
                NetworkAimDirection = normalizedDirection;
                return;
            }

            RPC_SetAimDirection(normalizedDirection);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetAimDirection(Vector3 aimDirection)
        {
            NetworkAimDirection = NormalizeAimDirection(aimDirection);
        }

        private void LateUpdate()
        {
            if (Object == null || _rotationController == null)
            {
                return;
            }

            if (Object.HasInputAuthority)
            {
                ApplyLocalRotation();
            }
            else
            {
                ApplyRemoteRotation();
            }
        }

        private void ApplyLocalRotation()
        {
            if (_weapon == null)
            {
                return;
            }

            Vector3 targetDirection = _weapon.AimDirection;

            if (targetDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _smoothedLocalDirection = Vector3.Slerp(
                _smoothedLocalDirection,
                NormalizeAimDirection(targetDirection),
                Time.deltaTime * _localLerpSpeed);

            _rotationController.SyncRotation(_smoothedLocalDirection);
        }

        private void ApplyRemoteRotation()
        {
            Vector3 targetDirection = NetworkAimDirection;

            if (targetDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _smoothedRemoteDirection = Vector3.Slerp(
                _smoothedRemoteDirection,
                NormalizeAimDirection(targetDirection),
                Time.deltaTime * _remoteLerpSpeed);

            _rotationController.SyncRotation(_smoothedRemoteDirection);
        }

        private static Vector3 NormalizeAimDirection(Vector3 direction)
        {
            direction.z = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector3.right;
            }

            return direction.normalized;
        }
    }
}
