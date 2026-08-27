using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    [RequireComponent(typeof(PlayerStatus))]
    public sealed class PvpProjectileAugmentSnapshotProvider : MonoBehaviour
    {
        private PlayerStatus _playerStatus;

        private void Awake()
        {
            _playerStatus = GetComponent<PlayerStatus>();
        }

        public PvpProjectileAugmentPayload CreatePayload()
        {
            PvpProjectileAugmentPayload payload = CreatePayloadFrom(_playerStatus);

            if (!payload.HasAnyAugment &&
                LocalPlayerStatusContext.TryGet(out PlayerStatus localPlayerStatus) &&
                localPlayerStatus != _playerStatus)
            {
                payload = CreatePayloadFrom(localPlayerStatus);
            }

            return payload;
        }

        public static PvpProjectileAugmentPayload CreatePayloadFrom(PlayerStatus playerStatus)
        {
            PvpProjectileAugmentPayload payload = PvpProjectileAugmentPayload.Empty;

            if (playerStatus == null)
            {
                Debug.LogWarning("[PvpProjectileAugmentSnapshotProvider] PlayerStatus is missing.");
                return payload;
            }

            Debug.Log($"[PvpProjectileAugmentSnapshotProvider] Read PlayerStatus: {playerStatus.name}, Behaviors: {playerStatus.CurrentBehaviors.Count}");

            for (int i = 0; i < playerStatus.CurrentBehaviors.Count; i++)
            {
                ActiveBehavior activeBehavior = playerStatus.CurrentBehaviors[i];
                ProjectileBehaviorSO behaviorSO = activeBehavior.BehaviorSO;

                if (behaviorSO == null)
                {
                    continue;
                }

                int networkCode = PvpProjectileBehaviorCode.Resolve(behaviorSO);
                int level = activeBehavior.Level;

                if (level <= 0)
                {
                    continue;
                }

                if (networkCode == 0)
                {
                    Debug.Log($"[PvpProjectileAugmentSnapshotProvider] Skip non-network behavior: {behaviorSO.name}, ID: {behaviorSO.BehaviorID}");
                    continue;
                }

                bool added = payload.TryAdd(new PvpProjectileAugmentEntry(networkCode, level));
                if (!added)
                {
                    Debug.LogWarning($"[PvpProjectileAugmentSnapshotProvider] Payload is full. Skipped behavior: {behaviorSO.BehaviorName}");
                    break;
                }

                Debug.Log($"[PvpProjectileAugmentSnapshotProvider] Added behavior payload: {behaviorSO.BehaviorName}({networkCode}) Lv.{level}");
            }

            return payload;
        }
    }
}

