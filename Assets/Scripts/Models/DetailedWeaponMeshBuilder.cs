using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Weapons;
using ShadowFire.Animation;
using ShadowFire.Effects;

namespace ShadowFire.Models
{
    public static class DetailedWeaponMeshBuilder
    {
        public static WeaponAnimationController BuildWeaponModel(GameObject root, WeaponDataSO data)
        {
            // Clear existing model children
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                if (root.transform.GetChild(i).name != "MuzzlePoint")
                {
                    Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
                }
            }

            GameObject modelRoot = new GameObject("Detailed_Weapon_Rig");
            modelRoot.transform.SetParent(root.transform, false);

            WeaponAnimationController anim = root.GetComponent<WeaponAnimationController>();
            if (anim == null) anim = root.AddComponent<WeaponAnimationController>();
            anim.RootModel = modelRoot.transform;

            Material gunMetalMat = ProceduralMeshGenerator.GetMaterial("gunmetal");
            Material glowCyanMat = ProceduralMeshGenerator.GetMaterial("glowcyan");
            Material glowRedMat = ProceduralMeshGenerator.GetMaterial("glowred");

            switch (data.WeaponType)
            {
                case WeaponType.Rifle:
                    BuildAssaultRifle(modelRoot.transform, anim, gunMetalMat, glowCyanMat);
                    break;
                case WeaponType.SMG:
                    BuildViperSMG(modelRoot.transform, anim, gunMetalMat, glowCyanMat);
                    break;
                case WeaponType.Sniper:
                    BuildApexSniper(modelRoot.transform, anim, gunMetalMat, glowCyanMat);
                    break;
                case WeaponType.Shotgun:
                    BuildBreakerShotgun(modelRoot.transform, anim, gunMetalMat, glowRedMat);
                    break;
                case WeaponType.RocketLauncher:
                    BuildHavocRocketLauncher(modelRoot.transform, anim, gunMetalMat, glowRedMat);
                    break;
            }

            anim.Initialize(data.WeaponType);
            return anim;
        }

        private static void BuildAssaultRifle(Transform root, WeaponAnimationController anim, Material bodyMat, Material glowMat)
        {
            // Main Receiver
            CreatePart("Receiver", root, new Vector3(0, 0, 0), new Vector3(0.06f, 0.09f, 0.32f), bodyMat);
            // Top Rail
            CreatePart("TopRail", root, new Vector3(0, 0.055f, 0), new Vector3(0.04f, 0.02f, 0.28f), bodyMat);
            // Barrel
            CreatePart("Barrel", root, new Vector3(0, 0.02f, 0.32f), new Vector3(0.035f, 0.035f, 0.35f), bodyMat);
            // Muzzle Brake
            CreatePart("MuzzleBrake", root, new Vector3(0, 0.02f, 0.51f), new Vector3(0.045f, 0.045f, 0.05f), bodyMat);
            // Stock
            CreatePart("Stock", root, new Vector3(0, -0.02f, -0.25f), new Vector3(0.05f, 0.12f, 0.22f), bodyMat);
            // Pistol Grip
            CreatePart("Grip", root, new Vector3(0, -0.10f, -0.08f), new Vector3(0.045f, 0.14f, 0.06f), bodyMat, new Vector3(-20f, 0, 0));
            // Holo Sight
            GameObject sight = CreatePart("HoloSight", root, new Vector3(0, 0.085f, 0.02f), new Vector3(0.05f, 0.05f, 0.08f), bodyMat);
            CreatePart("ReticleDot", sight.transform, new Vector3(0, 0, 0), new Vector3(0.015f, 0.015f, 0.015f), glowMat);
            anim.ScopeOptic = sight.transform;

            // Slide / Bolt
            GameObject bolt = CreatePart("BoltCarrier", root, new Vector3(0.032f, 0.03f, 0.02f), new Vector3(0.015f, 0.02f, 0.06f), bodyMat);
            anim.SlideOrBolt = bolt.transform;

            // Curved Magazine
            GameObject mag = CreatePart("CurvedMagazine", root, new Vector3(0, -0.14f, 0.08f), new Vector3(0.04f, 0.20f, 0.08f), bodyMat, new Vector3(15f, 0, 0));
            anim.Magazine = mag.transform;
        }

