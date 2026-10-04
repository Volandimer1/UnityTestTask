using UnityEngine;

namespace _Bludoku.Scripts.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private int _lastScreenWidth;
        private int _lastScreenHeight;
        private bool _hasAppliedSafeArea;

        private void OnEnable()
        {
            _rectTransform = (RectTransform)transform;
            _hasAppliedSafeArea = false;
            ApplySafeArea();
        }

        private void Update()
        {
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            int width = Screen.width;
            int height = Screen.height;
            if (width <= 0 || height <= 0)
                return;

            Rect safeArea = Screen.safeArea;
            if (_hasAppliedSafeArea && safeArea == _lastSafeArea &&
                width == _lastScreenWidth && height == _lastScreenHeight)
                return;

            _rectTransform.anchorMin = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
            _rectTransform.anchorMax = new Vector2(safeArea.xMax / width, safeArea.yMax / height);
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            _lastSafeArea = safeArea;
            _lastScreenWidth = width;
            _lastScreenHeight = height;
            _hasAppliedSafeArea = true;
        }
    }
}
