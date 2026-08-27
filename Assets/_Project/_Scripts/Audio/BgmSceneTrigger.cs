using UnityEngine;

namespace Shield_Shot.Audio
{
    public class BgmSceneTrigger : MonoBehaviour
    {
        [Tooltip("이 씬에서 재생할 BGM 그룹 (Title/Login 씬은 둘 다 TitleLogin으로 맞춘다)")]
        [SerializeField] private BgmGroup group;

        private void Start()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.RequestBgmGroup(group);
            }
            else
            {
                Debug.LogWarning("[BgmSceneTrigger] SoundManager 인스턴스를 찾을 수 없습니다.");
            }
        }
    }
}