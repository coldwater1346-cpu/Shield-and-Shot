using System.Collections.Generic;
using Shield_Shot.GameplayCore.Monster.Status;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    [RequireComponent(typeof(Collider))]
    public class ElementFieldCell : MonoBehaviour
    {
        private const string BurnStatusID = "Burn";
        private const string ShockStatusID = "Shock";
        private const string SlowStatusID = "Slow";
        private const string FrozenStatusID = "Frozen";
        private const string ToxicSmokeStatusID = "ToxicSmoke";

        [Header("Cell")]
        [SerializeField] private TerrainElementType _terrainElement = TerrainElementType.None;
        [SerializeField] private ElementType _currentElement = ElementType.None;
        [SerializeField] private float _remainingTime;
        [SerializeField, Min(0.01f)] private float _triggerHeight = 1f;

        [Header("Status Apply")]
        [SerializeField] private float _applyInterval = 0.35f;

        [Header("Burn")]
        [SerializeField] private float _burnDuration = 1f;
        [SerializeField] private float _burnTickInterval = 0.5f;
        [SerializeField] private float _burnDamagePerTick = 2f;

        [Header("Shock")]
        [SerializeField] private float _shockDuration = 0.8f;
        [SerializeField] private float _shockTickInterval = 0.4f;
        [SerializeField] private float _shockDamagePerTick = 2f;

        [Header("Slow")]
        [SerializeField] private float _slowDuration = 1f;
        [SerializeField, Range(0f, 1f)] private float _slowMultiplier = 0.5f;

        [Header("Frozen")]
        [SerializeField] private float _frozenDuration = 1.5f;

        [Header("Toxic Smoke")]
        [SerializeField] private float _toxicDuration = 1.5f;
        [SerializeField] private float _toxicTickInterval = 0.5f;
        [SerializeField] private float _toxicDamagePerTick = 2f;

        [Header("VFX")]
        [SerializeField] private VFXType _tickVfxType = VFXType.Hit;
        [SerializeField] private float _vfxAutoReleaseTime = 1.5f;

        private readonly Dictionary<int, StatusEffectController> _targets = new();
        private readonly Dictionary<int, float> _nextApplyTimeByTarget = new();
        private ElementReactionType _lastReactionType = ElementReactionType.None;

        public Vector2Int Coord { get; private set; }
        public TerrainElementType TerrainElement => _terrainElement;
        public ElementType CurrentElement => _currentElement;
        public bool IsActive => _currentElement != ElementType.None && _remainingTime > 0f;

        private void Reset()
        {
            Collider cellCollider = GetComponent<Collider>();
            cellCollider.isTrigger = true;
        }

        public void Initialize(Vector2Int coord, TerrainElementType terrainElement, float cellSize)
        {
            Initialize(coord, terrainElement, new Vector2(cellSize, cellSize));
        }

        public void Initialize(Vector2Int coord, TerrainElementType terrainElement, Vector2 cellWorldSize)
        {
            Coord = coord;
            _terrainElement = terrainElement;
            transform.localScale = Vector3.one;

            Collider cellCollider = GetComponent<Collider>();
            cellCollider.isTrigger = true;

            if (cellCollider is BoxCollider boxCollider)
            {
                boxCollider.size = new Vector3(cellWorldSize.x, _triggerHeight, cellWorldSize.y);
                boxCollider.center = new Vector3(0f, _triggerHeight * 0.5f, 0f);
            }
        }

        private void Update()
        {
            if (!IsActive && _terrainElement != TerrainElementType.Ice)
            {
                return;
            }

            if (IsActive)
            {
                _remainingTime -= Time.deltaTime;

                if (_remainingTime <= 0f)
                {
                    ClearElement();
                }
            }

            ApplyStatusToTargets();
        }

        private void OnTriggerEnter(Collider other)
        {
            StatusEffectController statusController =
                other.GetComponentInParent<StatusEffectController>();

            if (statusController == null)
            {
                return;
            }

            int id = statusController.GetInstanceID();
            _targets[id] = statusController;
        }

        private void OnTriggerExit(Collider other)
        {
            StatusEffectController statusController =
                other.GetComponentInParent<StatusEffectController>();

            if (statusController == null)
            {
                return;
            }

            int id = statusController.GetInstanceID();
            _targets.Remove(id);
            _nextApplyTimeByTarget.Remove(id);
        }

        public void ApplyElement(ElementType incomingElement, float duration)
        {
            ElementReactionResult reaction = ElementReactionResolver.Resolve(
                _terrainElement,
                _currentElement,
                incomingElement
            );

            ElementType resultElement = reaction.HasReaction
                ? reaction.ResultElement
                : incomingElement;

            if (resultElement == ElementType.None)
            {
                ClearElement();
                return;
            }

            _currentElement = resultElement;
            _remainingTime = Mathf.Max(_remainingTime, duration * Mathf.Max(0f, reaction.DurationMultiplier));
            _lastReactionType = reaction.ReactionType;

            OnElementChanged(reaction);
        }

        public void ClearElement()
        {
            _currentElement = ElementType.None;
            _remainingTime = 0f;
            _lastReactionType = ElementReactionType.None;
            _nextApplyTimeByTarget.Clear();

            // TODO: later disable cell VFX here.
        }

        public void SetTerrainElement(TerrainElementType terrainElement)
        {
            _terrainElement = terrainElement;
        }

        private void ApplyStatusToTargets()
        {
            if (_targets.Count == 0)
            {
                return;
            }

            foreach (var pair in _targets)
            {
                StatusEffectController target = pair.Value;
                if (target == null)
                {
                    continue;
                }

                int targetId = pair.Key;
                if (_nextApplyTimeByTarget.TryGetValue(targetId, out float nextApplyTime) &&
                    Time.time < nextApplyTime)
                {
                    continue;
                }

                _nextApplyTimeByTarget[targetId] = Time.time + _applyInterval;

                if (TryCreateStatusEffect(out StatusEffectData effect))
                {
                    target.ApplyOrRefresh(effect);
                }
            }
        }

        private bool TryCreateStatusEffect(out StatusEffectData effect)
        {
            ElementType effectElement = IsActive
                ? _currentElement
                : _terrainElement == TerrainElementType.Ice
                    ? ElementType.Ice
                    : ElementType.None;

            ElementReactionType reactionType = IsActive
                ? _lastReactionType
                : ElementReactionType.None;

            switch (effectElement)
            {
                case ElementType.Fire:
                    effect = new StatusEffectData(
                        statusID: BurnStatusID,
                        type: StatusEffectType.Burn,
                        duration: _burnDuration,
                        tickInterval: _burnTickInterval,
                        damagePerTick: _burnDamagePerTick,
                        source: this,
                        showDamagePopup: true,
                        tickVfxType: _tickVfxType,
                        vfxAutoReleaseTime: _vfxAutoReleaseTime
                    );
                    return true;

                case ElementType.Lightning:
                    effect = new StatusEffectData(
                        statusID: ShockStatusID,
                        type: StatusEffectType.Shock,
                        duration: _shockDuration,
                        tickInterval: _shockTickInterval,
                        damagePerTick: _shockDamagePerTick,
                        source: this,
                        showDamagePopup: true,
                        tickVfxType: _tickVfxType,
                        vfxAutoReleaseTime: _vfxAutoReleaseTime
                    );
                    return true;

                case ElementType.Ice:
                    if (reactionType == ElementReactionType.Freeze)
                    {
                        effect = new StatusEffectData(
                            statusID: FrozenStatusID,
                            type: StatusEffectType.Frozen,
                            duration: _frozenDuration,
                            source: this,
                            showDamagePopup: false,
                            tickVfxType: VFXType.None
                        );
                        return true;
                    }

                    effect = new StatusEffectData(
                        statusID: SlowStatusID,
                        type: StatusEffectType.Slow,
                        duration: _slowDuration,
                        slowMultiplier: _slowMultiplier,
                        source: this,
                        showDamagePopup: false,
                        tickVfxType: VFXType.None
                    );
                    return true;

                case ElementType.Poison:
                    effect = new StatusEffectData(
                        statusID: ToxicSmokeStatusID,
                        type: StatusEffectType.Poison,
                        duration: _toxicDuration,
                        tickInterval: _toxicTickInterval,
                        damagePerTick: _toxicDamagePerTick,
                        source: this,
                        showDamagePopup: true,
                        tickVfxType: _tickVfxType,
                        vfxAutoReleaseTime: _vfxAutoReleaseTime
                    );
                    return true;
            }

            effect = default;
            return false;
        }

        private void OnElementChanged(ElementReactionResult reaction)
        {
            // TODO: later drive material/VFX by _currentElement and reaction.ReactionType.
            Debug.Log($"[ElementFieldCell] {name} Element: {_currentElement}, Reaction: {reaction.ReactionType}");
        }
    }
}
