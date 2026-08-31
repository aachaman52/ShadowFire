using System;
using System.Collections;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.Audio;
using ShadowFire.Effects;

namespace ShadowFire.Weapons
{
    public class Weapon : MonoBehaviour
    {
        [Header("Data Configuration")]
        [SerializeField] private WeaponDataSO weaponData;

        [Header("Transforms & Emitters")]
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Transform weaponModelTransform;

        [Header("State")]
        [SerializeField] private int currentAmmo;
        [SerializeField] private int currentReserve;
        [SerializeField] private bool isReloading = false;

        private float _lastFireTime;
        private Camera _playerCamera;
        private Vector3 _modelInitialLocalPos;
        private Quaternion _modelInitialLocalRot;
        private Vector2 _currentRecoil;
        private Vector2 _targetRecoil;

        public WeaponDataSO Data => weaponData;
        public int CurrentAmmo => currentAmmo;
        public int CurrentReserve => currentReserve;
        public int MaxMagazine => weaponData != null ? Mathf.RoundToInt(weaponData.MagazineSize * (PlayerStats.Instance != null ? PlayerStats.Instance.MagazineMultiplier : 1f)) : 0;
        public bool IsReloading => isReloading;
        public WeaponType Type => weaponData != null ? weaponData.WeaponType : WeaponType.Rifle;

        public event Action<int, int> OnAmmoChanged;
        public event Action OnFired;
        public event Action OnReloadStarted;
        public event Action OnReloadFinished;

        private void Awake()
        {
            if (weaponModelTransform == null) weaponModelTransform = transform;
            _modelInitialLocalPos = weaponModelTransform.localPosition;
            _modelInitialLocalRot = weaponModelTransform.localRotation;
        }

        public void Initialize(WeaponDataSO data, Camera playerCam)
        {
            weaponData = data;
            _playerCamera = playerCam;
            if (weaponData != null)
            {
                currentAmmo = MaxMagazine;
                currentReserve = weaponData.MaxReserveAmmo;
            }

            if (muzzlePoint == null)
            {
                GameObject mp = new GameObject("MuzzlePoint");
                mp.transform.SetParent(transform);
                mp.transform.localPosition = new Vector3(0, 0, 0.7f);
                muzzlePoint = mp.transform;
            }

            OnAmmoChanged?.Invoke(currentAmmo, currentReserve);
        }

        private void Update()
        {
            HandleRecoilRecovery();
            HandleWeaponSway();
        }

        public bool TryFire()
        {
            if (weaponData == null || isReloading) return false;

            float fireInterval = 1f / (weaponData.FireRate * (PlayerStats.Instance != null ? PlayerStats.Instance.FireRateMultiplier : 1f));
            if (Time.time - _lastFireTime < fireInterval) return false;

            if (currentAmmo <= 0)
            {
                // Auto reload if out of ammo
                TryReload();
                return false;
            }

            _lastFireTime = Time.time;
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, currentReserve);

