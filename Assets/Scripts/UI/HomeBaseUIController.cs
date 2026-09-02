using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ShadowFire.Core;
using ShadowFire.Managers;
using ShadowFire.Missions;

namespace ShadowFire.UI
{
    public class HomeBaseUIController : MonoBehaviour
    {
        public static HomeBaseUIController Instance { get; private set; }

        [Header("Header Summary")]
        public TextMeshProUGUI PlayerLevelText;
        public TextMeshProUGUI XpText;
        public Slider XpSlider;
        public TextMeshProUGUI CreditsText;

        [Header("Nav Tabs")]
        public Button MissionsTabBtn;
        public Button WeaponsTabBtn;
        public Button AttributesTabBtn;
        public Button LoadoutTabBtn;
        public Button ReturnMainMenuBtn;

        [Header("Main Panels")]
        public GameObject MissionsPanel;
        public GameObject WeaponsPanel;
        public GameObject AttributesPanel;
        public GameObject LoadoutPanel;

        [Header("Missions UI")]
        public Transform MissionListContent;
        public Button[] MissionLaunchButtons;
        public TextMeshProUGUI[] MissionStatusTexts;

        [Header("Weapon Upgrades UI")]
        public Button[] WeaponSelectButtons;
        public TextMeshProUGUI SelectedWeaponNameText;
        public TextMeshProUGUI DamageStatsText;
        public TextMeshProUGUI FireRateStatsText;
        public TextMeshProUGUI MagStatsText;
        public TextMeshProUGUI ReloadStatsText;
        public Button UpgradeDamageBtn;
        public Button UpgradeFireRateBtn;
        public Button UpgradeMagBtn;
        public Button UpgradeReloadBtn;
        public TextMeshProUGUI DamageCostText;
        public TextMeshProUGUI FireRateCostText;
        public TextMeshProUGUI MagCostText;
        public TextMeshProUGUI ReloadCostText;

        [Header("Attribute Upgrades UI")]
        public TextMeshProUGUI HealthStatsText;
        public TextMeshProUGUI ArmorStatsText;
        public TextMeshProUGUI StaminaStatsText;
        public TextMeshProUGUI MoveStatsText;
        public Button UpgradeHealthBtn;
        public Button UpgradeArmorBtn;
        public Button UpgradeStaminaBtn;
        public Button UpgradeMoveBtn;
        public TextMeshProUGUI HealthCostText;
        public TextMeshProUGUI ArmorCostText;
        public TextMeshProUGUI StaminaCostText;
        public TextMeshProUGUI MoveCostText;

        [Header("Loadout UI")]
        public TextMeshProUGUI PrimaryEquippedText;
        public TextMeshProUGUI SecondaryEquippedText;
        public Button[] PrimarySelectButtons;
        public Button[] SecondarySelectButtons;

        private WeaponType _selectedWeapon = WeaponType.Rifle;
        private List<MissionDataSO> _missions;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _missions = MissionFactory.GetAllMissions();

            // Setup Tab navigation
            if (MissionsTabBtn != null) MissionsTabBtn.onClick.AddListener(() => SwitchTab(0));
            if (WeaponsTabBtn != null) WeaponsTabBtn.onClick.AddListener(() => SwitchTab(1));
            if (AttributesTabBtn != null) AttributesTabBtn.onClick.AddListener(() => SwitchTab(2));
            if (LoadoutTabBtn != null) LoadoutTabBtn.onClick.AddListener(() => SwitchTab(3));
            if (ReturnMainMenuBtn != null) ReturnMainMenuBtn.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

            // Setup Mission Launch Buttons
            if (MissionLaunchButtons != null)
            {
                for (int i = 0; i < MissionLaunchButtons.Length; i++)
                {
                    int index = i;
                    if (MissionLaunchButtons[i] != null)
                    {
                        MissionLaunchButtons[i].onClick.AddListener(() => LaunchMission(index + 1));
                    }
                }
            }

