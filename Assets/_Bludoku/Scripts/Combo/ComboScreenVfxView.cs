using System.Collections.Generic;
using _Bludoku.Scripts.Effects;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Bludoku.Scripts.Combo
{
    [RequireComponent(typeof(Canvas))]
    public class ComboScreenVfxView : MonoBehaviour
    {
        private static readonly int[] LightningSequence = { 0, 1, 2, 3, 2, 1 };

        [SerializeField] private Camera sceneCamera;
        [SerializeField] private RectTransform guiPanel;

        private ComboSystem _comboSystem;
        private RectTransform _root;
        private CanvasGroup _group;
        private Material _lightningMaterial;
        private Material _fireMaterial;
        private readonly List<RawImage>[] _lightning =
            { new List<RawImage>(), new List<RawImage>(), new List<RawImage>(), new List<RawImage>() };
        private readonly int[] _lightningCounts = new int[4];
        private RawImage[] _fire;
        private RawImage[] _fireOverlay;
        private Texture2D[] _lightningFrames;
        private Texture2D[] _fireSheets;
        private Vector3 _cameraPosition;
        private Vector2 _panelPosition;
        private int _comboCount;
        private float _successFlash;
        private int _lastFireFrame = -1;
        private float _lastWidth;
        private float _lastHeight;
        private Tween _cameraShake;
        private Tween _panelShake;

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
            if (_comboSystem == null)
                return;
            _comboSystem.ComboChanged += OnComboChanged;
            SetState(_comboSystem.ComboCount, _comboSystem.ConsecutiveMisses, true);
        }

        private void OnDisable()
        {
            if (_comboSystem != null)
                _comboSystem.ComboChanged -= OnComboChanged;
            StopShake();
            if (_root == null)
                return;
            _group.DOKill();
            _root.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_root != null)
                Destroy(_root.gameObject);
            if (_lightningMaterial != null)
                Destroy(_lightningMaterial);
            if (_fireMaterial != null)
                Destroy(_fireMaterial);
        }

        private void Update()
        {
            if (_root == null || !_root.gameObject.activeSelf || _comboCount < ComboVfxStages.EdgeLightning)
                return;

            float time = Time.unscaledTime;
            ResizeForCanvas();
            _successFlash = Mathf.MoveTowards(_successFlash, 0f, Time.unscaledDeltaTime * 2f);
            bool showLightning = _comboCount < ComboVfxStages.FireCount;
            int lightningFrame = Mathf.FloorToInt(time * 30f);
            float blueIntensity = _comboCount < 7 ? 0.32f : _comboCount < 8 ? 0.4f :
                _comboCount < 9 ? 0.5f : 0.6f;
            for (int edge = 0; edge < _lightning.Length; edge++)
            {
                for (int tile = 0; tile < _lightning[edge].Count; tile++)
                {
                    RawImage image = _lightning[edge][tile];
                    image.enabled = showLightning && tile < _lightningCounts[edge];
                    if (!image.enabled)
                        continue;
                    int sequenceIndex = (lightningFrame + edge + tile * 2) % LightningSequence.Length;
                    image.texture = _lightningFrames[LightningSequence[sequenceIndex]];
                    image.color = new Color(0.25f, 0.58f, 1f,
                        Mathf.Min(blueIntensity + _successFlash, 0.85f));
                }
            }

            bool showFire = _comboCount >= ComboVfxStages.FireEdges;
            int frame = Mathf.FloorToInt(time * 24f) % 64;
            if (showFire && frame != _lastFireFrame)
            {
                _lastFireFrame = frame;
                for (int i = 0; i < _fire.Length; i++)
                {
                    SetFireFrame(_fire[i], (frame + i * 13) % 64, i % 2 == 1);
                }
                for (int i = 0; i < _fireOverlay.Length; i++)
                    SetFireFrame(_fireOverlay[i], (frame + i * 17 + 7) % 64, i % 2 == 0);
            }
            float fireAlpha = _comboCount >= ComboVfxStages.MaximumIntensity ? 0.92f :
                _comboCount >= ComboVfxStages.FireIntensify ? 0.84f : 0.76f;
            float pulseAmount = _comboCount >= ComboVfxStages.MaximumIntensity ? 0.065f : 0.04f;
            for (int i = 0; i < _fire.Length; i++)
            {
                RawImage image = _fire[i];
                image.enabled = showFire;
                if (showFire)
                {
                    float pulse = Mathf.Sin(time * 5f + i * 0.85f);
                    image.rectTransform.localScale = Vector3.one * (1f + pulseAmount * pulse);
                    image.color = new Color(1f, 0.9f, 0.52f,
                        Mathf.Min(fireAlpha * (0.94f + 0.06f * pulse) + _successFlash * 0.15f, 1f));
                }
            }
            bool showOverlay = showFire;
            for (int i = 0; i < _fireOverlay.Length; i++)
            {
                RawImage image = _fireOverlay[i];
                image.enabled = showOverlay;
                if (showOverlay)
                {
                    float pulse = Mathf.Sin(time * 5f + (i * 2) * 0.85f + 0.6f);
                    image.rectTransform.localScale = Vector3.one * (1f + pulseAmount * pulse);
                    image.color = new Color(1f, 0.82f, 0.4f,
                        _comboCount >= ComboVfxStages.MaximumIntensity ? 0.55f :
                        _comboCount >= ComboVfxStages.FireIntensify ? 0.4f : 0.25f);
                }
            }
        }

        private void OnComboChanged(ComboChange change)
        {
            SetState(change.ComboCount, change.ConsecutiveMisses,
                change.Kind == ComboChangeKind.Restored);
            if (change.Kind != ComboChangeKind.SuccessfulAction)
                return;
            if (change.ComboCount >= ComboVfxStages.EdgeLightning)
                _successFlash = change.ComboCount >= ComboVfxStages.MaximumIntensity ? 0.28f : 0.16f;
            if (change.ComboCount >= ComboVfxStages.ScreenShake)
                Shake(change.ComboCount);
        }

        private void SetState(int comboCount, int consecutiveMisses, bool immediate)
        {
            _comboCount = comboCount;
            if (_root == null)
                return;
            bool visible = comboCount >= ComboVfxStages.EdgeLightning;
            _group.DOKill();
            if (visible)
                _root.gameObject.SetActive(true);
            float target = !visible ? 0f : consecutiveMisses == 0 ? 1f :
                consecutiveMisses == 1 ? 0.7f : 0.45f;
            if (immediate)
                _group.alpha = target;
            else
                _group.DOFade(target, 0.3f).OnComplete(() =>
                {
                    if (_comboCount < ComboVfxStages.EdgeLightning)
                        _root.gameObject.SetActive(false);
                });
            if (!visible && immediate)
                _root.gameObject.SetActive(false);
        }

        private void Shake(int comboCount)
        {
            StopShake();
            float intensity = comboCount < ComboVfxStages.MaximumIntensity ? 1f : 1.5f;
            if (sceneCamera != null)
            {
                _cameraPosition = sceneCamera.transform.localPosition;
                _cameraShake = sceneCamera.transform.DOShakePosition(0.16f, 0.035f * intensity, 9, 65f, false, true)
                    .OnComplete(() => sceneCamera.transform.localPosition = _cameraPosition);
            }
            if (guiPanel != null)
            {
                _panelPosition = guiPanel.anchoredPosition;
                _panelShake = guiPanel.DOShakeAnchorPos(0.16f, new Vector2(2.5f, 2.5f) * intensity,
                    9, 65f, false, true)
                    .OnComplete(() => guiPanel.anchoredPosition = _panelPosition);
            }
        }

        private void StopShake()
        {
            if (_cameraShake != null && _cameraShake.IsActive())
            {
                _cameraShake.Kill();
                sceneCamera.transform.localPosition = _cameraPosition;
            }
            if (_panelShake != null && _panelShake.IsActive())
            {
                _panelShake.Kill();
                guiPanel.anchoredPosition = _panelPosition;
            }
            _cameraShake = null;
            _panelShake = null;
        }

        private void EnsureVisuals()
        {
            if (_root != null)
                return;
            Shader shader = ComboVfxAssets.AdditiveShader;
            if (shader == null)
            {
                Debug.LogError("Combo VFX shader is missing from Resources/ComboVfx.");
                return;
            }
            _lightningMaterial = new Material(shader);
            _fireMaterial = new Material(shader);
            _fireMaterial.SetFloat("_FireWhiten", 0.45f);
            _fireMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lightningFrames = ComboVfxAssets.LoadLightningFrames();
            var rootObject = new GameObject("ComboScreenVfx", typeof(RectTransform), typeof(CanvasGroup));
            _root = (RectTransform)rootObject.transform;
            _root.SetParent(transform, false);
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.one;
            _root.offsetMin = Vector2.zero;
            _root.offsetMax = Vector2.zero;
            _root.SetSiblingIndex(1); // Above ScorePanel, below the settings button and menu panels.
            _group = rootObject.GetComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            _fireSheets = ComboVfxAssets.LoadAnimation("firewall");
            _fire = new RawImage[36]; // Seven tiles along each horizontal edge, eleven on each side.
            for (int i = 0; i < _fire.Length; i++)
            {
                _fire[i] = CreateImage("EdgeFire" + i, _fireSheets[0], _fireMaterial);
                _fire[i].uvRect = ComboVfxAssets.FrameUv(0);
                ConfigureFireTile(_fire[i].rectTransform, i);
            }
            _fireOverlay = new RawImage[18];
            for (int i = 0; i < _fireOverlay.Length; i++)
            {
                _fireOverlay[i] = CreateImage("EdgeFireOverlap" + i, _fireSheets[0], _fireMaterial);
                _fireOverlay[i].uvRect = ComboVfxAssets.FrameUv(0);
            }
            _root.gameObject.SetActive(false);
        }

        private void ResizeForCanvas()
        {
            float width = _root.rect.width;
            float height = _root.rect.height;
            if (Mathf.Approximately(width, _lastWidth) && Mathf.Approximately(height, _lastHeight))
                return;
            _lastWidth = width;
            _lastHeight = height;
            const float lightningThickness = 120f;
            const float lightningLength = lightningThickness * 4f; // 2048x512 source aspect ratio.
            for (int edge = 0; edge < _lightning.Length; edge++)
            {
                float edgeLength = edge < 2 ? width : height;
                int count = Mathf.CeilToInt(edgeLength / lightningLength);
                _lightningCounts[edge] = count;
                while (_lightning[edge].Count < count)
                {
                    int index = _lightning[edge].Count;
                    _lightning[edge].Add(CreateImage("EdgeLightning" + edge + "_" + index,
                        _lightningFrames[0], _lightningMaterial));
                }
                for (int tile = 0; tile < count; tile++)
                    ConfigureLightningTile(_lightning[edge][tile].rectTransform, edge, tile,
                        edgeLength, lightningLength, lightningThickness);
            }
            float fireSide = Mathf.Max(width / 7f, height / 11f) + 24f;
            for (int i = 0; i < _fire.Length; i++)
            {
                RectTransform rect = _fire[i].rectTransform;
                rect.sizeDelta = new Vector2(fireSide, fireSide);
            }
            for (int i = 0; i < _fireOverlay.Length; i++)
            {
                int baseIndex = i * 2;
                RectTransform source = _fire[baseIndex].rectTransform;
                RectTransform overlay = _fireOverlay[i].rectTransform;
                overlay.anchorMin = source.anchorMin;
                overlay.anchorMax = source.anchorMax;
                overlay.pivot = source.pivot;
                overlay.localEulerAngles = source.localEulerAngles;
                overlay.sizeDelta = new Vector2(fireSide * 0.82f, fireSide * 0.82f);
                overlay.anchoredPosition = source.anchoredPosition +
                    (baseIndex < 14 ? new Vector2(width / 14f, 0f) : new Vector2(0f, height / 22f));
            }
        }

        private void SetFireFrame(RawImage image, int frame, bool flipHorizontally)
        {
            image.texture = _fireSheets[frame / 16];
            Rect uv = ComboVfxAssets.FrameUv(frame);
            image.uvRect = flipHorizontally ?
                new Rect(uv.xMax, uv.y, -uv.width, uv.height) : uv;
        }

        private UnityEngine.UI.RawImage CreateImage(string name, Texture texture, Material material)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.RawImage));
            var rect = (RectTransform)imageObject.transform;
            rect.SetParent(_root, false);
            var image = imageObject.GetComponent<UnityEngine.UI.RawImage>();
            image.texture = texture;
            image.material = material;
            image.raycastTarget = false;
            return image;
        }

        private static void ConfigureLightningTile(RectTransform rect, int edge, int tile,
            float edgeLength, float tileLength, float thickness)
        {
            rect.anchorMin = rect.anchorMax = edge < 2 ?
                new Vector2(0.5f, edge) : new Vector2(edge == 2 ? 0f : 1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(tileLength, thickness);
            float along = -edgeLength * 0.5f + (tile + 0.5f) * tileLength;
            if (edge < 2)
            {
                rect.anchoredPosition = new Vector2(along, edge == 0 ? 25f : -25f);
                rect.localEulerAngles = edge == 0 ? Vector3.zero : new Vector3(0f, 0f, 180f);
            }
            else
            {
                rect.anchoredPosition = new Vector2(edge == 2 ? 25f : -25f, along);
                rect.localEulerAngles = new Vector3(0f, 0f, edge == 2 ? -90f : 90f);
            }
        }

        private static void ConfigureFireTile(RectTransform rect, int index)
        {
            bool vertical = index >= 14;
            int edge = vertical ? 2 + (index - 14) / 11 : index / 7;
            int tile = vertical ? (index - 14) % 11 : index % 7;
            int count = vertical ? 11 : 7;
            if (vertical)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(edge == 2 ? 0f : 1f, (tile + 0.5f) / count);
                rect.sizeDelta = new Vector2(180f, 180f);
                rect.anchoredPosition = new Vector2(edge == 2 ? 75f : -75f, 0f);
                rect.localEulerAngles = new Vector3(0f, 0f, edge == 2 ? -90f : 90f);
            }
            else
            {
                rect.anchorMin = rect.anchorMax = new Vector2((tile + 0.5f) / count, edge);
                rect.sizeDelta = new Vector2(180f, 180f);
                rect.anchoredPosition = new Vector2(0f, edge == 0 ? 75f : -75f);
                if (edge == 1)
                    rect.localEulerAngles = new Vector3(0f, 0f, 180f);
            }
        }
    }
}
