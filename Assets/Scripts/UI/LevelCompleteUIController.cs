using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.Missions;
using ShadowFire.Managers;

namespace ShadowFire.UI
{
    public class LevelCompleteUIController : MonoBehaviour
    {
        public static LevelCompleteUIController Instance { get; private set; }

        [Header("UI References")]
        public GameObject Container;
        public TextMeshProUGUI MissionTitleText;
        public TextMeshProUGUI KillsText;
        public TextMeshProUGUI BaseXpText;
        public TextMeshProUGUI KillXpText;
        public TextMeshProUGUI BonusXpText;
        public TextMeshProUGUI TotalXpText;
        public TextMeshProUGUI CreditsText;
        public TextMeshProUGUI TimeText;
        public TextMeshProUGUI PlayerLevelText;
        public Slider XpProgressBar;

        public Button ContinueButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (Container != null) Container.SetActive(false);

            if (ContinueButton != null)
            {
                ContinueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        public void ShowLevelComplete(string missionName, int kills, int baseXp, int killXp, int bonusXp, int totalXp, int credits, float timeSeconds)
        {
            if (Container != null) Container.SetActive(true);

            if (MissionTitleText != null) MissionTitleText.text = missionName.ToUpper();
            if (KillsText != null) KillsText.text = $"ENEMIES PURGED: {kills}";
            if (BaseXpText != null) BaseXpText.text = $"+{baseXp:N0} XP";
            if (KillXpText != null) KillXpText.text = $"+{killXp:N0} XP";
            if (BonusXpText != null) BonusXpText.text = $"+{bonusXp:N0} XP";
            if (TotalXpText != null) TotalXpText.text = $"TOTAL XP: +{totalXp:N0}";
            if (CreditsText != null) CreditsText.text = $"CREDITS: +{credits:N0} CR";

            int minutes = (int)(timeSeconds / 60);
            int seconds = (int)(timeSeconds % 60);
            if (TimeText != null) TimeText.text = $"TIME: {minutes:00}:{seconds:00}";

            if (ProgressionManager.Instance != null)
            {
                var data = ProgressionManager.Instance.Data;
                if (PlayerLevelText != null) PlayerLevelText.text = $"PLAYER LEVEL: {data.PlayerLevel}";
                if (XpProgressBar != null)
                {
                    float req = ProgressionManager.Instance.GetXpRequiredForLevel(data.PlayerLevel);
                    XpProgressBar.value = Mathf.Clamp01(data.CurrentXP / req);
                }
            }
        }

        private void OnContinueClicked()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CommitRewardsAndReturnHome();
            }
            else
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("HomeBase");
            }
        }
    }
}
