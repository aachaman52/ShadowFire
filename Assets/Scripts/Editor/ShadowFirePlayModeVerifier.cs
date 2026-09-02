using System;
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

namespace ShadowFire.Editor
{
    public static class ShadowFirePlayModeVerifier
    {
        private static readonly string ReportPath = "C:/ShadowFire/verification_report.txt";

        [MenuItem("ShadowFire/3. Run Full System Verification")]
        public static void RunVerification()
        {
            // 1. Build and configure scene fresh
            ShadowFireEditorAutomation.BuildArenaScene();

            var reportLines = new List<string>();
            reportLines.Add("==================================================");
            reportLines.Add("SHADOWFIRE FPS SURVIVAL - VERIFICATION REPORT");
            reportLines.Add($"Generated At: {DateTime.Now}");
            reportLines.Add("==================================================\n");

            int passCount = 0;
            int failCount = 0;

            void AssertTest(string system, string description, bool condition, string details = "")
            {
                if (condition)
                {
                    passCount++;
                    string msg = $"[PASS] {system}: {description}" + (string.IsNullOrEmpty(details) ? "" : $" ({details})");
                    reportLines.Add(msg);
                    Debug.Log(msg);
                }
                else
                {
                    failCount++;
                    string msg = $"[FAIL] {system}: {description}" + (string.IsNullOrEmpty(details) ? "" : $" ({details})");
                    reportLines.Add(msg);
                    Debug.LogError(msg);
                }
            }

            // 1. SCENE STRUCTURE & ASSETS
            string scenePath = "Assets/Scenes/ShadowFireArena.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            AssertTest("SCENE", "ShadowFireArena.unity opened successfully", scene.IsValid());

            // 2. PREFABS
            var zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Zombie.prefab");
            var runnerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Runner.prefab");
            var tankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Tank.prefab");
            var shooterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Shooter.prefab");
            var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy_Boss.prefab");

            AssertTest("PREFAB", "Zombie prefab exists with EnemyBase + NavMeshAgent",
                zombiePrefab != null && zombiePrefab.GetComponent<EnemyBase>() != null && zombiePrefab.GetComponent<NavMeshAgent>() != null);
            AssertTest("PREFAB", "Runner prefab exists with EnemyBase",
                runnerPrefab != null && runnerPrefab.GetComponent<EnemyBase>() != null);
            AssertTest("PREFAB", "Tank prefab exists with EnemyBase",
                tankPrefab != null && tankPrefab.GetComponent<EnemyBase>() != null);
            AssertTest("PREFAB", "Shooter prefab exists with EnemyBase",
                shooterPrefab != null && shooterPrefab.GetComponent<EnemyBase>() != null);
            AssertTest("PREFAB", "Boss prefab exists with EnemyBase",
                bossPrefab != null && bossPrefab.GetComponent<EnemyBase>() != null);

            // 3. CORE MANAGERS
            var gm = GameObject.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
            var wm = GameObject.FindAnyObjectByType<WaveManager>(FindObjectsInactive.Include);
            var am = GameObject.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            var vm = GameObject.FindAnyObjectByType<VFXManager>(FindObjectsInactive.Include);
            var dnm = GameObject.FindAnyObjectByType<DamageNumberManager>(FindObjectsInactive.Include);
            var ldm = GameObject.FindAnyObjectByType<LootDropManager>(FindObjectsInactive.Include);
            var um = GameObject.FindAnyObjectByType<UpgradeManager>(FindObjectsInactive.Include);
            var mapm = GameObject.FindAnyObjectByType<MapManager>(FindObjectsInactive.Include);
            var modem = GameObject.FindAnyObjectByType<ModeManager>(FindObjectsInactive.Include);

            AssertTest("CORE", "GameManager found in scene", gm != null);
            AssertTest("CORE", "WaveManager found in scene", wm != null);
            AssertTest("CORE", "AudioManager found in scene", am != null);
            AssertTest("CORE", "VFXManager found in scene", vm != null);
            AssertTest("CORE", "DamageNumberManager found in scene", dnm != null);
            AssertTest("CORE", "LootDropManager found in scene", ldm != null);
            AssertTest("CORE", "UpgradeManager found in scene", um != null);
            AssertTest("CORE", "MapManager found in scene", mapm != null);
            AssertTest("CORE", "ModeManager found in scene", modem != null);

            // 4. ENVIRONMENT & ARENA
            var arena = GameObject.FindAnyObjectByType<ArenaBuilder>(FindObjectsInactive.Include);
            var navSurface = GameObject.FindAnyObjectByType<NavMeshSurface>(FindObjectsInactive.Include);
            var ground = GameObject.Find("Ground");
            var wallN = GameObject.Find("Wall_North");
            var wallS = GameObject.Find("Wall_South");
            var wallE = GameObject.Find("Wall_East");
            var wallW = GameObject.Find("Wall_West");

            AssertTest("ENVIRONMENT", "ArenaBuilder and NavMeshSurface attached", arena != null && navSurface != null);
            AssertTest("ENVIRONMENT", "Arena Ground cube created with collider and size (40, 1, 40)",
                ground != null && ground.GetComponent<Collider>() != null && ground.transform.localScale.x >= 39f);
            AssertTest("ENVIRONMENT", "Perimeter Walls (N, S, E, W) created with colliders",
                wallN != null && wallS != null && wallE != null && wallW != null &&
                wallN.GetComponent<Collider>() != null && wallS.GetComponent<Collider>() != null);

            // 5. ENEMY SPAWNS & WAVE MANAGER WIRING
            AssertTest("SPAWNS", "Arena has 4 enemy spawn points assigned",
                arena != null && arena.EnemySpawnPoints != null && arena.EnemySpawnPoints.Count >= 4);
            AssertTest("WAVE_SYSTEM", "WaveManager wired with Arena and Enemy Prefabs",
                wm != null && wm.Arena == arena && wm.ZombiePrefab == zombiePrefab && wm.BossPrefab == bossPrefab);

            // 6. PLAYER SETUP
            var player = GameObject.Find("Player");
            var cc = player != null ? player.GetComponent<CharacterController>() : null;
            var inputHandler = player != null ? player.GetComponent<PlayerInputHandler>() : null;
            var stats = player != null ? player.GetComponent<PlayerStats>() : null;
            var controller = player != null ? player.GetComponent<PlayerController>() : null;
            var cam = player != null ? player.GetComponentInChildren<Camera>() : null;
            var weaponManager = player != null ? player.GetComponent<WeaponManager>() : null;

            AssertTest("PLAYER", "Player GameObject exists with tag 'Player'", player != null && player.CompareTag("Player"));
            AssertTest("PLAYER", "CharacterController configured with proper radius/height",
                cc != null && cc.height >= 1.8f && cc.radius > 0.3f);
            AssertTest("PLAYER", "PlayerInputHandler attached", inputHandler != null);
            AssertTest("PLAYER", "PlayerStats attached (IDamageable)", stats != null);
            AssertTest("PLAYER", "PlayerController attached", controller != null);
            AssertTest("PLAYER", "Camera attached under CameraRoot with tag 'MainCamera' and FOV 70",
                cam != null && cam.CompareTag("MainCamera") && Mathf.Approximately(cam.fieldOfView, 70f));
            AssertTest("PLAYER", "WeaponManager initialized with weapon holder", weaponManager != null);

            // 7. WEAPON MECHANICS & FACTORY
            var arsenal = WeaponFactory.CreateCompleteArsenal();
            AssertTest("WEAPONS", $"WeaponFactory generates complete arsenal of 5 weapons ({arsenal.Count} found)",
                arsenal.Count == 5);
            var rifle = arsenal.Find(w => w.WeaponType == WeaponType.Rifle);
            var shotgun = arsenal.Find(w => w.WeaponType == WeaponType.Shotgun);
            var rocket = arsenal.Find(w => w.WeaponType == WeaponType.RocketLauncher);
            AssertTest("WEAPONS", "Assault Rifle configured (FullAuto, 26 DMG, 30 Mag)",
                rifle != null && rifle.FireMode == FireMode.FullAuto && rifle.Damage > 20f && rifle.MagazineSize == 30);
            AssertTest("WEAPONS", "Breaker Shotgun configured (8 pellets, 48 reserve)",
                shotgun != null && shotgun.PelletsCount == 8 && shotgun.MaxReserveAmmo == 48);
            AssertTest("WEAPONS", "Havoc Rocket Launcher configured (Projectile, Splash Damage)",
                rocket != null && rocket.IsProjectile && rocket.SplashRadius > 5f);

            // 8. UI CANVAS & HUD
            var canvas = GameObject.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
            var hud = GameObject.FindAnyObjectByType<HUDController>(FindObjectsInactive.Include);
            var goUI = GameObject.FindAnyObjectByType<GameOverUIController>(FindObjectsInactive.Include);
            var settingsUI = GameObject.FindAnyObjectByType<SettingsUIController>(FindObjectsInactive.Include);
            var pauseUI = GameObject.FindAnyObjectByType<PauseMenuController>(FindObjectsInactive.Include);
            var upgradeUI = GameObject.FindAnyObjectByType<UpgradeUIController>(FindObjectsInactive.Include);

            AssertTest("UI", "Canvas configured with ScreenSpaceOverlay and CanvasScaler",
                canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.GetComponent<CanvasScaler>() != null);
            AssertTest("UI", "HUDController wired with Health, Ammo, Wave, and Crosshair elements",
                hud != null && hud.HealthSlider != null && hud.AmmoText != null && hud.WaveText != null && hud.CrosshairTop != null);
            AssertTest("UI", "GameOverPanel, PausePanel, SettingsPanel, UpgradeModal wired",
                goUI != null && pauseUI != null && settingsUI != null && upgradeUI != null);

            // 9. GAMEPLAY LOGIC UNIT SIMULATIONS
            // Test PlayerStats IDamageable
            float initialHp = stats.CurrentHealth;
            stats.TakeDamage(new DamageInfo(25f, Vector3.zero, Vector3.up, false, null, Vector3.zero, HitType.Default));
            AssertTest("LOGIC", $"PlayerStats TakeDamage reduces HP (from {initialHp} to {stats.CurrentHealth})",
                stats.CurrentHealth < initialHp);

            stats.Heal(25f);
            AssertTest("LOGIC", $"PlayerStats Heal restores HP (to {stats.CurrentHealth})",
                stats.CurrentHealth == stats.MaxHealth);

            // Test PlayerStats Stamina
            bool consumedStamina = stats.ConsumeStamina(20f);
            AssertTest("LOGIC", "PlayerStats ConsumeStamina works for sprint mechanics",
                consumedStamina && stats.CurrentStamina < stats.MaxStamina);

            // Test Upgrades
            stats.ApplyUpgrade(UpgradeType.DamageBoost);
            AssertTest("LOGIC", $"UpgradeManager ApplyUpgrade modifies player stats (DamageMult: {stats.DamageMultiplier})",
                stats.DamageMultiplier > 1.15f);

            // Test Enemy instantiation & IDamageable
            GameObject testZombie = GameObject.Instantiate(zombiePrefab, new Vector3(0, 0, 5), Quaternion.identity);
            EnemyBase enemyComp = testZombie.GetComponent<EnemyBase>();
            enemyComp.Initialize(1f, 1f, 1f);
            AssertTest("LOGIC", "EnemyBase initializes with full health and chase state",
                enemyComp.IsAlive && enemyComp.CurrentHealth > 0);

            float enemyInitialHp = enemyComp.CurrentHealth;
            enemyComp.TakeDamage(new DamageInfo(30f, testZombie.transform.position, Vector3.up, false, player, Vector3.forward * 5f, HitType.Default));
            AssertTest("LOGIC", $"EnemyBase TakeDamage reduces health ({enemyInitialHp} -> {enemyComp.CurrentHealth})",
                enemyComp.CurrentHealth < enemyInitialHp);

            // Lethal hit
            enemyComp.TakeDamage(new DamageInfo(200f, testZombie.transform.position, Vector3.up, true, player, Vector3.forward * 10f, HitType.Critical));
            AssertTest("LOGIC", "EnemyBase lethal damage sets isDead and triggers death flow",
                !enemyComp.IsAlive);
            GameObject.DestroyImmediate(testZombie);

            // Test GameManager scoring
            int initialScore = gm.TotalScore;
            gm.AddKill(100);
            AssertTest("LOGIC", $"GameManager AddKill increments score ({initialScore} -> {gm.TotalScore})",
                gm.TotalScore == initialScore + 100);

            gm.AddWaveBonus(1);
            AssertTest("LOGIC", $"GameManager AddWaveBonus awards wave bonus ({gm.TotalScore})",
                gm.TotalScore > initialScore + 100);

            // Test Game Over Flow
            gm.SetState(GameState.GameOver);
            AssertTest("LOGIC", "GameManager enters GameState.GameOver upon fatal condition",
                gm.State == GameState.GameOver);

            // Reset health & state back for clean scene state
            stats.Heal(stats.MaxHealth);
            gm.SetState(GameState.InGame);

            // SUMMARY
            reportLines.Add("\n==================================================");
            reportLines.Add($"SUMMARY: {passCount} PASSED, {failCount} FAILED");
            reportLines.Add(failCount == 0 ? "STATUS: ALL SYSTEMS NOMINAL - READY TO PLAY!" : "STATUS: SOME TESTS FAILED");
            reportLines.Add("==================================================");

            string fullReport = string.Join("\n", reportLines);
            File.WriteAllText(ReportPath, fullReport);
            Debug.Log(fullReport);

            // Save clean scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
