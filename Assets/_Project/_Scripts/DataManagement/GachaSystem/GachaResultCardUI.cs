using System.Collections;
using Shield_Shot.DataManagement.GachaSystem;
using Shield_Shot.DataManagement.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.GachaSystem
{
    public class GachaResultCardUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Card Roots")]
        [SerializeField] private RectTransform _cardRoot;
        [SerializeField] private GameObject _backRoot;
        [SerializeField] private GameObject _frontRoot;

        [Header("Front UI")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private TMP_Text _propertyText;
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _skillText;


        [Header("Animation")]
        [SerializeField] private float _flipDuration = 0.35f;

        [Header("Effects")]
        [SerializeField] private GameObject _backIdleEffect;
        [SerializeField] private GameObject _revealEffect;

        [Header("사운드 클립")]
        [SerializeField] private AudioClip _gachaClip; //  뽑기 효과음 

        private bool _isOpened;
        public bool IsOpened => _isOpened;
        private bool _isFlipping;

        private ItemGradeType _grade;

        private void Awake()
        {
            _backRoot.SetActive(true);
            _frontRoot.SetActive(false);
            StopEffect(_backIdleEffect);
            StopEffect(_revealEffect);
        }

        public void SetData(GachaController.GachaResultData data)
        {
            Item item = data.Item;
            ItemData itemData = item.ItemData;

            _grade = itemData.ItemGradeType;
            _iconImage.sprite = itemData.Icon;
            _nameText.text = itemData.ItemName;
            _gradeText.text = _grade.ToString();

            // 1. 속성 처리
            _propertyText.text = item.Property.ToString();


            if (item is WeaponItem weapon)
            {
                
                if (_skillText != null) _skillText.gameObject.SetActive(true);
                if (_damageText != null) _damageText.gameObject.SetActive(true);

                // 무기는 3중 가챠  스킬 적용
                _skillText.text = weapon.SkillType.ToString();

                // 데미지 수치 반영
                WeaponItemData weaponData = itemData as WeaponItemData;
                _damageText.text = weaponData != null ? weaponData.BaseDamage.ToString() : "0";
            }
            else
            {
                // 방패 
                
                if (_skillText != null) _skillText.gameObject.SetActive(true);

                //  방패는 (디스크립션을 출력
                _skillText.text = itemData != null ? itemData.Description : "설명 없음";

                // 방패는 기본 데미지가 없으므로 데미지 칸만 비활성화 
                if (_damageText != null) _damageText.gameObject.SetActive(false);
            }

            ShowBack();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isOpened || _isFlipping)
                return;

            StartCoroutine(FlipOpenRoutine());
            // 가차 사운드 재생
            if (_gachaClip != null)
            {
                Shield_Shot.Audio.SoundManager.Instance.PlayUI(_gachaClip, 0.1f);
            }
        }

        private IEnumerator FlipOpenRoutine()
        {
            _isFlipping = true;

            float halfDuration = _flipDuration * 0.5f;
            float time = 0f;

            // 1단계: 뒷면이 0도 -> 90도까지 접힘
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                float t = time / halfDuration;

                float y = Mathf.Lerp(0f, 90f, t);
                _cardRoot.localRotation = Quaternion.Euler(0f, y, 0f);

                yield return null;
            }

            // 2단계: 90도 순간 앞면으로 교체
            ShowFront();

            // 앞면은 -90도에서 시작해서 0도로 펴지게
            _cardRoot.localRotation = Quaternion.Euler(0f, -90f, 0f);

            time = 0f;

            while (time < halfDuration)
            {
                time += Time.deltaTime;
                float t = time / halfDuration;

                float y = Mathf.Lerp(-90f, 0f, t);
                _cardRoot.localRotation = Quaternion.Euler(0f, y, 0f);

                yield return null;
            }

            _cardRoot.localRotation = Quaternion.identity;
            _isOpened = true;
            _isFlipping = false;

            
            yield return StartCoroutine(DisableRevealEffectRoutine());
        }

        
        private IEnumerator DisableRevealEffectRoutine()
        {
            if (_revealEffect != null)
            {
                ParticleSystem ps = _revealEffect.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    // 파티클의 메인 duration(실행 시간)만큼 대기
                    yield return new WaitForSeconds(ps.main.duration);
                }
                else
                {
                   // yield return new WaitForSeconds(0.5f); // 예외 처리용 기본 대기 시간
                }

               
                StopEffect(_revealEffect);
            }
        }

        private void ShowBack()
        {
            _isOpened = false;
            _isFlipping = false;

            _cardRoot.localRotation = Quaternion.identity;

            _backRoot.SetActive(true);
            _frontRoot.SetActive(false);

            StopEffect(_revealEffect);
            PlayEffectByGrade(_backIdleEffect, _grade);
        }

        private void ShowFront()
        {
            _backRoot.SetActive(false);
            _frontRoot.SetActive(true);

            
            PlayEffectByGrade(_revealEffect, _grade);
            
        }

        private void SetParticleColor(GameObject effectRoot, Color color)
        {
            if (effectRoot == null)
                return;

            ParticleSystem[] particles = effectRoot.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem ps in particles)
            {
                var main = ps.main;
                main.startColor = color;
            }
        }

        private Color GetGradeColor(ItemGradeType grade)
        {
            switch (grade)
            {
                case ItemGradeType.C:
                    return Color.gray;

                case ItemGradeType.UC:
                    return Color.green;

                case ItemGradeType.Rare:
                    return Color.blue;

                case ItemGradeType.SR:
                    return new Color(0.6f, 0f, 1f); // 보라

                case ItemGradeType.SSR:
                    return Color.yellow;

                case ItemGradeType.UR:
                    return Color.red;

                default:
                    return Color.white;
            }
        }

        private void PlayEffectByGrade(GameObject effectRoot, ItemGradeType grade)
        {
            Color color = GetGradeColor(grade);

            SetParticleColor(effectRoot, color);
            PlayEffect(effectRoot);
        }

        private void PlayEffect(GameObject effectRoot)
        {
            if (effectRoot == null)
                return;

            effectRoot.SetActive(true);

            ParticleSystem[] particles = effectRoot.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem ps in particles)
            {
                ps.Clear(true);
                ps.Play(true);
            }
        }

        private void StopEffect(GameObject effectRoot)
        {
            if (effectRoot == null)
                return;

            ParticleSystem[] particles = effectRoot.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem ps in particles)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            effectRoot.SetActive(false);
        }

        

        //외부 오픈 메소드
    
        public void ForceOpenCard()
        {
            // 이미 열렸거나 뒤집히는 중이면 패스
            if (_isOpened || _isFlipping)
                return;

            
            StartCoroutine(FlipOpenRoutine());
        }
    }
}