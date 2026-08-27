using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Core
{
    public class WeaponVisualAnchors : MonoBehaviour
    {
        [Header("스킨별로 달라지는 참조")]
        [SerializeField] private Animator animator;
        [Tooltip("이 스킨의 실제 발사 기준점 (활 크기/그립 위치에 따라 스킨마다 위치가 다름)")]
        [SerializeField] private Transform firePoint;
        [Tooltip("차징 중 보여줄 프리뷰용 화살촉 위치")]
        [SerializeField] private Transform arrowTipPoint;
        [Tooltip("차징 중 활에 장전되는 프리뷰용 화살 오브젝트")]
        [SerializeField] private GameObject visualArrow;

        public Animator Animator => animator;
        public Transform FirePoint => firePoint;
        public Transform ArrowTipPoint => arrowTipPoint;
        public GameObject VisualArrow => visualArrow;
    }
}