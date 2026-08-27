using Fusion;
using Shield_Shot.GameplayCore.Weapon.Shield;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class NetworkShieldDebugger : NetworkBehaviour
    {
        private NetworkShieldActor _shieldActor;
        private ShieldOrbitController _localOrbit;
        private float _logTimer;

        public override void Spawned()
        {
            _shieldActor = GetComponent<NetworkShieldActor>();

            Debug.Log($"[ShieldDebug] ===== Spawned =====\n" +
                      $"  HasInputAuthority  = {Object.HasInputAuthority}\n" +
                      $"  HasStateAuthority  = {Object.HasStateAuthority}\n" +
                      $"  InputAuthority     = {Object.InputAuthority}\n" +
                      $"  StateAuthority     = {Object.StateAuthority}\n" +
                      $"  LocalPlayer        = {Runner.LocalPlayer}");

            // 방패 생성 타이밍이 Spawned 이후라 1프레임 뒤에 확인
            Invoke(nameof(CheckShieldReady), 0.5f);
        }

        private void CheckShieldReady()
        {
            _localOrbit = GetComponentInChildren<ShieldOrbitController>(true);

            var allOrbits = GetComponentsInChildren<ShieldOrbitController>(true);

            Debug.Log($"[ShieldDebug] ===== ShieldReady Check =====\n" +
                      $"  HasInputAuthority    = {Object.HasInputAuthority}\n" +
                      $"  LocalOrbitController = {(_localOrbit != null ? _localOrbit.gameObject.name : "NULL ← 방패 미생성!")}\n" +
                      $"  OrbitController 수   = {allOrbits.Length}\n" +
                      $"  NetworkShieldActor   = {(_shieldActor != null ? "있음" : "NULL")}");

            if (_shieldActor != null)
            {
                // 리플렉션으로 내부 필드 확인
                var localOrbitField = typeof(NetworkShieldActor)
                    .GetField("_localOrbitCtrl",
                              System.Reflection.BindingFlags.NonPublic |
                              System.Reflection.BindingFlags.Instance);
                var remoteOrbitField = typeof(NetworkShieldActor)
                    .GetField("_remoteOrbitCtrl",
                              System.Reflection.BindingFlags.NonPublic |
                              System.Reflection.BindingFlags.Instance);

                var localOrbit = localOrbitField?.GetValue(_shieldActor) as ShieldOrbitController;
                var remoteOrbit = remoteOrbitField?.GetValue(_shieldActor) as ShieldOrbitController;

                Debug.Log($"[ShieldDebug] NetworkShieldActor 내부 참조:\n" +
                          $"  _localOrbitCtrl  = {(localOrbit != null ? localOrbit.gameObject.name : "NULL ← InjectShieldReferences 미호출!")}\n" +
                          $"  _remoteOrbitCtrl = {(remoteOrbit != null ? remoteOrbit.gameObject.name : "NULL ← InjectRemoteShieldReferences 미호출!")}");
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (_shieldActor == null) return;

            _logTimer += Runner.DeltaTime;
            if (_logTimer < 1f) return;
            _logTimer = 0f;

            // Networked 값 리플렉션으로 읽기
            var angleProp = typeof(NetworkShieldActor)
                .GetProperty("NetworkedOrbitAngle",
                             System.Reflection.BindingFlags.NonPublic |
                             System.Reflection.BindingFlags.Instance);
            float networkedAngle = angleProp != null
                ? (float)angleProp.GetValue(_shieldActor)
                : -999f;

            string who = Object.HasInputAuthority ? "[MY  ACTOR]" : "[REMOTE ACTOR]";
            Debug.Log($"{who} NetworkedOrbitAngle={networkedAngle:F1}  " +
                      $"LocalOrbit={(_localOrbit != null ? _localOrbit.CurrentAngle.ToString("F1") : "null")}");
        }
    }
}