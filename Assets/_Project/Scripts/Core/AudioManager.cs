using ColorBlockJamClone.Data;
using UnityEngine;

namespace ColorBlockJamClone.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioConfigSO _config;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            PlayMusic();

            GameEvents.OnLevelCompleted += () => PlayOneShot(_config.levelComplete);
            GameEvents.OnLevelFailed    += () => PlayOneShot(_config.levelFail);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void PlayMusic()
        {
            if (_config == null || _config.backgroundMusic == null || _musicSource == null) 
                return;

            _musicSource.clip = _config.backgroundMusic;
            _musicSource.volume = _config.musicVolume;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip == null || _sfxSource == null || _config == null) 
                return;
                
            _sfxSource.pitch = 1f + Random.Range(-_config.pitchVariance, _config.pitchVariance);
            _sfxSource.PlayOneShot(clip, _config.sfxVolume);
        }

        public void PlayButtonClick() => PlayOneShot(_config.buttonClick);
        public void PlayBlockGrab()   => PlayOneShot(_config.blockGrab);
        public void PlayBlockSnap()   => PlayOneShot(_config.blockSnap);
        public void PlayBlockExit()   => PlayOneShot(_config.blockExit);
    }
}