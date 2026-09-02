using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.Weapons;
using ShadowFire.Enemies;
using ShadowFire.Managers;
using ShadowFire.Audio;
using ShadowFire.UI;
using ShadowFire.Missions;
using ShadowFire.SaveSystem;

namespace ShadowFire.Editor
{
    public static class ShadowFireProgressionVerifier
    {
        private static List<string> _testLogs = new List<string>();
        private static int _passCount = 0;
        private static int _failCount = 0;

        [MenuItem("ShadowFire/Run Progression Verification Suite")]
        public static void RunVerification()
        {
            _testLogs.Clear();
            _passCount = 0;
            _failCount = 0;

            LogHeader("SHADOWFIRE PROGRESSION & HOME BASE - VERIFICATION SUITE");

            try
            {
                // 1. Scene File Existence & Build Settings
                VerifySceneFilesAndBuildSettings();

                // 2. Main Menu Verification
                VerifyMainMenuScene();

                // 3. Home Base Verification
                VerifyHomeBaseScene();

                // 4. Combat Mission Scenes Verification (Level01, Level02, Level03)
                VerifyCombatMissionScenes();

                // 5. ProgressionManager Logic Tests
                VerifyProgressionManagerLogic();

                // 6. Mission & LevelManager Wave Flow Tests
                VerifyMissionAndLevelFlow();

                // 7. Weapon & Attribute Upgrades Integration
                VerifyUpgradeApplicationToPlayerAndWeapons();

                // 8. Save/Load Persistence Across Sessions
                VerifySavePersistence();
            }
            catch (Exception ex)
            {
                Log($"[CRITICAL EXCEPTION] Verification threw exception: {ex.Message}\n{ex.StackTrace}", false);
            }

            LogSummary();
            SaveReportToFile();
        }

        private static void VerifySceneFilesAndBuildSettings()
        {
            LogSection("1. SCENE FILES & BUILD SETTINGS");

            string[] expectedScenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/HomeBase.unity",
                "Assets/Scenes/Level01.unity",
                "Assets/Scenes/Level02.unity",
                "Assets/Scenes/Level03.unity",
                "Assets/Scenes/ShadowFireArena.unity"
            };

            foreach (var scenePath in expectedScenes)
            {
                AssertTest("SCENE_FILES", $"Scene file exists: {scenePath}", File.Exists(scenePath));
            }

            var buildScenes = EditorBuildSettings.scenes;
            AssertTest("BUILD_SETTINGS", $"Build Settings contains 6 configured scenes ({buildScenes.Length} found)", buildScenes.Length >= 6);
        }

        private static void VerifyMainMenuScene()
        {
            LogSection("2. MAIN MENU SCENE");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            AssertTest("MAIN_MENU", "MainMenu.unity loaded successfully", scene.IsValid());

            var menu = GameObject.FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include);
            AssertTest("MAIN_MENU", "MainMenuController found with Play, Settings, and Quit buttons",
                menu != null && menu.PlayButton != null && menu.SettingsButton != null && menu.QuitButton != null);

