using System;
using ColorBlockJamClone.Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJamClone.UI
{
    public class GameplayPanel : UIPanel
    {
        [SerializeField] private TMP_Text _levelNumberText;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private FeedbackConfigSO _feedback;

        public event Action RestartClicked;

        private Tween _pulseTween;
        private bool _warningActive;

        private void Awake()
        {
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
        }

        public override void Show()
        {
            base.Show();
            ResetTimerVisual();
        }

        public override void Hide()
        {
            base.Hide();
            ResetTimerVisual();
        }

        public void SetLevelIndex(int index) => _levelNumberText.text = $"{index + 1}";

        public void SetTimer(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            _timerText.text = $"{total / 60:00}:{total % 60:00}";

            if (_feedback == null) return;

            if (seconds <= _feedback.timerWarningThreshold && !_warningActive)
                StartWarning();
            else if (seconds > _feedback.timerWarningThreshold && _warningActive)
                ResetTimerVisual();
        }

        private void StartWarning()
        {
            _warningActive = true;
            _timerText.color = _feedback.timerWarningColor;

            _pulseTween?.Kill();
            _pulseTween = _timerText.transform
                .DOScale(_feedback.timerPulseScale, _feedback.timerPulsePeriod * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void ResetTimerVisual()
        {
            _warningActive = false;
            _pulseTween?.Kill();
            _timerText.transform.localScale = Vector3.one;
            if (_feedback != null) _timerText.color = _feedback.timerNormalColor;
        }
    }
}