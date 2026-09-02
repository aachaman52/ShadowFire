using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.SaveSystem;

namespace ShadowFire.UI
{
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