        private static void BuildViperSMG(Transform root, WeaponAnimationController anim, Material bodyMat, Material glowMat)
        {
            // Receiver
            CreatePart("Receiver", root, new Vector3(0, 0, 0), new Vector3(0.055f, 0.08f, 0.25f), bodyMat);
            // Shrouded Barrel
            CreatePart("SuppressorBarrel", root, new Vector3(0, 0.01f, 0.22f), new Vector3(0.045f, 0.045f, 0.22f), bodyMat);
            // Forward Grip
            CreatePart("Foregrip", root, new Vector3(0, -0.08f, 0.14f), new Vector3(0.035f, 0.10f, 0.04f), bodyMat);
            // Pistol Grip
            CreatePart("PistolGrip", root, new Vector3(0, -0.08f, -0.08f), new Vector3(0.04f, 0.12f, 0.05f), bodyMat, new Vector3(-25f, 0, 0));
            // Compact Stock
            CreatePart("WireStock", root, new Vector3(0, -0.01f, -0.18f), new Vector3(0.045f, 0.06f, 0.12f), bodyMat);
            // Top Compact Sight
            CreatePart("ReflexSight", root, new Vector3(0, 0.055f, 0.02f), new Vector3(0.035f, 0.035f, 0.06f), glowMat);

            // Slide
            GameObject slide = CreatePart("Slide", root, new Vector3(0, 0.045f, -0.02f), new Vector3(0.052f, 0.025f, 0.14f), bodyMat);
            anim.SlideOrBolt = slide.transform;

            // Extended Stick Magazine
            GameObject mag = CreatePart("ExtendedMag", root, new Vector3(0, -0.18f, -0.02f), new Vector3(0.035f, 0.26f, 0.05f), bodyMat, new Vector3(-10f, 0, 0));
            anim.Magazine = mag.transform;
        }

        private static void BuildApexSniper(Transform root, WeaponAnimationController anim, Material bodyMat, Material glowMat)
        {
            // Long Chassis
            CreatePart("Chassis", root, new Vector3(0, 0, 0), new Vector3(0.065f, 0.09f, 0.45f), bodyMat);
            // Heavy Fluted Barrel
            CreatePart("HeavyBarrel", root, new Vector3(0, 0.02f, 0.50f), new Vector3(0.04f, 0.04f, 0.60f), bodyMat);
            // Massive Muzzle Brake
            CreatePart("MuzzleBrake", root, new Vector3(0, 0.02f, 0.82f), new Vector3(0.065f, 0.065f, 0.08f), bodyMat);
            // Precision Stock with Cheek Rest
            CreatePart("PrecisionStock", root, new Vector3(0, -0.01f, -0.32f), new Vector3(0.055f, 0.14f, 0.28f), bodyMat);
            // Grip
            CreatePart("Grip", root, new Vector3(0, -0.11f, -0.10f), new Vector3(0.045f, 0.15f, 0.06f), bodyMat, new Vector3(-18f, 0, 0));

            // High-Power Scope
            GameObject scope = CreatePart("TelescopicScope", root, new Vector3(0, 0.095f, 0.05f), new Vector3(0.06f, 0.06f, 0.28f), bodyMat);
            CreatePart("LensFront", scope.transform, new Vector3(0, 0, 0.14f), new Vector3(0.05f, 0.05f, 0.01f), glowMat);
            CreatePart("LensRear", scope.transform, new Vector3(0, 0, -0.14f), new Vector3(0.045f, 0.045f, 0.01f), glowMat);
            anim.ScopeOptic = scope.transform;

            // Folded Bipod
            CreatePart("BipodLeft", root, new Vector3(-0.045f, -0.06f, 0.40f), new Vector3(0.02f, 0.14f, 0.02f), bodyMat, new Vector3(-40f, -15f, 0));
            CreatePart("BipodRight", root, new Vector3(0.045f, -0.06f, 0.40f), new Vector3(0.02f, 0.14f, 0.02f), bodyMat, new Vector3(-40f, 15f, 0));

            // Bolt Handle
            GameObject bolt = CreatePart("BoltHandle", root, new Vector3(0.04f, 0.035f, -0.04f), new Vector3(0.06f, 0.02f, 0.02f), bodyMat);
            anim.SlideOrBolt = bolt.transform;

            // Box Magazine
            GameObject mag = CreatePart("BoxMagazine", root, new Vector3(0, -0.09f, 0.06f), new Vector3(0.045f, 0.12f, 0.09f), bodyMat);
            anim.Magazine = mag.transform;
        }

