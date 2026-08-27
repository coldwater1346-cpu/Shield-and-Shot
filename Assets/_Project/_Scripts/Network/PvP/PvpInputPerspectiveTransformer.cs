using Shield_Shot.GameplayCore.Network.Match;
using Shield_Shot.InputSystem;
using Shield_Shot.InputSystem.Data;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpInputPerspectiveTransformer : MonoBehaviour, IInputContextTransformer
    {
        [SerializeField] private PlayerSide _localSide = PlayerSide.None;

        public PlayerSide LocalSide => _localSide;

        public void SetLocalSide(PlayerSide localSide)
        {
            _localSide = localSide;
            Debug.Log($"[PvpInputPerspectiveTransformer] Local side set: {_localSide}");
        }

        public InputContext Transform(InputContext context)
        {
            if (_localSide != PlayerSide.Top)
            {
                return context;
            }

            context.dragVector = -context.dragVector;
            return context;
        }
    }
}