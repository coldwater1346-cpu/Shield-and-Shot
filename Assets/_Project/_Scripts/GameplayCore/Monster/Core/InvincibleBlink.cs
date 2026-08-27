using UnityEngine;
using Shield_Shot.GameplayCore.Monster.Core;   // HealthComponent

public class InvincibleBlink : MonoBehaviour
{
    [SerializeField] private HealthComponent _health;
    [SerializeField] private Renderer[] _renderers;

    [Header("점멸")]
    [SerializeField] private Color _flashColor = Color.white;   // 번쩍일 색
    [SerializeField] private float _blinkSpeed = 6f;           // 클수록 빠른 펄스
    [Tooltip("URP Lit: _BaseColor / Standard: _Color")]
    [SerializeField] private string _colorProperty = "_BaseColor";

    private int _propId;
    private MaterialPropertyBlock _mpb;
    private Color[] _baseColors;
    private bool _active;

    private void Awake()
    {
        if (_health == null) _health = GetComponent<HealthComponent>();
        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<Renderer>(true);

        _propId = Shader.PropertyToID(_colorProperty);
        _mpb = new MaterialPropertyBlock();
        _baseColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            var m = _renderers[i] ? _renderers[i].sharedMaterial : null;
            _baseColors[i] = (m != null && m.HasProperty(_propId)) ? m.GetColor(_propId) : Color.white;
        }

        var t = GetComponentInChildren<Renderer>().sharedMaterial;
        Debug.Log($"shader={t.shader.name}");
        for (int i = 0; i < t.shader.GetPropertyCount(); i++)
            if (t.shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Color)
                Debug.Log($"Color prop: {t.shader.GetPropertyName(i)}");
    }

    private void Update()
    {
        bool inv = _health != null && _health.IsInvincible;

        if (inv)
        {
            // 0~1 사인 펄스 (부드럽게 밝아졌다 돌아옴)
            float t = (Mathf.Sin(Time.time * _blinkSpeed) + 1f) * 0.5f;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                Color c = Color.Lerp(_baseColors[i], _flashColor, t);
                _renderers[i].GetPropertyBlock(_mpb);
                _mpb.SetColor(_propId, c);
                _renderers[i].SetPropertyBlock(_mpb);
            }
            _active = true;
        }
        else if (_active)   // 무적 끝 → 원래 색 복원
        {
            _active = false;
            Restore();
        }
    }

    private void Restore()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null) continue;
            _renderers[i].GetPropertyBlock(_mpb);
            _mpb.SetColor(_propId, _baseColors[i]);
            _renderers[i].SetPropertyBlock(_mpb);
        }
    }

    private void OnEnable() => _active = false;                 // 풀 재사용 대비
    private void OnDisable() { if (_active) { _active = false; Restore(); } }
}