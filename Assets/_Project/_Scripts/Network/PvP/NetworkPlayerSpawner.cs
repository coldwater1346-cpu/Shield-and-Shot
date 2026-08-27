using Fusion;
using Shield_Shot.GameplayCore.Network.Match;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class NetworkPlayerSpawner : MonoBehaviour
    {
        [Header("Network Prefabs")]
        [SerializeField] private NetworkObject _weaponActorPrefab;

        [Header("Spawn Scale")]
        [SerializeField, Min(0.01f)] private float _weaponActorSpawnScale = 1f;

        [Header("Scene Services")]
        [SerializeField] private PvpSpawnPointProvider _spawnPointProvider;

        public async Task SpawnPlayersAsync(NetworkRunner runner, MatchContext matchContext)
        {
            if (runner == null)
            {
                Debug.LogError("[NetworkPlayerSpawner] NetworkRunner is missing.");
                return;
            }

            if (!runner.IsServer)
            {
                return;
            }

            if (matchContext == null)
            {
                Debug.LogError("[NetworkPlayerSpawner] MatchContext is missing.");
                return;
            }

            if (_weaponActorPrefab == null)
            {
                Debug.LogError("[NetworkPlayerSpawner] Weapon actor prefab is not assigned.");
                return;
            }

            if (_spawnPointProvider == null)
            {
                Debug.LogError("[NetworkPlayerSpawner] SpawnPointProvider is not assigned.");
                return;
            }

            foreach (MatchPlayerInfo playerInfo in matchContext.Players)
            {
                await SpawnPlayerIfNeededAsync(runner, playerInfo);
            }
        }

        private async Task SpawnPlayerIfNeededAsync(NetworkRunner runner, MatchPlayerInfo playerInfo)
        {
            if (playerInfo == null || !playerInfo.PlayerRef.IsRealPlayer)
            {
                return;
            }

            if (runner.TryGetPlayerObject(playerInfo.PlayerRef, out NetworkObject existingObject) && existingObject != null)
            {
                return;
            }

            if (!_spawnPointProvider.TryGetWeaponSpawnPose(playerInfo.Side, out Pose spawnPose))
            {
                Debug.LogError($"[NetworkPlayerSpawner] Weapon spawn pose missing for side: {playerInfo.Side}.");
                return;
            }

            NetworkObject playerObject;
            try
            {
                NetworkSpawnOp spawnOp = runner.SpawnAsync(
                    _weaponActorPrefab,
                    spawnPose.position,
                    spawnPose.rotation,
                    playerInfo.PlayerRef,
                    onBeforeSpawned: (_, spawnedObject) =>
                    {
                        if (spawnedObject.TryGetComponent(out PvpWeaponActorIdentity identity))
                        {
                            identity.Initialize(playerInfo.PlayerRef, playerInfo.Side, spawnPose, _weaponActorSpawnScale);
                        }
                        else
                        {
                            Debug.LogError("[NetworkPlayerSpawner] PvpWeaponActorIdentity is missing on weapon actor prefab.");
                        }
                    });

                playerObject = await spawnOp;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[NetworkPlayerSpawner] Failed to spawn weapon actor for {playerInfo.PlayerRef}. {exception}");
                return;
            }

            if (playerObject == null)
            {
                Debug.LogError($"[NetworkPlayerSpawner] SpawnAsync returned null for {playerInfo.PlayerRef}.");
                return;
            }

            runner.SetPlayerObject(playerInfo.PlayerRef, playerObject);
            playerObject.transform.SetPositionAndRotation(spawnPose.position, spawnPose.rotation);
            playerObject.transform.localScale = Vector3.one * _weaponActorSpawnScale;

            Debug.Log(
                $"[NetworkPlayerSpawner] Spawned {playerInfo.PlayerRef} at {playerInfo.Side}. " +
                $"SpawnPose: {spawnPose.position}, Rotation: {spawnPose.rotation.eulerAngles}, " +
                $"Scale: {_weaponActorSpawnScale}, ActualPosition: {playerObject.transform.position}");
        }
    }
}
