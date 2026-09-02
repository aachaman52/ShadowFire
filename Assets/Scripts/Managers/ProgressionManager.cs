using System;
using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.SaveSystem;

namespace ShadowFire.Managers
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        public const int MaxAttributeLevel = 10;
        public const int MaxWeaponUpgradeLevel = 10;

        public event Action OnProgressionUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public GameSaveData Data => SaveSystem.SaveSystem.Load();

        public void AddXP(float amount)
        {
            var data = Data;
            data.CurrentXP += amount;
            data.TotalXP += amount;

            while (data.CurrentXP >= GetXpRequiredForLevel(data.PlayerLevel))
            {
                data.CurrentXP -= GetXpRequiredForLevel(data.PlayerLevel);
                data.PlayerLevel++;
            }

            SaveSystem.SaveSystem.Save(data);
            OnProgressionUpdated?.Invoke();
        }

        public float GetXpRequiredForLevel(int level)
        {
            return Mathf.Round(100f * Mathf.Pow(1.35f, Mathf.Max(1, level) - 1));
        }

        public void AddCredits(int amount)
        {
            var data = Data;
            data.Credits += amount;
            SaveSystem.SaveSystem.Save(data);
            OnProgressionUpdated?.Invoke();
        }

        public bool TrySpendCredits(int amount)
        {
            var data = Data;
            if (data.Credits >= amount)
            {
                data.Credits -= amount;
                SaveSystem.SaveSystem.Save(data);
                OnProgressionUpdated?.Invoke();
                return true;
            }
            return false;
        }

        // ================= ATTRIBUTE UPGRADES =================

        public int GetAttributeLevel(AttributeType type)
        {
            var data = Data;
            switch (type)
            {
                case AttributeType.Health: return data.HealthUpgradeLevel;
                case AttributeType.Armor: return data.ArmorUpgradeLevel;
                case AttributeType.Stamina: return data.StaminaUpgradeLevel;
                case AttributeType.Movement: return data.MovementUpgradeLevel;
                default: return 0;
            }
        }

        public int GetAttributeCost(AttributeType type)
        {
            int lvl = GetAttributeLevel(type);
            if (lvl >= MaxAttributeLevel) return -1;
            // Cost scale: 400 + level * 200
            return 400 + (lvl * 200);
        }

        public float GetAttributeCurrentValue(AttributeType type)
        {
            int lvl = GetAttributeLevel(type);
            switch (type)
            {
                case AttributeType.Health: return 100f + (lvl * 15f);
                case AttributeType.Armor: return lvl * 5f;
                case AttributeType.Stamina: return 100f + (lvl * 15f);
                case AttributeType.Movement: return 1.0f + (lvl * 0.05f);
                default: return 1f;
            }
        }

        public float GetAttributeNextValue(AttributeType type)
        {
            int lvl = Mathf.Min(MaxAttributeLevel, GetAttributeLevel(type) + 1);
            switch (type)
            {
                case AttributeType.Health: return 100f + (lvl * 15f);
                case AttributeType.Armor: return lvl * 5f;
                case AttributeType.Stamina: return 100f + (lvl * 15f);
                case AttributeType.Movement: return 1.0f + (lvl * 0.05f);
                default: return 1f;
            }
        }

        public bool UpgradeAttribute(AttributeType type)
        {
            int lvl = GetAttributeLevel(type);
            if (lvl >= MaxAttributeLevel) return false;

            int cost = GetAttributeCost(type);
            if (!TrySpendCredits(cost)) return false;

            var data = Data;
            switch (type)
            {
                case AttributeType.Health: data.HealthUpgradeLevel++; break;
                case AttributeType.Armor: data.ArmorUpgradeLevel++; break;
                case AttributeType.Stamina: data.StaminaUpgradeLevel++; break;
                case AttributeType.Movement: data.MovementUpgradeLevel++; break;
            }

            SaveSystem.SaveSystem.Save(data);
            OnProgressionUpdated?.Invoke();
            return true;
        }

        // ================= WEAPON UPGRADES =================

        public int GetWeaponUpgradeLevel(WeaponType weapon, WeaponUpgradeType upgrade)
        {
            var wData = Data.GetWeaponData(weapon);
            switch (upgrade)
            {
                case WeaponUpgradeType.Damage: return wData.DamageLevel;
                case WeaponUpgradeType.FireRate: return wData.FireRateLevel;
                case WeaponUpgradeType.Magazine: return wData.MagazineLevel;
                case WeaponUpgradeType.Reload: return wData.ReloadLevel;
                default: return 0;
            }
        }

        public int GetWeaponUpgradeCost(WeaponType weapon, WeaponUpgradeType upgrade)
        {
            int lvl = GetWeaponUpgradeLevel(weapon, upgrade);
            if (lvl >= MaxWeaponUpgradeLevel) return -1;
            return 350 + (lvl * 175);
        }

        public float GetWeaponCurrentMultiplier(WeaponType weapon, WeaponUpgradeType upgrade)
        {
            int lvl = GetWeaponUpgradeLevel(weapon, upgrade);
            switch (upgrade)
            {
                case WeaponUpgradeType.Damage: return 1.0f + (lvl * 0.12f);
                case WeaponUpgradeType.FireRate: return 1.0f + (lvl * 0.08f);
                case WeaponUpgradeType.Magazine: return 1.0f + (lvl * 0.15f);
                case WeaponUpgradeType.Reload: return Mathf.Max(0.4f, 1.0f - (lvl * 0.06f));
                default: return 1f;
            }
        }

        public float GetWeaponNextMultiplier(WeaponType weapon, WeaponUpgradeType upgrade)
        {
            int lvl = Mathf.Min(MaxWeaponUpgradeLevel, GetWeaponUpgradeLevel(weapon, upgrade) + 1);
            switch (upgrade)
            {
                case WeaponUpgradeType.Damage: return 1.0f + (lvl * 0.12f);
                case WeaponUpgradeType.FireRate: return 1.0f + (lvl * 0.08f);
                case WeaponUpgradeType.Magazine: return 1.0f + (lvl * 0.15f);
                case WeaponUpgradeType.Reload: return Mathf.Max(0.4f, 1.0f - (lvl * 0.06f));
                default: return 1f;
            }
        }

        public bool UpgradeWeapon(WeaponType weapon, WeaponUpgradeType upgrade)
        {
            int lvl = GetWeaponUpgradeLevel(weapon, upgrade);
            if (lvl >= MaxWeaponUpgradeLevel) return false;

            int cost = GetWeaponUpgradeCost(weapon, upgrade);
            if (!TrySpendCredits(cost)) return false;

            var data = Data;
            var wData = data.GetWeaponData(weapon);
            switch (upgrade)
            {
                case WeaponUpgradeType.Damage: wData.DamageLevel++; break;
                case WeaponUpgradeType.FireRate: wData.FireRateLevel++; break;
                case WeaponUpgradeType.Magazine: wData.MagazineLevel++; break;
                case WeaponUpgradeType.Reload: wData.ReloadLevel++; break;
            }

            SaveSystem.SaveSystem.Save(data);
            OnProgressionUpdated?.Invoke();
            return true;
        }

        // ================= LOADOUT =================

        public void SetSelectedLoadout(WeaponType primary, WeaponType secondary)
        {
            var data = Data;
            data.SelectedPrimaryWeapon = primary;
            data.SelectedSecondaryWeapon = secondary;
            SaveSystem.SaveSystem.Save(data);
            OnProgressionUpdated?.Invoke();
        }

        // ================= MISSION UNLOCKS =================

        public bool IsMissionUnlocked(int missionId)
        {
            if (missionId <= 1) return true;
            return Data.HighestLevelUnlocked >= missionId;
        }

        public bool IsMissionCompleted(int missionId)
        {
            return Data.CompletedLevelIDs.Contains(missionId);
        }

        public void CompleteMission(int missionId, int xpReward, int creditReward)
        {
            var data = Data;
            if (!data.CompletedLevelIDs.Contains(missionId))
            {
                data.CompletedLevelIDs.Add(missionId);
            }

            // Unlock next mission
            data.HighestLevelUnlocked = Mathf.Max(data.HighestLevelUnlocked, missionId + 1);

            SaveSystem.SaveSystem.Save(data);

            AddXP(xpReward);
            AddCredits(creditReward);
        }
    }
}
