using System;
using System.Collections.Generic;
using Shield_Shot.GameplayCore.Monster.Attack;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.GameplayCore.Monster.Pool;
using Shield_Shot.GameplayCore.Monster.Spawn;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    /// 몬스터/보스 그룹을 스폰·초기화. 위치는 SpawnPointResolver, 등록·보스UI는 이벤트로 위임.
    public class MonsterSpawner : MonoBehaviour
    {
        [SerializeField] private SpawnPointResolver _spawnPoints;
        [SerializeField] private FormationSetSO _formationSet;
        [SerializeField] private ScriptableObject _defaultFormation;

        public event Action<MonsterBase> Spawned;      // 스폰된 모든 몬스터(등록용)
        public event Action<MonsterBase> BossSpawned;  // 보스(체력바 UI용)

        public void SpawnGroup(MonsterGroupPlan group, int difficulty)
        {
            MonsterUnitSO unit = group.Unit;
            if (unit == null || unit.Prefab == null) return;

            ISpawnFormation formation =
                (_formationSet != null ? _formationSet.GetForCount(group.Count) : null)
                ?? (_defaultFormation as ISpawnFormation);

            Vector3 center = _spawnPoints.GetMonsterSpawnCenter();
            List<Vector3> positions = formation != null
                ? formation.CalculatePositions(center, group.Count)
                : Fallback(center, group.Count);

            for (int k = 0; k < group.Count; k++)
            {
                //MonsterBase m = MonsterPoolManager.Instance.Get(
                //    unit.Prefab,
                //    _spawnPoints.ResolveY(positions[k]),
                //    _spawnPoints.GetMonsterSpawnRotation());

                MonsterBase m = MonsterFactory.Instance.Get(
                    unit.Prefab,
                    _spawnPoints.ResolveY(positions[k]),
                    _spawnPoints.GetMonsterSpawnRotation());
                m.Initialize(group, difficulty);
                Spawned?.Invoke(m);
            }
        }

        public void SpawnBossGroup(MonsterGroupPlan group, int difficulty)
        {
            MonsterUnitSO boss = group.Unit;
            if (boss == null || boss.Prefab == null) { Debug.LogError("[SpawnBoss] 설정 누락"); return; }

            Pose pose = _spawnPoints.GetBossSpawnPose();
            pose.position = _spawnPoints.ResolveY(pose.position);
            pose.position += Vector3.up * boss.SpawnYOffset;

            //MonsterBase b = MonsterPoolManager.Instance.Get(boss.Prefab, pose.position, pose.rotation);
            MonsterBase b = MonsterFactory.Instance.Get(boss.Prefab, pose.position, pose.rotation);

            if (b.TryGetComponent<AttackComponent>(out var attack)) attack.enabled = true;
            if (b.TryGetComponent<BossAttackDriver>(out var driver)) driver.enabled = false; // 이중 발사 방지

            b.Initialize(group, difficulty);
            b.Health.IsBoss = true;

            Spawned?.Invoke(b);
            BossSpawned?.Invoke(b);
            b.gameObject.SetActive(true);
        }

        private static List<Vector3> Fallback(Vector3 center, int count)
        {
            var list = new List<Vector3>(count);
            const float spacing = 1.5f;
            float totalW = spacing * (count - 1);
            for (int i = 0; i < count; i++)
                list.Add(center + new Vector3(-totalW * 0.5f + spacing * i, 0f, 0f));
            return list;
        }
    }
}