using UnityEngine;
using ShadowFire.Effects;

namespace ShadowFire.Models
{
    public static class GunModelSetup
    {
        public static void GenerateGunMaterials() { }

        public static GameObject InstantiateGun(Transform parent, Vector3 localPos, Quaternion localRot, float targetScale = 1.0f)
        {
            GameObject weaponHolder = new GameObject("Enemy_Rifle");
            weaponHolder.transform.SetParent(parent, false);
            weaponHolder.transform.localPosition = localPos;
            weaponHolder.transform.localRotation = localRot;
            weaponHolder.transform.localScale = Vector3.one * targetScale;

            // 1. Receiver / Main Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Rifle_Body";
            body.transform.SetParent(weaponHolder.transform, false);
            body.transform.localPosition = new Vector3(0, 0, 0);
            body.transform.localScale = new Vector3(0.12f, 0.16f, 0.65f);
            body.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("gunmetal");
            Object.Destroy(body.GetComponent<Collider>());

            // 2. Extended Heavy Barrel
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Rifle_Barrel";
            barrel.transform.SetParent(weaponHolder.transform, false);
            barrel.transform.localPosition = new Vector3(0, 0.02f, 0.58f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            barrel.transform.localScale = new Vector3(0.06f, 0.32f, 0.06f);
            barrel.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("matteblack");
            Object.Destroy(barrel.GetComponent<Collider>());

            // 3. Glowing Red Muzzle Tip / Flash Hider
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tip.name = "Muzzle_Tip";
            tip.transform.SetParent(weaponHolder.transform, false);
            tip.transform.localPosition = new Vector3(0, 0.02f, 0.92f);
            tip.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            tip.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f);
            tip.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
            Object.Destroy(tip.GetComponent<Collider>());

            // 4. Curved Ammo Magazine
            GameObject mag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mag.name = "Rifle_Mag";
            mag.transform.SetParent(weaponHolder.transform, false);
            mag.transform.localPosition = new Vector3(0, -0.18f, 0.10f);
            mag.transform.localRotation = Quaternion.Euler(18f, 0, 0);
            mag.transform.localScale = new Vector3(0.08f, 0.26f, 0.14f);
            mag.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("matteblack");
            Object.Destroy(mag.GetComponent<Collider>());

            // 5. Tactical Scope with Glowing Reticle
            GameObject scope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            scope.name = "Rifle_Scope";
            scope.transform.SetParent(weaponHolder.transform, false);
            scope.transform.localPosition = new Vector3(0, 0.14f, 0.04f);
            scope.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            scope.transform.localScale = new Vector3(0.07f, 0.16f, 0.07f);
            scope.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
            Object.Destroy(scope.GetComponent<Collider>());

            // 6. Grip & Stock
            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "Rifle_Grip";
            grip.transform.SetParent(weaponHolder.transform, false);
            grip.transform.localPosition = new Vector3(0, -0.14f, -0.14f);
            grip.transform.localRotation = Quaternion.Euler(-25f, 0, 0);
            grip.transform.localScale = new Vector3(0.07f, 0.18f, 0.08f);
            grip.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("darkgrey");
            Object.Destroy(grip.GetComponent<Collider>());

            GameObject stock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stock.name = "Rifle_Stock";
            stock.transform.SetParent(weaponHolder.transform, false);
            stock.transform.localPosition = new Vector3(0, -0.02f, -0.42f);
            stock.transform.localScale = new Vector3(0.09f, 0.14f, 0.28f);
            stock.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("matteblack");
            Object.Destroy(stock.GetComponent<Collider>());

            return weaponHolder;
        }
    }
}
