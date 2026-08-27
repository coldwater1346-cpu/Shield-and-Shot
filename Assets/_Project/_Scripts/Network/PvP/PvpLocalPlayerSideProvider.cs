using Fusion;
using Shield_Shot.GameplayCore.Network.Match;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpLocalPlayerSideProvider : MonoBehaviour
    {
        [Header("Cache")]
        [SerializeField] private bool _logWhenFound = true;

        private PvpWeaponActorIdentity _localActor;
        private bool _hasCached;

        public bool TryGetLocalSide(out PlayerSide side)
        {
            if (_hasCached && _localActor != null)
            {
                side = _localActor.Side;
                return true;
            }

            PvpWeaponActorIdentity[] actors = FindObjectsByType<PvpWeaponActorIdentity>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

            for (int i = 0; i < actors.Length; i++)
            {
                PvpWeaponActorIdentity actor = actors[i];
                if (actor == null || actor.Object == null)
                {
                    continue;
                }

                if (!actor.Object.HasInputAuthority)
                {
                    continue;
                }

                _localActor = actor;
                _hasCached = true;
                side = actor.Side;

                if (_logWhenFound)
                {
                    Debug.Log($"[PvpLocalPlayerSideProvider] Local side found: {side}, Owner: {actor.Owner}");
                }

                return true;
            }

            side = PlayerSide.Bottom;
            return false;
        }

        public PlayerSide GetLocalSideOrDefault(PlayerSide fallback = PlayerSide.Bottom)
        {
            return TryGetLocalSide(out PlayerSide side)
                ? side
                : fallback;
        }

        public void ClearCache()
        {
            _localActor = null;
            _hasCached = false;
        }
    }
}