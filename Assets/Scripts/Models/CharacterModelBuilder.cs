using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Animation;
using ShadowFire.Effects;

namespace ShadowFire.Models
{
    public static class CharacterModelBuilder
    {
        public static ProceduralCharacterAnimator BuildHumanoidModel(GameObject root, EnemyType type, float scaleMultiplier = 1.0f)
        {
            // Clear existing children
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            }

            GameObject modelRoot = new GameObject("Model_Rig");
            modelRoot.transform.SetParent(root.transform, false);
            modelRoot.transform.localScale = Vector3.one * scaleMultiplier;

            ProceduralCharacterAnimator animator = root.GetComponent<ProceduralCharacterAnimator>();
            if (animator == null) animator = root.AddComponent<ProceduralCharacterAnimator>();
            animator.SetEnemyType(type);

            animator.RootBone = modelRoot.transform;

            // Materials
            Material skinMat = GetSkinMaterial(type);
            Material armorMat = GetArmorMaterial(type);
            Material eyeMat = GetEyeMaterial(type);

            // 1. Pelvis & Spine
            GameObject pelvis = CreateLimb("Pelvis", modelRoot.transform, new Vector3(0, 1.0f, 0), new Vector3(0.35f, 0.2f, 0.25f), skinMat);
            animator.Pelvis = pelvis.transform;

            GameObject spine = CreateLimb("Spine", pelvis.transform, new Vector3(0, 0.25f, 0), new Vector3(0.32f, 0.35f, 0.24f), skinMat);
            animator.Spine = spine.transform;

            GameObject chest = CreateLimb("Chest", spine.transform, new Vector3(0, 0.35f, 0), new Vector3(0.48f, 0.4f, 0.3f), armorMat);
            animator.Chest = chest.transform;

            // 2. Head & Glowing Eyes
            GameObject neck = CreateLimb("Neck", chest.transform, new Vector3(0, 0.25f, 0), new Vector3(0.14f, 0.12f, 0.14f), skinMat);
            GameObject head = CreateLimb("Head", neck.transform, new Vector3(0, 0.18f, 0), new Vector3(0.26f, 0.28f, 0.28f), skinMat);
            animator.Head = head.transform;

            // Eyes
            CreateLimb("Eye_Left", head.transform, new Vector3(-0.07f, 0.04f, 0.13f), new Vector3(0.04f, 0.04f, 0.04f), eyeMat);
            CreateLimb("Eye_Right", head.transform, new Vector3(0.07f, 0.04f, 0.13f), new Vector3(0.04f, 0.04f, 0.04f), eyeMat);

            // Horns for Boss
            if (type == EnemyType.Boss)
            {
                CreateLimb("Horn_Left", head.transform, new Vector3(-0.15f, 0.22f, -0.05f), new Vector3(0.08f, 0.35f, 0.08f), armorMat, new Vector3(25f, -30f, 0));
                CreateLimb("Horn_Right", head.transform, new Vector3(0.15f, 0.22f, -0.05f), new Vector3(0.08f, 0.35f, 0.08f), armorMat, new Vector3(25f, 30f, 0));
            }

            // 3. Left Arm
            GameObject lShoulder = CreateLimb("LeftShoulder", chest.transform, new Vector3(-0.3f, 0.12f, 0), new Vector3(0.14f, 0.14f, 0.14f), armorMat);
            animator.LeftShoulder = lShoulder.transform;
            GameObject lUpperArm = CreateLimb("LeftUpperArm", lShoulder.transform, new Vector3(0, -0.22f, 0), new Vector3(0.12f, 0.32f, 0.12f), skinMat);
            animator.LeftUpperArm = lUpperArm.transform;
            GameObject lForearm = CreateLimb("LeftForearm", lUpperArm.transform, new Vector3(0, -0.28f, 0), new Vector3(0.10f, 0.28f, 0.10f), skinMat);
            animator.LeftForearm = lForearm.transform;
            GameObject lHand = CreateLimb("LeftHand", lForearm.transform, new Vector3(0, -0.20f, 0), new Vector3(0.12f, 0.15f, 0.14f), armorMat);
            animator.LeftHand = lHand.transform;

            // 4. Right Arm
            GameObject rShoulder = CreateLimb("RightShoulder", chest.transform, new Vector3(0.3f, 0.12f, 0), new Vector3(0.14f, 0.14f, 0.14f), armorMat);
            animator.RightShoulder = rShoulder.transform;
            GameObject rUpperArm = CreateLimb("RightUpperArm", rShoulder.transform, new Vector3(0, -0.22f, 0), new Vector3(0.12f, 0.32f, 0.12f), skinMat);
            animator.RightUpperArm = rUpperArm.transform;
            GameObject rForearm = CreateLimb("RightForearm", rUpperArm.transform, new Vector3(0, -0.28f, 0), new Vector3(0.10f, 0.28f, 0.10f), skinMat);
            animator.RightForearm = rForearm.transform;
            GameObject rHand = CreateLimb("RightHand", rForearm.transform, new Vector3(0, -0.20f, 0), new Vector3(0.12f, 0.15f, 0.14f), armorMat);
            animator.RightHand = rHand.transform;

