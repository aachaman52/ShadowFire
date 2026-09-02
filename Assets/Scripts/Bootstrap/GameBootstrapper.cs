using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

namespace ShadowFire.Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField] private bool autoBuildOnAwake = true;

        private void Awake()
        {
            if (autoBuildOnAwake)
            {
                BootstrapGame();
            }
        }

        public void BootstrapGame()
        {
            Debug.Log("[ShadowFire] Initializing ShadowFire Survival Game Engine with 4 Pillars...");

            // 1. Core Singletons & Managers
            EnsureSingleton<GameManager>("GameManager");
            EnsureSingleton<AudioManager>("AudioManager");
            EnsureSingleton<VFXManager>("VFXManager");
            EnsureSingleton<LootDropManager>("LootDropManager");
            EnsureSingleton<DamageNumberManager>("DamageNumberManager");
            EnsureSingleton<UpgradeManager>("UpgradeManager");
            WaveManager waveManager = EnsureSingleton<WaveManager>("WaveManager");

            // 2. Map & Mode Managers
            ShadowFire.Maps.MapManager mapManager = EnsureSingleton<ShadowFire.Maps.MapManager>("MapManager");
            ShadowFire.Modes.ModeManager modeManager = EnsureSingleton<ShadowFire.Modes.ModeManager>("ModeManager");

            // 3. Build 3D Arena
            GameObject arenaObj = new GameObject("Environment_Arena");
            ArenaBuilder arena = arenaObj.AddComponent<ArenaBuilder>();
            arena.BuildArena();
            waveManager.Arena = arena;

            // 4. NavMesh Surface Baker
            NavMeshRuntimeBaker navBaker = arenaObj.AddComponent<NavMeshRuntimeBaker>();
            navBaker.BakeNavMesh();

            // 5. Initialize Default Map & Game Mode
            mapManager.LoadMap(ShadowFire.Maps.MapTheme.OutpostRuin);
            modeManager.SetGameMode(ShadowFire.Modes.GameModeType.Survival);

            // 6. Build Player
            GameObject playerObj = BuildPlayer(arena.PlayerSpawnPoint != null ? arena.PlayerSpawnPoint.position : new Vector3(0, 3.5f, 0));

            // 7. Build Complete UI Canvas & HUD
            BuildGameUI(playerObj);

            Debug.Log("[ShadowFire] Bootstrap complete. Entering Wave 1.");
        }

        private T EnsureSingleton<T>(string name) where T : Component
        {
            T existing = FindFirstObjectByType<T>();
            if (existing != null) return existing;

            GameObject obj = new GameObject($"[{name}]");
            return obj.AddComponent<T>();
        }

        private GameObject BuildPlayer(Vector3 spawnPosition)
        {
            GameObject player = new GameObject("Player");
            player.transform.position = spawnPosition;
            player.layer = LayerMask.NameToLayer("Player");
            player.tag = "Player";

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 2.0f;
            cc.center = new Vector3(0, 1.0f, 0);

            player.AddComponent<PlayerInputHandler>();
            PlayerStats stats = player.AddComponent<PlayerStats>();
            PlayerController controller = player.AddComponent<PlayerController>();

            // Player Camera
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(player.transform);
            camObj.transform.localPosition = new Vector3(0, 1.7f, 0);
            camObj.transform.localRotation = Quaternion.identity;

            Camera cam = camObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.fieldOfView = 75f;
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<CameraShake>();

            controller.SetCameraTransform(camObj.transform);

            // Weapon Manager
            WeaponManager wm = player.AddComponent<WeaponManager>();
            List<WeaponDataSO> arsenal = WeaponFactory.CreateCompleteArsenal();
            wm.InitializeWeapons(arsenal);

            return player;
        }

        private void BuildGameUI(GameObject player)
        {
            GameObject canvasObj = new GameObject("Game_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // HUD Controller
            HUDController hud = canvasObj.AddComponent<HUDController>();
            MapAndModeSelectUI mapModeUI = canvasObj.AddComponent<MapAndModeSelectUI>();

            // 1. Top Wave & Enemy Counter Banner
            GameObject waveBanner = CreateUIPanel(canvasObj.transform, "WaveBanner", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(500, 80));
            hud.WaveText = CreateUIText(waveBanner.transform, "WAVE 1", 28, TextAlignmentOptions.Center, Color.white, new Vector2(0, 18), new Vector2(480, 35));
            hud.EnemiesRemainingText = CreateUIText(waveBanner.transform, "ENEMIES: 0", 18, TextAlignmentOptions.Center, new Color(1f, 0.4f, 0.4f), new Vector2(0, -12), new Vector2(480, 25));

            // Objective Text
            mapModeUI.ObjectiveText = CreateUIText(canvasObj.transform, "OBJECTIVE: SURVIVE", 18, TextAlignmentOptions.Center, new Color(0.3f, 0.9f, 1f), new Vector2(0, -90), new Vector2(600, 30), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            hud.ScoreText = CreateUIText(canvasObj.transform, "SCORE: 0", 20, TextAlignmentOptions.TopRight, new Color(1f, 0.85f, 0.2f), new Vector2(-120, -35), new Vector2(220, 35), new Vector2(1, 1), new Vector2(1, 1));
            hud.CountdownText = CreateUIText(canvasObj.transform, "", 36, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.2f), new Vector2(0, 180), new Vector2(600, 60), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            // 2. Bottom Left: Health, Armor, Stamina, XP
            GameObject playerStatsPanel = CreateUIPanel(canvasObj.transform, "PlayerStatsPanel", new Vector2(0, 0), new Vector2(0, 0), new Vector2(190, 100), new Vector2(340, 160));
            
            // Health Bar
            GameObject hpBarObj = CreateUISlider(playerStatsPanel.transform, "HealthBar", new Vector2(0, 45), new Vector2(300, 22), new Color(0.15f, 0.85f, 0.35f));
            hud.HealthSlider = hpBarObj.GetComponent<Slider>();
            hud.HealthText = CreateUIText(hpBarObj.transform, "100 / 100", 14, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(300, 22));

            // Armor Bar
            GameObject armorBarObj = CreateUISlider(playerStatsPanel.transform, "ArmorBar", new Vector2(0, 18), new Vector2(300, 14), new Color(0.2f, 0.7f, 1f));
            hud.ArmorSlider = armorBarObj.GetComponent<Slider>();
            hud.ArmorText = CreateUIText(armorBarObj.transform, "ARMOR: 0", 12, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(300, 14));

            // Stamina Bar
            GameObject staBarObj = CreateUISlider(playerStatsPanel.transform, "StaminaBar", new Vector2(0, -5), new Vector2(300, 12), new Color(1f, 0.8f, 0.1f));
            hud.StaminaSlider = staBarObj.GetComponent<Slider>();

            // XP Bar & Level
            GameObject xpBarObj = CreateUISlider(playerStatsPanel.transform, "XpBar", new Vector2(0, -28), new Vector2(300, 12), new Color(0.7f, 0.3f, 1f));
            hud.XpSlider = xpBarObj.GetComponent<Slider>();
            hud.LevelText = CreateUIText(playerStatsPanel.transform, "LVL 1", 16, TextAlignmentOptions.Left, new Color(0.8f, 0.4f, 1f), new Vector2(-110, -50), new Vector2(100, 25));

            // 3. Bottom Right: Weapon & Ammo
            GameObject weaponPanel = CreateUIPanel(canvasObj.transform, "WeaponPanel", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-180, 90), new Vector2(300, 140));
            hud.WeaponNameText = CreateUIText(weaponPanel.transform, "ASSAULT RIFLE", 20, TextAlignmentOptions.Right, Color.white, new Vector2(-20, 25), new Vector2(260, 30));
            hud.AmmoText = CreateUIText(weaponPanel.transform, "30 / 180", 32, TextAlignmentOptions.Right, new Color(1f, 0.85f, 0.2f), new Vector2(-20, -15), new Vector2(260, 45));
            hud.ReloadIndicatorText = CreateUIText(weaponPanel.transform, "RELOADING...", 16, TextAlignmentOptions.Right, new Color(1f, 0.3f, 0.3f), new Vector2(-20, -50), new Vector2(260, 25));
            hud.ReloadIndicatorText.gameObject.SetActive(false);

            // 4. Boss Health Bar (Top Center)
            GameObject bossBarObj = CreateUIPanel(canvasObj.transform, "BossBarContainer", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -120), new Vector2(550, 50));
            hud.BossBarContainer = bossBarObj;
            hud.BossNameText = CreateUIText(bossBarObj.transform, "SHADOW OVERLORD", 18, TextAlignmentOptions.Center, new Color(1f, 0.2f, 0.2f), new Vector2(0, 18), new Vector2(500, 25));
            GameObject bSliderObj = CreateUISlider(bossBarObj.transform, "BossHealthSlider", new Vector2(0, -8), new Vector2(500, 20), new Color(0.9f, 0.1f, 0.1f));
            hud.BossHealthSlider = bSliderObj.GetComponent<Slider>();
            bossBarObj.SetActive(false);

            // 5. Crosshair & Hitmarker
            BuildCrosshairUI(canvasObj.transform, hud);

            // 6. Damage Screen Flash & Low Health Vignette
            GameObject flashObj = CreateFullscreenImage(canvasObj.transform, "DamageFlash", new Color(1, 0, 0, 0));
            GameObject vignetteObj = CreateFullscreenImage(canvasObj.transform, "LowHealthVignette", new Color(0.8f, 0, 0, 0));
            ScreenDamageFlash flashComp = canvasObj.AddComponent<ScreenDamageFlash>();
            flashComp.BindImages(flashObj.GetComponent<Image>(), vignetteObj.GetComponent<Image>());

            // 7. Upgrade Modal UI
            BuildUpgradeModalUI(canvasObj);

            // 8. Settings, Pause & Game Over UI
            BuildMenusUI(canvasObj);
        }

        private void BuildCrosshairUI(Transform parent, HUDController hud)
        {
            GameObject crosshairRoot = new GameObject("CrosshairRoot");
            crosshairRoot.transform.SetParent(parent, false);
            RectTransform crt = crosshairRoot.AddComponent<RectTransform>();
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(60, 60);

            hud.CrosshairTop = CreateCrosshairLine(crosshairRoot.transform, new Vector2(2, 8));
            hud.CrosshairBottom = CreateCrosshairLine(crosshairRoot.transform, new Vector2(2, 8));
            hud.CrosshairLeft = CreateCrosshairLine(crosshairRoot.transform, new Vector2(8, 2));
            hud.CrosshairRight = CreateCrosshairLine(crosshairRoot.transform, new Vector2(8, 2));

            // Hitmarker
            GameObject hm = new GameObject("Hitmarker");
            hm.transform.SetParent(crosshairRoot.transform, false);
            Image hmImg = hm.AddComponent<Image>();
            hmImg.color = Color.white;
            RectTransform hmRt = hm.GetComponent<RectTransform>();
            hmRt.sizeDelta = new Vector2(16, 16);
            hm.transform.rotation = Quaternion.Euler(0, 0, 45f);
            hud.HitmarkerImage = hmImg;
            hm.SetActive(false);
        }

        private RectTransform CreateCrosshairLine(Transform parent, Vector2 size)
        {
            GameObject line = new GameObject("CrosshairLine");
            line.transform.SetParent(parent, false);
            Image img = line.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.85f);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            return rt;
        }

        private void BuildUpgradeModalUI(GameObject canvasObj)
        {
            GameObject modal = CreateUIPanel(canvasObj.transform, "UpgradeModal", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 480));
            Image bg = modal.GetComponent<Image>();
            bg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);

            CreateUIText(modal.transform, "LEVEL UP — CHOOSE UPGRADE", 26, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(0, 190), new Vector2(800, 45));

            UpgradeUIController upgradeUI = canvasObj.AddComponent<UpgradeUIController>();
            upgradeUI.Container = modal;
            upgradeUI.CardButtons = new Button[3];
            upgradeUI.CardTitles = new TextMeshProUGUI[3];
            upgradeUI.CardDescriptions = new TextMeshProUGUI[3];

            float[] cardX = new float[] { -260f, 0f, 260f };
            for (int i = 0; i < 3; i++)
            {
                GameObject card = CreateUIPanel(modal.transform, $"UpgradeCard_{i}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(cardX[i], -20), new Vector2(230, 320));
                Image cardBg = card.GetComponent<Image>();
                cardBg.color = new Color(0.16f, 0.2f, 0.26f);

                Button btn = card.AddComponent<Button>();
                upgradeUI.CardButtons[i] = btn;

                upgradeUI.CardTitles[i] = CreateUIText(card.transform, "TITLE", 18, TextAlignmentOptions.Center, new Color(0.2f, 0.8f, 1f), new Vector2(0, 110), new Vector2(200, 50));
                upgradeUI.CardDescriptions[i] = CreateUIText(card.transform, "Description", 15, TextAlignmentOptions.Center, Color.white, new Vector2(0, 0), new Vector2(200, 120));

                CreateUIText(card.transform, "[ SELECT ]", 14, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(0, -120), new Vector2(180, 30));
            }

            modal.SetActive(false);
        }

        private void BuildMenusUI(GameObject canvasObj)
        {
            Transform parent = canvasObj.transform;

            // Settings Panel
            GameObject settingsPanel = CreateUIPanel(parent, "SettingsPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 500));
            settingsPanel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.12f, 0.96f);
            CreateUIText(settingsPanel.transform, "SETTINGS", 28, TextAlignmentOptions.Center, Color.white, new Vector2(0, 200), new Vector2(500, 45));

            SettingsUIController settingsUI = canvasObj.AddComponent<SettingsUIController>();
            settingsUI.SettingsPanel = settingsPanel;

            // Sensitivity Slider
            GameObject sensObj = CreateUISlider(settingsPanel.transform, "SensitivitySlider", new Vector2(0, 110), new Vector2(300, 20), new Color(0.2f, 0.8f, 1f));
            settingsUI.SensitivitySlider = sensObj.GetComponent<Slider>();
            settingsUI.SensitivitySlider.minValue = 0.5f;
            settingsUI.SensitivitySlider.maxValue = 4.0f;
            CreateUIText(settingsPanel.transform, "MOUSE SENSITIVITY", 14, TextAlignmentOptions.Left, Color.white, new Vector2(-150, 140), new Vector2(200, 25));
            settingsUI.SensitivityValueText = CreateUIText(settingsPanel.transform, "1.8", 14, TextAlignmentOptions.Right, Color.white, new Vector2(150, 140), new Vector2(60, 25));

            // FOV Slider
            GameObject fovObj = CreateUISlider(settingsPanel.transform, "FovSlider", new Vector2(0, 40), new Vector2(300, 20), new Color(0.2f, 0.8f, 1f));
            settingsUI.FovSlider = fovObj.GetComponent<Slider>();
            settingsUI.FovSlider.minValue = 60f;
            settingsUI.FovSlider.maxValue = 110f;
            CreateUIText(settingsPanel.transform, "FIELD OF VIEW", 14, TextAlignmentOptions.Left, Color.white, new Vector2(-150, 70), new Vector2(200, 25));
            settingsUI.FovValueText = CreateUIText(settingsPanel.transform, "75", 14, TextAlignmentOptions.Right, Color.white, new Vector2(150, 70), new Vector2(60, 25));

            // Master Volume Slider
            GameObject volObj = CreateUISlider(settingsPanel.transform, "MasterVolumeSlider", new Vector2(0, -30), new Vector2(300, 20), new Color(0.2f, 0.8f, 1f));
            settingsUI.MasterVolumeSlider = volObj.GetComponent<Slider>();
            settingsUI.MasterVolumeSlider.minValue = 0f;
            settingsUI.MasterVolumeSlider.maxValue = 1f;
            CreateUIText(settingsPanel.transform, "MASTER VOLUME", 14, TextAlignmentOptions.Left, Color.white, new Vector2(-150, 0), new Vector2(200, 25));

            // Close Button
            GameObject closeBtnObj = CreateUIPanel(settingsPanel.transform, "CloseSettingsBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(200, 45));
            closeBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.7f);
            settingsUI.CloseButton = closeBtnObj.AddComponent<Button>();
            CreateUIText(closeBtnObj.transform, "SAVE & CLOSE", 16, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(180, 30));
            settingsPanel.SetActive(false);

            // Pause Menu Panel
            GameObject pausePanel = CreateUIPanel(parent, "PausePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(450, 400));
            pausePanel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.12f, 0.95f);
            CreateUIText(pausePanel.transform, "GAME PAUSED", 28, TextAlignmentOptions.Center, Color.white, new Vector2(0, 140), new Vector2(400, 45));

            PauseMenuController pauseUI = canvasObj.AddComponent<PauseMenuController>();
            pauseUI.PausePanel = pausePanel;

            GameObject resumeBtnObj = CreateUIPanel(pausePanel.transform, "ResumeBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(280, 45));
            resumeBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.55f, 0.35f);
            pauseUI.ResumeButton = resumeBtnObj.AddComponent<Button>();
            CreateUIText(resumeBtnObj.transform, "RESUME", 18, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(260, 35));

            GameObject setBtnObj = CreateUIPanel(pausePanel.transform, "SettingsBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -15), new Vector2(280, 45));
            setBtnObj.GetComponent<Image>().color = new Color(0.25f, 0.3f, 0.38f);
            pauseUI.SettingsButton = setBtnObj.AddComponent<Button>();
            CreateUIText(setBtnObj.transform, "SETTINGS", 18, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(260, 35));

            GameObject quitBtnObj = CreateUIPanel(pausePanel.transform, "QuitBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(280, 45));
            quitBtnObj.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);
            pauseUI.MainMenuButton = quitBtnObj.AddComponent<Button>();
            CreateUIText(quitBtnObj.transform, "MAIN MENU", 18, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(260, 35));
            pausePanel.SetActive(false);

            // Game Over Panel
            GameObject goPanel = CreateUIPanel(parent, "GameOverPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 540));
            goPanel.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 0.98f);
            CreateUIText(goPanel.transform, "SIGNAL LOST — DEFEAT", 30, TextAlignmentOptions.Center, new Color(1f, 0.2f, 0.2f), new Vector2(0, 210), new Vector2(550, 45));

            GameOverUIController goUI = canvasObj.AddComponent<GameOverUIController>();
            goUI.GameOverPanel = goPanel;

            goUI.WavesSurvivedText = CreateUIText(goPanel.transform, "WAVES SURVIVED: 0", 20, TextAlignmentOptions.Center, Color.white, new Vector2(0, 130), new Vector2(500, 35));
            goUI.TotalKillsText = CreateUIText(goPanel.transform, "ENEMIES PURGED: 0", 20, TextAlignmentOptions.Center, Color.white, new Vector2(0, 90), new Vector2(500, 35));
            goUI.FinalScoreText = CreateUIText(goPanel.transform, "FINAL SCORE: 0", 24, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f), new Vector2(0, 45), new Vector2(500, 40));
            goUI.TimeSurvivedText = CreateUIText(goPanel.transform, "TIME SURVIVED: 00:00", 18, TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.7f), new Vector2(0, 0), new Vector2(500, 30));
            goUI.HighScoreText = CreateUIText(goPanel.transform, "PERSONAL BEST: 0", 18, TextAlignmentOptions.Center, new Color(0.4f, 0.8f, 1f), new Vector2(0, -40), new Vector2(500, 30));

            GameObject restartBtn = CreateUIPanel(goPanel.transform, "RestartBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(300, 45));
            restartBtn.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.35f);
            goUI.RestartButton = restartBtn.AddComponent<Button>();
            CreateUIText(restartBtn.transform, "PLAY AGAIN", 18, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(280, 35));

            GameObject goMenuBtn = CreateUIPanel(goPanel.transform, "GOMenuBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -170), new Vector2(300, 45));
            goMenuBtn.GetComponent<Image>().color = new Color(0.4f, 0.2f, 0.2f);
            goUI.MainMenuButton = goMenuBtn.AddComponent<Button>();
            CreateUIText(goMenuBtn.transform, "MAIN MENU", 18, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(280, 35));

            goPanel.SetActive(false);
        }

        private GameObject CreateUIPanel(Transform parent, string name, Vector2 minAnchor, Vector2 maxAnchor, Vector2 pos, Vector2 size)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0.12f, 0.14f, 0.18f, 0.85f);
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = minAnchor;
            rt.anchorMax = maxAnchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return panel;
        }

        private GameObject CreateUISlider(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);
            RectTransform sRt = sliderObj.AddComponent<RectTransform>();
            sRt.anchoredPosition = pos;
            sRt.sizeDelta = size;

            Slider slider = sliderObj.AddComponent<Slider>();

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.18f, 0.85f);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Fill Area
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
            RectTransform fRt = fill.GetComponent<RectTransform>();
            fRt.sizeDelta = Vector2.zero;

            slider.fillRect = fRt;
            slider.targetGraphic = bgImg;
            slider.value = 1f;
            return sliderObj;
        }

        private TextMeshProUGUI CreateUIText(Transform parent, string text, float fontSize, TextAlignmentOptions alignment, Color color, Vector2 pos, Vector2 size, Vector2? minAnchor = null, Vector2? maxAnchor = null)
        {
            GameObject textObj = new GameObject("Text_" + text.Replace(" ", "_"));
            textObj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;

            RectTransform rt = textObj.GetComponent<RectTransform>();
            if (minAnchor.HasValue) rt.anchorMin = minAnchor.Value;
            if (maxAnchor.HasValue) rt.anchorMax = maxAnchor.Value;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            return tmp;
        }

        private GameObject CreateFullscreenImage(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return obj;
        }
    }
}
