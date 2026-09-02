using System;
using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Player
{
    public class PlayerStats : MonoBehaviour, IDamageable, IKnockbackable
    {
        public static PlayerStats Instance { get; private set; }

        [Header("Health & Armor")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float armor = 0f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;
        [SerializeField] private float staminaDrainRate = 25f;
        [SerializeField] private float staminaRegenRate = 20f;
        [SerializeField] private float staminaRegenDelay = 1.0f;
        private float _lastStaminaUseTime;

        [Header("Level & XP")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float currentXp = 0f;
        [SerializeField] private float xpRequiredForNextLevel = 100f;

        [Header("Stat Multipliers (Upgrades)")]
        public float DamageMultiplier = 1.0f;
        public float ReloadSpeedMultiplier = 1.0f;
        public float FireRateMultiplier = 1.0f;
        public float MagazineMultiplier = 1.0f;
        public float SprintMultiplier = 1.0f;
        public float CriticalChance = 0.05f;
        public bool HasExplosiveAmmo = false;
        public float LifestealPercent = 0.0f;

        // IDamageable implementation
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float CurrentArmor => armor;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public int CurrentLevel => currentLevel;
        public float CurrentXp => currentXp;
        public float XpRequired => xpRequiredForNextLevel;
        public bool IsAlive => currentHealth > 0;

        public event Action<DamageInfo> OnDamaged;
        public event Action<DamageInfo> OnDied;
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnStaminaChanged;
        public event Action<float, float, int> OnXpChanged;
        public event Action<int> OnLevelUp;

        private CharacterController _characterController;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            _characterController = GetComponent<CharacterController>();
            ReloadStatsFromSave();
        }

        private float _lastDamageTime;

        public void ReloadStatsFromSave()
        {
            var data = SaveSystem.SaveSystem.Load();
            maxHealth = 250f + (data.HealthUpgradeLevel * 35f);
            armor = 20f + (data.ArmorUpgradeLevel * 10f);
            maxStamina = 150f + (data.StaminaUpgradeLevel * 25f);
            SprintMultiplier = 1.0f + (data.MovementUpgradeLevel * 0.05f);

            currentHealth = maxHealth;
            currentStamina = maxStamina;
            currentLevel = data.PlayerLevel;
            currentXp = data.CurrentXP;
            CalculateXpRequirement();
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            OnXpChanged?.Invoke(currentXp, xpRequiredForNextLevel, currentLevel);
        }

        private void Update()
        {
            // Stamina regeneration
            if (Time.time - _lastStaminaUseTime > staminaRegenDelay)
            {
                if (currentStamina < maxStamina)
                {
                    currentStamina = Mathf.Min(maxStamina, currentStamina + 35f * Time.deltaTime);
                    OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                }
            }

            // Passive Health regeneration when out of combat
            if (Time.time - _lastDamageTime > 4.0f && currentHealth < maxHealth && IsAlive)
            {
                currentHealth = Mathf.Min(maxHealth, currentHealth + 6f * Time.deltaTime);
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }

        public bool ConsumeStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                _lastStaminaUseTime = Time.time;
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                return true;
            }
            return false;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive) return;
            _lastDamageTime = Time.time;

            // Armor damage reduction: Effective damage = Damage * 100 / (100 + armor)
            float damageReductionFactor = 100f / (100f + Mathf.Max(0, armor));
            float actualDamage = damageInfo.Amount * damageReductionFactor;

            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
            damageInfo.Amount = actualDamage;

            OnDamaged?.Invoke(damageInfo);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die(damageInfo);
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void AddArmor(float amount)
        {
            armor += amount;
        }

        public void AddXp(float amount)
        {
            if (!IsAlive) return;
            currentXp += amount;
            while (currentXp >= xpRequiredForNextLevel)
            {
                currentXp -= xpRequiredForNextLevel;
                currentLevel++;
                CalculateXpRequirement();
                if (IsAlive)
                {
                    OnLevelUp?.Invoke(currentLevel);
                }
            }
            OnXpChanged?.Invoke(currentXp, xpRequiredForNextLevel, currentLevel);
        }

        private void CalculateXpRequirement()
        {
            // Base 100 * (1.35 ^ (level - 1))
            xpRequiredForNextLevel = Mathf.Round(100f * Mathf.Pow(1.35f, currentLevel - 1));
        }

        public void ApplyUpgrade(UpgradeType upgrade)
        {
            switch (upgrade)
            {
                case UpgradeType.DamageBoost:
                    DamageMultiplier += 0.20f;
                    break;
                case UpgradeType.FasterReload:
                    ReloadSpeedMultiplier = Mathf.Max(0.3f, ReloadSpeedMultiplier - 0.25f);
                    break;
                case UpgradeType.BiggerMagazine:
                    MagazineMultiplier += 0.30f;
                    break;
                case UpgradeType.FasterSprint:
                    SprintMultiplier += 0.20f;
                    break;
                case UpgradeType.MaxHealth:
                    maxHealth += 25f;
                    Heal(25f);
                    break;
                case UpgradeType.ArmorBoost:
                    AddArmor(15f);
                    break;
                case UpgradeType.CriticalChance:
                    CriticalChance += 0.15f;
                    break;
                case UpgradeType.FireRateBoost:
                    FireRateMultiplier += 0.20f;
                    break;
                case UpgradeType.ExplosiveAmmo:
                    HasExplosiveAmmo = true;
                    break;
                case UpgradeType.Lifesteal:
                    LifestealPercent += 0.10f;
                    break;
            }
        }

        public void ApplyLifesteal(float damageDealt)
        {
            if (LifestealPercent > 0 && IsAlive)
            {
                Heal(damageDealt * LifestealPercent);
            }
        }

        public void ApplyKnockback(Vector3 force)
        {
            // Player knockback impulse handled through controller if desired
        }

        private void Die(DamageInfo damageInfo)
        {
            OnDied?.Invoke(damageInfo);
            Debug.Log("[ShadowFire] Player has fallen. Game Over.");
        }
    }
}
