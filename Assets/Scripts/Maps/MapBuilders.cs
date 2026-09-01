using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Effects;

namespace ShadowFire.Maps
{
    public class HazardZone : MonoBehaviour
    {
        [SerializeField] private float damagePerSecond = 18f;
        [SerializeField] private string hazardType = "Toxic Acid";

        private void OnTriggerStay(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                DamageInfo dInfo = new DamageInfo(damagePerSecond * Time.deltaTime, other.transform.position, Vector3.up, false, gameObject, Vector3.zero, HitType.Default);
                damageable.TakeDamage(dInfo);
            }
        }
    }

    public static class MapGeometryFactory
    {
        public static void BuildOutpostRuin(Transform parent, MapDataSO mapData, out Transform playerSpawn, out Transform bossSpawn, out List<Transform> enemySpawns)
        {
            enemySpawns = new List<Transform>();
            float width = 75f;
            float length = 75f;

            // 1. Floor
            GameObject floor = CreateBlock("Floor_Main", parent, new Vector3(0, -0.5f, 0), new Vector3(width, 1f, length), mapData.GroundColor);
            floor.layer = LayerMask.NameToLayer("Ground");

            // 2. Perimeter Walls
            CreatePerimeter(parent, width, length, 6f, mapData.WallColor);

            // 3. Central Fortified Bunker
            GameObject bunker = CreateBlock("Central_Bunker", parent, new Vector3(0, 1.25f, 0), new Vector3(18f, 2.5f, 18f), mapData.WallColor);
            bunker.layer = LayerMask.NameToLayer("Ground");

            // Ramps
            CreateBlock("Ramp_North", parent, new Vector3(0, 1.25f, 12f), new Vector3(6f, 0.6f, 8f), mapData.WallColor, new Vector3(18f, 0, 0)).layer = LayerMask.NameToLayer("Ground");
            CreateBlock("Ramp_South", parent, new Vector3(0, 1.25f, -12f), new Vector3(6f, 0.6f, 8f), mapData.WallColor, new Vector3(-18f, 0, 0)).layer = LayerMask.NameToLayer("Ground");

            // Searchlights & Barricades
            CreateSearchlights(parent, 28f, mapData.AccentColor);

            // Spawns
            playerSpawn = CreateSpawnNode(parent, "PlayerSpawn", new Vector3(0, 3.2f, 0));
            bossSpawn = CreateSpawnNode(parent, "BossSpawn", new Vector3(0, 0.5f, 28f));

            Vector3[] coords = new Vector3[] {
                new Vector3(-30f, 0.5f, 0f), new Vector3(30f, 0.5f, 0f),
                new Vector3(0f, 0.5f, 30f), new Vector3(0f, 0.5f, -30f),
                new Vector3(-24f, 0.5f, -24f), new Vector3(24f, 0.5f, 24f)
            };
            foreach (var c in coords) enemySpawns.Add(CreateSpawnNode(parent, "EnemySpawn", c));
        }

        public static void BuildToxicBiolab(Transform parent, MapDataSO mapData, out Transform playerSpawn, out Transform bossSpawn, out List<Transform> enemySpawns)
        {
            enemySpawns = new List<Transform>();
            float width = 70f;
            float length = 70f;

            // 1. Metal Floor
            GameObject floor = CreateBlock("Biolab_Floor", parent, new Vector3(0, -0.5f, 0), new Vector3(width, 1f, length), new Color(0.1f, 0.15f, 0.12f));
            floor.layer = LayerMask.NameToLayer("Ground");

            CreatePerimeter(parent, width, length, 7f, new Color(0.18f, 0.22f, 0.18f));

            // 2. Toxic Acid Hazard Pools in 4 Quadrants
            Vector3[] acidPositions = new Vector3[] {
                new Vector3(-16f, 0.1f, 16f),
                new Vector3(16f, 0.1f, 16f),
                new Vector3(-16f, 0.1f, -16f),
                new Vector3(16f, 0.1f, -16f)
            };

            foreach (var aPos in acidPositions)
            {
                GameObject pool = CreateBlock("Acid_Pool", parent, aPos, new Vector3(10f, 0.2f, 10f), ProceduralMeshGenerator.GetMaterial("glowgreen").color);
                pool.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowgreen");
                
                BoxCollider col = pool.GetComponent<BoxCollider>() ?? pool.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(10f, 3f, 10f);
                pool.AddComponent<HazardZone>();
            }

            // 3. Catwalk Bridge crossing over the acid pools
            CreateBlock("Catwalk_X", parent, new Vector3(0, 1.5f, 0), new Vector3(4f, 0.5f, 60f), mapData.WallColor).layer = LayerMask.NameToLayer("Ground");
            CreateBlock("Catwalk_Z", parent, new Vector3(0, 1.5f, 0), new Vector3(60f, 0.5f, 4f), mapData.WallColor).layer = LayerMask.NameToLayer("Ground");

            // 4. Containment Pods
            for (int i = 0; i < 6; i++)
            {
                float angle = i * (360f / 6f);
                Vector3 pPos = Quaternion.Euler(0, angle, 0) * new Vector3(0, 2f, 22f);
                GameObject pod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pod.name = $"ContainmentPod_{i}";
                pod.transform.SetParent(parent, false);
                pod.transform.localPosition = pPos;
                pod.transform.localScale = new Vector3(2.5f, 4f, 2.5f);
                pod.layer = LayerMask.NameToLayer("Props");
                pod.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowcyan");
            }

            // Spawns
            playerSpawn = CreateSpawnNode(parent, "PlayerSpawn", new Vector3(0, 2.5f, 0));
            bossSpawn = CreateSpawnNode(parent, "BossSpawn", new Vector3(0, 0.5f, 25f));

            Vector3[] coords = new Vector3[] {
                new Vector3(-28f, 0.5f, 0f), new Vector3(28f, 0.5f, 0f),
                new Vector3(0f, 0.5f, 28f), new Vector3(0f, 0.5f, -28f),
                new Vector3(-22f, 0.5f, 22f), new Vector3(22f, 0.5f, -22f)
            };
            foreach (var c in coords) enemySpawns.Add(CreateSpawnNode(parent, "EnemySpawn", c));
        }

        public static void BuildInfernoCrater(Transform parent, MapDataSO mapData, out Transform playerSpawn, out Transform bossSpawn, out List<Transform> enemySpawns)
        {
            enemySpawns = new List<Transform>();
            float width = 80f;
            float length = 80f;

            // 1. Dark Ash Ground
            GameObject floor = CreateBlock("Ash_Caldera", parent, new Vector3(0, -0.5f, 0), new Vector3(width, 1f, length), new Color(0.12f, 0.08f, 0.08f));
            floor.layer = LayerMask.NameToLayer("Ground");

            CreatePerimeter(parent, width, length, 8f, new Color(0.22f, 0.12f, 0.12f));

            // 2. Magma Fissures (Cross Pattern)
            GameObject fissureX = CreateBlock("Magma_Rift_X", parent, new Vector3(0, 0.05f, 0), new Vector3(6f, 0.1f, 70f), Color.red);
            fissureX.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
            BoxCollider colX = fissureX.GetComponent<BoxCollider>() ?? fissureX.AddComponent<BoxCollider>();
            colX.isTrigger = true;
            colX.size = new Vector3(6f, 2f, 70f);
            fissureX.AddComponent<HazardZone>();

            // 3. Ancient Obsidian Obelisks
            Vector3[] obeliskPositions = new Vector3[] {
                new Vector3(-18f, 4f, 18f), new Vector3(18f, 4f, 18f),
                new Vector3(-18f, 4f, -18f), new Vector3(18f, 4f, -18f),
                new Vector3(0, 5f, 0)
            };

            for (int i = 0; i < obeliskPositions.Length; i++)
            {
                GameObject obelisk = CreateBlock($"Obsidian_Obelisk_{i}", parent, obeliskPositions[i], new Vector3(3f, 8f, 3f), new Color(0.15f, 0.05f, 0.05f));
                obelisk.layer = LayerMask.NameToLayer("Props");
                obelisk.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("gunmetal");
            }

            // Spawns
            playerSpawn = CreateSpawnNode(parent, "PlayerSpawn", new Vector3(0, 1.5f, -25f));
            bossSpawn = CreateSpawnNode(parent, "BossSpawn", new Vector3(0, 1.5f, 25f));

            Vector3[] coords = new Vector3[] {
                new Vector3(-32f, 0.5f, 0f), new Vector3(32f, 0.5f, 0f),
                new Vector3(0f, 0.5f, 32f), new Vector3(-25f, 0.5f, -25f),
                new Vector3(25f, 0.5f, 25f)
            };
            foreach (var c in coords) enemySpawns.Add(CreateSpawnNode(parent, "EnemySpawn", c));
        }

        private static GameObject CreateBlock(string name, Transform parent, Vector3 pos, Vector3 size, Color color, Vector3? rot = null)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = pos;
            obj.transform.localScale = size;
            if (rot.HasValue) obj.transform.localRotation = Quaternion.Euler(rot.Value);

            var mr = obj.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            mr.material = mat;

            return obj;
        }

        private static void CreatePerimeter(Transform parent, float width, float length, float height, Color color)
        {
            CreateBlock("Wall_North", parent, new Vector3(0, height / 2f, length / 2f), new Vector3(width, height, 1.5f), color).layer = LayerMask.NameToLayer("Ground");
            CreateBlock("Wall_South", parent, new Vector3(0, height / 2f, -length / 2f), new Vector3(width, height, 1.5f), color).layer = LayerMask.NameToLayer("Ground");
            CreateBlock("Wall_East", parent, new Vector3(width / 2f, height / 2f, 0), new Vector3(1.5f, height, length), color).layer = LayerMask.NameToLayer("Ground");
            CreateBlock("Wall_West", parent, new Vector3(-width / 2f, height / 2f, 0), new Vector3(1.5f, height, length), color).layer = LayerMask.NameToLayer("Ground");
        }

        private static void CreateSearchlights(Transform parent, float radius, Color color)
        {
            Vector3[] lightPos = new Vector3[] {
                new Vector3(-radius, 8f, -radius), new Vector3(radius, 8f, -radius),
                new Vector3(-radius, 8f, radius), new Vector3(radius, 8f, radius)
            };

            foreach (var lp in lightPos)
            {
                GameObject spotObj = new GameObject("Arena_Spotlight");
                spotObj.transform.SetParent(parent, false);
                spotObj.transform.localPosition = lp;
                spotObj.transform.LookAt(parent.position);

                Light spot = spotObj.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.color = color;
                spot.intensity = 5.0f;
                spot.range = 50f;
                spot.spotAngle = 65f;
            }
        }

        private static Transform CreateSpawnNode(Transform parent, string name, Vector3 pos)
        {
            GameObject node = new GameObject(name);
            node.transform.SetParent(parent, false);
            node.transform.localPosition = pos;
            return node.transform;
        }
    }
}
