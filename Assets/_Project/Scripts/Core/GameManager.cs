using UnityEngine;
using UnityEngine.SceneManagement;

namespace ColorBlockJamClone.Core
{
    /// <summary>
    /// This class persists across scenes
    /// Manages the game and holds game state
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;

        [SerializeField] private string _gameplaySceneName = "Gameplay";

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ServiceLocator.Register(this);

            SceneManager.LoadScene(_gameplaySceneName, LoadSceneMode.Single);

            Debug.Log("[GameManager] Bootstrap complete.");
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            ServiceLocator.Clear();
            Instance = null;
        }

        public void SetState(GameState newState)
        {
            if (State == newState)
                return;

            Debug.Log($"[GameManager] State changed to: {State} -> {newState}");

            State = newState;
        }
    }
}