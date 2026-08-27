//using Shield_Shot.Monster.Core;
//using Shield_Shot.Monster.Movement;
//using Shield_Shot.Monster.Pool;
//using Shield_Shot.Monster.Spawn;
//using Shield_Shot.Monster.Trait;
//using Shield_Shot.Monster.Wave;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Shield_Shot.Monster
//{
//    public class SpawnTester : MonoBehaviour
//    {
//        [SerializeField] private WaveSO waveSO;
//        [SerializeField] private MonsterPoolManager poolManager;

//        [Header("스폰 범위 (좌우)")]
//        [SerializeField] private float spawnRangeX = 5f;

//        [ContextMenu("1회 소환 테스트")]
//        public void SpawnOnce()
//        {
//            if (!waveSO.TryPickSpawnGroup(out SpawnGroup group)) return;
//            int count = waveSO.RollSpawnCount(waveSO.TotalSpawnCount);
//            SpawnMonsters(group, count);
//        }

//        [ContextMenu("웨이브 시뮬레이션 시작")]
//        public void StartWaveSimulation() => StartCoroutine(WaveRoutine());

//        private IEnumerator WaveRoutine()
//        {
//            int remaining = waveSO.TotalSpawnCount;

//            while (remaining > 0)
//            {
//                if (!waveSO.TryPickSpawnGroup(out SpawnGroup group)) break;

//                int count = waveSO.RollSpawnCount(remaining);
//                SpawnMonsters(group, count);
//                remaining -= count;

//                Debug.Log($"[SpawnTester] {count}마리 소환 / 남은 수 {remaining}");

//                yield return new WaitForSeconds(waveSO.SpawnInterval);
//            }

//            Debug.Log("[SpawnTester] 웨이브 완료");
//        }

//        private void SpawnMonsters(SpawnGroup group, int count)
//        {
//            ISpawnFormation formation = waveSO.GetFormationForCount(count);
//            Vector3 center = GetRandomCenter();

//            List<Vector3> positions = formation != null
//                ? formation.CalculatePositions(center, count)
//                : Fallback(center, count);

//            for (int i = 0; i < count; i++)
//            {
//                MonsterBase monster = poolManager != null
//                    ? poolManager.Get(group.MonsterPrefab, positions[i], transform.rotation)
//                    : Instantiate(group.MonsterPrefab, positions[i], transform.rotation).GetComponent<MonsterBase>();

//                monster.Initialize(group);
//            }
//        }

//        private Vector3 GetRandomCenter()
//        {
//            float randomX = Random.Range(-spawnRangeX, spawnRangeX);
//            return transform.position + new Vector3(randomX, 0f, 0f);
//        }

//        private static List<Vector3> Fallback(Vector3 center, int count)
//        {
//            var list = new List<Vector3>(count);
//            for (int i = 0; i < count; i++) list.Add(center);
//            return list;
//        }

//        private void OnDrawGizmosSelected()
//        {
//            Gizmos.color = Color.cyan;
//            Vector3 left = transform.position + Vector3.left * spawnRangeX;
//            Vector3 right = transform.position + Vector3.right * spawnRangeX;
//            Gizmos.DrawLine(left, right);
//            Gizmos.DrawWireSphere(left, 0.2f);
//            Gizmos.DrawWireSphere(right, 0.2f);
//        }
//    }
//}