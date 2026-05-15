using DG.Tweening;
using UnityEngine;

namespace ColorBlockJamClone.Data
{
    [CreateAssetMenu(fileName = "FeedbackConfig", menuName = "ColorBlockJamClone/Feedback Config")]
    public class FeedbackConfigSO : ScriptableObject
    {
        [Header("Block Pickup")]
        public float blockPickupScale = 1.08f;
        public float blockPickupDuration = 0.15f;
        public Ease blockPickupEase = Ease.OutBack;
        public float blockLiftHeight = 0.6f;

        [Header("Block Hold")]
        public float blockFloatAmplitude = 0.08f;
        public float blockFloatPeriod = 0.7f;

        [Header("Block Snap")]
        public float blockSnapDuration = 0.22f;
        public Ease blockSnapEase = Ease.OutBack;

        [Header("Button Press")]
        public float buttonPressScale = 0.9f;
        public float buttonPressDuration = 0.08f;
        public Ease buttonPressEase = Ease.OutQuad;

        [Header("Timer Warning")]
        [Min(0f)] public float timerWarningThreshold = 10f;
        public Color timerNormalColor = Color.white;
        public Color timerWarningColor = new(1f, 0.35f, 0.35f);
        public float timerPulseScale = 1.15f;
        public float timerPulsePeriod = 0.55f;
    }
}