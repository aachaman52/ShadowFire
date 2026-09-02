using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.Weapons;
using ShadowFire.Enemies;
using ShadowFire.Environment;
using ShadowFire.Managers;
using ShadowFire.Audio;
using ShadowFire.Effects;
using ShadowFire.UI;
using ShadowFire.Missions;
using ShadowFire.Models;

namespace ShadowFire.Editor
{
    public static class ShadowFireProgressionBuilder
    {
        [MenuItem("ShadowFire/Build All Progression Scenes")]
        public static void BuildAllProgressionScenes()
        {
            CreateMissionAssets();
            BuildMainMenuScene();
            BuildHomeBaseScene();
            BuildCombatLevelScene(1, "Assets/Scenes/Level01.unity");
            BuildCombatLevelScene(2, "Assets/Scenes/Level02.unity");
            BuildCombatLevelScene(3, "Assets/Scenes/Level03.unity");
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ShadowFireProgressionBuilder] All progression scenes and Build Settings successfully generated!");
        }

        public static void CreateMissionAssets()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Missions"))
            {
                AssetDatabase.CreateFolder("Assets", "Missions");
            }

            var m1 = ScriptableObject.CreateInstance<MissionDataSO>();
            m1.MissionID = 1;
            m1.MissionName = "Abandoned Outpost";
            m1.Description = "Infiltrate the forward defense perimeter and eradicate 3 waves of hostiles.";
            m1.SceneName = "Level01";
            m1.TotalWaves = 3;
            m1.DifficultyMultiplier = 1.0f;
            m1.BaseXpReward = 1000;
            m1.CreditReward = 600;
            m1.CompletionBonus = 250;
            m1.HasBossFinalWave = false;
            AssetDatabase.CreateAsset(m1, "Assets/Missions/Mission_01.asset");

            var m2 = ScriptableObject.CreateInstance<MissionDataSO>();
            m2.MissionID = 2;
            m2.MissionName = "Industrial Sector";
            m2.Description = "Secure the power core against heavy runner swarms and armored hostiles across 4 waves.";
            m2.SceneName = "Level02";
            m2.TotalWaves = 4;
            m2.DifficultyMultiplier = 1.35f;
            m2.BaseXpReward = 2000;
            m2.CreditReward = 1100;
            m2.CompletionBonus = 500;
            m2.HasBossFinalWave = false;
            AssetDatabase.CreateAsset(m2, "Assets/Missions/Mission_02.asset");

            var m3 = ScriptableObject.CreateInstance<MissionDataSO>();
            m3.MissionID = 3;
            m3.MissionName = "Research Facility";
            m3.Description = "Deep penetration into the bio-core. Survive escalating waves and eliminate the Boss Titan.";
            m3.SceneName = "Level03";
            m3.TotalWaves = 5;
            m3.DifficultyMultiplier = 1.8f;
            m3.BaseXpReward = 3500;
            m3.CreditReward = 2200;
            m3.CompletionBonus = 1000;
            m3.HasBossFinalWave = true;
            m3.BossType = EnemyType.Boss;
            AssetDatabase.CreateAsset(m3, "Assets/Missions/Mission_03.asset");

