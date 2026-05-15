using System;
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

        public event Action RestartClicked;

        private void Awake()
        {
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
        }

        public void SetLevelIndex(int index) => _levelNumberText.text = $"{index + 1}";

        public void SetTimer(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            _timerText.text = $"{total / 60:00}:{total % 60:00}";
        }
    }
}