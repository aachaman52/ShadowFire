using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using ShadowFire.Core;
using ShadowFire.Bootstrap;
using ShadowFire.Player;
using ShadowFire.Weapons;
using ShadowFire.Enemies;
using ShadowFire.Environment;
using ShadowFire.Managers;
using ShadowFire.Audio;
using ShadowFire.Effects;
using ShadowFire.UI;
using ShadowFire.Maps;
using ShadowFire.Modes;
using ShadowFire.Models;

namespace ShadowFire.Editor
{
    [InitializeOnLoad]
    public static class ShadowFireEditorAutomation
    {
        private static readonly string CmdFilePath = "Temp/unity_cmd.json";
        private static readonly string ResultFilePath = "Temp/unity_result.json";
        private static readonly string LogFilePath = "Temp/unity_automation.log";

        static ShadowFireEditorAutomation()
        {
            EditorApplication.update += OnEditorUpdate;
            Debug.Log("[ShadowFireEditorAutomation] Initialized and listening for commands.");
        }

        private static void OnEditorUpdate()
        {
            if (File.Exists(CmdFilePath))
            {
                try
                {
                    string json = File.ReadAllText(CmdFilePath);
                    if (string.IsNullOrWhiteSpace(json)) return;
                    File.Delete(CmdFilePath);
                    CommandData cmd = JsonUtility.FromJson<CommandData>(json);
                    if (cmd == null || string.IsNullOrEmpty(cmd.action))
                    {
                        WriteResult(false, "Invalid command payload");
                        return;
                    }
                    ExecuteCommand(cmd);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ShadowFireEditorAutomation] Error processing command: {ex}");
                    WriteResult(false, $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        [Serializable]
        public class CommandData
        {
            public string action;
            public string param;
        }

        [Serializable]
        public class ResultData
        {
            public bool success;
            public string message;
            public string data;
        }

        private static void WriteResult(bool success, string message, string data = "")
        {
            ResultData res = new ResultData { success = success, message = message, data = data };
            File.WriteAllText(ResultFilePath, JsonUtility.ToJson(res, true));
            File.AppendAllText(LogFilePath, $"[{DateTime.Now}] Success: {success} | Msg: {message}\n");
        }

        public static void ExecuteCommand(CommandData cmd)
        {
            Debug.Log($"[ShadowFireEditorAutomation] Executing command: {cmd.action}");
            switch (cmd.action)
            {
                case "ping":
                    WriteResult(true, "Unity Editor is responsive and ready.");
                    break;

                case "refresh":
                    AssetDatabase.Refresh();
                    WriteResult(true, "AssetDatabase refreshed.");
                    break;

                case "inspect_scene":
                    InspectCurrentScene();
                    break;

                case "create_prefabs":
                    CreateAllPrefabs();
                    break;

                case "build_arena_scene":
                    BuildArenaScene();
                    break;

                case "build_progression_scenes":
                    ShadowFireProgressionBuilder.BuildAllProgressionScenes();
                    WriteResult(true, "All progression scenes built successfully!");
                    break;

                case "run_progression_tests":
                    ShadowFireProgressionVerifier.RunVerification();
                    WriteResult(true, "Progression verification suite executed.");
                    break;

                case "bake_navmesh":
                    BakeNavMeshInScene();
                    break;

                case "save_scene":
                    SaveActiveScene();
                    break;

                case "start_playmode":
                    StartPlayModeTest();
                    break;

                case "stop_playmode":
                    EditorApplication.isPlaying = false;
                    WriteResult(true, "Play mode stopped.");
                    break;

                default:
                    WriteResult(false, $"Unknown command: {cmd.action}");
                    break;
            }
        }

        [MenuItem("ShadowFire/1. Create Enemy Prefabs")]
        public static void CreateAllPrefabs()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            CreateEnemyPrefab(EnemyType.Zombie, "Enemy_Zombie");
            CreateEnemyPrefab(EnemyType.Runner, "Enemy_Runner");
            CreateEnemyPrefab(EnemyType.Tank, "Enemy_Tank");
            CreateEnemyPrefab(EnemyType.Shooter, "Enemy_Shooter");
            CreateEnemyPrefab(EnemyType.Boss, "Enemy_Boss");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            WriteResult(true, "All enemy prefabs created in Assets/Prefabs.");
        }

        private static void CreateEnemyPrefab(EnemyType type, string prefabName)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.name = prefabName;
            obj.tag = type == EnemyType.Boss ? "Boss" : "Enemy";
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) obj.layer = enemyLayer;

            var agent = obj.AddComponent<NavMeshAgent>();
            var mr = obj.GetComponent<MeshRenderer>();

            EnemyBase enemyComp = null;

            switch (type)
            {
                case EnemyType.Zombie:
                    obj.transform.localScale = new Vector3(0.85f, 0.95f, 0.85f);
                    mr.sharedMaterial = ProceduralMeshGenerator.GetMaterial("enemy");
                    agent.radius = 0.4f;
                    agent.height = 1.75f;
                    enemyComp = obj.AddComponent<ZombieEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.35f, 0.2f, 0.35f), Quaternion.identity, 0.85f);
                    break;