            // Setup Weapon Select Buttons
            if (WeaponSelectButtons != null)
            {
                WeaponType[] types = new WeaponType[] { WeaponType.Rifle, WeaponType.SMG, WeaponType.Sniper, WeaponType.Shotgun, WeaponType.RocketLauncher };
                for (int i = 0; i < WeaponSelectButtons.Length && i < types.Length; i++)
                {
                    WeaponType wType = types[i];
                    if (WeaponSelectButtons[i] != null)
                    {
                        WeaponSelectButtons[i].onClick.AddListener(() => SelectWeapon(wType));
                    }
                }
            }

            // Setup Weapon Upgrade Buttons
            if (UpgradeDamageBtn != null) UpgradeDamageBtn.onClick.AddListener(() => UpgradeWeaponStat(WeaponUpgradeType.Damage));
            if (UpgradeFireRateBtn != null) UpgradeFireRateBtn.onClick.AddListener(() => UpgradeWeaponStat(WeaponUpgradeType.FireRate));
            if (UpgradeMagBtn != null) UpgradeMagBtn.onClick.AddListener(() => UpgradeWeaponStat(WeaponUpgradeType.Magazine));
            if (UpgradeReloadBtn != null) UpgradeReloadBtn.onClick.AddListener(() => UpgradeWeaponStat(WeaponUpgradeType.Reload));

            // Setup Attribute Upgrade Buttons
            if (UpgradeHealthBtn != null) UpgradeHealthBtn.onClick.AddListener(() => UpgradeAttributeStat(AttributeType.Health));
            if (UpgradeArmorBtn != null) UpgradeArmorBtn.onClick.AddListener(() => UpgradeAttributeStat(AttributeType.Armor));
            if (UpgradeStaminaBtn != null) UpgradeStaminaBtn.onClick.AddListener(() => UpgradeAttributeStat(AttributeType.Stamina));
            if (UpgradeMoveBtn != null) UpgradeMoveBtn.onClick.AddListener(() => UpgradeAttributeStat(AttributeType.Movement));

            // Setup Loadout Selection Buttons
            if (PrimarySelectButtons != null)
            {
                WeaponType[] types = new WeaponType[] { WeaponType.Rifle, WeaponType.SMG, WeaponType.Sniper, WeaponType.Shotgun, WeaponType.RocketLauncher };
                for (int i = 0; i < PrimarySelectButtons.Length && i < types.Length; i++)
                {
                    WeaponType wType = types[i];
                    if (PrimarySelectButtons[i] != null)
                    {
                        PrimarySelectButtons[i].onClick.AddListener(() => SetPrimaryWeapon(wType));
                    }
                }
            }

            if (SecondarySelectButtons != null)
            {
                WeaponType[] types = new WeaponType[] { WeaponType.Rifle, WeaponType.SMG, WeaponType.Sniper, WeaponType.Shotgun, WeaponType.RocketLauncher };
                for (int i = 0; i < SecondarySelectButtons.Length && i < types.Length; i++)
                {
                    WeaponType wType = types[i];
                    if (SecondarySelectButtons[i] != null)
                    {
                        SecondarySelectButtons[i].onClick.AddListener(() => SetSecondaryWeapon(wType));
                    }
                }
            }

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionUpdated += RefreshAllUI;
            }

