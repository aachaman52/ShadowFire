using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.SaveSystem
{
    [Serializable]
    public class WeaponUpgradeData
    {
        public WeaponType Type;
        public bool Unlocked = true;
        public int DamageLevel = 0;
        public int FireRateLevel = 0;
        public int MagazineLevel = 0;
        public int ReloadLevel = 0;

        public WeaponUpgradeData() { }

        public WeaponUpgradeData(WeaponType type, bool unlocked = true)
        {
            Type = type;
            Unlocked = unlocked;
            DamageLevel = 0;
            FireRateLevel = 0;
            MagazineLevel = 0;
            ReloadLevel = 0;
        }
    }

    [Serializable]
    public class GameSaveData
    {
        [Header("Legacy / Run Stats")]
        public int HighScore = 0;
        public int HighestWave = 1;
        public int TotalLifetimeKills = 0;

        [Header("Player Progression")]
        public int PlayerLevel = 1;
        public float CurrentXP = 0f;
        public float TotalXP = 0f;
        public int Credits = 1200; // Starter credits

        [Header("Mission Unlocks")]
        public int HighestLevelUnlocked = 1;
        public List<int> CompletedLevelIDs = new List<int>();

        [Header("Player Attribute Upgrades (Max 10)")]
        public int HealthUpgradeLevel = 0;
        public int ArmorUpgradeLevel = 0;
        public int StaminaUpgradeLevel = 0;
        public int MovementUpgradeLevel = 0;

        [Header("Weapon Upgrades")]
        public List<WeaponUpgradeData> WeaponUpgrades = new List<WeaponUpgradeData>();

        [Header("Loadout Selection")]
        public WeaponType SelectedPrimaryWeapon = WeaponType.Rifle;
        public WeaponType SelectedSecondaryWeapon = WeaponType.Shotgun;

        [Header("Settings")]
        public float MouseSensitivity = 1.8f;
        public float FieldOfView = 75f;
        public float MasterVolume = 1.0f;
        public float MusicVolume = 0.7f;
        public float SfxVolume = 1.0f;
        public int QualityLevel = 2;
        public bool IsFullscreen = true;

        public WeaponUpgradeData GetWeaponData(WeaponType type)
        {
            if (WeaponUpgrades == null) WeaponUpgrades = new List<WeaponUpgradeData>();
            var data = WeaponUpgrades.Find(w => w.Type == type);
            if (data == null)
            {
                data = new WeaponUpgradeData(type, true);
                WeaponUpgrades.Add(data);
            }
            return data;
        }
    }

    public static class SaveSystem
    {
        private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "shadowfire_save.json");
        private static GameSaveData _cachedData;

        public static GameSaveData Load()
        {
            if (_cachedData != null) return _cachedData;

            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    _cachedData = JsonUtility.FromJson<GameSaveData>(json);
                    EnsureDefaultCollections(_cachedData);
                    return _cachedData;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to load save file: {e.Message}. Generating new save data.");
                }
            }

            _cachedData = new GameSaveData();
            EnsureDefaultCollections(_cachedData);
            Save(_cachedData);
            return _cachedData;
        }

        private static void EnsureDefaultCollections(GameSaveData data)
        {
            if (data.CompletedLevelIDs == null) data.CompletedLevelIDs = new List<int>();
            if (data.WeaponUpgrades == null) data.WeaponUpgrades = new List<WeaponUpgradeData>();

            // Ensure all 5 weapon types exist
            WeaponType[] allTypes = new WeaponType[] { WeaponType.Rifle, WeaponType.SMG, WeaponType.Sniper, WeaponType.Shotgun, WeaponType.RocketLauncher };
            foreach (var type in allTypes)
            {
                if (!data.WeaponUpgrades.Exists(w => w.Type == type))
                {
                    data.WeaponUpgrades.Add(new WeaponUpgradeData(type, true));
                }
            }
        }

        public static void Save(GameSaveData data)
        {
            _cachedData = data;
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write save file: {e.Message}");
            }
        }

        public static void SaveGameStats(int score, int wave, int kills)
        {
            GameSaveData data = Load();
            data.HighScore = Mathf.Max(data.HighScore, score);
            data.HighestWave = Mathf.Max(data.HighestWave, wave);
            data.TotalLifetimeKills += kills;
            Save(data);
        }
    }
}
