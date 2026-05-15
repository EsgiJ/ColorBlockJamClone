using UnityEngine;

namespace ColorBlockJamClone.Data
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "ColorBlockJamClone/Audio Config")]
    public class AudioConfigSO : ScriptableObject
    {
        [Header("Music")]
        public AudioClip backgroundMusic;
        [Range(0f, 1f)] public float musicVolume = 0.4f;

        [Header("SFX")]
        public AudioClip buttonClick;
        public AudioClip blockGrab;
        public AudioClip blockSnap;
        public AudioClip blockExit;
        public AudioClip levelComplete;
        public AudioClip levelFail;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;

        [Header("SFX Variation")]
        [Range(0f, 0.3f)] public float pitchVariance = 0.08f;
    }
}