using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJamClone.UI
{
    public class FailPanel : UIPanel
    {
        [SerializeField] private Button _retryButton;
        public event Action RetryClicked;

        private void Awake() => _retryButton.onClick.AddListener(() => RetryClicked?.Invoke());
    }
}