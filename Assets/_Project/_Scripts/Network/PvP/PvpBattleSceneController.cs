using Fusion;
using Shield_Shot.Core.SceneFlow;
using Shield_Shot.GameplayCore.Field;
using Shield_Shot.GameplayCore.Network;
using Shield_Shot.GameplayCore.Network.Match;
using System.Collections;
using UnityEngine;
using Shield_Shot.InputSystem;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpBattleSceneController : BaseSceneController
    {
        [Header("Input")]
        [SerializeField] private PlayerInputReceiver _inputReceiver;
        [SerializeField] private PvpInputPerspectiveTransformer _inputPerspectiveTransformer;

        [Header("Scene Services")]
        [SerializeField] private PvpSpawnPointProvider _spawnPointProvider;
        [SerializeField] private NetworkPlayerSpawner _playerSpawner;
        [SerializeField] private PvpCameraPerspectiveController _cameraPerspectiveController;

        [Header("Arena")]
        [SerializeField] private ArenaTerrainPainter _arenaTerrainPainter;
        [SerializeField] private ArenaBoundaryBuilder _arenaBoundaryBuilder;
        [SerializeField] private ArenaRandomReflectWallBuilder _randomReflectWallBuilder;
        [SerializeField] private ArenaCameraPresetController _arenaCameraPresetController;
        [SerializeField] private PvpMatchStateController _matchStateController;

        private MatchContext _matchContext;
        private PlayerSide _localSide = PlayerSide.None;

        protected override void OnEnterScene(SceneTransitionData transitionData)
        {
            if (!TryLoadMatchContext())
            {
                Debug.LogError("[PvpBattleSceneController] MatchContext is missing.");
                return;
            }

            if (!TryResolveLocalSide(out _localSide))
            {
                Debug.LogError("[PvpBattleSceneController] Failed to resolve local player side.");
                return;
            }

            Debug.Log($"[PvpBattleSceneController] PvP scene entered.");
            Debug.Log($"[PvpBattleSceneController] Local player side: {_localSide}");

            StartCoroutine(CoEnterSceneAfterArenaReady());
        }

        private IEnumerator CoEnterSceneAfterArenaReady()
        {
            yield return null;

            yield return CoGenerateArenaFromNetworkSeed();
            InitializeArena();
            BindInputPerspective();
            ApplyCameraPerspective();
            LogLocalSpawnPose();
            SpawnNetworkPlayers();
        }

        protected override void OnExitScene()
        {
        }

        private bool TryLoadMatchContext()
        {
            if (!MatchContextStore.TryGet(out MatchContext matchContext))
            {
                NetworkMatchManager.Instance?.TryEnsureCurrentMatchContext(out matchContext);
            }

            if (matchContext == null)
            {
                return false;
            }

            _matchContext = matchContext;
            MatchContextStore.Set(_matchContext);
            return true;
        }

        private bool TryResolveLocalSide(out PlayerSide side)
        {
            side = PlayerSide.None;

            NetworkRunner runner = NetworkMatchManager.Instance != null
                ? NetworkMatchManager.Instance.CurrentRunner
                : null;

            if (runner == null)
            {
                return false;
            }

            PlayerRef localPlayer = runner.LocalPlayer;

            foreach (MatchPlayerInfo player in _matchContext.Players)
            {
                if (player.PlayerRef == localPlayer)
                {
                    side = player.Side;
                    return true;
                }
            }

            return false;
        }

        private void LogLocalSpawnPose()
        {
            if (_spawnPointProvider == null)
            {
                Debug.LogError("[PvpBattleSceneController] SpawnPointProvider is missing.");
                return;
            }

            if (!_spawnPointProvider.TryGetWeaponSpawnPose(_localSide, out Pose weaponPose))
            {
                Debug.LogError($"[PvpBattleSceneController] Weapon spawn pose missing for side: {_localSide}");
                return;
            }

            Debug.Log(
                $"[PvpBattleSceneController] Local weapon spawn position: {weaponPose.position}, " +
                $"rotation: {weaponPose.rotation.eulerAngles}");
        }

        private void SpawnNetworkPlayers()
        {
            if (_playerSpawner == null)
            {
                Debug.LogError("[PvpBattleSceneController] NetworkPlayerSpawner is missing.");
                return;
            }

            NetworkRunner runner = NetworkMatchManager.Instance != null
                ? NetworkMatchManager.Instance.CurrentRunner
                : null;

            _ = _playerSpawner.SpawnPlayersAsync(runner, _matchContext);
        }

        private IEnumerator CoGenerateArenaFromNetworkSeed()
        {
            if (_arenaTerrainPainter == null)
            {
                _arenaTerrainPainter = FindFirstObjectByType<ArenaTerrainPainter>();
            }

            if (_matchStateController == null)
            {
                _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
            }

            if (_arenaTerrainPainter == null)
            {
                Debug.LogWarning("[PvpBattleSceneController] ArenaTerrainPainter is missing.");
                yield break;
            }

            float timeoutAt = Time.realtimeSinceStartup + 5f;
            while (!CanReadMatchStateNetworkedProperties() && Time.realtimeSinceStartup < timeoutAt)
            {
                if (_matchStateController == null)
                {
                    _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
                }

                yield return null;
            }

            if (!CanReadMatchStateNetworkedProperties())
            {
                Debug.LogWarning("[PvpBattleSceneController] MatchStateController is not spawned yet. Arena generation uses local painter settings.");
                _arenaTerrainPainter.GenerateThemeTerrain();
                yield break;
            }

            if (_matchStateController.Object != null && _matchStateController.Object.HasStateAuthority)
            {
                _matchStateController.EnsureArenaSeedInitialized();
            }

            while (!_matchStateController.ArenaSeedInitialized && Time.realtimeSinceStartup < timeoutAt)
            {
                yield return null;
            }

            if (!_matchStateController.ArenaSeedInitialized)
            {
                Debug.LogWarning("[PvpBattleSceneController] Timed out waiting for arena seed. Arena generation uses local painter settings.");
                _arenaTerrainPainter.GenerateThemeTerrain();
                yield break;
            }

            _arenaTerrainPainter.GenerateThemeTerrain(_matchStateController.ArenaSeed);
            Debug.Log($"[PvpBattleSceneController] Arena generated from network seed: {_matchStateController.ArenaSeed}");
        }

        private bool CanReadMatchStateNetworkedProperties()
        {
            return _matchStateController != null &&
                   _matchStateController.Object != null &&
                   _matchStateController.Object.IsValid;
        }

        private void InitializeArena()
        {
            if (_arenaBoundaryBuilder == null)
            {
                _arenaBoundaryBuilder = FindFirstObjectByType<ArenaBoundaryBuilder>();
            }

            if (_arenaBoundaryBuilder != null)
            {
                _arenaBoundaryBuilder.BuildWalls();
            }

            if (_randomReflectWallBuilder == null)
            {
                _randomReflectWallBuilder = FindFirstObjectByType<ArenaRandomReflectWallBuilder>();
            }

            if (_randomReflectWallBuilder == null && _arenaBoundaryBuilder != null)
            {
                _randomReflectWallBuilder = _arenaBoundaryBuilder.GetComponent<ArenaRandomReflectWallBuilder>();
                if (_randomReflectWallBuilder == null)
                {
                    _randomReflectWallBuilder = _arenaBoundaryBuilder.gameObject.AddComponent<ArenaRandomReflectWallBuilder>();
                }
            }

            if (_randomReflectWallBuilder != null)
            {
                _randomReflectWallBuilder.ConfigureFromBoundaryBuilder(_arenaBoundaryBuilder);
                _randomReflectWallBuilder.SetGeneratedWallLayerName("PvpWall");

                int wallSeed = CanReadMatchStateNetworkedProperties() && _matchStateController.ArenaSeedInitialized
                    ? _matchStateController.ArenaSeed
                    : 12345;

                _randomReflectWallBuilder.BuildRandomWalls(wallSeed);
            }

            if (_arenaCameraPresetController == null)
            {
                _arenaCameraPresetController = FindFirstObjectByType<ArenaCameraPresetController>();
            }

            if (_arenaCameraPresetController != null)
            {
                _arenaCameraPresetController.ApplyCameraPreset();
            }
        }

        private void ApplyCameraPerspective()
        {
            if (_cameraPerspectiveController == null)
            {
                Debug.LogError("[PvpBattleSceneController] CameraPerspectiveController is missing.");
                return;
            }

            _cameraPerspectiveController.ApplyPerspective(_localSide);
        }
        private void BindInputPerspective()
        {
            if (_inputReceiver == null)
            {
                _inputReceiver = FindFirstObjectByType<PlayerInputReceiver>();
            }

            if (_inputPerspectiveTransformer == null)
            {
                _inputPerspectiveTransformer = FindFirstObjectByType<PvpInputPerspectiveTransformer>();
            }

            if (_inputReceiver == null)
            {
                Debug.LogError("[PvpBattleSceneController] PlayerInputReceiver is missing.");
                return;
            }

            if (_inputPerspectiveTransformer == null)
            {
                Debug.LogError("[PvpBattleSceneController] PvpInputPerspectiveTransformer is missing.");
                return;
            }

            _inputPerspectiveTransformer.SetLocalSide(_localSide);
            _inputReceiver.SetContextTransformer(_inputPerspectiveTransformer);

            Debug.Log($"[PvpBattleSceneController] Input perspective bound for side: {_localSide}");
        }
    }
}
