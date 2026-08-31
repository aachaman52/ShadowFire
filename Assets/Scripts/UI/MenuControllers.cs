using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.Core;
using ShadowFire.Managers;
using ShadowFire.SaveSystem;

namespace ShadowFire.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }

        [Header("UI References")]
        public GameObject PausePanel;
        public Button ResumeButton;
        public Button SettingsButton;
        public Button MainMenuButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (ResumeButton != null)
            {
                ResumeButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
                });
            }

            if (SettingsButton != null)
            {
                SettingsButton.onClick.AddListener(() =>
                {
                    if (SettingsUIController.Instance != null) SettingsUIController.Instance.OpenSettings();
                });
            }

            if (MainMenuButton != null)
            {
                MainMenuButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null) GameManager.Instance.QuitToMainMenu();
                });
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            if (PausePanel != null) PausePanel.SetActive(false);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (PausePanel != null)
            {
                PausePanel.SetActive(state == GameState.Paused);
            }
        }
    }

    public class GameOverUIController : MonoBehaviour
    {
        public static GameOverUIController Instance { get; private set; }

        [Header("UI References")]
        public GameObject GameOverPanel;
        public TextMeshProUGUI WavesSurvivedText;
        public TextMeshProUGUI TotalKillsText;
        public TextMeshProUGUI FinalScoreText;
        public TextMeshProUGUI TimeSurvivedText;
        public TextMeshProUGUI HighScoreText;

        public Button RestartButton;
        public Button MainMenuButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (RestartButton != null)
            {
                RestartButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null) GameManager.Instance.RestartGame();
                });
            }

            if (MainMenuButton != null)
            {
                MainMenuButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null) GameManager.Instance.QuitToMainMenu();
                });
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            if (GameOverPanel != null) GameOverPanel.SetActive(false);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                if (GameOverPanel != null) GameOverPanel.SetActive(true);

                if (GameManager.Instance != null)
                {
                    if (WavesSurvivedText != null) WavesSurvivedText.text = $"WAVES SURVIVED: {GameManager.Instance.HighestWave - 1}";
                    if (TotalKillsText != null) TotalKillsText.text = $"ENEMIES PURGED: {GameManager.Instance.TotalKills}";
                    if (FinalScoreText != null) FinalScoreText.text = $"FINAL SCORE: {GameManager.Instance.TotalScore:N0}";

                    int minutes = (int)(GameManager.Instance.TimeSurvived / 60);
                    int seconds = (int)(GameManager.Instance.TimeSurvived % 60);
                    if (TimeSurvivedText != null) TimeSurvivedText.text = $"TIME SURVIVED: {minutes:00}:{seconds:00}";
                }

                GameSaveData data = SaveSystem.SaveSystem.Load();
                if (HighScoreText != null) HighScoreText.text = $"PERSONAL BEST: {data.HighScore:N0}";
            }
            else
            {
                if (GameOverPanel != null) GameOverPanel.SetActive(false);
            }
        }
    }

    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        public Button PlayButton;
        public Button SettingsButton;
        public Button QuitButton;
        public TextMeshProUGUI HighScoreText;
        public TextMeshProUGUI HighestWaveText;
        public TextMeshProUGUI TotalKillsText;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (PlayButton != null)
            {
                PlayButton.onClick.AddListener(() =>
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                });
            }

            if (SettingsButton != null)
            {
                SettingsButton.onClick.AddListener(() =>
                {
                    if (SettingsUIController.Instance != null) SettingsUIController.Instance.OpenSettings();
                });
            }

            if (QuitButton != null)
            {
                QuitButton.onClick.AddListener(() =>
                {
                    Application.Quit();
                });
            }

            GameSaveData data = SaveSystem.SaveSystem.Load();
            if (HighScoreText != null) HighScoreText.text = $"HIGH SCORE: {data.HighScore:N0}";
            if (HighestWaveText != null) HighestWaveText.text = $"HIGHEST WAVE: {data.HighestWave}";
            if (TotalKillsText != null) TotalKillsText.text = $"TOTAL KILLS: {data.TotalLifetimeKills:N0}";
        }
    }
}
