using Fusion;
using System;
using System.Collections.Generic;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    [Serializable]
    public struct PvpStartingAugmentEntry
    {
        public ProjectileBehaviorSO BehaviorSO;
        [Min(1)] public int Level;
    }

    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(PlayerStatus))]
    public sealed class PvpStartingAugmentGranter : NetworkBehaviour
    {
        [Header("Starting Augments")]
        [SerializeField] private List<PvpStartingAugmentEntry> _startingAugments = new();
        [SerializeField] private bool _grantOnlyForInputAuthority = true;

        private PlayerStatus _playerStatus;
        private bool _granted;

        private void Awake()
        {
            _playerStatus = GetComponent<PlayerStatus>();
        }

        public override void Spawned()
        {
            TryGrantStartingAugment();
        }

        private void TryGrantStartingAugment()
        {
            if (_granted)
            {
                return;
            }

            if (_grantOnlyForInputAuthority && !Object.HasInputAuthority)
            {
                return;
            }

            if (_playerStatus == null)
            {
                _playerStatus = GetComponent<PlayerStatus>();
            }

            if (_playerStatus == null)
            {
                Debug.LogWarning("[PvpStartingAugmentGranter] PlayerStatus is missing.");
                return;
            }

            if (_startingAugments == null || _startingAugments.Count == 0)
            {
                Debug.LogWarning("[PvpStartingAugmentGranter] Starting augment list is empty.");
                return;
            }

            int grantedCount = 0;
            for (int i = 0; i < _startingAugments.Count; i++)
            {
                PvpStartingAugmentEntry entry = _startingAugments[i];
                ProjectileBehaviorSO behaviorSO = entry.BehaviorSO;

                if (behaviorSO == null)
                {
                    continue;
                }

                int targetLevel = Mathf.Max(1, entry.Level);
                int currentLevel = GetCurrentLevel(behaviorSO);

                for (int level = currentLevel; level < targetLevel; level++)
                {
                    _playerStatus.AddOrUpgradeBehavior(behaviorSO);
                }

                grantedCount++;
                Debug.Log($"[PvpStartingAugmentGranter] Granted starting augment: {behaviorSO.BehaviorName} Lv.{targetLevel}");
            }

            _granted = true;
            Debug.Log($"[PvpStartingAugmentGranter] Starting augment grant completed. Count: {grantedCount}");
        }

        private int GetCurrentLevel(ProjectileBehaviorSO behaviorSO)
        {
            if (behaviorSO == null)
            {
                return 0;
            }

            for (int i = 0; i < _playerStatus.CurrentBehaviors.Count; i++)
            {
                ActiveBehavior activeBehavior = _playerStatus.CurrentBehaviors[i];
                if (activeBehavior.BehaviorSO == null)
                {
                    continue;
                }

                if (activeBehavior.BehaviorSO.BehaviorID == behaviorSO.BehaviorID)
                {
                    return activeBehavior.Level;
                }
            }

            return 0;
        }
    }
}