            SwitchTab(0);
            RefreshAllUI();
        }

        private void OnDestroy()
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnProgressionUpdated -= RefreshAllUI;
            }
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void SwitchTab(int tabIndex)
        {
            if (MissionsPanel != null) MissionsPanel.SetActive(tabIndex == 0);
            if (WeaponsPanel != null) WeaponsPanel.SetActive(tabIndex == 1);
            if (AttributesPanel != null) AttributesPanel.SetActive(tabIndex == 2);
            if (LoadoutPanel != null) LoadoutPanel.SetActive(tabIndex == 3);
            RefreshAllUI();
        }

        public void RefreshAllUI()
        {
            if (ProgressionManager.Instance == null) return;
            var data = ProgressionManager.Instance.Data;

            // 1. Header
            if (PlayerLevelText != null) PlayerLevelText.text = $"LEVEL {data.PlayerLevel}";
            float reqXp = ProgressionManager.Instance.GetXpRequiredForLevel(data.PlayerLevel);
            if (XpText != null) XpText.text = $"{data.CurrentXP:N0} / {reqXp:N0} XP";
            if (XpSlider != null) XpSlider.value = Mathf.Clamp01(data.CurrentXP / reqXp);
            if (CreditsText != null) CreditsText.text = $"{data.Credits:N0} CR";

            // 2. Missions List
            for (int i = 0; i < _missions.Count; i++)
            {
                var mission = _missions[i];
                bool isUnlocked = ProgressionManager.Instance.IsMissionUnlocked(mission.MissionID);
                bool isCompleted = ProgressionManager.Instance.IsMissionCompleted(mission.MissionID);

                if (MissionStatusTexts != null && i < MissionStatusTexts.Length && MissionStatusTexts[i] != null)
                {
                    if (isCompleted)
                    {
                        MissionStatusTexts[i].text = "<color=#2ECC71>COMPLETED</color>";
                    }
                    else if (isUnlocked)
                    {
                        MissionStatusTexts[i].text = "<color=#3498DB>AVAILABLE</color>";
                    }
                    else
                    {
                        MissionStatusTexts[i].text = $"<color=#E74C3C>LOCKED (LVL {mission.RequiredPlayerLevel})</color>";
                    }
                }

                if (MissionLaunchButtons != null && i < MissionLaunchButtons.Length && MissionLaunchButtons[i] != null)
                {
                    MissionLaunchButtons[i].interactable = isUnlocked;
                }
            }

            // 3. Weapon Upgrades
            if (SelectedWeaponNameText != null) SelectedWeaponNameText.text = _selectedWeapon.ToString().ToUpper();
            int dmgLvl = ProgressionManager.Instance.GetWeaponUpgradeLevel(_selectedWeapon, WeaponUpgradeType.Damage);
            int rateLvl = ProgressionManager.Instance.GetWeaponUpgradeLevel(_selectedWeapon, WeaponUpgradeType.FireRate);
            int magLvl = ProgressionManager.Instance.GetWeaponUpgradeLevel(_selectedWeapon, WeaponUpgradeType.Magazine);
            int reloadLvl = ProgressionManager.Instance.GetWeaponUpgradeLevel(_selectedWeapon, WeaponUpgradeType.Reload);

            int dmgCost = ProgressionManager.Instance.GetWeaponUpgradeCost(_selectedWeapon, WeaponUpgradeType.Damage);
            int rateCost = ProgressionManager.Instance.GetWeaponUpgradeCost(_selectedWeapon, WeaponUpgradeType.FireRate);
            int magCost = ProgressionManager.Instance.GetWeaponUpgradeCost(_selectedWeapon, WeaponUpgradeType.Magazine);
            int reloadCost = ProgressionManager.Instance.GetWeaponUpgradeCost(_selectedWeapon, WeaponUpgradeType.Reload);

            if (DamageStatsText != null) DamageStatsText.text = $"DAMAGE: LVL {dmgLvl} ({(1f + dmgLvl * 0.12f) * 100:0}% DMG)";
            if (FireRateStatsText != null) FireRateStatsText.text = $"FIRE RATE: LVL {rateLvl} (+{rateLvl * 8}%)";
            if (MagStatsText != null) MagStatsText.text = $"MAGAZINE: LVL {magLvl} (+{magLvl * 15}%)";
            if (ReloadStatsText != null) ReloadStatsText.text = $"RELOAD SPEED: LVL {reloadLvl} (-{reloadLvl * 6}%)";

            if (DamageCostText != null) DamageCostText.text = dmgCost > 0 ? $"{dmgCost} CR" : "MAX";
            if (FireRateCostText != null) FireRateCostText.text = rateCost > 0 ? $"{rateCost} CR" : "MAX";
            if (MagCostText != null) MagCostText.text = magCost > 0 ? $"{magCost} CR" : "MAX";
            if (ReloadCostText != null) ReloadCostText.text = reloadCost > 0 ? $"{reloadCost} CR" : "MAX";

            if (UpgradeDamageBtn != null) UpgradeDamageBtn.interactable = (dmgCost > 0 && data.Credits >= dmgCost);
            if (UpgradeFireRateBtn != null) UpgradeFireRateBtn.interactable = (rateCost > 0 && data.Credits >= rateCost);
            if (UpgradeMagBtn != null) UpgradeMagBtn.interactable = (magCost > 0 && data.Credits >= magCost);
            if (UpgradeReloadBtn != null) UpgradeReloadBtn.interactable = (reloadCost > 0 && data.Credits >= reloadCost);

            // 4. Attribute Upgrades
            int hpLvl = data.HealthUpgradeLevel;
            int armLvl = data.ArmorUpgradeLevel;
            int stmLvl = data.StaminaUpgradeLevel;
            int movLvl = data.MovementUpgradeLevel;

            int hpCost = ProgressionManager.Instance.GetAttributeCost(AttributeType.Health);
            int armCost = ProgressionManager.Instance.GetAttributeCost(AttributeType.Armor);
            int stmCost = ProgressionManager.Instance.GetAttributeCost(AttributeType.Stamina);
            int movCost = ProgressionManager.Instance.GetAttributeCost(AttributeType.Movement);

            if (HealthStatsText != null) HealthStatsText.text = $"MAX HEALTH: {100 + hpLvl * 15} HP (LVL {hpLvl})";
            if (ArmorStatsText != null) ArmorStatsText.text = $"ARMOR PLATING: {armLvl * 5} ARMOR (LVL {armLvl})";
            if (StaminaStatsText != null) StaminaStatsText.text = $"MAX STAMINA: {100 + stmLvl * 15} (LVL {stmLvl})";
            if (MoveStatsText != null) MoveStatsText.text = $"SPRINT SPEED: +{movLvl * 5}% (LVL {movLvl})";

            if (HealthCostText != null) HealthCostText.text = hpCost > 0 ? $"{hpCost} CR" : "MAX";
            if (ArmorCostText != null) ArmorCostText.text = armCost > 0 ? $"{armCost} CR" : "MAX";
            if (StaminaCostText != null) StaminaCostText.text = stmCost > 0 ? $"{stmCost} CR" : "MAX";
            if (MoveCostText != null) MoveCostText.text = movCost > 0 ? $"{movCost} CR" : "MAX";

            if (UpgradeHealthBtn != null) UpgradeHealthBtn.interactable = (hpCost > 0 && data.Credits >= hpCost);
            if (UpgradeArmorBtn != null) UpgradeArmorBtn.interactable = (armCost > 0 && data.Credits >= armCost);
            if (UpgradeStaminaBtn != null) UpgradeStaminaBtn.interactable = (stmCost > 0 && data.Credits >= stmCost);
            if (UpgradeMoveBtn != null) UpgradeMoveBtn.interactable = (movCost > 0 && data.Credits >= movCost);

            // 5. Loadout Selection
            if (PrimaryEquippedText != null) PrimaryEquippedText.text = $"PRIMARY: {data.SelectedPrimaryWeapon.ToString().ToUpper()}";
            if (SecondaryEquippedText != null) SecondaryEquippedText.text = $"SECONDARY: {data.SelectedSecondaryWeapon.ToString().ToUpper()}";
        }

        public void SelectWeapon(WeaponType type)
        {
            _selectedWeapon = type;
            RefreshAllUI();
        }

        public void UpgradeWeaponStat(WeaponUpgradeType upgrade)
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.UpgradeWeapon(_selectedWeapon, upgrade);
            }
        }

        public void UpgradeAttributeStat(AttributeType attribute)
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.UpgradeAttribute(attribute);
            }
        }

        public void SetPrimaryWeapon(WeaponType type)
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.SetSelectedLoadout(type, ProgressionManager.Instance.Data.SelectedSecondaryWeapon);
            }
        }

        public void SetSecondaryWeapon(WeaponType type)
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.SetSelectedLoadout(ProgressionManager.Instance.Data.SelectedPrimaryWeapon, type);
            }
        }

        public void LaunchMission(int missionId)
        {
            var mission = MissionFactory.GetMissionByID(missionId);
            if (mission != null && ProgressionManager.Instance != null && ProgressionManager.Instance.IsMissionUnlocked(missionId))
            {
                Debug.Log($"[ShadowFire] Deploying to {mission.MissionName} ({mission.SceneName})...");
                SceneManager.LoadScene(mission.SceneName);
            }
        }
    }
}
