using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shield_Shot.GameplayCore.Augment
{
    public class UIAugmentCard : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Component References")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Image _iconImage;

        private ProjectileBehaviorSO _targetBehaviorSO;
        private System.Action<ProjectileBehaviorSO> _onSelectedCallback;

        /// <summary>
        /// 카드 UI에 증강 SO 데이터를 바인딩한다.
        /// </summary>
        public void SetupCard(ProjectileBehaviorSO so, System.Action<ProjectileBehaviorSO> onSelected)
        {
            _targetBehaviorSO = so;
            _onSelectedCallback = onSelected;

            _titleText.text = so.BehaviorName;
            _iconImage.sprite = so.Icon;
        }

        // 카드가 클릭되었을 때 실행되는 유니티 UI 이벤트
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_targetBehaviorSO != null)
            {
                _onSelectedCallback?.Invoke(_targetBehaviorSO);
            }
        }
    }
}