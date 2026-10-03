using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Bludoku.Scripts.Combo
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ComboFeedbackView : MonoBehaviour
    {
        private TMP_Text _text;
        private ComboSystem _comboSystem;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        public void Bind(ComboSystem comboSystem)
        {
            if (_comboSystem != null && isActiveAndEnabled)
                _comboSystem.ComboChanged -= OnComboChanged;

            _comboSystem = comboSystem;
            if (isActiveAndEnabled)
            {
                _comboSystem.ComboChanged += OnComboChanged;
                ShowCount(_comboSystem.ComboCount);
            }
        }

        private void OnEnable()
        {
            if (_comboSystem == null)
            {
                ShowCount(0);
                return;
            }

            _comboSystem.ComboChanged += OnComboChanged;
            ShowCount(_comboSystem.ComboCount);
        }

        private void OnDisable()
        {
            if (_comboSystem != null)
                _comboSystem.ComboChanged -= OnComboChanged;
            _text.transform.DOKill();
            _text.transform.localScale = Vector3.one;
        }

        private void OnComboChanged(ComboChange change)
        {
            ShowCount(change.ComboCount);

            if (change.Kind == ComboChangeKind.SuccessfulAction && change.ComboCount >= 2)
            {
                _text.transform.DOKill();
                _text.transform.DOPunchScale(Vector3.one * Mathf.Min(0.15f + change.ComboCount * 0.04f, 0.4f),
                    0.3f, 5, 0.4f);
            }
        }

        private void ShowCount(int comboCount)
        {
            _text.enabled = comboCount >= 2;
            if (_text.enabled)
            {
                _text.text = $"x{comboCount}";
            }
            else
            {
                _text.transform.DOKill();
                _text.transform.localScale = Vector3.one;
                _text.text = string.Empty;
            }
        }
    }
}
