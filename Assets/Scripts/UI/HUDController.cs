using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.Weapons;
using ShadowFire.Managers;
using ShadowFire.Enemies;

namespace ShadowFire.UI
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("Player Bars")]
        public Slider HealthSlider;
        public TextMeshProUGUI HealthText;
        public Slider ArmorSlider;
        public TextMeshProUGUI ArmorText;
        public Slider StaminaSlider;
        public Slider XpSlider;
        public TextMeshProUGUI LevelText;

        [Header("Weapon & Ammo")]
        public TextMeshProUGUI WeaponNameText;
        public TextMeshProUGUI AmmoText;
        public TextMeshProUGUI ReloadIndicatorText;

        [Header("Wave & Objective")]
        public TextMeshProUGUI WaveText;
        public TextMeshProUGUI EnemiesRemainingText;
        public TextMeshProUGUI CountdownText;
        public TextMeshProUGUI ScoreText;

        [Header("Boss Bar")]
        public GameObject BossBarContainer;
        public Slider BossHealthSlider;
        public TextMeshProUGUI BossNameText;

        [Header("Crosshair & Hitmarker")]
        public RectTransform CrosshairTop;
        public RectTransform CrosshairBottom;
        public RectTransform CrosshairLeft;
        public RectTransform CrosshairRight;
        public Image HitmarkerImage;

        private float _crosshairSpread = 12f;
        private float _targetSpread = 12f;
        private Coroutine _hitmarkerRoutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            // Subscribe to Player Events
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnHealthChanged += UpdateHealth;
                PlayerStats.Instance.OnStaminaChanged += UpdateStamina;
                PlayerStats.Instance.OnXpChanged += UpdateXp;
            }

            // Subscribe to Weapon Events
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.OnWeaponSwitched += HandleWeaponSwitched;
                if (WeaponManager.Instance.ActiveWeapon != null)
                {
                    HandleWeaponSwitched(WeaponManager.Instance.ActiveWeapon);
                }
            }

            // Subscribe to Wave Events
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted += HandleWaveStarted;
                WaveManager.Instance.OnEnemyCountChanged += HandleEnemyCountChanged;
                WaveManager.Instance.OnCountdownTick += HandleCountdownTick;
            }

            // Subscribe to GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += (score) => { if (ScoreText != null) ScoreText.text = $"SCORE: {score:N0}"; };
            }

            if (BossBarContainer != null) BossBarContainer.SetActive(false);
            if (HitmarkerImage != null) HitmarkerImage.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateCrosshairDynamics();
            CheckBossStatus();
        }

        private void UpdateHealth(float current, float max)
        {
            if (HealthSlider != null)
            {
                HealthSlider.maxValue = max;
                HealthSlider.value = current;
            }
            if (HealthText != null)
            {
                HealthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }

            if (PlayerStats.Instance != null && ArmorSlider != null)
            {
                ArmorSlider.value = PlayerStats.Instance.CurrentArmor;
                if (ArmorText != null) ArmorText.text = $"ARMOR: {Mathf.CeilToInt(PlayerStats.Instance.CurrentArmor)}";
            }
        }

        private void UpdateStamina(float current, float max)
        {
            if (StaminaSlider != null)
            {
                StaminaSlider.maxValue = max;
                StaminaSlider.value = current;
            }
        }

        private void UpdateXp(float current, float max, int level)
        {
            if (XpSlider != null)
            {
                XpSlider.maxValue = max;
                XpSlider.value = current;
            }
            if (LevelText != null)
            {
                LevelText.text = $"LVL {level}";
            }
        }

        private void HandleWeaponSwitched(Weapon weapon)
        {
            if (weapon == null) return;

            if (WeaponNameText != null) WeaponNameText.text = weapon.Data.WeaponName.ToUpper();
            UpdateAmmoDisplay(weapon.CurrentAmmo, weapon.CurrentReserve);

            weapon.OnAmmoChanged -= UpdateAmmoDisplay;
            weapon.OnAmmoChanged += UpdateAmmoDisplay;

            weapon.OnReloadStarted -= HandleReloadStarted;
            weapon.OnReloadStarted += HandleReloadStarted;

            weapon.OnReloadFinished -= HandleReloadFinished;
            weapon.OnReloadFinished += HandleReloadFinished;

            if (ReloadIndicatorText != null) ReloadIndicatorText.gameObject.SetActive(weapon.IsReloading);
        }

        private void UpdateAmmoDisplay(int current, int reserve)
        {
            if (AmmoText != null)
            {
                AmmoText.text = $"{current} <size=60%>/ {reserve}</size>";
            }
        }

        private void HandleReloadStarted()
        {
            if (ReloadIndicatorText != null) ReloadIndicatorText.gameObject.SetActive(true);
        }

        private void HandleReloadFinished()
        {
            if (ReloadIndicatorText != null) ReloadIndicatorText.gameObject.SetActive(false);
        }

        private void HandleWaveStarted(int wave)
        {
            if (WaveText != null) WaveText.text = $"WAVE {wave}";
            if (CountdownText != null) CountdownText.gameObject.SetActive(false);
        }

        private void HandleEnemyCountChanged(int remaining, int total)
        {
            if (EnemiesRemainingText != null)
            {
                EnemiesRemainingText.text = $"ENEMIES: {remaining}";
            }
        }

        private void HandleCountdownTick(int secondsRemaining)
        {
            if (CountdownText != null)
            {
                if (secondsRemaining > 0)
                {
                    CountdownText.gameObject.SetActive(true);
                    CountdownText.text = $"NEXT WAVE IN {secondsRemaining}...";
                }
                else
                {
                    CountdownText.gameObject.SetActive(false);
                }
            }
        }

        private void CheckBossStatus()
        {
            if (BossEnemy.ActiveBoss != null && BossEnemy.ActiveBoss.IsAlive)
            {
                if (BossBarContainer != null && !BossBarContainer.activeSelf)
                {
                    BossBarContainer.SetActive(true);
                    if (BossNameText != null) BossNameText.text = BossEnemy.ActiveBoss.EnemyName;
                }

                if (BossHealthSlider != null)
                {
                    BossHealthSlider.maxValue = BossEnemy.ActiveBoss.MaxHealth;
                    BossHealthSlider.value = BossEnemy.ActiveBoss.CurrentHealth;
                }
            }
            else
            {
                if (BossBarContainer != null && BossBarContainer.activeSelf)
                {
                    BossBarContainer.SetActive(false);
                }
            }
        }

        private void UpdateCrosshairDynamics()
        {
            if (CrosshairTop == null) return;

            bool isMoving = PlayerController.Instance != null && PlayerController.Instance.IsMoving;
            bool isSprinting = PlayerController.Instance != null && PlayerController.Instance.IsSprinting;

            _targetSpread = 12f;
            if (isMoving) _targetSpread = 22f;
            if (isSprinting) _targetSpread = 36f;

            _crosshairSpread = Mathf.Lerp(_crosshairSpread, _targetSpread, Time.deltaTime * 10f);

            CrosshairTop.anchoredPosition = new Vector2(0, _crosshairSpread);
            CrosshairBottom.anchoredPosition = new Vector2(0, -_crosshairSpread);
            CrosshairLeft.anchoredPosition = new Vector2(-_crosshairSpread, 0);
            CrosshairRight.anchoredPosition = new Vector2(_crosshairSpread, 0);
        }

        public void TriggerHitmarker(bool isKill = false)
        {
            if (HitmarkerImage == null) return;

            if (_hitmarkerRoutine != null) StopCoroutine(_hitmarkerRoutine);
            _hitmarkerRoutine = StartCoroutine(HitmarkerRoutine(isKill));
        }

        private IEnumerator HitmarkerRoutine(bool isKill)
        {
            HitmarkerImage.gameObject.SetActive(true);
            HitmarkerImage.color = isKill ? new Color(1f, 0.2f, 0.2f, 1f) : new Color(1f, 1f, 1f, 0.9f);

            yield return new WaitForSeconds(0.12f);
            HitmarkerImage.gameObject.SetActive(false);
        }
    }
}
