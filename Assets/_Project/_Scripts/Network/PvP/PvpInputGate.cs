using Shield_Shot.InputSystem;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpInputGate : MonoBehaviour, IInputGate
    {
        [Header("References")]
        [SerializeField] private PvpMatchStateController _matchStateController;

        public bool CanReceiveInput
        {
            get
            {
                if (_matchStateController == null)
                {
                    _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
                }

                if (_matchStateController == null ||
                    _matchStateController.Object == null ||
                    !_matchStateController.Object.IsValid)
                {
                    return false;
                }

                return _matchStateController.CurrentState == PvpMatchState.Fighting;
            }
        }
    }
}
