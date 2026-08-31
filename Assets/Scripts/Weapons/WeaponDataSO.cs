using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Weapons
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ShadowFire/Weapon Data")]
    public class WeaponDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string WeaponName = "Assault Rifle";
        public WeaponType WeaponType = WeaponType.Rifle;
        public FireMode FireMode = FireMode.FullAuto;

        [Header("Combat Stats")]
        public float Damage = 25f;
        public float FireRate = 10f; // Rounds per second
        public int MagazineSize = 30;
        public int MaxReserveAmmo = 180;
        public float ReloadTime = 1.8f;
        public float Range = 100f;
        public float Spread = 0.02f;
        public int PelletsCount = 1;
        public int PiercingCount = 1;

        [Header("Projectile & Explosive")]
        public bool IsProjectile = false;
        public float ProjectileSpeed = 60f;
        public float SplashRadius = 0f;
        public float SplashDamage = 0f;
        public float KnockbackForce = 10f;

        [Header("Recoil & Camera Shake")]
        public Vector2 RecoilKick = new Vector2(0.8f, 1.2f);
        public float RecoilRecoverySpeed = 6f;
        public float CameraShakeIntensity = 0.3f;

        [Header("Aim & Scope")]
        public float NormalFOV = 75f;
        public float ZoomFOV = 60f;
        public bool HasScopeOverlay = false;

        [Header("Visuals & Colors")]
        public Color WeaponColor = new Color(0.2f, 0.22f, 0.25f);
        public Color MuzzleFlashColor = new Color(1f, 0.7f, 0.2f);
        public Color TracerColor = new Color(1f, 0.85f, 0.4f);
    }
}
