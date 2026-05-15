using System;

namespace ColorBlockJamClone.Gameplay.Timer
{
    public class Timer
    {
        public float Duration { get; private set; }
        public float Remaining { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action OnExpired;

        public Timer(float duration) => Reset(duration);

        public void Reset(float duration)
        {
            Duration = duration;
            Remaining = duration;
            IsRunning = false;
        }

        public void Start() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public void Tick(float dt)
        {
            if (!IsRunning) 
                return;

            Remaining -= dt;
            
            if (Remaining <= 0f)
            {
                Remaining = 0f;
                IsRunning = false;
                OnExpired?.Invoke();
            }
        }
    }
}