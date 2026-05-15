using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ColorBlockJamClone.UI
{
    public class StartPanel : UIPanel
    {   
        [SerializeField] private TMP_Text _tapToStartText;

        private Tween _pulseTween;

        private void OnEnable()
        {
            StartPulse();
        }

        private void OnDisable()
        {
            _pulseTween?.Kill();
            _tapToStartText.transform.localScale = Vector3.one;
        }

        private void StartPulse()
        {
            _pulseTween = _tapToStartText.transform
                .DOScale(1.1f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}