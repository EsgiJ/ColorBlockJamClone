using ColorBlockJamClone.Core;
using UnityEngine;

namespace ColorBlockJamClone.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private StartPanel _start;
        [SerializeField] private GameplayPanel _gameplay;
        [SerializeField] private CompletePanel _complete;
        [SerializeField] private FailPanel _fail;
        [SerializeField] private GameplayBootstrapper _bootstrapper;

        private void OnEnable()
        {
            GameEvents.OnLevelLoaded   += OnLevelLoaded;
            GameEvents.OnLevelStarted  += OnLevelStarted;
            GameEvents.OnLevelCompleted += OnLevelCompleted;
            GameEvents.OnLevelFailed   += OnLevelFailed;
            GameEvents.OnTimerTick     += OnTimerTick;

            _gameplay.RestartClicked += _bootstrapper.RestartLevel;
            _complete.NextClicked    += _bootstrapper.NextLevel;
            _fail.RetryClicked       += _bootstrapper.RestartLevel;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelLoaded   -= OnLevelLoaded;
            GameEvents.OnLevelStarted  -= OnLevelStarted;
            GameEvents.OnLevelCompleted -= OnLevelCompleted;
            GameEvents.OnLevelFailed   -= OnLevelFailed;
            GameEvents.OnTimerTick     -= OnTimerTick;

            _gameplay.RestartClicked -= _bootstrapper.RestartLevel;
            _complete.NextClicked    -= _bootstrapper.NextLevel;
            _fail.RetryClicked       -= _bootstrapper.RestartLevel;
        }

        private void OnLevelLoaded(int index)
        {
            _gameplay.SetLevelIndex(index);
            _gameplay.SetTimer(_bootstrapper.CurrentLevelDuration);

            _start.Show();
            _gameplay.Hide();
            _complete.Hide();
            _fail.Hide();
        }

        private void OnLevelStarted()
        {
            _start.Hide();
            _gameplay.Show();
        }

        private void OnLevelCompleted()
        {
            _complete.Show();
        }

        private void OnLevelFailed()
        {
            _fail.Show();
        }

        private void OnTimerTick(float remaining)
        {
            _gameplay.SetTimer(remaining);
        }
    }
}