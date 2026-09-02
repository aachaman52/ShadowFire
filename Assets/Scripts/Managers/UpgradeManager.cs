using System;
using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.Audio;
using ShadowFire.UI;

namespace ShadowFire.Managers
{
    [System.Serializable]
    public struct UpgradeCardData
    {
        public UpgradeType Type;
        public string Title;
        public string Description;
        public string IconName;

        public UpgradeCardData(UpgradeType type, string title, string description, string iconName = "")
        {
            Type = type;
            Title = title;
            Description = description;
            IconName = iconName;
        }
    }

    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        private readonly List<UpgradeCardData> _allUpgrades = new List<UpgradeCardData>()
        {
            new UpgradeCardData(UpgradeType.DamageBoost, "HEAVY CALIBER", "+20% Bullet & Weapon Damage"),
            new UpgradeCardData(UpgradeType.FasterReload, "QUICK MAGS", "+25% Faster Weapon Reload Speed"),
            new UpgradeCardData(UpgradeType.BiggerMagazine, "DRUM MAGAZINE", "+30% Magazine Capacity for all weapons"),
            new UpgradeCardData(UpgradeType.FasterSprint, "ADRENALINE RUSH", "+20% Sprint Movement Speed"),
            new UpgradeCardData(UpgradeType.MaxHealth, "VITALITY MATRIX", "+25 Max HP and instant 25 HP heal"),
            new UpgradeCardData(UpgradeType.ArmorBoost, "TITANIUM PLATES", "+15 Armor (Damage reduction)"),
            new UpgradeCardData(UpgradeType.CriticalChance, "HOLLOW POINT", "+15% Critical Hit Chance (x2 Damage)"),
            new UpgradeCardData(UpgradeType.FireRateBoost, "OVERCLOCK FIRING", "+20% Increased Fire Rate"),
            new UpgradeCardData(UpgradeType.ExplosiveAmmo, "SHOCK SHELLS", "Kinetic rounds trigger mini-explosions on impact"),
            new UpgradeCardData(UpgradeType.Lifesteal, "VAMPIRIC LEECH", "10% of all weapon damage dealt converts to HP")
        };

        public event Action<List<UpgradeCardData>> OnUpgradeChoicesGenerated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnLevelUp += HandleLevelUp;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            if (PlayerStats.Instance != null && !PlayerStats.Instance.IsAlive) return;
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLevelUp();
            }

            // Reward player with instant full health and stamina upon leveling up
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.Heal(100f);
            }

            // In progression-based missions, progression is managed at Home Base.
            // Ensure timeScale remains 1 so gameplay never freezes!
            Time.timeScale = 1f;
        }

        private List<UpgradeCardData> GetThreeRandomUpgrades()
        {
            List<UpgradeCardData> pool = new List<UpgradeCardData>(_allUpgrades);
            List<UpgradeCardData> selected = new List<UpgradeCardData>();

            for (int i = 0; i < 3 && pool.Count > 0; i++)
            {
                int r = UnityEngine.Random.Range(0, pool.Count);
                selected.Add(pool[r]);
                pool.RemoveAt(r);
            }

            return selected;
        }

        public void SelectUpgrade(UpgradeType type)
        {
            if (PlayerStats.Instance != null && !PlayerStats.Instance.IsAlive) return;
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver) return;

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ApplyUpgrade(type);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUpgradeSelect();
            }

            // Resume game
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.InGame);
            }
        }
    }
}
