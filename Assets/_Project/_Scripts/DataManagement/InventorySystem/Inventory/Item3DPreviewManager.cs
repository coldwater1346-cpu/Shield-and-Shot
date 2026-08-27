using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class Item3DPreviewManager : MonoBehaviour
    {
        public static Item3DPreviewManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform _spawnAnchor;
        [SerializeField] private float _rotateSpeed = 5f;
        [SerializeField] private float _autoRotateSpeed = 22f;

        [Header("Gun Preview Settings")]
        [SerializeField]
        private List<string> _rotatedGunPrefabNames =
            new List<string>();

        [SerializeField]
        private Vector3 _gunPreviewEulerAngles =
            new Vector3(0f, 0f, -90f);

        [Header("Shield Preview Settings")]
        [SerializeField]
        private List<string> _scaledShieldPrefabNames =
            new List<string>();

        [SerializeField]
        private Vector3 _shieldPreviewScale =
            new Vector3(0.7f, 0.7f, 0.7f);

        private GameObject _currentPreviewModel;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (_currentPreviewModel != null)
            {
                _currentPreviewModel.transform.Rotate(
                    Vector3.up,
                    _autoRotateSpeed * Time.deltaTime,
                    Space.World);
            }
        }

        public void SetPreviewItem(GameObject itemPrefab)
        {
            if (_currentPreviewModel != null)
            {
                Destroy(_currentPreviewModel);
                _currentPreviewModel = null;
            }

            if (itemPrefab == null || _spawnAnchor == null)
            {
                return;
            }

            _currentPreviewModel =
                Instantiate(itemPrefab, _spawnAnchor);

            Transform previewTransform =
                _currentPreviewModel.transform;

            previewTransform.localPosition = Vector3.zero;

            // 모든 아이템의 기본 회전값
            previewTransform.localRotation =
                Quaternion.identity;

            // 총 프리팹이면 미리보기 회전 보정
            if (_rotatedGunPrefabNames != null &&
                _rotatedGunPrefabNames.Contains(itemPrefab.name))
            {
                previewTransform.localRotation =
                    Quaternion.Euler(
                        _gunPreviewEulerAngles);
            }

            // 방패 프리팹이면 미리보기 크기 보정
            if (_scaledShieldPrefabNames != null &&
                _scaledShieldPrefabNames.Contains(itemPrefab.name))
            {
                previewTransform.localScale =
                    _shieldPreviewScale;
            }
        }

        public void RotateModel(float mouseX)
        {
            if (_currentPreviewModel == null)
            {
                return;
            }

            _currentPreviewModel.transform.Rotate(
                Vector3.up,
                -mouseX * _rotateSpeed,
                Space.World);
        }

        public void ClearPreview()
        {
            if (_currentPreviewModel == null)
            {
                return;
            }

            Destroy(_currentPreviewModel);
            _currentPreviewModel = null;
        }

        private void SetLayerRecursively(
            GameObject obj,
            int newLayer)
        {
            if (obj == null)
            {
                return;
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(
                    child.gameObject,
                    newLayer);
            }
        }
    }
}