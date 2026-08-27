using UnityEngine;
using UnityEngine.EventSystems;

namespace Shield_Shot.Audio
{
    public class UIButtonClickSfx : MonoBehaviour, IPointerClickHandler
    {
        [Header("이 버튼 전용 클릭음")]
        [SerializeField] private AudioClip clickSfxClip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickSfxClip == null || SoundManager.Instance == null) return;
            SoundManager.Instance.PlayUI(clickSfxClip, volume);
        }
    }
}