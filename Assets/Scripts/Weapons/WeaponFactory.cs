using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Weapons
{
    public static class WeaponFactory
    {
        public static WeaponDataSO CreateRifleData()
        {
            WeaponDataSO data = ScriptableObject.CreateInstance<WeaponDataSO>();
            data.WeaponName = "Assault Rifle";
            data.WeaponType = WeaponType.Rifle;
            data.FireMode = FireMode.FullAuto;
            data.Damage = 26f;
            data.FireRate = 10.5f;
            data.MagazineSize = 30;
            data.MaxReserveAmmo = 180;
            data.ReloadTime = 1.7f;
            data.Range = 100f;
            data.Spread = 0.025f;
            data.PelletsCount = 1;
            data.PiercingCount = 1;
            data.RecoilKick = new Vector2(0.6f, 1.1f);
            data.RecoilRecoverySpeed = 7f;
            data.CameraShakeIntensity = 0.22f;
            data.ZoomFOV = 55f;
            data.WeaponColor = new Color(0.2f, 0.22f, 0.25f);
            data.MuzzleFlashColor = new Color(1f, 0.75f, 0.2f);
            data.TracerColor = new Color(1f, 0.85f, 0.4f);
            return data;
        }

        public static WeaponDataSO CreateSMGData()
        {
            WeaponDataSO data = ScriptableObject.CreateInstance<WeaponDataSO>();
            data.WeaponName = "Viper SMG";
            data.WeaponType = WeaponType.SMG;
            data.FireMode = FireMode.FullAuto;
            data.Damage = 16f;
            data.FireRate = 17.0f;
            data.MagazineSize = 45;
            data.MaxReserveAmmo = 270;
            data.ReloadTime = 1.4f;
            data.Range = 60f;
            data.Spread = 0.055f;
            data.PelletsCount = 1;
            data.PiercingCount = 1;
            data.RecoilKick = new Vector2(0.9f, 1.4f);
            data.RecoilRecoverySpeed = 8.5f;
            data.CameraShakeIntensity = 0.18f;
            data.ZoomFOV = 60f;
            data.WeaponColor = new Color(0.15f, 0.15f, 0.18f);
            data.MuzzleFlashColor = new Color(1f, 0.6f, 0.1f);
            data.TracerColor = new Color(1f, 0.7f, 0.3f);
            return data;
        }

        public static WeaponDataSO CreateSniperData()
        {
            WeaponDataSO data = ScriptableObject.CreateInstance<WeaponDataSO>();
            data.WeaponName = "Apex Sniper";
            data.WeaponType = WeaponType.Sniper;
            data.FireMode = FireMode.BoltAction;
            data.Damage = 190f;
            data.FireRate = 1.1f;
            data.MagazineSize = 5;
            data.MaxReserveAmmo = 30;
            data.ReloadTime = 2.4f;
            data.Range = 250f;
            data.Spread = 0.001f;
            data.PelletsCount = 1;
            data.PiercingCount = 4;
            data.KnockbackForce = 35f;
            data.RecoilKick = new Vector2(1.5f, 4.0f);
            data.RecoilRecoverySpeed = 4.5f;
            data.CameraShakeIntensity = 0.65f;
            data.ZoomFOV = 22f;
            data.HasScopeOverlay = true;
            data.WeaponColor = new Color(0.28f, 0.32f, 0.35f);
            data.MuzzleFlashColor = new Color(0.4f, 0.8f, 1f);
            data.TracerColor = new Color(0.3f, 0.75f, 1f);
            return data;
        }

        public static WeaponDataSO CreateShotgunData()
        {
            WeaponDataSO data = ScriptableObject.CreateInstance<WeaponDataSO>();
            data.WeaponName = "Breaker Shotgun";
            data.WeaponType = WeaponType.Shotgun;
            data.FireMode = FireMode.SemiAuto;
            data.Damage = 18f; // per pellet
            data.FireRate = 1.4f;
            data.MagazineSize = 8;
            data.MaxReserveAmmo = 48;
            data.ReloadTime = 2.2f;
            data.Range = 45f;
            data.Spread = 0.085f;
            data.PelletsCount = 8; // 8 x 18 = 144 total close damage
            data.PiercingCount = 1;
            data.KnockbackForce = 28f;
            data.RecoilKick = new Vector2(1.8f, 3.5f);
            data.RecoilRecoverySpeed = 5.0f;
            data.CameraShakeIntensity = 0.55f;
            data.ZoomFOV = 62f;
            data.WeaponColor = new Color(0.38f, 0.22f, 0.18f);
            data.MuzzleFlashColor = new Color(1f, 0.5f, 0.1f);
            data.TracerColor = new Color(1f, 0.7f, 0.2f);
            return data;
        }

        public static WeaponDataSO CreateRocketLauncherData()
        {
            WeaponDataSO data = ScriptableObject.CreateInstance<WeaponDataSO>();
            data.WeaponName = "Havoc Rocket";
            data.WeaponType = WeaponType.RocketLauncher;
            data.FireMode = FireMode.SemiAuto;
            data.Damage = 260f;
            data.FireRate = 0.8f;
            data.MagazineSize = 1;
            data.MaxReserveAmmo = 12;
            data.ReloadTime = 2.6f;
            data.Range = 120f;
            data.Spread = 0.01f;
            data.PelletsCount = 1;
            data.PiercingCount = 1;
            data.IsProjectile = true;
            data.ProjectileSpeed = 45f;
            data.SplashRadius = 7.5f;
            data.SplashDamage = 260f;
            data.KnockbackForce = 45f;
            data.RecoilKick = new Vector2(2.5f, 5.0f);
            data.RecoilRecoverySpeed = 3.5f;
            data.CameraShakeIntensity = 0.85f;
            data.ZoomFOV = 65f;
            data.WeaponColor = new Color(0.18f, 0.35f, 0.25f);
            data.MuzzleFlashColor = new Color(1f, 0.3f, 0.1f);
            data.TracerColor = new Color(1f, 0.4f, 0.1f);
            return data;
        }

        public static List<WeaponDataSO> CreateCompleteArsenal()
        {
            return new List<WeaponDataSO>
            {
                CreateRifleData(),
                CreateSMGData(),
                CreateSniperData(),
                CreateShotgunData(),
                CreateRocketLauncherData()
            };
        }
    }
}