        private static void BuildBreakerShotgun(Transform root, WeaponAnimationController anim, Material bodyMat, Material glowMat)
        {
            // Heavy Receiver
            CreatePart("Receiver", root, new Vector3(0, 0, 0), new Vector3(0.075f, 0.10f, 0.35f), bodyMat);
            // Main Wide Barrel
            CreatePart("Barrel", root, new Vector3(0, 0.03f, 0.35f), new Vector3(0.05f, 0.05f, 0.40f), bodyMat);
            // Tubular Magazine Underneath
            CreatePart("MagazineTube", root, new Vector3(0, -0.025f, 0.32f), new Vector3(0.04f, 0.04f, 0.36f), bodyMat);
            // Heat Shield on top
            CreatePart("HeatShield", root, new Vector3(0, 0.06f, 0.28f), new Vector3(0.06f, 0.02f, 0.25f), bodyMat);
            // Stock
            CreatePart("Stock", root, new Vector3(0, -0.03f, -0.26f), new Vector3(0.06f, 0.13f, 0.24f), bodyMat);
            // Grip
            CreatePart("Grip", root, new Vector3(0, -0.11f, -0.08f), new Vector3(0.05f, 0.14f, 0.06f), bodyMat, new Vector3(-25f, 0, 0));

            // Pump Grip (Fore-end)
            GameObject pump = CreatePart("PumpSlide", root, new Vector3(0, -0.025f, 0.28f), new Vector3(0.065f, 0.065f, 0.16f), bodyMat);
            anim.PumpGrip = pump.transform;
        }

        private static void BuildHavocRocketLauncher(Transform root, WeaponAnimationController anim, Material bodyMat, Material glowMat)
        {
            // Heavy Cylindrical Launch Tube
            CreatePart("LaunchTube", root, new Vector3(0, 0, 0.05f), new Vector3(0.14f, 0.14f, 0.70f), bodyMat);
            // Rear Exhaust Flange
            CreatePart("ExhaustFlange", root, new Vector3(0, 0, -0.32f), new Vector3(0.18f, 0.18f, 0.06f), bodyMat);
            // Dual Grips
            CreatePart("MainGrip", root, new Vector3(0, -0.14f, 0), new Vector3(0.05f, 0.16f, 0.06f), bodyMat, new Vector3(-15f, 0, 0));
            CreatePart("ForwardGrip", root, new Vector3(0, -0.14f, 0.22f), new Vector3(0.05f, 0.14f, 0.05f), bodyMat);
            // Targeting Computer Display
            GameObject display = CreatePart("TargetingComputer", root, new Vector3(0.09f, 0.08f, 0.05f), new Vector3(0.06f, 0.08f, 0.10f), bodyMat);
            CreatePart("TargetingScreen", display.transform, new Vector3(0.032f, 0, 0), new Vector3(0.005f, 0.06f, 0.08f), glowMat);
            anim.ScopeOptic = display.transform;

            // Loaded Rocket Warhead Visible at Front
            CreatePart("RocketWarhead", root, new Vector3(0, 0, 0.42f), new Vector3(0.12f, 0.12f, 0.14f), glowMat);
        }

        private static GameObject CreatePart(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, Vector3? localRot = null)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;
            if (localRot.HasValue) obj.transform.localRotation = Quaternion.Euler(localRot.Value);

            var col = obj.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Object.Destroy(col);
                else Object.DestroyImmediate(col);
            }
            var mr = obj.GetComponent<MeshRenderer>();
            mr.material = mat;

            return obj;
        }
    }
}
