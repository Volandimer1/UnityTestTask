using UnityEngine;

namespace _Bludoku.Scripts.Extentions.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class GameplayCameraLayout : MonoBehaviour
    {
        // The board is a 9x9 grid with one world unit per cell. The largest
        // five-cell tray figure is 2.5 units high at its resting 0.5 scale.
        private const float BoardHalfSize = 4.5f;
        private const float TrayFigureHalfSize = 1.25f;

        [SerializeField] private RectTransform scorePanel;
        [SerializeField] private Transform board;
        [SerializeField] private SpriteRenderer boardFrame;
        [SerializeField] private Transform tray;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private float topPadding = 24f;
        [SerializeField] private float bottomPadding = 32f;
        [SerializeField] private float sidePadding = 32f;
        [SerializeField] private float preferredOrthographicSize = 8.76f;

        private UnityEngine.Camera _camera;
        private Canvas _canvas;
        private readonly Vector3[] _scoreCorners = new Vector3[4];
        private int _lastScreenWidth;
        private int _lastScreenHeight;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            if (scorePanel == null || board == null || boardFrame == null || tray == null || background == null ||
                background.sprite == null || !_camera.orthographic)
            {
                Debug.LogError("Gameplay camera layout is missing a reference or an orthographic camera.", this);
                enabled = false;
                return;
            }

            _canvas = scorePanel.GetComponentInParent<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("Gameplay camera layout requires the score panel to be on a Canvas.", this);
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            int width = Screen.width;
            int height = Screen.height;
            Rect safeArea = Screen.safeArea;
            if (width <= 0 || height <= 0 || safeArea.width <= 0f || safeArea.height <= 0f ||
                (width == _lastScreenWidth && height == _lastScreenHeight && safeArea == _lastSafeArea))
                return;

            Canvas.ForceUpdateCanvases();
            scorePanel.GetWorldCorners(_scoreCorners);
            UnityEngine.Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : _canvas.worldCamera;
            float scoreBottom = RectTransformUtility.WorldToScreenPoint(uiCamera, _scoreCorners[0]).y;
            float uiScale = _canvas.scaleFactor;
            float top = Mathf.Min(scoreBottom, safeArea.yMax) - Mathf.Max(0f, topPadding) * uiScale;
            float bottom = safeArea.yMin + Mathf.Max(0f, bottomPadding) * uiScale;
            float availableWidth = safeArea.width - 2f * Mathf.Max(0f, sidePadding) * uiScale;
            float availableHeight = top - bottom;
            if (availableWidth <= 0f || availableHeight <= 0f)
                return;

            Bounds frameBounds = boardFrame.bounds;
            float boardTop = Mathf.Max(board.position.y + BoardHalfSize, frameBounds.max.y);
            float boardBottom = Mathf.Min(board.position.y - BoardHalfSize, frameBounds.min.y);
            float contentBottom = Mathf.Min(boardBottom, tray.position.y - TrayFigureHalfSize);
            float contentHeight = boardTop - contentBottom;
            float contentWidth = Mathf.Max(BoardHalfSize * 2f, frameBounds.size.x);
            float heightFit = contentHeight * height / (2f * availableHeight);
            float widthFit = contentWidth * height / (2f * availableWidth);
            float size = Mathf.Max(preferredOrthographicSize, heightFit, widthFit);

            _camera.orthographicSize = size;
            float worldUnitsPerPixel = 2f * size / height;
            float cameraX = board.position.x - (safeArea.center.x - width * 0.5f) * worldUnitsPerPixel;
            float cameraY = boardTop - (top - height * 0.5f) * worldUnitsPerPixel;
            transform.position = new Vector3(cameraX, cameraY, transform.position.z);

            Vector2 spriteSize = background.sprite.bounds.size;
            float visibleWidth = 2f * size * width / height;
            float coverScale = Mathf.Max(visibleWidth / spriteSize.x,
                2f * size / spriteSize.y) * 1.01f;
            background.transform.localScale = new Vector3(coverScale, coverScale,
                background.transform.localScale.z);
            background.transform.position = new Vector3(cameraX, cameraY,
                background.transform.position.z);

            _lastScreenWidth = width;
            _lastScreenHeight = height;
            _lastSafeArea = safeArea;
        }
    }
}
