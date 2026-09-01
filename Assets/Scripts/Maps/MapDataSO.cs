using UnityEngine;

namespace ShadowFire.Maps
{
    public enum MapTheme
    {
        OutpostRuin,
        ToxicBiolab,
        InfernoCrater
    }

    [CreateAssetMenu(fileName = "NewMapData", menuName = "ShadowFire/Map Data")]
    public class MapDataSO : ScriptableObject
    {
        public string MapName = "Outpost Ruin";
        public MapTheme Theme = MapTheme.OutpostRuin;
        [TextArea(2, 4)]
        public string Description = "Fortified industrial outpost ruins under moonlit siege.";

        [Header("Atmosphere & Lighting")]
        public Color SkyAmbientColor = new Color(0.15f, 0.2f, 0.3f);
        public Color DirectionalLightColor = new Color(0.4f, 0.5f, 0.75f);
        public float DirectionalIntensity = 0.55f;
        public Color FogColor = new Color(0.06f, 0.08f, 0.12f);
        public float FogDensity = 0.015f;

        [Header("Environment Colors")]
        public Color GroundColor = new Color(0.12f, 0.14f, 0.16f);
        public Color WallColor = new Color(0.18f, 0.20f, 0.22f);
        public Color AccentColor = new Color(0.2f, 0.8f, 1.0f);

        [Header("Hazards")]
        public bool HasEnvironmentalHazards = false;
        public float HazardDamagePerSecond = 15f;
    }
}
