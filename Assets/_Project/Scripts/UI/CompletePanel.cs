using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorBlockJamClone.UI
{
    public class CompletePanel : UIPanel
    {
        [SerializeField] private Button _nextButton;
        public event Action NextClicked;

        private void Awake() => _nextButton.onClick.AddListener(() => NextClicked?.Invoke());
    }
}