using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Status
{
    public sealed class FrozenMaterialVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatusEffectController _statusEffectController;
        [SerializeField] private Renderer[] _renderers;

        [Header("Frozen")]
        [SerializeField] private Material _frozenMaterial;
        [SerializeField] private bool _replaceAllMaterialSlots = true;

        private readonly Dictionary<Renderer, Material[]> _originalMaterialsByRenderer = new();
        private bool _isFrozenVisualActive;

        private void Reset()
        {
            _statusEffectController = GetComponent<StatusEffectController>();
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Awake()
        {
            if (_statusEffectController == null)
            {
                _statusEffectController = GetComponent<StatusEffectController>();
            }

            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>(true);
            }
        }

        private void OnEnable()
        {
            if (_statusEffectController == null)
            {
                return;
            }

            _statusEffectController.EffectAppliedOrRefreshed += HandleEffectAppliedOrRefreshed;
            _statusEffectController.EffectRemoved += HandleEffectRemoved;

            if (_statusEffectController.HasEffectType(StatusEffectType.Frozen))
            {
                ApplyFrozenMaterial();
            }
        }

        private void OnDisable()
        {
            if (_statusEffectController != null)
            {
                _statusEffectController.EffectAppliedOrRefreshed -= HandleEffectAppliedOrRefreshed;
                _statusEffectController.EffectRemoved -= HandleEffectRemoved;
            }

            RestoreOriginalMaterials();
        }

        private void HandleEffectAppliedOrRefreshed(StatusEffectData effect)
        {
            if (effect.Type == StatusEffectType.Frozen)
            {
                ApplyFrozenMaterial();
            }
        }

        private void HandleEffectRemoved(StatusEffectData effect)
        {
            if (effect.Type != StatusEffectType.Frozen ||
                (_statusEffectController != null && _statusEffectController.HasEffectType(StatusEffectType.Frozen)))
            {
                return;
            }

            RestoreOriginalMaterials();
        }

        private void ApplyFrozenMaterial()
        {
            if (_isFrozenVisualActive || _frozenMaterial == null)
            {
                return;
            }

            _originalMaterialsByRenderer.Clear();

            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer targetRenderer = _renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                Material[] originalMaterials = targetRenderer.sharedMaterials;
                _originalMaterialsByRenderer[targetRenderer] = originalMaterials;

                Material[] frozenMaterials = new Material[originalMaterials.Length];
                for (int materialIndex = 0; materialIndex < frozenMaterials.Length; materialIndex++)
                {
                    frozenMaterials[materialIndex] = _replaceAllMaterialSlots
                        ? _frozenMaterial
                        : originalMaterials[materialIndex];
                }

                if (!_replaceAllMaterialSlots && frozenMaterials.Length > 0)
                {
                    frozenMaterials[0] = _frozenMaterial;
                }

                targetRenderer.sharedMaterials = frozenMaterials;
            }

            _isFrozenVisualActive = true;
        }

        private void RestoreOriginalMaterials()
        {
            if (!_isFrozenVisualActive)
            {
                return;
            }

            foreach (var pair in _originalMaterialsByRenderer)
            {
                Renderer targetRenderer = pair.Key;
                if (targetRenderer != null)
                {
                    targetRenderer.sharedMaterials = pair.Value;
                }
            }

            _originalMaterialsByRenderer.Clear();
            _isFrozenVisualActive = false;
        }
    }
}
