using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShadowFire.Core;
using ShadowFire.Managers;
using ShadowFire.Audio;
using ShadowFire.UI;

namespace ShadowFire.Missions
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Active Mission Configuration")]
        [SerializeField] private int missionID = 1;
        public MissionDataSO ActiveMission;

        [Header("Level Progress")]
        private float _startTime;
        private int _totalKillsInMission;
        private bool _isLevelCompleted = false;

        public int TotalWavesInLevel => ActiveMission != null ? ActiveMission.TotalWaves : 3;
        public bool IsLevelCompleted => _isLevelCompleted;
        public float TimeElapsed => Time.time - _startTime;

        public event Action<MissionDataSO> OnLevelStarted;
        public event Action OnLevelCompleted;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            if (ActiveMission == null)
            {
                ActiveMission = MissionFactory.GetMissionByID(missionID);
            }
        }

        private void Start()
        {
            _startTime = Time.time;
            _totalKillsInMission = 0;
            _isLevelCompleted = false;

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveCompleted += HandleWaveCompleted;
            }

            OnLevelStarted?.Invoke(ActiveMission);
            Debug.Log($"[ShadowFire] LevelManager: Started {ActiveMission.MissionName} ({ActiveMission.TotalWaves} Waves)");
        }

        private void OnDestroy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveCompleted -= HandleWaveCompleted;
            }
        }

        private void HandleWaveCompleted(int completedWave)
        {
            if (_isLevelCompleted) return;

            if (completedWave >= TotalWavesInLevel)
            {
                CompleteLevel();
            }
        }

        public void CompleteLevel()
        {
            if (_isLevelCompleted) return;
            _isLevelCompleted = true;

            int kills = GameManager.Instance != null ? GameManager.Instance.TotalKills : 0;
            int killXp = kills * 25;
            int baseXp = ActiveMission != null ? ActiveMission.BaseXpReward : 1000;
            int bonusXp = ActiveMission != null ? ActiveMission.CompletionBonus : 250;
            int totalXp = baseXp + killXp + bonusXp;

            int credits = ActiveMission != null ? ActiveMission.CreditReward : 600;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.LevelComplete);
            }

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLevelUp();
            }

            if (LevelCompleteUIController.Instance != null)
            {
                LevelCompleteUIController.Instance.ShowLevelComplete(
                    ActiveMission.MissionName,
                    kills,
                    baseXp,
                    killXp,
                    bonusXp,
                    totalXp,
                    credits,
                    TimeElapsed
                );
            }

            OnLevelCompleted?.Invoke();
            Debug.Log($"[ShadowFire] Level Completed: {ActiveMission.MissionName}! Total XP: {totalXp}, Credits: {credits}");
        }

        public void CommitRewardsAndReturnHome()
        {
            int kills = GameManager.Instance != null ? GameManager.Instance.TotalKills : 0;
            int killXp = kills * 25;
            int baseXp = ActiveMission != null ? ActiveMission.BaseXpReward : 1000;
            int bonusXp = ActiveMission != null ? ActiveMission.CompletionBonus : 250;
            int totalXp = baseXp + killXp + bonusXp;
            int credits = ActiveMission != null ? ActiveMission.CreditReward : 600;

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.CompleteMission(ActiveMission.MissionID, totalXp, credits);
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene("HomeBase");
        }
    }
}