            ExecuteFire();
            ApplyRecoil();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGunshot(weaponData.WeaponType);
            }
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.AddTrauma(weaponData.CameraShakeIntensity);
            }

            OnFired?.Invoke();
            return true;
        }

        private void ExecuteFire()
        {
            Vector3 fireOrigin = _playerCamera != null ? _playerCamera.transform.position : transform.position;
            Vector3 forwardDirection = _playerCamera != null ? _playerCamera.transform.forward : transform.forward;

            // Spawn Muzzle Flash
            if (VFXManager.Instance != null && muzzlePoint != null)
            {
                VFXManager.Instance.SpawnMuzzleFlash(muzzlePoint.position, muzzlePoint.rotation, weaponData.MuzzleFlashColor);
            }

            if (weaponData.IsProjectile)
            {
                // Projectile Weapon (Rocket)
                GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.transform.localScale = Vector3.one * 0.35f;
                projObj.transform.position = muzzlePoint != null ? muzzlePoint.position : fireOrigin;
                
                var projMat = ProceduralMeshGenerator.GetMaterial("glowred");
                projObj.GetComponent<MeshRenderer>().material = projMat;
                Destroy(projObj.GetComponent<Collider>());

                Projectile proj = projObj.AddComponent<Projectile>();
                float finalDamage = weaponData.Damage * (PlayerStats.Instance != null ? PlayerStats.Instance.DamageMultiplier : 1f);
                proj.Initialize(forwardDirection, weaponData.ProjectileSpeed, finalDamage, PlayerStats.Instance != null ? PlayerStats.Instance.gameObject : gameObject, true, weaponData.SplashRadius, weaponData.KnockbackForce);
                return;
            }

            // Hitscan / Pellets
            int pellets = Mathf.Max(1, weaponData.PelletsCount);
            for (int i = 0; i < pellets; i++)
            {
                Vector3 spreadDir = forwardDirection;
                if (weaponData.Spread > 0f)
                {
                    spreadDir += _playerCamera.transform.right * UnityEngine.Random.Range(-weaponData.Spread, weaponData.Spread);
                    spreadDir += _playerCamera.transform.up * UnityEngine.Random.Range(-weaponData.Spread, weaponData.Spread);
                    spreadDir.Normalize();
                }

                Ray ray = new Ray(fireOrigin, spreadDir);
                RaycastHit[] hits = Physics.RaycastAll(ray, weaponData.Range, ~LayerMask.GetMask("Ignore Raycast", "Player"));
                
                // Sort hits by distance for penetration
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                int piercesRemaining = weaponData.PiercingCount;
                Vector3 tracerEnd = fireOrigin + spreadDir * weaponData.Range;

                for (int h = 0; h < hits.Length; h++)
                {
                    RaycastHit hit = hits[h];
                    tracerEnd = hit.point;

                    // Damage target
                    IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                    if (damageable != null && damageable.IsAlive)
                    {
                        bool isCrit = UnityEngine.Random.value < (PlayerStats.Instance != null ? PlayerStats.Instance.CriticalChance : 0.05f);
                        float baseDamage = weaponData.Damage * (PlayerStats.Instance != null ? PlayerStats.Instance.DamageMultiplier : 1f);
                        float finalDamage = isCrit ? baseDamage * 2.0f : baseDamage;

                        DamageInfo dInfo = new DamageInfo(finalDamage, hit.point, hit.normal, isCrit, PlayerStats.Instance != null ? PlayerStats.Instance.gameObject : gameObject, spreadDir * weaponData.KnockbackForce, isCrit ? HitType.Critical : HitType.Default);
                        damageable.TakeDamage(dInfo);

                        if (PlayerStats.Instance != null)
                        {
                            PlayerStats.Instance.ApplyLifesteal(finalDamage);
                            if (PlayerStats.Instance.HasExplosiveAmmo)
                            {
                                if (VFXManager.Instance != null) VFXManager.Instance.SpawnExplosion(hit.point, 0.4f);
                            }
                        }

                        if (VFXManager.Instance != null)
                        {
                            VFXManager.Instance.SpawnBloodSplatter(hit.point, hit.normal);
                        }
                    }
                    else
                    {
                        // Hit environmental surface
                        if (VFXManager.Instance != null)
                        {
                            VFXManager.Instance.SpawnHitSparks(hit.point, hit.normal);
                        }
                        break; // Stop piercing on static geometry
                    }

                    piercesRemaining--;
                    if (piercesRemaining <= 0) break;
                }

                // Bullet Tracer
                if (VFXManager.Instance != null)
                {
                    Vector3 mPos = muzzlePoint != null ? muzzlePoint.position : fireOrigin;
                    VFXManager.Instance.SpawnTracer(mPos, tracerEnd, weaponData.TracerColor);
                }
            }
        }

        public bool TryReload()
        {
            if (isReloading || weaponData == null) return false;
            int maxMag = MaxMagazine;
            if (currentAmmo >= maxMag || currentReserve <= 0) return false;

            StartCoroutine(ReloadRoutine());
            return true;
        }

        private IEnumerator ReloadRoutine()
        {
            isReloading = true;
            OnReloadStarted?.Invoke();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayReload();
            }

            float reloadDuration = weaponData.ReloadTime * (PlayerStats.Instance != null ? PlayerStats.Instance.ReloadSpeedMultiplier : 1f);
            yield return new WaitForSeconds(reloadDuration);

            int maxMag = MaxMagazine;
            int needed = maxMag - currentAmmo;
            int toLoad = Mathf.Min(needed, currentReserve);

            currentAmmo += toLoad;
            currentReserve -= toLoad;

            isReloading = false;
            OnAmmoChanged?.Invoke(currentAmmo, currentReserve);
            OnReloadFinished?.Invoke();
        }

        public void AddReserveAmmo(int amount)
        {
            if (weaponData == null) return;
            currentReserve = Mathf.Min(weaponData.MaxReserveAmmo, currentReserve + amount);
            OnAmmoChanged?.Invoke(currentAmmo, currentReserve);
        }

        private void ApplyRecoil()
        {
            _targetRecoil += new Vector2(
                UnityEngine.Random.Range(-weaponData.RecoilKick.x, weaponData.RecoilKick.x),
                weaponData.RecoilKick.y
            );
        }

        private void HandleRecoilRecovery()
        {
            _targetRecoil = Vector2.Lerp(_targetRecoil, Vector2.zero, Time.deltaTime * weaponData.RecoilRecoverySpeed);
            _currentRecoil = Vector2.Lerp(_currentRecoil, _targetRecoil, Time.deltaTime * 20f);

            if (_playerCamera != null)
            {
                // Recoil subtly pushes camera
                _playerCamera.transform.localRotation *= Quaternion.Euler(-_currentRecoil.y * 0.1f, _currentRecoil.x * 0.1f, 0);
            }
        }

        private void HandleWeaponSway()
        {
            if (PlayerInputHandler.Instance == null || weaponModelTransform == null) return;

            Vector2 look = PlayerInputHandler.Instance.LookInput;
            float swayX = -look.x * 0.02f;
            float swayY = -look.y * 0.02f;

            swayX = Mathf.Clamp(swayX, -0.06f, 0.06f);
            swayY = Mathf.Clamp(swayY, -0.06f, 0.06f);

            Vector3 targetPos = _modelInitialLocalPos + new Vector3(swayX, swayY, 0);
            weaponModelTransform.localPosition = Vector3.Lerp(weaponModelTransform.localPosition, targetPos, Time.deltaTime * 8f);
        }
    }
}
