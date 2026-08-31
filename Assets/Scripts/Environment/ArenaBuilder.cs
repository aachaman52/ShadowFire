using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Effects;

namespace ShadowFire.Environment
{
    public class ArenaBuilder : MonoBehaviour
    {
        [Header("Arena Configuration")]
        [SerializeField] private float arenaWidth = 70f;
        [SerializeField] private float arenaLength = 70f;
        [SerializeField] private float wallHeight = 6f;

        [Header("Spawn Points")]
        public Transform PlayerSpawnPoint;
        public List<Transform> EnemySpawnPoints = new List<Transform>();
        public Transform BossSpawnPoint;

        public void BuildArena()
        {
            // Clear any old generated arena elements under this transform
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            EnemySpawnPoints.Clear();

            GameObject arenaRoot = new GameObject("Arena_Geometry");
            arenaRoot.transform.SetParent(transform);

            // 1. Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_Main";
            floor.transform.SetParent(arenaRoot.transform);
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(arenaWidth, 1f, arenaLength);
            floor.layer = LayerMask.NameToLayer("Ground");
            
            Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            floorMat.color = new Color(0.12f, 0.14f, 0.16f);
            if (floorMat.HasProperty("_Smoothness")) floorMat.SetFloat("_Smoothness", 0.4f);
            floor.GetComponent<MeshRenderer>().material = floorMat;

            // 2. Perimeter Walls
            BuildPerimeterWalls(arenaRoot.transform);

            // 3. Central Fortified Structure
            BuildCentralStructure(arenaRoot.transform);

            // 4. Tactical Covers, Crates, Barrels
            BuildTacticalCovers(arenaRoot.transform);

            // 5. Lighting & Atmosphere
            BuildAtmosphericLighting(arenaRoot.transform);

            // 6. Spawn Anchors
            SetupSpawnAnchors();
        }

        private void BuildPerimeterWalls(Transform parent)
        {
            Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            wallMat.color = new Color(0.18f, 0.2f, 0.22f);

            // North Wall
            CreateWall(parent, new Vector3(0, wallHeight / 2, arenaLength / 2), new Vector3(arenaWidth, wallHeight, 1.5f), wallMat);
            // South Wall
            CreateWall(parent, new Vector3(0, wallHeight / 2, -arenaLength / 2), new Vector3(arenaWidth, wallHeight, 1.5f), wallMat);
            // East Wall
            CreateWall(parent, new Vector3(arenaWidth / 2, wallHeight / 2, 0), new Vector3(1.5f, wallHeight, arenaLength), wallMat);
            // West Wall
            CreateWall(parent, new Vector3(-arenaWidth / 2, wallHeight / 2, 0), new Vector3(1.5f, wallHeight, arenaLength), wallMat);
        }

        private void CreateWall(Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Perimeter_Wall";
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.layer = LayerMask.NameToLayer("Ground");
            wall.GetComponent<MeshRenderer>().material = mat;
        }

        private void BuildCentralStructure(Transform parent)
        {
            Material platformMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            platformMat.color = new Color(0.22f, 0.24f, 0.28f);

            // Central elevated bunker
            GameObject bunker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bunker.name = "Central_Bunker_Platform";
            bunker.transform.SetParent(parent);
            bunker.transform.position = new Vector3(0, 1.25f, 0);
            bunker.transform.localScale = new Vector3(16f, 2.5f, 16f);
            bunker.layer = LayerMask.NameToLayer("Ground");
            bunker.GetComponent<MeshRenderer>().material = platformMat;

            // Access Ramps (North and South)
            CreateRamp(parent, new Vector3(0, 1.25f, 11f), new Vector3(6f, 0.6f, 8f), new Vector3(18f, 0, 0), platformMat);
            CreateRamp(parent, new Vector3(0, 1.25f, -11f), new Vector3(6f, 0.6f, 8f), new Vector3(-18f, 0, 0), platformMat);

            // Guard rails on bunker
            CreateRail(parent, new Vector3(7.5f, 3.1f, 0), new Vector3(0.5f, 1.2f, 16f));
            CreateRail(parent, new Vector3(-7.5f, 3.1f, 0), new Vector3(0.5f, 1.2f, 16f));
        }

        private void CreateRamp(Transform parent, Vector3 pos, Vector3 scale, Vector3 rot, Material mat)
        {
            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Access_Ramp";
            ramp.transform.SetParent(parent);
            ramp.transform.position = pos;
            ramp.transform.rotation = Quaternion.Euler(rot);
            ramp.transform.localScale = scale;
            ramp.layer = LayerMask.NameToLayer("Ground");
            ramp.GetComponent<MeshRenderer>().material = mat;
        }

        private void CreateRail(Transform parent, Vector3 pos, Vector3 scale)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Bunker_Rail";
            rail.transform.SetParent(parent);
            rail.transform.position = pos;
            rail.transform.localScale = scale;
            rail.layer = LayerMask.NameToLayer("Ground");
            rail.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("gunmetal");
        }