            AssetDatabase.SaveAssets();
        }

        [MenuItem("ShadowFire/Build Main Menu Scene")]
        public static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting & Camera
            GameObject camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.09f);
            camObj.AddComponent<AudioListener>();

            GameObject lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.8f, 0.9f, 1f);
            light.intensity = 1.0f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0);

            // Core Managers
            GameObject coreRoot = new GameObject("--- CORE ---");
            coreRoot.AddComponent<AudioManager>();
            coreRoot.AddComponent<ProgressionManager>();

            // UI Canvas
            GameObject canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1.0f; // Responsive height fit
            canvasObj.AddComponent<GraphicRaycaster>();

            var menuController = canvasObj.AddComponent<MainMenuController>();

            // Title
            CreateUIText(canvasObj.transform, "SHADOWFIRE", 68, TextAlignmentOptions.Center, new Color(1f, 0.35f, 0.15f), new Vector2(0, 220), new Vector2(900, 90));
            CreateUIText(canvasObj.transform, "TACTICAL SURVIVAL PROTOCOL", 20, TextAlignmentOptions.Center, new Color(0.3f, 0.75f, 1f), new Vector2(0, 150), new Vector2(600, 40));

            // Buttons
            GameObject playBtnObj = CreateUIButton(canvasObj.transform, "PlayBtn", "ENTER HOME BASE", 22, new Color(0.18f, 0.55f, 0.35f), new Vector2(0, 20), new Vector2(340, 55));
            menuController.PlayButton = playBtnObj.GetComponent<Button>();

            GameObject setBtnObj = CreateUIButton(canvasObj.transform, "SettingsBtn", "SETTINGS", 20, new Color(0.2f, 0.3f, 0.45f), new Vector2(0, -50), new Vector2(340, 50));
            menuController.SettingsButton = setBtnObj.GetComponent<Button>();

            GameObject quitBtnObj = CreateUIButton(canvasObj.transform, "QuitBtn", "QUIT GAME", 20, new Color(0.5f, 0.2f, 0.2f), new Vector2(0, -120), new Vector2(340, 50));
            menuController.QuitButton = quitBtnObj.GetComponent<Button>();

            // Stats footer
            menuController.HighScoreText = CreateUIText(canvasObj.transform, "HIGH SCORE: 0", 16, TextAlignmentOptions.Center, new Color(0.6f, 0.7f, 0.8f), new Vector2(0, -220), new Vector2(500, 30));
            menuController.HighestWaveText = CreateUIText(canvasObj.transform, "HIGHEST WAVE: 1", 16, TextAlignmentOptions.Center, new Color(0.6f, 0.7f, 0.8f), new Vector2(0, -250), new Vector2(500, 30));
            menuController.TotalKillsText = CreateUIText(canvasObj.transform, "TOTAL KILLS: 0", 16, TextAlignmentOptions.Center, new Color(0.6f, 0.7f, 0.8f), new Vector2(0, -280), new Vector2(500, 30));

            // Settings Modal
            BuildSettingsModalUI(canvasObj);

            // EventSystem
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("[ShadowFireProgressionBuilder] Saved MainMenu.unity");
        }

        [MenuItem("ShadowFire/Build Home Base Scene")]
        public static void BuildHomeBaseScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting & Camera
            GameObject camObj = new GameObject("Main Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
            camObj.AddComponent<AudioListener>();

            GameObject lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.9f, 0.95f, 1f);
            light.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(45f, -40f, 0);

            // Core Managers
            GameObject coreRoot = new GameObject("--- CORE ---");
            coreRoot.AddComponent<AudioManager>();
            coreRoot.AddComponent<ProgressionManager>();

            // UI Canvas (Match Height = 1.0f ensures whole layout fits on any resolution)
            GameObject canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1.0f;
            canvasObj.AddComponent<GraphicRaycaster>();

            var homeUI = canvasObj.AddComponent<HomeBaseUIController>();

            // 1. Top Header
            GameObject header = CreateUIPanel(canvasObj.transform, "TopHeader", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -35), new Vector2(1820, 60));
            header.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 0.95f);

            CreateUIText(header.transform, "SHADOWFIRE // OPERATIONAL HUB", 22, TextAlignmentOptions.Left, new Color(1f, 0.4f, 0.2f), new Vector2(30, 0), new Vector2(450, 40), new Vector2(0, 0.5f), new Vector2(0, 0.5f));

            homeUI.PlayerLevelText = CreateUIText(header.transform, "LEVEL 1", 20, TextAlignmentOptions.Center, new Color(0.3f, 0.85f, 1f), new Vector2(-150, 10), new Vector2(250, 25), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            homeUI.XpText = CreateUIText(header.transform, "0 / 100 XP", 12, TextAlignmentOptions.Center, Color.white, new Vector2(-150, -12), new Vector2(250, 18), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            GameObject sliderObj = CreateUISlider(header.transform, "XpSlider", new Vector2(-150, -22), new Vector2(240, 6), new Color(0.3f, 0.85f, 1f));
            homeUI.XpSlider = sliderObj.GetComponent<Slider>();

            homeUI.CreditsText = CreateUIText(header.transform, "1,200 CR", 22, TextAlignmentOptions.Right, new Color(1f, 0.85f, 0.2f), new Vector2(-30, 0), new Vector2(300, 40), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));

            // 2. Nav Bar Tabs
            GameObject navBar = CreateUIPanel(canvasObj.transform, "NavBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(1820, 45));
            navBar.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.9f);

            homeUI.MissionsTabBtn = CreateUIButton(navBar.transform, "TabMissions", "MISSIONS & DEPLOYMENT", 15, new Color(0.18f, 0.28f, 0.42f), new Vector2(-540, 0), new Vector2(250, 36)).GetComponent<Button>();
            homeUI.WeaponsTabBtn = CreateUIButton(navBar.transform, "TabWeapons", "WEAPON UPGRADES", 15, new Color(0.18f, 0.28f, 0.42f), new Vector2(-270, 0), new Vector2(230, 36)).GetComponent<Button>();
            homeUI.AttributesTabBtn = CreateUIButton(navBar.transform, "TabAttributes", "PLAYER ATTRIBUTES", 15, new Color(0.18f, 0.28f, 0.42f), new Vector2(0, 0), new Vector2(230, 36)).GetComponent<Button>();
            homeUI.LoadoutTabBtn = CreateUIButton(navBar.transform, "TabLoadout", "LOADOUT CONFIG", 15, new Color(0.18f, 0.28f, 0.42f), new Vector2(270, 0), new Vector2(230, 36)).GetComponent<Button>();
            homeUI.ReturnMainMenuBtn = CreateUIButton(navBar.transform, "TabMenu", "MAIN MENU", 15, new Color(0.45f, 0.2f, 0.2f), new Vector2(540, 0), new Vector2(190, 36)).GetComponent<Button>();

            // 3. Missions Panel (Centered, compact height)
            GameObject mPanel = CreateUIPanel(canvasObj.transform, "MissionsPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -35), new Vector2(1650, 680));
            mPanel.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.13f, 0.92f);
            homeUI.MissionsPanel = mPanel;

            CreateUIText(mPanel.transform, "SELECT COMBAT MISSION", 26, TextAlignmentOptions.Center, Color.white, new Vector2(0, 290), new Vector2(600, 35));

            homeUI.MissionLaunchButtons = new Button[3];
            homeUI.MissionStatusTexts = new TextMeshProUGUI[3];

            var missions = MissionFactory.GetAllMissions();
            float[] cardX = new float[] { -500f, 0f, 500f };
            for (int i = 0; i < 3; i++)
            {
                var m = missions[i];
                GameObject card = CreateUIPanel(mPanel.transform, $"MissionCard_{i}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(cardX[i], -20), new Vector2(460, 520));
                card.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.22f);

                CreateUIText(card.transform, $"MISSION 0{m.MissionID}", 15, TextAlignmentOptions.Center, new Color(1f, 0.45f, 0.2f), new Vector2(0, 215), new Vector2(380, 25));
                CreateUIText(card.transform, m.MissionName.ToUpper(), 22, TextAlignmentOptions.Center, Color.white, new Vector2(0, 175), new Vector2(420, 35));

                CreateUIText(card.transform, m.Description, 13, TextAlignmentOptions.Center, new Color(0.75f, 0.8f, 0.85f), new Vector2(0, 85), new Vector2(400, 95));

                CreateUIText(card.transform, $"WAVES: {m.TotalWaves}   |   THREAT: {m.DifficultyMultiplier:0.0}x", 15, TextAlignmentOptions.Center, new Color(0.3f, 0.85f, 1f), new Vector2(0, 5), new Vector2(380, 25));
                CreateUIText(card.transform, $"REWARDS: +{m.BaseXpReward:N0} XP  |  +{m.CreditReward:N0} CR", 15, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(0, -35), new Vector2(380, 25));

                homeUI.MissionStatusTexts[i] = CreateUIText(card.transform, "AVAILABLE", 16, TextAlignmentOptions.Center, Color.green, new Vector2(0, -90), new Vector2(380, 30));

                GameObject launchBtn = CreateUIButton(card.transform, "LaunchBtn", "DEPLOY MISSION", 18, new Color(0.18f, 0.6f, 0.35f), new Vector2(0, -165), new Vector2(340, 50));
                homeUI.MissionLaunchButtons[i] = launchBtn.GetComponent<Button>();
            }

            // 4. Weapons Upgrade Panel
            GameObject wPanel = CreateUIPanel(canvasObj.transform, "WeaponsPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -35), new Vector2(1650, 680));
            wPanel.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.13f, 0.92f);
            homeUI.WeaponsPanel = wPanel;

            CreateUIText(wPanel.transform, "WEAPON ARSENAL & UPGRADES", 26, TextAlignmentOptions.Center, Color.white, new Vector2(0, 290), new Vector2(600, 35));

            homeUI.WeaponSelectButtons = new Button[5];
            string[] wNames = new string[] { "ASSAULT RIFLE", "VIPER SMG", "APEX SNIPER", "BREAKER SHOTGUN", "HAVOC ROCKET" };
            for (int i = 0; i < 5; i++)
            {
                float x = -560f + (i * 280f);
                GameObject wBtn = CreateUIButton(wPanel.transform, $"WSelect_{i}", wNames[i], 13, new Color(0.18f, 0.25f, 0.38f), new Vector2(x, 225), new Vector2(250, 40));
                homeUI.WeaponSelectButtons[i] = wBtn.GetComponent<Button>();
            }

            homeUI.SelectedWeaponNameText = CreateUIText(wPanel.transform, "ASSAULT RIFLE", 28, TextAlignmentOptions.Center, new Color(1f, 0.5f, 0.2f), new Vector2(0, 160), new Vector2(600, 35));

            // Weapon Stat Rows
            float[] rowY = new float[] { 70f, 0f, -70f, -140f };

            // Damage Row
            homeUI.DamageStatsText = CreateUIText(wPanel.transform, "DAMAGE: LVL 0 (100%)", 17, TextAlignmentOptions.Left, Color.white, new Vector2(-420, rowY[0]), new Vector2(400, 30));
            homeUI.DamageCostText = CreateUIText(wPanel.transform, "350 CR", 17, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(80, rowY[0]), new Vector2(140, 30));
            homeUI.UpgradeDamageBtn = CreateUIButton(wPanel.transform, "UpDmgBtn", "UPGRADE", 15, new Color(0.18f, 0.55f, 0.35f), new Vector2(320, rowY[0]), new Vector2(160, 40)).GetComponent<Button>();

            // Fire Rate Row
            homeUI.FireRateStatsText = CreateUIText(wPanel.transform, "FIRE RATE: LVL 0 (+0%)", 17, TextAlignmentOptions.Left, Color.white, new Vector2(-420, rowY[1]), new Vector2(400, 30));
            homeUI.FireRateCostText = CreateUIText(wPanel.transform, "350 CR", 17, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(80, rowY[1]), new Vector2(140, 30));
            homeUI.UpgradeFireRateBtn = CreateUIButton(wPanel.transform, "UpRateBtn", "UPGRADE", 15, new Color(0.18f, 0.55f, 0.35f), new Vector2(320, rowY[1]), new Vector2(160, 40)).GetComponent<Button>();

            // Magazine Row
            homeUI.MagStatsText = CreateUIText(wPanel.transform, "MAGAZINE: LVL 0 (+0%)", 17, TextAlignmentOptions.Left, Color.white, new Vector2(-420, rowY[2]), new Vector2(400, 30));
            homeUI.MagCostText = CreateUIText(wPanel.transform, "350 CR", 17, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(80, rowY[2]), new Vector2(140, 30));
            homeUI.UpgradeMagBtn = CreateUIButton(wPanel.transform, "UpMagBtn", "UPGRADE", 15, new Color(0.18f, 0.55f, 0.35f), new Vector2(320, rowY[2]), new Vector2(160, 40)).GetComponent<Button>();

            // Reload Row
            homeUI.ReloadStatsText = CreateUIText(wPanel.transform, "RELOAD SPEED: LVL 0 (-0%)", 17, TextAlignmentOptions.Left, Color.white, new Vector2(-420, rowY[3]), new Vector2(400, 30));
            homeUI.ReloadCostText = CreateUIText(wPanel.transform, "350 CR", 17, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(80, rowY[3]), new Vector2(140, 30));
            homeUI.UpgradeReloadBtn = CreateUIButton(wPanel.transform, "UpReloadBtn", "UPGRADE", 15, new Color(0.18f, 0.55f, 0.35f), new Vector2(320, rowY[3]), new Vector2(160, 40)).GetComponent<Button>();

            // 5. Attributes Panel
            GameObject aPanel = CreateUIPanel(canvasObj.transform, "AttributesPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -35), new Vector2(1650, 680));
            aPanel.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.13f, 0.92f);
            homeUI.AttributesPanel = aPanel;

            CreateUIText(aPanel.transform, "PLAYER ATTRIBUTE MATRIX", 26, TextAlignmentOptions.Center, Color.white, new Vector2(0, 290), new Vector2(600, 35));

            // Health
            homeUI.HealthStatsText = CreateUIText(aPanel.transform, "MAX HEALTH: 100 HP (LVL 0)", 17, TextAlignmentOptions.Left, Color.white, new Vector2(-420, 80), new Vector2(420, 30));
            homeUI.HealthCostText = CreateUIText(aPanel.transform, "400 CR", 17, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(80, 80), new Vector2(140, 30));
            homeUI.UpgradeHealthBtn = CreateUIButton(aPanel.transform, "UpHpBtn", "UPGRADE", 15, new Color(0.18f, 0.55f, 0.35f), new Vector2(320, 80), new Vector2(160, 40)).GetComponent<Button>();

            // Armor
            homeUI.ArmorStatsText = CreateUIText(aPanel.transform, "ARMOR PLATING: 0 ARMOR (LVL 0)", 17, TextAlignmentOptions.Left, Color.white, new Vector2(-420, 10), new Vector2(420, 30));
            homeUI.ArmorCostText = CreateUIText(aPanel.transform, "400 CR", 17, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(80, 10), new Vector2(140, 30));
            homeUI.UpgradeArmorBtn = CreateUIButton(aPanel.transform, "UpArmBtn", "UPGRADE", 15, new Color(0.18f, 0.55f, 0.35f), new Vector2(320, 10), new Vector2(160, 40)).GetComponent<Button>();

            // Stamina
            homeUI.StaminaStatsText = CreateUIText(aPanel.transform, "MAX STAMINA: 100 (LVL 0)", 17, TextAlignmentOptions.Left, Color.white, new Vector2(-420, -60), new Vector2(420, 30));
            homeUI.StaminaCostText = CreateUIText(aPanel.transform, "400 CR", 17, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(80, -60), new Vector2(140, 30));
            homeUI.UpgradeStaminaBtn = CreateUIButton(aPanel.transform, "UpStmBtn", "UPGRADE", 15, new Color(0.18f, 0.55f, 0.35f), new Vector2(320, -60), new Vector2(160, 40)).GetComponent<Button>();

            // Movement
            homeUI.MoveStatsText = CreateUIText(aPanel.transform, "SPRINT SPEED: +0% (LVL 0)", 17, TextAlignmentOptions.Left, Color.white, new Vector2(-420, -130), new Vector2(420, 30));
            homeUI.MoveCostText = CreateUIText(aPanel.transform, "400 CR", 17, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(80, -130), new Vector2(140, 30));
            homeUI.UpgradeMoveBtn = CreateUIButton(aPanel.transform, "UpMovBtn", "UPGRADE", 15, new Color(0.18f, 0.55f, 0.35f), new Vector2(320, -130), new Vector2(160, 40)).GetComponent<Button>();

            // 6. Loadout Panel
            GameObject lPanel = CreateUIPanel(canvasObj.transform, "LoadoutPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -35), new Vector2(1650, 680));
            lPanel.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.13f, 0.92f);
            homeUI.LoadoutPanel = lPanel;

            CreateUIText(lPanel.transform, "WEAPON LOADOUT SELECTION", 26, TextAlignmentOptions.Center, Color.white, new Vector2(0, 290), new Vector2(600, 35));

            homeUI.PrimaryEquippedText = CreateUIText(lPanel.transform, "PRIMARY: ASSAULT RIFLE", 20, TextAlignmentOptions.Center, new Color(0.3f, 0.85f, 1f), new Vector2(0, 200), new Vector2(600, 30));
            homeUI.PrimarySelectButtons = new Button[5];
            for (int i = 0; i < 5; i++)
            {
                float x = -560f + (i * 280f);
                GameObject pBtn = CreateUIButton(lPanel.transform, $"PrimaryBtn_{i}", wNames[i], 12, new Color(0.18f, 0.35f, 0.5f), new Vector2(x, 140), new Vector2(250, 40));
                homeUI.PrimarySelectButtons[i] = pBtn.GetComponent<Button>();
            }

            homeUI.SecondaryEquippedText = CreateUIText(lPanel.transform, "SECONDARY: BREAKER SHOTGUN", 20, TextAlignmentOptions.Center, new Color(1f, 0.7f, 0.2f), new Vector2(0, 30), new Vector2(600, 30));
            homeUI.SecondarySelectButtons = new Button[5];
            for (int i = 0; i < 5; i++)
            {
                float x = -560f + (i * 280f);
                GameObject sBtn = CreateUIButton(lPanel.transform, $"SecondaryBtn_{i}", wNames[i], 12, new Color(0.45f, 0.3f, 0.15f), new Vector2(x, -30), new Vector2(250, 40));
                homeUI.SecondarySelectButtons[i] = sBtn.GetComponent<Button>();
            }

            // EventSystem
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/HomeBase.unity");
            Debug.Log("[ShadowFireProgressionBuilder] Saved HomeBase.unity");
        }

        public static void BuildCombatLevelScene(int missionId, string saveScenePath)
        {
            // 1. Build the complete arena and dependencies
            ShadowFireEditorAutomation.BuildArenaScene();

            // 2. Open the scene
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/ShadowFireArena.unity");

            // 3. Attach LevelManager to CORE
            GameObject coreRoot = GameObject.Find("--- CORE ---");
            if (coreRoot != null)
            {
                var lm = coreRoot.AddComponent<LevelManager>();
                var mField = typeof(LevelManager).GetField("missionID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (mField != null) mField.SetValue(lm, missionId);
                lm.ActiveMission = AssetDatabase.LoadAssetAtPath<MissionDataSO>($"Assets/Missions/Mission_0{missionId}.asset") ?? MissionFactory.GetMissionByID(missionId);
            }

            // 4. Attach LevelCompleteUI to Canvas
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                BuildLevelCompleteModalUI(canvasObj);
            }

            EditorSceneManager.SaveScene(scene, saveScenePath);
            Debug.Log($"[ShadowFireProgressionBuilder] Saved {saveScenePath} for Mission {missionId}");
        }

        private static void BuildLevelCompleteModalUI(GameObject canvasObj)
        {
            GameObject modal = CreateUIPanel(canvasObj.transform, "LevelCompleteModal", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 680));
            modal.GetComponent<Image>().color = new Color(0.06f, 0.09f, 0.14f, 0.98f);

            var lcUI = canvasObj.AddComponent<LevelCompleteUIController>();
            lcUI.Container = modal;

            CreateUIText(modal.transform, "LEVEL COMPLETE", 34, TextAlignmentOptions.Center, new Color(0.2f, 0.9f, 0.4f), new Vector2(0, 270), new Vector2(700, 50));
            lcUI.MissionTitleText = CreateUIText(modal.transform, "ABANDONED OUTPOST", 20, TextAlignmentOptions.Center, new Color(0.3f, 0.75f, 1f), new Vector2(0, 225), new Vector2(700, 35));

            lcUI.KillsText = CreateUIText(modal.transform, "ENEMIES PURGED: 24", 18, TextAlignmentOptions.Left, Color.white, new Vector2(-220, 140), new Vector2(400, 30));
            lcUI.TimeText = CreateUIText(modal.transform, "TIME: 04:30", 18, TextAlignmentOptions.Right, new Color(0.7f, 0.7f, 0.7f), new Vector2(220, 140), new Vector2(300, 30));

            lcUI.BaseXpText = CreateUIText(modal.transform, "BASE XP: +1,000 XP", 16, TextAlignmentOptions.Left, Color.white, new Vector2(-220, 80), new Vector2(400, 25));
            lcUI.KillXpText = CreateUIText(modal.transform, "KILL XP: +600 XP", 16, TextAlignmentOptions.Left, Color.white, new Vector2(-220, 45), new Vector2(400, 25));
            lcUI.BonusXpText = CreateUIText(modal.transform, "BONUS XP: +250 XP", 16, TextAlignmentOptions.Left, Color.white, new Vector2(-220, 10), new Vector2(400, 25));

            lcUI.TotalXpText = CreateUIText(modal.transform, "TOTAL XP: +1,850", 22, TextAlignmentOptions.Center, new Color(0.2f, 0.85f, 1f), new Vector2(0, -45), new Vector2(600, 35));
            lcUI.CreditsText = CreateUIText(modal.transform, "CREDITS EARNED: +600 CR", 22, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(0, -90), new Vector2(600, 35));

            lcUI.PlayerLevelText = CreateUIText(modal.transform, "PLAYER LEVEL: 1", 16, TextAlignmentOptions.Center, Color.white, new Vector2(0, -145), new Vector2(400, 25));
            GameObject slider = CreateUISlider(modal.transform, "LcXpSlider", new Vector2(0, -175), new Vector2(400, 12), new Color(0.3f, 0.85f, 1f));
            lcUI.XpProgressBar = slider.GetComponent<Slider>();

            GameObject contBtn = CreateUIButton(modal.transform, "ContinueBtn", "CONTINUE TO HOME BASE", 20, new Color(0.18f, 0.55f, 0.35f), new Vector2(0, -250), new Vector2(420, 55));
            lcUI.ContinueButton = contBtn.GetComponent<Button>();

            modal.SetActive(false);
        }

        private static void BuildSettingsModalUI(GameObject canvasObj)
        {
            GameObject settingsPanel = CreateUIPanel(canvasObj.transform, "SettingsPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 500));
            settingsPanel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.12f, 0.96f);
            CreateUIText(settingsPanel.transform, "SETTINGS", 28, TextAlignmentOptions.Center, Color.white, new Vector2(0, 200), new Vector2(500, 45));

            SettingsUIController settingsUI = canvasObj.AddComponent<SettingsUIController>();
            settingsUI.SettingsPanel = settingsPanel;

            GameObject sensObj = CreateUISlider(settingsPanel.transform, "SensitivitySlider", new Vector2(0, 110), new Vector2(300, 20), new Color(0.2f, 0.8f, 1f));
            settingsUI.SensitivitySlider = sensObj.GetComponent<Slider>();
            settingsUI.SensitivitySlider.minValue = 0.5f;
            settingsUI.SensitivitySlider.maxValue = 4.0f;
            CreateUIText(settingsPanel.transform, "MOUSE SENSITIVITY", 14, TextAlignmentOptions.Left, Color.white, new Vector2(-150, 140), new Vector2(200, 25));
            settingsUI.SensitivityValueText = CreateUIText(settingsPanel.transform, "1.8", 14, TextAlignmentOptions.Right, Color.white, new Vector2(150, 140), new Vector2(60, 25));

            GameObject fovObj = CreateUISlider(settingsPanel.transform, "FovSlider", new Vector2(0, 40), new Vector2(300, 20), new Color(0.2f, 0.8f, 1f));
            settingsUI.FovSlider = fovObj.GetComponent<Slider>();
            settingsUI.FovSlider.minValue = 60f;
            settingsUI.FovSlider.maxValue = 110f;
            CreateUIText(settingsPanel.transform, "FIELD OF VIEW", 14, TextAlignmentOptions.Left, Color.white, new Vector2(-150, 70), new Vector2(200, 25));
            settingsUI.FovValueText = CreateUIText(settingsPanel.transform, "75", 14, TextAlignmentOptions.Right, Color.white, new Vector2(150, 70), new Vector2(60, 25));

            GameObject volObj = CreateUISlider(settingsPanel.transform, "MasterVolumeSlider", new Vector2(0, -30), new Vector2(300, 20), new Color(0.2f, 0.8f, 1f));
            settingsUI.MasterVolumeSlider = volObj.GetComponent<Slider>();
            settingsUI.MasterVolumeSlider.minValue = 0f;
            settingsUI.MasterVolumeSlider.maxValue = 1f;
            CreateUIText(settingsPanel.transform, "MASTER VOLUME", 14, TextAlignmentOptions.Left, Color.white, new Vector2(-150, 0), new Vector2(200, 25));

            GameObject closeBtnObj = CreateUIButton(settingsPanel.transform, "CloseSettingsBtn", "SAVE & CLOSE", 16, new Color(0.2f, 0.45f, 0.7f), new Vector2(0, -180), new Vector2(200, 45));
            settingsUI.CloseButton = closeBtnObj.GetComponent<Button>();
            settingsPanel.SetActive(false);
        }

        [MenuItem("ShadowFire/Configure Build Settings")]
        public static void ConfigureBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/HomeBase.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level01.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level02.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level03.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/ShadowFireArena.unity", true)
            };

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[ShadowFireProgressionBuilder] Configured {scenes.Count} scenes in Build Settings.");
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 minAnchor, Vector2 maxAnchor, Vector2 pos, Vector2 size, bool raycastTarget = false)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.12f, 0.14f, 0.18f, 0.85f);
            img.raycastTarget = raycastTarget;
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = minAnchor;
            rt.anchorMax = maxAnchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return panel;
        }

        private static GameObject CreateUIButton(Transform parent, string name, string label, float fontSize, Color color, Vector2 pos, Vector2 size)
        {
            GameObject btnObj = CreateUIPanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, true);
            btnObj.GetComponent<Image>().color = color;
            Button btn = btnObj.AddComponent<Button>();
            
            var colors = btn.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.35f;
            colors.pressedColor = color * 0.75f;
            colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            btn.colors = colors;

            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            CreateUIText(btnObj.transform, label, fontSize, TextAlignmentOptions.Center, Color.white, Vector2.zero, size - new Vector2(10, 10));
            return btnObj;
        }

        private static GameObject CreateUISlider(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);
            RectTransform sRt = sliderObj.AddComponent<RectTransform>();
            sRt.anchoredPosition = pos;
            sRt.sizeDelta = size;

            Slider slider = sliderObj.AddComponent<Slider>();

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.18f, 0.85f);
            bgImg.raycastTarget = false;
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.raycastTarget = false;
            RectTransform fRt = fill.GetComponent<RectTransform>();
            fRt.sizeDelta = Vector2.zero;

            slider.fillRect = fRt;
            slider.targetGraphic = bgImg;
            slider.value = 1f;
            return sliderObj;
        }

        private static TextMeshProUGUI CreateUIText(Transform parent, string text, float fontSize, TextAlignmentOptions alignment, Color color, Vector2 pos, Vector2 size, Vector2? minAnchor = null, Vector2? maxAnchor = null)
        {
            GameObject textObj = new GameObject("Text_" + text.Replace(" ", "_").Replace("//", "").Replace(":", ""));
            textObj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.raycastTarget = false;

            RectTransform rt = textObj.GetComponent<RectTransform>();
            if (minAnchor.HasValue) rt.anchorMin = minAnchor.Value;
            if (maxAnchor.HasValue) rt.anchorMax = maxAnchor.Value;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            return tmp;
        }
    }
}
