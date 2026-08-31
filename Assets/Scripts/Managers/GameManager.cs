using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.SaveSystem;
using ShadowFire.Audio;

namespace ShadowFire.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State & Statistics")]
        [SerializeField] private GameState currentState = GameState.InGame;
        [SerializeField] private int totalScore = 0;
        [SerializeField] private int totalKills = 0;
        [SerializeField] private float timeSurvived = 0f;
        [SerializeField] private int highestWaveReached = 1;

        public GameState State => currentState;
        public int TotalScore => totalScore;
        public int TotalKills => totalKills;
        public float TimeSurvived => timeSurvived;
        public int HighestWave => highestWaveReached;

        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnKillsChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnDied += HandlePlayerDied;
            }

            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (currentState == GameState.InGame || currentState == GameState.WaveCountdown)
            {
                timeSurvived += Time.deltaTime;
            }

            // Pause toggle
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.InGame)
                {
                    PauseGame();
                }
                else if (currentState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
        }

        public void AddKill(int scoreValue = 100)
        {
            totalKills++;
            totalScore += scoreValue;
            OnKillsChanged?.Invoke(totalKills);
            OnScoreChanged?.Invoke(totalScore);
        }

        public void AddWaveBonus(int waveNum)
        {
            highestWaveReached = Mathf.Max(highestWaveReached, waveNum);
            totalScore += waveNum * 500;
            OnScoreChanged?.Invoke(totalScore);
        }

        public void PauseGame()
        {
            if (currentState != GameState.InGame && currentState != GameState.WaveCountdown) return;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            SetState(GameState.InGame);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void HandlePlayerDied(DamageInfo info)
        {
            SetState(GameState.GameOver);
            Time.timeScale = 0.2f; // Dramatic slow-mo
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Save run stats
            SaveSystem.SaveSystem.SaveGameStats(totalScore, highestWaveReached, totalKills);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOver();
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
