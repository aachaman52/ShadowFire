using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Environment;
using ShadowFire.Managers;

namespace ShadowFire.Maps
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [Header("Active Map")]
        [SerializeField] private MapTheme currentMapTheme = MapTheme.OutpostRuin;
        [SerializeField] private Transform mapRoot;

        public Transform PlayerSpawnPoint { get; private set; }
        public Transform BossSpawnPoint { get; private set; }
        public List<Transform> EnemySpawnPoints { get; private set; } = new List<Transform>();
        public MapTheme CurrentTheme => currentMapTheme;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        public void LoadMap(MapTheme theme)
        {
            currentMapTheme = theme;

            if (mapRoot == null)
            {
                mapRoot = new GameObject("Environment_ActiveMap").transform;
            }

            // Clear old map
            for (int i = mapRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(mapRoot.GetChild(i).gameObject);
            }

            MapDataSO mapData = ScriptableObject.CreateInstance<MapDataSO>();
            mapData.Theme = theme;

            Transform pSpawn, bSpawn;
            List<Transform> eSpawns;

            switch (theme)
            {
                case MapTheme.ToxicBiolab:
                    mapData.MapName = "Toxic Biolab";
                    mapData.SkyAmbientColor = new Color(0.1f, 0.25f, 0.15f);
                    mapData.DirectionalLightColor = new Color(0.2f, 0.9f, 0.4f);
                    mapData.FogColor = new Color(0.04f, 0.15f, 0.08f);
                    mapData.FogDensity = 0.025f;
                    mapData.AccentColor = new Color(0.2f, 1f, 0.4f);
                    MapGeometryFactory.BuildToxicBiolab(mapRoot, mapData, out pSpawn, out bSpawn, out eSpawns);
                    break;

                case MapTheme.InfernoCrater:
                    mapData.MapName = "Inferno Crater";
                    mapData.SkyAmbientColor = new Color(0.35f, 0.12f, 0.08f);
                    mapData.DirectionalLightColor = new Color(1f, 0.4f, 0.2f);
                    mapData.FogColor = new Color(0.18f, 0.05f, 0.03f);
                    mapData.FogDensity = 0.02f;
                    mapData.AccentColor = new Color(1f, 0.3f, 0.1f);
                    MapGeometryFactory.BuildInfernoCrater(mapRoot, mapData, out pSpawn, out bSpawn, out eSpawns);
                    break;

                default:
                    mapData.MapName = "Outpost Ruin";
                    mapData.SkyAmbientColor = new Color(0.15f, 0.2f, 0.3f);
                    mapData.DirectionalLightColor = new Color(0.4f, 0.5f, 0.75f);
                    mapData.FogColor = new Color(0.06f, 0.08f, 0.12f);
                    mapData.FogDensity = 0.015f;
                    mapData.AccentColor = new Color(0.2f, 0.8f, 1f);
                    MapGeometryFactory.BuildOutpostRuin(mapRoot, mapData, out pSpawn, out bSpawn, out eSpawns);
                    break;
            }

            PlayerSpawnPoint = pSpawn;
            BossSpawnPoint = bSpawn;
            EnemySpawnPoints = eSpawns;

            // Apply Atmosphere
            RenderSettings.ambientSkyColor = mapData.SkyAmbientColor;
            RenderSettings.fogColor = mapData.FogColor;
            RenderSettings.fogDensity = mapData.FogDensity;
            RenderSettings.fog = true;

            // Bake NavMesh
            NavMeshRuntimeBaker baker = mapRoot.GetComponent<NavMeshRuntimeBaker>() ?? mapRoot.gameObject.AddComponent<NavMeshRuntimeBaker>();
            baker.BakeNavMesh();

            // Link to WaveManager if active
            if (WaveManager.Instance != null && WaveManager.Instance.Arena != null)
            {
                WaveManager.Instance.Arena.PlayerSpawnPoint = PlayerSpawnPoint;
                WaveManager.Instance.Arena.BossSpawnPoint = BossSpawnPoint;
                WaveManager.Instance.Arena.EnemySpawnPoints = EnemySpawnPoints;
            }

            Debug.Log($"[ShadowFire] Map Loaded: {mapData.MapName} ({theme})");
        }
    }
}