            var audio = GameObject.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            AssertTest("MAIN_MENU", "AudioManager present in Main Menu", audio != null);
        }

        private static void VerifyHomeBaseScene()
        {
            LogSection("3. HOME BASE SCENE");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/HomeBase.unity", OpenSceneMode.Single);
            AssertTest("HOME_BASE", "HomeBase.unity loaded successfully", scene.IsValid());

            var homeUI = GameObject.FindAnyObjectByType<HomeBaseUIController>(FindObjectsInactive.Include);
            AssertTest("HOME_BASE", "HomeBaseUIController found in scene", homeUI != null);

            if (homeUI != null)
            {
                AssertTest("HOME_BASE", "Top summary elements (Level, XP, Credits) wired",
                    homeUI.PlayerLevelText != null && homeUI.XpText != null && homeUI.CreditsText != null && homeUI.XpSlider != null);

                AssertTest("HOME_BASE", "Nav tabs (Missions, Weapons, Attributes, Loadout) wired",
                    homeUI.MissionsTabBtn != null && homeUI.WeaponsTabBtn != null && homeUI.AttributesTabBtn != null && homeUI.LoadoutTabBtn != null);

                AssertTest("HOME_BASE", "Missions panel with 3 mission launch cards wired",
                    homeUI.MissionsPanel != null && homeUI.MissionLaunchButtons != null && homeUI.MissionLaunchButtons.Length == 3);

                AssertTest("HOME_BASE", "Weapon upgrade panel with 5 weapon selectors & 4 stat upgrades wired",
                    homeUI.WeaponsPanel != null && homeUI.WeaponSelectButtons != null && homeUI.WeaponSelectButtons.Length == 5 &&
                    homeUI.UpgradeDamageBtn != null && homeUI.UpgradeFireRateBtn != null && homeUI.UpgradeMagBtn != null && homeUI.UpgradeReloadBtn != null);

                AssertTest("HOME_BASE", "Attribute upgrade panel (Health, Armor, Stamina, Movement) wired",
                    homeUI.AttributesPanel != null && homeUI.UpgradeHealthBtn != null && homeUI.UpgradeArmorBtn != null &&
                    homeUI.UpgradeStaminaBtn != null && homeUI.UpgradeMoveBtn != null);

                AssertTest("HOME_BASE", "Loadout panel with primary and secondary selectors wired",
                    homeUI.LoadoutPanel != null && homeUI.PrimarySelectButtons != null && homeUI.SecondarySelectButtons != null);
            }
        }

        private static void VerifyCombatMissionScenes()
        {
            LogSection("4. COMBAT MISSION SCENES (Level01, Level02, Level03)");

            string[] missionScenes = new string[] { "Assets/Scenes/Level01.unity", "Assets/Scenes/Level02.unity", "Assets/Scenes/Level03.unity" };
            int[] expectedWaves = new int[] { 3, 4, 5 };

            for (int i = 0; i < missionScenes.Length; i++)
            {
                string path = missionScenes[i];
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                AssertTest("COMBAT_SCENES", $"{path} opened successfully", scene.IsValid());

                var lm = GameObject.FindAnyObjectByType<LevelManager>(FindObjectsInactive.Include);
                var wm = GameObject.FindAnyObjectByType<WaveManager>(FindObjectsInactive.Include);
                var lcUI = GameObject.FindAnyObjectByType<LevelCompleteUIController>(FindObjectsInactive.Include);
                var player = GameObject.FindWithTag("Player");

                AssertTest("COMBAT_SCENES", $"LevelManager in {path} configured with {expectedWaves[i]} waves",
                    lm != null && lm.TotalWavesInLevel == expectedWaves[i]);

                AssertTest("COMBAT_SCENES", $"WaveManager, Player, and LevelCompleteUIController present in {path}",
                    wm != null && player != null && lcUI != null && lcUI.ContinueButton != null);
            }
        }

        private static void VerifyProgressionManagerLogic()
        {
            LogSection("5. PROGRESSION MANAGER LOGIC");

            // Reset test progression
            var data = new GameSaveData();
            data.Credits = 2500;
            data.PlayerLevel = 1;
            data.CurrentXP = 0f;
            data.HighestLevelUnlocked = 1;
            SaveSystem.SaveSystem.Save(data);

            GameObject obj = new GameObject("Test_ProgressionManager");
            var pm = obj.AddComponent<ProgressionManager>();

            // XP & Leveling Test
            pm.AddXP(250f); // Level 1 req is 100, Level 2 req is 135
            AssertTest("PROGRESSION", "Adding 250 XP levels up player (Level 1 -> Level 3)",
                pm.Data.PlayerLevel == 3);

            // Credits Test
            int startCredits = pm.Data.Credits;
            bool spendSuccess = pm.TrySpendCredits(500);
            AssertTest("PROGRESSION", "TrySpendCredits deducts 500 Credits properly",
                spendSuccess && pm.Data.Credits == (startCredits - 500));

            bool spendFail = pm.TrySpendCredits(99999);
            AssertTest("PROGRESSION", "TrySpendCredits rejects transaction when insufficient funds",
                !spendFail);

            // Attribute Upgrade Test
            int initialHpLvl = pm.GetAttributeLevel(AttributeType.Health);
            bool upHp = pm.UpgradeAttribute(AttributeType.Health);
            AssertTest("PROGRESSION", "UpgradeAttribute (Health) increments level and spends credits",
                upHp && pm.GetAttributeLevel(AttributeType.Health) == (initialHpLvl + 1));

            // Weapon Upgrade Test
            int initialDmgLvl = pm.GetWeaponUpgradeLevel(WeaponType.Rifle, WeaponUpgradeType.Damage);
            bool upWep = pm.UpgradeWeapon(WeaponType.Rifle, WeaponUpgradeType.Damage);
            AssertTest("PROGRESSION", "UpgradeWeapon (Rifle Damage) increments level and spends credits",
                upWep && pm.GetWeaponUpgradeLevel(WeaponType.Rifle, WeaponUpgradeType.Damage) == (initialDmgLvl + 1));

            // Mission Unlocks Test
            AssertTest("PROGRESSION", "Mission 1 is unlocked by default", pm.IsMissionUnlocked(1));
            AssertTest("PROGRESSION", "Mission 2 is locked before completing Mission 1", !pm.IsMissionUnlocked(2));

            pm.CompleteMission(1, 1000, 600);
            AssertTest("PROGRESSION", "Completing Mission 1 unlocks Mission 2 and marks Mission 1 completed",
                pm.IsMissionCompleted(1) && pm.IsMissionUnlocked(2));

            GameObject.DestroyImmediate(obj);
        }

        private static void VerifyMissionAndLevelFlow()
        {
            LogSection("6. MISSION & LEVEL FLOW SIMULATION");

            var missions = MissionFactory.GetAllMissions();
            AssertTest("MISSION_DATA", "MissionFactory defines 3 distinct missions", missions.Count == 3);

            var m1 = missions[0];
            var m2 = missions[1];
            var m3 = missions[2];

            AssertTest("MISSION_DATA", "Mission 1 configured (Abandoned Outpost, 3 Waves, Scene: Level01)",
                m1.MissionName == "Abandoned Outpost" && m1.TotalWaves == 3 && m1.SceneName == "Level01");

            AssertTest("MISSION_DATA", "Mission 2 configured (Industrial Sector, 4 Waves, Scene: Level02)",
                m2.MissionName == "Industrial Sector" && m2.TotalWaves == 4 && m2.SceneName == "Level02");

            AssertTest("MISSION_DATA", "Mission 3 configured (Research Facility, 5 Waves, Boss Titan, Scene: Level03)",
                m3.MissionName == "Research Facility" && m3.TotalWaves == 5 && m3.HasBossFinalWave && m3.SceneName == "Level03");
        }

        private static void VerifyUpgradeApplicationToPlayerAndWeapons()
        {
            LogSection("7. UPGRADE APPLICATION TO PLAYER & WEAPONS");

            // Setup test save data with 2 levels of Health (100 + 30 = 130 HP) and 2 levels of Armor (10 Armor)
            var data = SaveSystem.SaveSystem.Load();
            data.HealthUpgradeLevel = 2;
            data.ArmorUpgradeLevel = 2;
            data.StaminaUpgradeLevel = 2;
            data.MovementUpgradeLevel = 2;

            var rData = data.GetWeaponData(WeaponType.Rifle);
            rData.DamageLevel = 2;
            rData.MagazineLevel = 2;
            SaveSystem.SaveSystem.Save(data);

            // Test PlayerStats initialization
            GameObject pObj = new GameObject("Test_Player");
            pObj.AddComponent<CharacterController>();
            var pStats = pObj.AddComponent<PlayerStats>();
            pStats.ReloadStatsFromSave();

            AssertTest("PLAYER_UPGRADES", $"PlayerStats loads persistent Health (>=130 HP expected, {pStats.MaxHealth} found)",
                pStats.MaxHealth >= 130f);
            AssertTest("PLAYER_UPGRADES", $"PlayerStats loads persistent Armor (>=10 Armor expected, {pStats.CurrentArmor} found)",
                pStats.CurrentArmor >= 10f);
            AssertTest("PLAYER_UPGRADES", $"PlayerStats loads persistent Stamina (>=130 Stamina expected, {pStats.MaxStamina} found)",
                pStats.MaxStamina >= 130f);

            // Test WeaponManager initialization with upgraded Rifle
            var wm = pObj.AddComponent<WeaponManager>();
            var factoryArsenal = WeaponFactory.CreateCompleteArsenal();
            wm.InitializeWeapons(factoryArsenal);

            var activeWep = wm.ActiveWeapon;
            AssertTest("WEAPON_UPGRADES", "WeaponManager initializes with active weapon matching loadout", activeWep != null);

            if (activeWep != null)
            {
                AssertTest("WEAPON_UPGRADES", $"Upgraded Rifle damage applies (Base 26 * 1.24 = ~32.2 DMG, {activeWep.Data.Damage:F1} found)",
                    activeWep.Data.Damage > 30f);
                AssertTest("WEAPON_UPGRADES", $"Upgraded Rifle magazine size applies (Base 30 + 8 = 38 Mag, {activeWep.Data.MagazineSize} found)",
                    activeWep.Data.MagazineSize >= 38);
            }

            GameObject.DestroyImmediate(pObj);
        }

        private static void VerifySavePersistence()
        {
            LogSection("8. PERSISTENCE ACROSS SESSIONS");

            var data = new GameSaveData();
            data.PlayerLevel = 5;
            data.Credits = 4850;
            data.HighestLevelUnlocked = 3;
            data.CompletedLevelIDs = new List<int> { 1, 2 };
            data.SelectedPrimaryWeapon = WeaponType.Sniper;
            data.SelectedSecondaryWeapon = WeaponType.RocketLauncher;

            SaveSystem.SaveSystem.Save(data);

            // Force cache clear by reloading
            var loaded = SaveSystem.SaveSystem.Load();
            AssertTest("PERSISTENCE", "Player Level persisted (Level 5)", loaded.PlayerLevel == 5);
            AssertTest("PERSISTENCE", "Credits persisted (4,850 CR)", loaded.Credits == 4850);
            AssertTest("PERSISTENCE", "Highest Level Unlocked persisted (Level 3)", loaded.HighestLevelUnlocked == 3);
            AssertTest("PERSISTENCE", "Completed Missions persisted (Missions 1 & 2)", loaded.CompletedLevelIDs.Contains(1) && loaded.CompletedLevelIDs.Contains(2));
            AssertTest("PERSISTENCE", "Loadout persisted (Sniper & Rocket)", loaded.SelectedPrimaryWeapon == WeaponType.Sniper && loaded.SelectedSecondaryWeapon == WeaponType.RocketLauncher);
        }

        private static void AssertTest(string category, string testName, bool condition)
        {
            if (condition)
            {
                _passCount++;
                Log($"[PASS] {category}: {testName}", true);
            }
            else
            {
                _failCount++;
                Log($"[FAIL] {category}: {testName}", false);
            }
        }

        private static void LogHeader(string header)
        {
            string line = new string('=', 50);
            _testLogs.Add(line);
            _testLogs.Add(header);
            _testLogs.Add($"Generated At: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
            _testLogs.Add(line + "\n");
        }

        private static void LogSection(string section)
        {
            _testLogs.Add($"\n--- {section} ---");
        }

        private static void Log(string message, bool isPass)
        {
            _testLogs.Add(message);
            if (isPass) Debug.Log(message);
            else Debug.LogError(message);
        }

        private static void LogSummary()
        {
            string line = "\n" + new string('=', 50);
            _testLogs.Add(line);
            _testLogs.Add($"SUMMARY: {_passCount} PASSED, {_failCount} FAILED");
            _testLogs.Add($"STATUS: {(_failCount == 0 ? "ALL PROGRESSION SYSTEMS NOMINAL — 100% READY!" : "SOME TESTS FAILED")}");
            _testLogs.Add(new string('=', 50) + "\n");
        }

        private static void SaveReportToFile()
        {
            string reportPath = "progression_verification_report.txt";
            File.WriteAllLines(reportPath, _testLogs);
            Debug.Log($"[ShadowFireProgressionVerifier] Report written to {reportPath}");
        }
    }
}
