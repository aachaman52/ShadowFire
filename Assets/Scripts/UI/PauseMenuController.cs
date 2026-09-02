using UnityEngine;
using UnityEngine.UI;
using ShadowFire.Core;
using ShadowFire.Managers;

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
}