            // 5. Left Leg
            GameObject lThigh = CreateLimb("LeftThigh", pelvis.transform, new Vector3(-0.16f, -0.15f, 0), new Vector3(0.15f, 0.38f, 0.15f), skinMat);
            animator.LeftThigh = lThigh.transform;
            GameObject lShin = CreateLimb("LeftShin", lThigh.transform, new Vector3(0, -0.35f, 0), new Vector3(0.12f, 0.38f, 0.12f), skinMat);
            animator.LeftShin = lShin.transform;
            GameObject lFoot = CreateLimb("LeftFoot", lShin.transform, new Vector3(0, -0.26f, 0.08f), new Vector3(0.14f, 0.10f, 0.26f), armorMat);
            animator.LeftFoot = lFoot.transform;

            // 6. Right Leg
            GameObject rThigh = CreateLimb("RightThigh", pelvis.transform, new Vector3(0.16f, -0.15f, 0), new Vector3(0.15f, 0.38f, 0.15f), skinMat);
            animator.RightThigh = rThigh.transform;
            GameObject rShin = CreateLimb("RightShin", rThigh.transform, new Vector3(0, -0.35f, 0), new Vector3(0.12f, 0.38f, 0.12f), skinMat);
            animator.RightShin = rShin.transform;
            GameObject rFoot = CreateLimb("RightFoot", rShin.transform, new Vector3(0, -0.26f, 0.08f), new Vector3(0.14f, 0.10f, 0.26f), armorMat);
            animator.RightFoot = rFoot.transform;

            // 7. Demon Wings (Boss only)
            if (type == EnemyType.Boss)
            {
                GameObject lWing = CreateLimb("LeftWing", chest.transform, new Vector3(-0.35f, 0.2f, -0.2f), new Vector3(1.2f, 0.6f, 0.05f), armorMat, new Vector3(10f, -35f, 20f));
                animator.LeftWing = lWing.transform;
                GameObject rWing = CreateLimb("RightWing", chest.transform, new Vector3(0.35f, 0.2f, -0.2f), new Vector3(1.2f, 0.6f, 0.05f), armorMat, new Vector3(10f, 35f, -20f));
                animator.RightWing = rWing.transform;
            }

            // 8. Bio Sac for Spitter
            if (type == EnemyType.Shooter)
            {
                CreateLimb("BioSac", chest.transform, new Vector3(0, 0.1f, -0.22f), new Vector3(0.35f, 0.4f, 0.3f), ProceduralMeshGenerator.GetMaterial("glowcyan"));
            }

            return animator;
        }

        private static GameObject CreateLimb(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, Vector3? localRot = null)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;
            if (localRot.HasValue) obj.transform.localRotation = Quaternion.Euler(localRot.Value);

            Object.Destroy(obj.GetComponent<Collider>());
            var mr = obj.GetComponent<MeshRenderer>();
            mr.material = mat;

            return obj;
        }

        private static Material GetSkinMaterial(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Zombie: return ProceduralMeshGenerator.GetMaterial("enemy");
                case EnemyType.Runner: return ProceduralMeshGenerator.GetMaterial("glowred");
                case EnemyType.Tank: return ProceduralMeshGenerator.GetMaterial("gunmetal");
                case EnemyType.Shooter: return ProceduralMeshGenerator.GetMaterial("enemy");
                case EnemyType.Boss: return ProceduralMeshGenerator.GetMaterial("boss");
                default: return ProceduralMeshGenerator.GetMaterial("enemy");
            }
        }

        private static Material GetArmorMaterial(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Tank: return ProceduralMeshGenerator.GetMaterial("gunmetal");
                case EnemyType.Boss: return ProceduralMeshGenerator.GetMaterial("glowred");
                default: return ProceduralMeshGenerator.GetMaterial("gunmetal");
            }
        }

        private static Material GetEyeMaterial(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Runner: return ProceduralMeshGenerator.GetMaterial("glowgold");
                case EnemyType.Shooter: return ProceduralMeshGenerator.GetMaterial("glowcyan");
                case EnemyType.Boss: return ProceduralMeshGenerator.GetMaterial("glowgold");
                default: return ProceduralMeshGenerator.GetMaterial("glowred");
            }
        }
    }
}
