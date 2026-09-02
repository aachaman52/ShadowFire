using System;
using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.Managers;

namespace ShadowFire.Weapons
{
    public class WeaponManager : MonoBehaviour
    {
        public static WeaponManager Instance { get; private set; }

        [Header("Weapon Configurations")]
        [SerializeField] private List<WeaponDataSO> startingWeaponData = new List<WeaponDataSO>();
        [SerializeField] private Transform weaponHolderTransform;
        [SerializeField] private Camera playerCamera;

        [Header("State")]
        [SerializeField] private int activeWeaponIndex = 0;
        private List<Weapon> _weapons = new List<Weapon>();

        public Weapon ActiveWeapon => _weapons.Count > 0 && activeWeaponIndex < _weapons.Count ? _weapons[activeWeaponIndex] : null;
        public IReadOnlyList<Weapon> Weapons => _weapons;
        public int ActiveWeaponIndex => activeWeaponIndex;

        public event Action<Weapon> OnWeaponSwitched;

        private float _defaultCameraFOV = 75f;
        private float _targetCameraFOV = 75f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera != null) _defaultCameraFOV = playerCamera.fieldOfView;
        }

        private void Start()
        {
            if (_weapons.Count == 0 && startingWeaponData.Count > 0)
            {
                InitializeWeapons(startingWeaponData);
            }
        }

        public void InitializeWeapons(List<WeaponDataSO> dataList)
        {
            // Clear old weapons
            foreach (var w in _weapons)
            {
                if (w != null) Destroy(w.gameObject);
            }
            _weapons.Clear();

            if (weaponHolderTransform == null)
            {
                GameObject holder = new GameObject("WeaponHolder");
                holder.transform.SetParent(playerCamera != null ? playerCamera.transform : transform);
                holder.transform.localPosition = new Vector3(0.28f, -0.22f, 0.45f);
                holder.transform.localRotation = Quaternion.identity;
                weaponHolderTransform = holder.transform;
            }

            var saveData = SaveSystem.SaveSystem.Load();

            for (int i = 0; i < dataList.Count; i++)
            {
                WeaponDataSO originalData = dataList[i];
                if (originalData == null) continue;

                WeaponDataSO data = ScriptableObject.Instantiate(originalData);
                var wUpgrade = saveData.GetWeaponData(data.WeaponType);
                if (wUpgrade != null)
                {
                    data.Damage *= (1.0f + wUpgrade.DamageLevel * 0.12f);
                    data.FireRate *= (1.0f + wUpgrade.FireRateLevel * 0.08f);
                    data.MagazineSize += (wUpgrade.MagazineLevel * 4);
                    data.ReloadTime = Mathf.Max(0.5f, data.ReloadTime * Mathf.Max(0.4f, 1.0f - wUpgrade.ReloadLevel * 0.06f));
                }

                GameObject weaponObj = new GameObject($"Weapon_{data.WeaponName}");
                weaponObj.transform.SetParent(weaponHolderTransform, false);
                weaponObj.transform.localPosition = Vector3.zero;
                weaponObj.transform.localRotation = Quaternion.identity;

                // Build precision multi-part 3D weapon model & animation controller
                var animController = ShadowFire.Models.DetailedWeaponMeshBuilder.BuildWeaponModel(weaponObj, data);

                Weapon weapon = weaponObj.AddComponent<Weapon>();
                weapon.Initialize(data, playerCamera);
                _weapons.Add(weapon);

                weaponObj.SetActive(false);
            }

            int primaryIdx = _weapons.FindIndex(w => w.Data.WeaponType == saveData.SelectedPrimaryWeapon);
            if (primaryIdx >= 0)
            {
                SelectWeapon(primaryIdx);
            }
            else if (_weapons.Count > 0)
            {
                SelectWeapon(0);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.InGame && GameManager.Instance.State != GameState.WaveCountdown) return;
            HandleWeaponInput();
            HandleScopeZoom();
        }

        private void HandleWeaponInput()
        {
            var input = PlayerInputHandler.Instance;
            if (input == null || _weapons.Count == 0) return;

            // Slot switching (1-5)
            if (input.WeaponSlotRequested >= 0 && input.WeaponSlotRequested < _weapons.Count)
            {
                SelectWeapon(input.WeaponSlotRequested);
            }
            // Scroll switching
            else if (Mathf.Abs(input.ScrollDelta) > 0.05f)
            {
                if (input.ScrollDelta > 0)
                {
                    int next = (activeWeaponIndex + 1) % _weapons.Count;
                    SelectWeapon(next);
                }
                else
                {
                    int prev = (activeWeaponIndex - 1 + _weapons.Count) % _weapons.Count;
                    SelectWeapon(prev);
                }
            }

            // Firing
            var current = ActiveWeapon;
            if (current != null)
            {
                if (current.Data.FireMode == FireMode.FullAuto)
                {
                    if (input.IsFiring) current.TryFire();
                }
                else
                {
                    if (input.FireTriggered) current.TryFire();
                }

                // Reload
                if (input.ReloadTriggered)
                {
                    current.TryReload();
                }
            }
        }

        public void SelectWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count) return;

            for (int i = 0; i < _weapons.Count; i++)
            {
                _weapons[i].gameObject.SetActive(i == index);
            }

            activeWeaponIndex = index;
            OnWeaponSwitched?.Invoke(_weapons[activeWeaponIndex]);
        }

        private void HandleScopeZoom()
        {
            if (playerCamera == null || ActiveWeapon == null) return;

            var input = PlayerInputHandler.Instance;
            bool zooming = input != null && input.ScopeHeld;

            _targetCameraFOV = zooming ? ActiveWeapon.Data.ZoomFOV : _defaultCameraFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, _targetCameraFOV, Time.deltaTime * 12f);
        }

        public void RefillAllAmmo()
        {
            foreach (var w in _weapons)
            {
                if (w != null && w.Data != null)
                {
                    w.AddReserveAmmo(w.Data.MaxReserveAmmo);
                }
            }
        }
    }
}
