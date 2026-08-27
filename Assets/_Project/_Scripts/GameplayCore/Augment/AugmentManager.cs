using System.Collections.Generic;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Augment
{
    public class AugmentManager : Singleton<AugmentManager>
    {
        [Header("Augment Database")]
        [SerializeField] private AugmentDatabaseSO database;

        [Header("Popup Settings")]
        [SerializeField] private int selectCardCount = 2;

        public List<ProjectileBehaviorSO> RollAugments()
        {
            if (database == null || database.AllBehaviors == null)
            {
                Debug.LogError("[AugmentManager] Database or behavior list is empty.");
                return null;
            }

            bool hasPlayerStatus = LocalPlayerStatusContext.TryGet(out PlayerStatus playerStatus);
            if (!hasPlayerStatus)
            {
                Debug.LogWarning("[AugmentManager] Local PlayerStatus를 아직 찾지 못했습니다. 모든 특성을 0레벨로 간주하고 카드를 뽑습니다.");
            }

            List<ProjectileBehaviorSO> availablePool = new List<ProjectileBehaviorSO>();

            foreach (var behavior in database.AllBehaviors)
            {
                if (behavior == null || string.IsNullOrEmpty(behavior.BehaviorID)) continue;

                int currentLevel = hasPlayerStatus ? playerStatus.GetBehaviorLevel(behavior) : 0;
                if (currentLevel < behavior.MaxLevel)
                {
                    availablePool.Add(behavior);
                }
            }

            if (availablePool.Count == 0)
            {
                Debug.Log("[AugmentManager] 뽑을 수 있는 특성이 없습니다 (전부 마스터 레벨).");
                return null;
            }

            List<ProjectileBehaviorSO> result = new List<ProjectileBehaviorSO>();

            for (int i = 0; i < selectCardCount; i++)
            {
                if (availablePool.Count > 0)
                {
                    int randomIndex = Random.Range(0, availablePool.Count);
                    result.Add(availablePool[randomIndex]);
                    availablePool.RemoveAt(randomIndex);
                }
                else
                {
                    result.Add(null);
                }
            }

            return result;
        }

        public void UpgradeAugment(string behaviorID)
        {
            if (string.IsNullOrEmpty(behaviorID)) return;

            ProjectileBehaviorSO targetSO = FindBehaviorSOInDatabase(behaviorID);
            if (targetSO == null)
            {
                Debug.LogError($"[AugmentManager] Could not find behavior SO for ID [{behaviorID}].");
                return;
            }

            if (!LocalPlayerStatusContext.TryGet(out PlayerStatus playerStatus))
            {
                Debug.LogError("[AugmentManager] Local PlayerStatus is missing.");
                return;
            }

            int currentLevel = playerStatus.GetBehaviorLevel(targetSO);
            if (currentLevel >= targetSO.MaxLevel)
            {
                Debug.LogWarning($"[AugmentManager] {behaviorID} is already at max level ({targetSO.MaxLevel}).");
                return;
            }

            playerStatus.AddOrUpgradeBehavior(targetSO);

            // 임시 디버그
            for (int i = 0; i < playerStatus.CurrentBehaviors.Count; i++)
            {
                var b = playerStatus.CurrentBehaviors[i];
                Debug.Log($"[UpgradeDebug] [{i}] {b.BehaviorSO?.BehaviorName}(ID:{b.BehaviorSO?.BehaviorID}) Lv.{b.Level}");
            }

            Debug.Log($"[AugmentManager] {behaviorID} upgraded. New level: {playerStatus.GetBehaviorLevel(targetSO)}");
        }

        private ProjectileBehaviorSO FindBehaviorSOInDatabase(string behaviorID)
        {
            if (database == null || database.AllBehaviors == null) return null;

            foreach (var behavior in database.AllBehaviors)
            {
                if (behavior != null && behavior.BehaviorID == behaviorID)
                {
                    return behavior;
                }
            }

            return null;
        }

        public int GetCurrentLevel(string behaviorID)
        {
            ProjectileBehaviorSO so = FindBehaviorSOInDatabase(behaviorID);
            if (so == null) return 0;

            if (!LocalPlayerStatusContext.TryGet(out PlayerStatus playerStatus)) return 0;
            return playerStatus.GetBehaviorLevel(so);
        }
    }
}