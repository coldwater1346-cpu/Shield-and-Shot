using Shield_Shot.GameplayCore.Monster.Status;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    [RequireComponent(typeof(StatusEffectController))]
    public class ElementFieldEffectTarget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldEffectSystem _effectSystem;
        [SerializeField] private StatusEffectController _statusEffectController;

        [Header("Register")]
        [SerializeField] private bool _registerOnEnable = true;

        private void Reset()
        {
            _statusEffectController = GetComponent<StatusEffectController>();
            _effectSystem = FindFirstObjectByType<ElementFieldEffectSystem>();
        }

        private void Awake()
        {
            if (_statusEffectController == null)
            {
                _statusEffectController = GetComponent<StatusEffectController>();
            }
        }

        private void OnEnable()
        {
            if (_registerOnEnable)
            {
                Register();
            }
        }

        private void OnDisable()
        {
            Unregister();
        }

        [ContextMenu("Register")]
        public void Register()
        {
            ElementFieldEffectSystem effectSystem = ResolveEffectSystem();

            if (effectSystem == null)
            {
                Debug.LogWarning("[ElementFieldEffectTarget] ElementFieldEffectSystem is missing.");
                return;
            }

            if (_statusEffectController == null)
            {
                _statusEffectController = GetComponent<StatusEffectController>();
            }

            if (_statusEffectController == null)
            {
                Debug.LogWarning("[ElementFieldEffectTarget] StatusEffectController is missing.");
                return;
            }

            effectSystem.RegisterTarget(_statusEffectController);
        }

        [ContextMenu("Unregister")]
        public void Unregister()
        {
            ElementFieldEffectSystem effectSystem = ResolveEffectSystem();

            if (effectSystem == null || _statusEffectController == null)
            {
                return;
            }

            effectSystem.UnregisterTarget(_statusEffectController);
        }

        private ElementFieldEffectSystem ResolveEffectSystem()
        {
            if (_effectSystem == null)
            {
                _effectSystem = FindFirstObjectByType<ElementFieldEffectSystem>();
            }

            return _effectSystem;
        }
    }
}