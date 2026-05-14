using System;

namespace ColorBlockJamClone.Core
{
    public static class GameEvents
    {
        public static event Action<int> OnLevelLoaded;      // level index
        public static event Action OnLevelStarted;          // first input received
        public static event Action OnLevelCompleted;
        public static event Action OnLevelFailed;

        public static event Action<int> OnBlockExited;      // remaining blocks
        public static event Action<float> OnTimerTick;      // remaining seconds

        public static void RaiseLevelLoaded(int index) => OnLevelLoaded?.Invoke(index);
        public static void RaiseLevelStarted() => OnLevelStarted?.Invoke();
        public static void RaiseLevelCompleted() => OnLevelCompleted?.Invoke();
        public static void RaiseLevelFailed() => OnLevelFailed?.Invoke();
        public static void RaiseBlockExited(int remaining) => OnBlockExited?.Invoke(remaining);
        public static void RaiseTimerTick(float remaining) => OnTimerTick?.Invoke(remaining);

        public static void Clear()
        {
            OnLevelLoaded = null;
            OnLevelStarted = null;
            OnLevelCompleted = null;
            OnLevelFailed = null;
            OnBlockExited = null;
            OnTimerTick = null;
        }
    }
}