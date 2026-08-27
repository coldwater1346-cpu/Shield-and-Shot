using Fusion;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpMatchBootstrapper : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private PvpMatchStateController _matchStateController;

        [Header("Polling")]
        [SerializeField] private float _checkIntervalSeconds = 0.25f;

        private float _nextCheckTime;
        private bool _countdownRequested;

        private void Awake()
        {
            if (_matchStateController == null)
            {
                _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
            }
        }

        public override void Spawned()
        {
            Debug.Log($"[PvpMatchBootstrapper] Spawned. StateAuthority: {Object.HasStateAuthority}");
        }

        private void Update()
        {
            if (Runner == null ||
                Object == null ||
                !Object.HasStateAuthority ||
                _countdownRequested)
            {
                return;
            }

            if (Time.time < _nextCheckTime)
            {
                return;
            }

            _nextCheckTime = Time.time + _checkIntervalSeconds;

            TryStartCountdown();
        }

        private void TryStartCountdown()
        {
            if (_matchStateController == null)
            {
                _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
                if (_matchStateController == null)
                {
                    Debug.LogWarning("[PvpMatchBootstrapper] MatchStateController is missing.");
                    return;
                }
            }

            if (_matchStateController.CurrentState != PvpMatchState.WaitingForPlayers)
            {
                return;
            }

            int weaponCount = CountSpawnedWeaponActors();

            Debug.Log($"[PvpMatchBootstrapper] Waiting weapon actors: {weaponCount}/2");

            if (weaponCount < 2)
            {
                return;
            }

            _countdownRequested = true;
            _matchStateController.StartCountdown();
        }

        private static int CountSpawnedWeaponActors()
        {
            PvpWeaponActorIdentity[] actors = FindObjectsByType<PvpWeaponActorIdentity>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

            int count = 0;

            for (int i = 0; i < actors.Length; i++)
            {
                if (actors[i] == null)
                {
                    continue;
                }

                count++;
            }

            return count;
        }
    }
}