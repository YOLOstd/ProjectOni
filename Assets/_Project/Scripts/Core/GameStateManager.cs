using UnityEngine;

namespace ProjectOni.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        Cutscene
    }

    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            // Trigger state change events if needed
        }
    }
}