        private void BuildTacticalCovers(Transform parent)
        {
            // Barricades and shipping containers
            Vector3[] containerPositions = new Vector3[]
            {
                new Vector3(-18f, 1.5f, 18f),
                new Vector3(18f, 1.5f, 18f),
                new Vector3(-18f, 1.5f, -18f),
                new Vector3(18f, 1.5f, -18f),
                new Vector3(0, 1.5f, 24f),
                new Vector3(0, 1.5f, -24f),
                new Vector3(24f, 1.5f, 0),
                new Vector3(-24f, 1.5f, 0)
            };

            for (int i = 0; i < containerPositions.Length; i++)
            {
                GameObject container = GameObject.CreatePrimitive(PrimitiveType.Cube);
                container.name = $"Cover_Container_{i}";
                container.transform.SetParent(parent);
                container.transform.position = containerPositions[i];
                container.transform.localScale = new Vector3(4f, 3f, 7f);
                container.transform.rotation = Quaternion.Euler(0, (i % 2 == 0 ? 30f : -30f), 0);
                container.layer = LayerMask.NameToLayer("Props");

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = (i % 2 == 0) ? new Color(0.2f, 0.35f, 0.45f) : new Color(0.45f, 0.25f, 0.2f);
                container.GetComponent<MeshRenderer>().material = mat;

                // Scatter adjacent crates & barrels
                ProceduralMeshGenerator.CreateCrate(containerPositions[i] + new Vector3(3f, 0.75f, 0), new Vector3(1.5f, 1.5f, 1.5f)).transform.SetParent(parent);
                ProceduralMeshGenerator.CreateBarrel(containerPositions[i] + new Vector3(-3f, 0.6f, 1f), true).transform.SetParent(parent);
            }
        }

        private void BuildAtmosphericLighting(Transform parent)
        {
            // Moon / Night Key Light
            GameObject moonLightObj = new GameObject("Moon_Directional_Light");
            moonLightObj.transform.SetParent(parent);
            Light moonLight = moonLightObj.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.35f, 0.45f, 0.7f);
            moonLight.intensity = 0.5f;
            moonLight.shadows = LightShadows.Soft;
            moonLightObj.transform.rotation = Quaternion.Euler(50f, -40f, 0f);

            // 4 Floodlights at corners illuminating the arena
            Vector3[] floodlightPos = new Vector3[]
            {
                new Vector3(-28f, 7f, -28f),
                new Vector3(28f, 7f, -28f),
                new Vector3(-28f, 7f, 28f),
                new Vector3(28f, 7f, 28f)
            };

            for (int i = 0; i < floodlightPos.Length; i++)
            {
                GameObject lightPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lightPole.name = $"Floodlight_Pole_{i}";
                lightPole.transform.SetParent(parent);
                lightPole.transform.position = floodlightPos[i] - new Vector3(0, 3.5f, 0);
                lightPole.transform.localScale = new Vector3(0.4f, 7f, 0.4f);
                lightPole.layer = LayerMask.NameToLayer("Props");

                GameObject spotObj = new GameObject($"Floodlight_Source_{i}");
                spotObj.transform.SetParent(parent);
                spotObj.transform.position = floodlightPos[i];
                spotObj.transform.LookAt(Vector3.zero);

                Light spot = spotObj.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.color = new Color(1f, 0.85f, 0.6f);
                spot.intensity = 4.5f;
                spot.range = 55f;
                spot.spotAngle = 65f;
                spot.shadows = LightShadows.Soft;
            }
        }

        private void SetupSpawnAnchors()
        {
            GameObject anchorsRoot = new GameObject("Spawn_Anchors");
            anchorsRoot.transform.SetParent(transform);

            // Player Spawn (on central bunker)
            GameObject pSpawn = new GameObject("PlayerSpawn");
            pSpawn.transform.SetParent(anchorsRoot.transform);
            pSpawn.transform.position = new Vector3(0, 3.2f, 0);
            PlayerSpawnPoint = pSpawn.transform;

            // Boss Spawn
            GameObject bSpawn = new GameObject("BossSpawn");
            bSpawn.transform.SetParent(anchorsRoot.transform);
            bSpawn.transform.position = new Vector3(0, 0.5f, 28f);
            BossSpawnPoint = bSpawn.transform;

            // 8 Perimeter Enemy Spawn Portals
            Vector3[] spawnCoords = new Vector3[]
            {
                new Vector3(-30f, 0.5f, 0f),
                new Vector3(30f, 0.5f, 0f),
                new Vector3(0f, 0.5f, 30f),
                new Vector3(0f, 0.5f, -30f),
                new Vector3(-25f, 0.5f, -25f),
                new Vector3(25f, 0.5f, -25f),
                new Vector3(-25f, 0.5f, 25f),
                new Vector3(25f, 0.5f, 25f)
            };

            for (int i = 0; i < spawnCoords.Length; i++)
            {
                GameObject sp = new GameObject($"EnemySpawn_{i}");
                sp.transform.SetParent(anchorsRoot.transform);
                sp.transform.position = spawnCoords[i];
                EnemySpawnPoints.Add(sp.transform);

                // Add visual horde gate beacon
                GameObject gateBeacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                gateBeacon.name = $"Spawn_Beacon_{i}";
                gateBeacon.transform.SetParent(sp.transform);
                gateBeacon.transform.localPosition = new Vector3(0, 0.1f, 0);
                gateBeacon.transform.localScale = new Vector3(2f, 0.1f, 2f);
                gateBeacon.GetComponent<Collider>().enabled = false;
                gateBeacon.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
            }
        }
    }
}
