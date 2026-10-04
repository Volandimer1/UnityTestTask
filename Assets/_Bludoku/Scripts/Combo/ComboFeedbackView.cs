using _Bludoku.Scripts.Effects;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Bludoku.Scripts.Combo
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ComboFeedbackView : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private RectTransform _labelRect;
        private Vector2 _labelPosition;
        private Color _originalColor;
        private ComboSystem _comboSystem;
        private RectTransform _effectsRoot;
        private CanvasGroup _effectsGroup;
        private Material _comboTextMaterial;
        private Material _fireMaterial;
        private RawImage _fire;
        private Texture2D[] _torchSheets;
        private int _comboCount;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _labelRect = (RectTransform)transform;
            _labelPosition = _labelRect.anchoredPosition;
            _originalColor = _text.color;
            _text.enableAutoSizing = false;
            _text.fontSize = 72f;
            _text.alignment = TextAlignmentOptions.Center;
            _comboTextMaterial = new Material(_text.fontSharedMaterial);
            _comboTextMaterial.EnableKeyword("OUTLINE_ON");
            _text.fontSharedMaterial = _comboTextMaterial;
        }

        public void Bind(ComboSystem comboSystem)
        {
            if (_comboSystem != null && isActiveAndEnabled)
                _comboSystem.ComboChanged -= OnComboChanged;
            _comboSystem = comboSystem;
            EnsureVisuals();
            if (isActiveAndEnabled)
            {
                _comboSystem.ComboChanged += OnComboChanged;
                SetState(_comboSystem.ComboCount, _comboSystem.ConsecutiveMisses, true);
            }
        }

        private void OnEnable()
        {
            if (_comboSystem != null)
            {
                _comboSystem.ComboChanged += OnComboChanged;
                SetState(_comboSystem.ComboCount, _comboSystem.ConsecutiveMisses, true);
            }
            else
                _text.enabled = false;
        }

        private void OnDisable()
        {
            if (_comboSystem != null)
                _comboSystem.ComboChanged -= OnComboChanged;
            _labelRect.DOKill();
            _labelRect.localScale = Vector3.one;
            _labelRect.anchoredPosition = _labelPosition;
            if (_effectsRoot == null)
                return;
            _effectsRoot.DOKill();
            _effectsGroup.DOKill();
            _effectsRoot.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_effectsRoot != null)
                Destroy(_effectsRoot.gameObject);
            if (_fireMaterial != null)
                Destroy(_fireMaterial);
            if (_comboTextMaterial != null)
                Destroy(_comboTextMaterial);
        }

        private void Update()
        {
            if (_effectsRoot == null || !_effectsRoot.gameObject.activeSelf)
                return;

            float time = Time.unscaledTime;
            if (_comboCount >= ComboVfxStages.ElectricOutline &&
                _comboCount < ComboVfxStages.FireCount)
            {
                SetOutline(ElectricOutlineWidth(_comboCount) + 0.12f * Mathf.Sin(time * 9f),
                    new Color32(22, (byte)(140 + 40 * Mathf.Sin(time * 9f)), 255, 255));
            }

            bool fireVisible = _comboCount >= ComboVfxStages.FireCount;
            _fire.enabled = fireVisible;
            if (fireVisible)
            {
                int fireFrame = Mathf.FloorToInt(time * 24f) % 64;
                _fire.texture = _torchSheets[fireFrame / 16];
                _fire.uvRect = ComboVfxAssets.FrameUv(fireFrame);
                _fire.color = new Color(1f, 0.9f, 0.52f,
                    _comboCount >= ComboVfxStages.MaximumIntensity ? 1f : 0.88f);
                _fire.rectTransform.localScale = Vector3.one *
                    (1f + 0.045f * Mathf.Sin(time * 5f));
            }
        }

        private void OnComboChanged(ComboChange change)
        {
            SetState(change.ComboCount, change.ConsecutiveMisses,
                change.Kind == ComboChangeKind.Restored);
            if (change.Kind != ComboChangeKind.SuccessfulAction || change.ComboCount < ComboVfxStages.ShowCount)
                return;

            _labelRect.DOPunchScale(Vector3.one * Mathf.Min(0.15f + change.ComboCount * 0.04f, 0.4f),
                0.3f, 5, 0.4f);
            if (_effectsRoot == null)
                return;
            _effectsRoot.DOKill();
            _effectsRoot.localScale = Vector3.one;
            _effectsRoot.DOPunchScale(Vector3.one * Mathf.Min(0.12f + change.ComboCount * 0.025f, 0.3f),
                0.32f, 5, 0.5f);
            if (change.ComboCount >= ComboVfxStages.Electric)
            {
                _effectsRoot.DOShakeAnchorPos(0.17f, new Vector2(2f, 2f), 8, 80f, false, true);
                _labelRect.DOShakeAnchorPos(0.17f, new Vector2(2f, 2f), 8, 80f, false, true)
                    .OnComplete(() => _labelRect.anchoredPosition = _labelPosition);
            }
        }

        private void SetState(int comboCount, int consecutiveMisses, bool immediate)
        {
            _comboCount = comboCount;
            bool visible = comboCount >= ComboVfxStages.ShowCount;
            _labelRect.DOKill();
            _labelRect.anchoredPosition = _labelPosition;
            float baseScale = visible ? 1f + Mathf.Min(comboCount - ComboVfxStages.ShowCount, 8) * 0.025f : 1f;
            _labelRect.localScale = Vector3.one * baseScale;
            _text.enabled = visible;
            _text.text = visible ? $"x{comboCount}" : string.Empty;
            _text.color = !visible ? _originalColor :
                comboCount >= ComboVfxStages.FireCount ? new Color(1f, 0.94f, 0.7f) :
                comboCount >= ComboVfxStages.Electric ? new Color(0.82f, 0.95f, 1f) : _originalColor;
            SetOutline(!visible ? 0f :
                    comboCount >= ComboVfxStages.FireCount ? 0.24f :
                    comboCount >= ComboVfxStages.ElectricOutline ? ElectricOutlineWidth(comboCount) : 0.06f,
                comboCount >= ComboVfxStages.FireCount ?
                    new Color32(35, 17, 34, 255) : new Color32(22, 91, 210, 255));

            if (_effectsRoot == null)
                return;
            if (visible)
            {
                // The TMP label itself is the layout container. Keep its center fixed while its width grows.
                float width = Mathf.Ceil(_text.GetPreferredValues().x + 24f);
                _labelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                _effectsRoot.sizeDelta = new Vector2(width * baseScale, _labelRect.rect.height * baseScale);
                UpdateEffectsLayout();
            }
            _effectsGroup.DOKill();
            if (visible)
                _effectsRoot.gameObject.SetActive(true);
            float target = !visible ? 0f : consecutiveMisses == 0 ? 1f :
                consecutiveMisses == 1 ? 0.7f : 0.45f;
            if (immediate)
                _effectsGroup.alpha = target;
            else
                _effectsGroup.DOFade(target, 0.24f).OnComplete(() =>
                {
                    if (_comboCount < ComboVfxStages.ShowCount)
                        _effectsRoot.gameObject.SetActive(false);
                });
            if (!visible && immediate)
                _effectsRoot.gameObject.SetActive(false);
        }

        private void EnsureVisuals()
        {
            if (_effectsRoot != null)
                return;
            Shader shader = ComboVfxAssets.AdditiveShader;
            if (shader == null)
            {
                Debug.LogError("Combo VFX shader is missing from Resources/ComboVfx.");
                return;
            }
            _fireMaterial = new Material(shader);
            _fireMaterial.SetFloat("_FireWhiten", 0.45f);
            _fireMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _torchSheets = ComboVfxAssets.LoadAnimation("torch");

            var rootObject = new GameObject("ComboEffects", typeof(RectTransform), typeof(CanvasGroup));
            _effectsRoot = (RectTransform)rootObject.transform;
            _effectsRoot.SetParent(transform.parent, false);
            _effectsRoot.SetSiblingIndex(transform.GetSiblingIndex());
            _effectsRoot.anchorMin = _labelRect.anchorMin;
            _effectsRoot.anchorMax = _labelRect.anchorMax;
            _effectsRoot.pivot = _labelRect.pivot;
            _effectsRoot.anchoredPosition = _labelPosition;
            _effectsRoot.sizeDelta = _labelRect.sizeDelta;
            _effectsGroup = rootObject.GetComponent<CanvasGroup>();
            _effectsGroup.blocksRaycasts = false;
            _effectsGroup.interactable = false;

            _fire = CreateImage("ComboFire", _torchSheets[0], _fireMaterial);
            _fire.uvRect = ComboVfxAssets.FrameUv(0);
            _effectsRoot.gameObject.SetActive(false);
        }

        private void UpdateEffectsLayout()
        {
            float width = _effectsRoot.rect.width;
            float height = _effectsRoot.rect.height;
            RectTransform fireRect = _fire.rectTransform;
            fireRect.anchorMin = fireRect.anchorMax = new Vector2(0.5f, 0.5f);
            fireRect.sizeDelta = new Vector2(width * 2.64f, height * 2.42f);
            fireRect.anchoredPosition = new Vector2(0f, 60f);
        }

        private void SetOutline(float width, Color color)
        {
            _comboTextMaterial.SetFloat("_OutlineWidth", width);
            _comboTextMaterial.SetColor("_OutlineColor", color);
            _text.UpdateMeshPadding();
            _text.SetVerticesDirty();
        }

        private static float ElectricOutlineWidth(int comboCount)
        {
            return 0.34f +
                (Mathf.Min(comboCount, ComboVfxStages.FireCount - 1) - ComboVfxStages.ElectricOutline) * 0.012f;
        }

        private UnityEngine.UI.RawImage CreateImage(string name, Texture texture, Material material)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.RawImage));
            var rect = (RectTransform)imageObject.transform;
            rect.SetParent(_effectsRoot, false);
            var image = imageObject.GetComponent<UnityEngine.UI.RawImage>();
            image.texture = texture;
            image.material = material;
            image.raycastTarget = false;
            return image;
        }
    }
}
