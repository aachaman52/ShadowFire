using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.Core;
using ShadowFire.Managers;
using ShadowFire.SaveSystem;

namespace ShadowFire.UI
{
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
}