                case EnemyType.Runner:
                    obj.transform.localScale = new Vector3(0.75f, 0.85f, 0.75f);
                    mr.sharedMaterial = ProceduralMeshGenerator.GetMaterial("glowred");
                    agent.radius = 0.35f;
                    agent.height = 1.6f;
                    enemyComp = obj.AddComponent<RunnerEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.32f, 0.15f, 0.32f), Quaternion.identity, 0.75f);
                    break;

                case EnemyType.Tank:
                    obj.transform.localScale = new Vector3(1.15f, 1.25f, 1.15f);
                    mr.sharedMaterial = ProceduralMeshGenerator.GetMaterial("gunmetal");
                    agent.radius = 0.6f;
                    agent.height = 2.1f;
                    enemyComp = obj.AddComponent<TankEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.55f, 0.25f, 0.5f), Quaternion.identity, 1.25f);
                    break;

                case EnemyType.Shooter:
                    obj.transform.localScale = new Vector3(0.85f, 0.95f, 0.85f);
                    mr.sharedMaterial = ProceduralMeshGenerator.GetMaterial("glowcyan");
                    agent.radius = 0.4f;
                    agent.height = 1.75f;
                    enemyComp = obj.AddComponent<ShooterEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.35f, 0.2f, 0.35f), Quaternion.identity, 0.95f);
                    break;

                case EnemyType.Boss:
                    obj.transform.localScale = new Vector3(1.5f, 1.75f, 1.5f);
                    mr.sharedMaterial = ProceduralMeshGenerator.GetMaterial("boss");
                    agent.radius = 0.8f;
                    agent.height = 2.8f;
                    enemyComp = obj.AddComponent<BossEnemy>();
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(0.7f, 0.35f, 0.65f), Quaternion.identity, 1.6f);
                    GunModelSetup.InstantiateGun(obj.transform, new Vector3(-0.7f, 0.35f, 0.65f), Quaternion.identity, 1.6f);
                    break;
            }

            string prefabPath = $"Assets/Prefabs/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            GameObject.DestroyImmediate(obj);
            Debug.Log($"[ShadowFireEditorAutomation] Saved prefab: {prefabPath}");
        }

        [MenuItem("ShadowFire/2. Build Arena Scene & References")]
        public static void BuildArenaScene()
        {
            string scenePath = "Assets/Scenes/ShadowFireArena.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Clear existing scene root objects
            var roots = scene.GetRootGameObjects();
            foreach (var r in roots)
            {
                GameObject.DestroyImmediate(r);
            }

            // 1. --- CORE ---
            GameObject coreRoot = new GameObject("--- CORE ---");
            
            GameObject gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(coreRoot.transform);
            var gm = gmObj.AddComponent<GameManager>();

            GameObject gbObj = new GameObject("GameBootstrapper");
            gbObj.transform.SetParent(coreRoot.transform);
            var gb = gbObj.AddComponent<GameBootstrapper>();
            // Set autoBuildOnAwake = false since scene is pre-baked
            var gbField = typeof(GameBootstrapper).GetField("autoBuildOnAwake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gbField != null) gbField.SetValue(gb, false);

            GameObject wmObj = new GameObject("WaveManager");
            wmObj.transform.SetParent(coreRoot.transform);
            var wm = wmObj.AddComponent<WaveManager>();

            GameObject audioObj = new GameObject("AudioManager");
            audioObj.transform.SetParent(coreRoot.transform);
            audioObj.AddComponent<AudioManager>();

            GameObject vfxObj = new GameObject("VFXManager");
            vfxObj.transform.SetParent(coreRoot.transform);
            vfxObj.AddComponent<VFXManager>();

            GameObject dnmObj = new GameObject("DamageNumberManager");
            dnmObj.transform.SetParent(coreRoot.transform);
            dnmObj.AddComponent<DamageNumberManager>();

            GameObject ldmObj = new GameObject("LootDropManager");
            ldmObj.transform.SetParent(coreRoot.transform);
            ldmObj.AddComponent<LootDropManager>();

            GameObject umObj = new GameObject("UpgradeManager");
            umObj.transform.SetParent(coreRoot.transform);
            umObj.AddComponent<UpgradeManager>();

            GameObject mapmObj = new GameObject("MapManager");
            mapmObj.transform.SetParent(coreRoot.transform);
            mapmObj.AddComponent<MapManager>();

            GameObject modemObj = new GameObject("ModeManager");
            modemObj.transform.SetParent(coreRoot.transform);
            modemObj.AddComponent<ModeManager>();

            // 2. --- ENVIRONMENT ---
            GameObject envRoot = new GameObject("--- ENVIRONMENT ---");
            GameObject arenaObj = new GameObject("Arena");
            arenaObj.transform.SetParent(envRoot.transform);
            
            var arenaBuilder = arenaObj.AddComponent<ArenaBuilder>();
            var navBaker = arenaObj.AddComponent<NavMeshRuntimeBaker>();
            var navSurface = arenaObj.AddComponent<NavMeshSurface>();
            navSurface.collectObjects = CollectObjects.All;
            navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = 0;

            Material groundMat = ProceduralMeshGenerator.GetMaterial("gunmetal");
            Material wallMat = ProceduralMeshGenerator.GetMaterial("default");

            // Ground: Position (0, -0.5, 0), Scale (40, 1, 40)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(arenaObj.transform);
            ground.transform.position = new Vector3(0, -0.5f, 0);
            ground.transform.localScale = new Vector3(40, 1, 40);
            ground.layer = groundLayer;
            ground.tag = "Ground";
            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

            // Walls: North, South, East, West
            GameObject wallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallN.name = "Wall_North";
            wallN.transform.SetParent(arenaObj.transform);
            wallN.transform.position = new Vector3(0, 2, 20);
            wallN.transform.localScale = new Vector3(40, 4, 1);
            wallN.layer = groundLayer;
            wallN.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            GameObject wallS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallS.name = "Wall_South";
            wallS.transform.SetParent(arenaObj.transform);
            wallS.transform.position = new Vector3(0, 2, -20);
            wallS.transform.localScale = new Vector3(40, 4, 1);
            wallS.layer = groundLayer;
            wallS.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            GameObject wallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallE.name = "Wall_East";
            wallE.transform.SetParent(arenaObj.transform);
            wallE.transform.position = new Vector3(20, 2, 0);
            wallE.transform.localScale = new Vector3(1, 4, 40);
            wallE.layer = groundLayer;
            wallE.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            GameObject wallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW.name = "Wall_West";
            wallW.transform.SetParent(arenaObj.transform);
            wallW.transform.position = new Vector3(-20, 2, 0);
            wallW.transform.localScale = new Vector3(1, 4, 40);
            wallW.layer = groundLayer;
            wallW.GetComponent<MeshRenderer>().sharedMaterial = wallMat;

            // 3. --- ENEMY SPAWNS ---
            GameObject spawnsRoot = new GameObject("--- ENEMY SPAWNS ---");
            GameObject enemySpawns = new GameObject("EnemySpawns");
            enemySpawns.transform.SetParent(spawnsRoot.transform);

            List<Transform> spawnList = new List<Transform>();
            Vector3[] spawnCoords = new Vector3[]
            {
                new Vector3(-15, 0, 15),
                new Vector3(15, 0, 15),
                new Vector3(-15, 0, -15),
                new Vector3(15, 0, -15)
            };

            for (int i = 0; i < spawnCoords.Length; i++)
            {
                GameObject sp = new GameObject($"SpawnPoint_{i + 1}");
                sp.transform.SetParent(enemySpawns.transform);
                sp.transform.position = spawnCoords[i];
                spawnList.Add(sp.transform);

                // Add visual beacon
                GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beacon.name = "Beacon";
                beacon.transform.SetParent(sp.transform);
                beacon.transform.localPosition = new Vector3(0, 0.05f, 0);
                beacon.transform.localScale = new Vector3(2f, 0.05f, 2f);
                beacon.GetComponent<Collider>().enabled = false;
                beacon.GetComponent<MeshRenderer>().sharedMaterial = ProceduralMeshGenerator.GetMaterial("glowred");
            }

            GameObject bossSpawn = new GameObject("BossSpawnPoint");
            bossSpawn.transform.SetParent(enemySpawns.transform);
            bossSpawn.transform.position = new Vector3(0, 0, 16);

            GameObject pSpawnObj = new GameObject("PlayerSpawnPoint");
            pSpawnObj.transform.SetParent(enemySpawns.transform);
            pSpawnObj.transform.position = new Vector3(0, 1, 0);

            arenaBuilder.PlayerSpawnPoint = pSpawnObj.transform;
            arenaBuilder.BossSpawnPoint = bossSpawn.transform;
            arenaBuilder.EnemySpawnPoints = spawnList;

            // Ensure all enemy prefabs exist
            CreateAllPrefabs();

            // Wire WaveManager references
            wm.Arena = arenaBuilder;
            wm.ZombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Zombie.prefab");
            wm.RunnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Runner.prefab");
            wm.TankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Tank.prefab");
            wm.ShooterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Shooter.prefab");
            wm.BossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Boss.prefab");

            // 4. --- LIGHTING ---
            GameObject lightRoot = new GameObject("--- LIGHTING ---");
            GameObject dirLight = new GameObject("Directional Light");
            dirLight.transform.SetParent(lightRoot.transform);
            dirLight.transform.position = new Vector3(0, 10, 0);
            dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            var lightComp = dirLight.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = new Color(0.85f, 0.9f, 1f);
            lightComp.intensity = 1.0f;
            lightComp.shadows = LightShadows.Soft;

            // 5. --- PLAYER ---
            GameObject playerRoot = new GameObject("--- PLAYER ---");
            GameObject player = new GameObject("Player");
            player.transform.SetParent(playerRoot.transform);
            player.transform.position = new Vector3(0, 1, 0);
            player.tag = "Player";
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0) player.layer = playerLayer;

            var cc = player.AddComponent<CharacterController>();
            cc.radius = 0.4f;
            cc.height = 2.0f;
            cc.center = new Vector3(0, 1.0f, 0);

            var inputHandler = player.AddComponent<PlayerInputHandler>();
            var playerStats = player.AddComponent<PlayerStats>();
            var playerController = player.AddComponent<PlayerController>();

            GameObject camRoot = new GameObject("CameraRoot");
            camRoot.transform.SetParent(player.transform);
            camRoot.transform.localPosition = new Vector3(0, 1.6f, 0);
            camRoot.transform.localRotation = Quaternion.identity;

            GameObject mainCamObj = new GameObject("Main Camera");
            mainCamObj.transform.SetParent(camRoot.transform);
            mainCamObj.transform.localPosition = Vector3.zero;
            mainCamObj.transform.localRotation = Quaternion.identity;
            mainCamObj.tag = "MainCamera";

            var cam = mainCamObj.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.1f;
            mainCamObj.AddComponent<AudioListener>();
            mainCamObj.AddComponent<CameraShake>();

            playerController.SetCameraTransform(camRoot.transform);

            GameObject weaponHolder = new GameObject("WeaponHolder");
            weaponHolder.transform.SetParent(camRoot.transform);
            weaponHolder.transform.localPosition = new Vector3(0.28f, -0.22f, 0.45f);
            weaponHolder.transform.localRotation = Quaternion.identity;

            var weaponManager = player.AddComponent<WeaponManager>();
            List<WeaponDataSO> arsenal = WeaponFactory.CreateCompleteArsenal();
            var swField = typeof(WeaponManager).GetField("startingWeaponData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (swField != null) swField.SetValue(weaponManager, arsenal);
            weaponManager.InitializeWeapons(arsenal);

            // 6. --- UI ---
            GameObject uiRoot = new GameObject("--- UI ---");
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(uiRoot.transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            GameObject esObj = new GameObject("EventSystem");
            esObj.transform.SetParent(uiRoot.transform);
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Build UI components on Canvas
            BuildCanvasUI(canvasObj);

            // 7. Bake NavMesh
            navSurface.BuildNavMesh();
            Debug.Log("[ShadowFireEditorAutomation] NavMesh baked successfully.");

            // 8. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            WriteResult(true, "ShadowFireArena scene built, wired, and saved successfully.");
        }

        private static void BuildCanvasUI(GameObject canvasObj)
        {
            HUDController hud = canvasObj.AddComponent<HUDController>();
            MapAndModeSelectUI mapModeUI = canvasObj.AddComponent<MapAndModeSelectUI>();

            // 1. Top Wave & Enemy Counter Banner
            GameObject waveBanner = CreateUIPanel(canvasObj.transform, "WaveBanner", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(500, 80));
            hud.WaveText = CreateUIText(waveBanner.transform, "WAVE 1", 28, TextAlignmentOptions.Center, Color.white, new Vector2(0, 18), new Vector2(480, 35));
            hud.EnemiesRemainingText = CreateUIText(waveBanner.transform, "ENEMIES: 0", 18, TextAlignmentOptions.Center, new Color(1f, 0.4f, 0.4f), new Vector2(0, -12), new Vector2(480, 25));

            mapModeUI.ObjectiveText = CreateUIText(canvasObj.transform, "OBJECTIVE: SURVIVE", 18, TextAlignmentOptions.Center, new Color(0.3f, 0.9f, 1f), new Vector2(0, -90), new Vector2(600, 30), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            hud.ScoreText = CreateUIText(canvasObj.transform, "SCORE: 0", 20, TextAlignmentOptions.TopRight, new Color(1f, 0.85f, 0.2f), new Vector2(-120, -35), new Vector2(220, 35), new Vector2(1, 1), new Vector2(1, 1));
            hud.CountdownText = CreateUIText(canvasObj.transform, "", 36, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.2f), new Vector2(0, 180), new Vector2(600, 60), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            // 2. Bottom Left: Health, Armor, Stamina, XP
            GameObject playerStatsPanel = CreateUIPanel(canvasObj.transform, "PlayerStatsPanel", new Vector2(0, 0), new Vector2(0, 0), new Vector2(190, 100), new Vector2(340, 160));
            
            GameObject hpBarObj = CreateUISlider(playerStatsPanel.transform, "HealthBar", new Vector2(0, 45), new Vector2(300, 22), new Color(0.15f, 0.85f, 0.35f));
            hud.HealthSlider = hpBarObj.GetComponent<Slider>();
            hud.HealthText = CreateUIText(hpBarObj.transform, "100 / 100", 14, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(300, 22));

            GameObject armorBarObj = CreateUISlider(playerStatsPanel.transform, "ArmorBar", new Vector2(0, 18), new Vector2(300, 14), new Color(0.2f, 0.7f, 1f));
            hud.ArmorSlider = armorBarObj.GetComponent<Slider>();
            hud.ArmorText = CreateUIText(armorBarObj.transform, "ARMOR: 0", 12, TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(300, 14));

            GameObject staBarObj = CreateUISlider(playerStatsPanel.transform, "StaminaBar", new Vector2(0, -5), new Vector2(300, 12), new Color(1f, 0.8f, 0.1f));
            hud.StaminaSlider = staBarObj.GetComponent<Slider>();

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

            // 8. Menus (Settings, Pause, Game Over)
            BuildMenusUI(canvasObj);
        }

        private static void BuildCrosshairUI(Transform parent, HUDController hud)
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

        private static RectTransform CreateCrosshairLine(Transform parent, Vector2 size)
        {
            GameObject line = new GameObject("CrosshairLine");
            line.transform.SetParent(parent, false);
            Image img = line.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.85f);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            return rt;
        }

        private static void BuildUpgradeModalUI(GameObject canvasObj)
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

        private static void BuildMenusUI(GameObject canvasObj)
        {
            Transform parent = canvasObj.transform;

            // Settings Panel
            GameObject settingsPanel = CreateUIPanel(parent, "SettingsPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 500));
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

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 minAnchor, Vector2 maxAnchor, Vector2 pos, Vector2 size)
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
            RectTransform fRt = fill.GetComponent<RectTransform>();
            fRt.sizeDelta = Vector2.zero;

            slider.fillRect = fRt;
            slider.targetGraphic = bgImg;
            slider.value = 1f;
            return sliderObj;
        }

        private static TextMeshProUGUI CreateUIText(Transform parent, string text, float fontSize, TextAlignmentOptions alignment, Color color, Vector2 pos, Vector2 size, Vector2? minAnchor = null, Vector2? maxAnchor = null)
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

        private static GameObject CreateFullscreenImage(Transform parent, string name, Color color)
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

        public static void BakeNavMeshInScene()
        {
            var surface = GameObject.FindFirstObjectByType<NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                WriteResult(true, "NavMesh baked and scene saved.");
            }
            else
            {
                WriteResult(false, "No NavMeshSurface found in active scene.");
            }
        }

        public static void SaveActiveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            WriteResult(saved, saved ? "Scene saved successfully." : "Failed to save scene.");
        }

        public static void InspectCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            List<string> objectNames = new List<string>();

            foreach (var r in roots)
            {
                TraverseHierarchy(r.transform, 0, objectNames);
            }

            string data = string.Join("\n", objectNames);
            WriteResult(true, $"Scene '{scene.name}' hierarchy inspected.", data);
        }

        private static void TraverseHierarchy(Transform t, int depth, List<string> list)
        {
            string indent = new string('-', depth * 2);
            string compList = "";
            var comps = t.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c != null) compList += $" [{c.GetType().Name}]";
            }
            list.Add($"{indent} {t.name} (Tag: {t.tag}, Layer: {LayerMask.LayerToName(t.gameObject.layer)}){compList}");

            for (int i = 0; i < t.childCount; i++)
            {
                TraverseHierarchy(t.GetChild(i), depth + 1, list);
            }
        }

        public static void StartPlayModeTest()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
                WriteResult(true, "Entered Play Mode.");
            }
            else
            {
                WriteResult(true, "Already in Play Mode.");
            }
        }
    }
}
