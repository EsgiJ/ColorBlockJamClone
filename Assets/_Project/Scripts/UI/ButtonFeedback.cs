using ColorBlockJamClone.Core;
using ColorBlockJamClone.Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorBlockJamClone.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private FeedbackConfigSO _feedback;
        [SerializeField] private bool _playClickSound = true;

        private Vector3 _baseScale;
        private Tween _scaleTween;

        private void Awake() => _baseScale = transform.localScale;

        public void OnPointerDown(PointerEventData e)
        {
            if (_feedback == null) 
                return;
            
            _scaleTween?.Kill();
            _scaleTween = transform
                .DOScale(_baseScale * _feedback.buttonPressScale, _feedback.buttonPressDuration)
                .SetEase(_feedback.buttonPressEase);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (_feedback == null) 
                return;
            
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_baseScale, _feedback.buttonPressDuration)
                .SetEase(_feedback.buttonPressEase);

            if (_playClickSound && AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClick();
        }
    }
}