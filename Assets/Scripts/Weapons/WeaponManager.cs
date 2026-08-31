using System;
using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Player;

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

            for (int i = 0; i < dataList.Count; i++)
            {
                WeaponDataSO data = dataList[i];
                if (data == null) continue;

                GameObject weaponObj = new GameObject($"Weapon_{data.WeaponName}");
                weaponObj.transform.SetParent(weaponHolderTransform, false);
                weaponObj.transform.localPosition = Vector3.zero;
                weaponObj.transform.localRotation = Quaternion.identity;

                // Procedural weapon visual representation
                BuildWeaponVisualMesh(weaponObj, data);

                Weapon weapon = weaponObj.AddComponent<Weapon>();
                weapon.Initialize(data, playerCamera);
                _weapons.Add(weapon);

                weaponObj.SetActive(false);
            }

            if (_weapons.Count > 0)
            {
                SelectWeapon(0);
            }
        }

        private void BuildWeaponVisualMesh(GameObject root, WeaponDataSO data)
        {
            // Gun Barrel / Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Model_Body";
            body.transform.SetParent(root.transform, false);
            
            // Adjust proportions based on weapon type
            switch (data.WeaponType)
            {
                case WeaponType.Rifle:
                    body.transform.localScale = new Vector3(0.06f, 0.10f, 0.55f);
                    break;
                case WeaponType.SMG:
                    body.transform.localScale = new Vector3(0.05f, 0.09f, 0.35f);
                    break;
                case WeaponType.Sniper:
                    body.transform.localScale = new Vector3(0.06f, 0.11f, 0.85f);
                    break;
                case WeaponType.Shotgun:
                    body.transform.localScale = new Vector3(0.08f, 0.12f, 0.50f);
                    break;
                case WeaponType.RocketLauncher:
                    body.transform.localScale = new Vector3(0.12f, 0.14f, 0.70f);
                    break;
            }

            Destroy(body.GetComponent<Collider>());
            var mr = body.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = data.WeaponColor;
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.8f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.6f);
            mr.material = mat;
        }

        private void Update()
        {
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
