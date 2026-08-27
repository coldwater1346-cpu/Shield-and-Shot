using UnityEngine;
using UnityEngine.Events;
using Shield_Shot.Core;

namespace Shield_Shot.Ads
{
    public class AdButtonBinder : MonoBehaviour
    {
        [SerializeField] private UnityEvent onGachaReward;
        [SerializeField] private UnityEvent onReviveReward;

        public void OnClickGacha()
        {
            AdManager.Instance.ShowGachaAd(() =>
            {
                onGachaReward?.Invoke();
            });
        }

        public void OnClickRevive()
        {
            AdManager.Instance.ShowReviveAd(() =>
            {
                onReviveReward?.Invoke();
            });
        }
    }

}