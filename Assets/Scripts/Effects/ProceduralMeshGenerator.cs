using UnityEngine;

namespace ShadowFire.Effects
{
    public static class ProceduralMeshGenerator
    {
        private static Material _defaultLitMat;
        private static Material _gunMetalMat;
        private static Material _enemyMat;
        private static Material _bossMat;
        private static Material _glowCyanMat;
        private static Material _glowRedMat;
        private static Material _glowGoldMat;
        private static Material _glowGreenMat;

        public static Material GetMaterial(string type)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            switch (type.ToLower())
            {
                case "gunmetal":
                    if (_gunMetalMat == null)
                    {
                        _gunMetalMat = new Material(shader) { name = "M_GunMetal" };
                        _gunMetalMat.color = new Color(0.18f, 0.2f, 0.24f);
                        if (_gunMetalMat.HasProperty("_Metallic")) _gunMetalMat.SetFloat("_Metallic", 0.85f);
                        if (_gunMetalMat.HasProperty("_Smoothness")) _gunMetalMat.SetFloat("_Smoothness", 0.7f);
                    }
                    return _gunMetalMat;

                case "enemy":
                    if (_enemyMat == null)
                    {
                        _enemyMat = new Material(shader) { name = "M_EnemyZombie" };
                        _enemyMat.color = new Color(0.35f, 0.45f, 0.32f);
                        if (_enemyMat.HasProperty("_Smoothness")) _enemyMat.SetFloat("_Smoothness", 0.2f);
                    }
                    return _enemyMat;

                case "boss":
                    if (_bossMat == null)
                    {
                        _bossMat = new Material(shader) { name = "M_BossDemon" };
                        _bossMat.color = new Color(0.55f, 0.1f, 0.1f);
                        if (_bossMat.HasProperty("_Metallic")) _bossMat.SetFloat("_Metallic", 0.6f);
                        if (_bossMat.HasProperty("_Smoothness")) _bossMat.SetFloat("_Smoothness", 0.8f);
                    }
                    return _bossMat;

                case "glowcyan":
                    if (_glowCyanMat == null)
                    {
                        _glowCyanMat = new Material(shader) { name = "M_GlowCyan" };
                        _glowCyanMat.color = new Color(0.1f, 0.8f, 1f);
                        _glowCyanMat.EnableKeyword("_EMISSION");
                        if (_glowCyanMat.HasProperty("_EmissionColor"))
                            _glowCyanMat.SetColor("_EmissionColor", new Color(0.1f, 0.8f, 1f) * 2.5f);
                    }
                    return _glowCyanMat;

                case "glowred":
                    if (_glowRedMat == null)
                    {
                        _glowRedMat = new Material(shader) { name = "M_GlowRed" };
                        _glowRedMat.color = new Color(1f, 0.15f, 0.15f);
                        _glowRedMat.EnableKeyword("_EMISSION");
                        if (_glowRedMat.HasProperty("_EmissionColor"))
                            _glowRedMat.SetColor("_EmissionColor", new Color(1f, 0.15f, 0.15f) * 3f);
                    }
                    return _glowRedMat;

                case "glowgold":
                    if (_glowGoldMat == null)
                    {
                        _glowGoldMat = new Material(shader) { name = "M_GlowGold" };
                        _glowGoldMat.color = new Color(1f, 0.8f, 0.2f);
                        _glowGoldMat.EnableKeyword("_EMISSION");
                        if (_glowGoldMat.HasProperty("_EmissionColor"))
                            _glowGoldMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.2f) * 2.5f);
                    }
                    return _glowGoldMat;

                case "glowgreen":
                    if (_glowGreenMat == null)
                    {
                        _glowGreenMat = new Material(shader) { name = "M_GlowGreen" };
                        _glowGreenMat.color = new Color(0.2f, 1f, 0.3f);
                        _glowGreenMat.EnableKeyword("_EMISSION");
                        if (_glowGreenMat.HasProperty("_EmissionColor"))
                            _glowGreenMat.SetColor("_EmissionColor", new Color(0.2f, 1f, 0.3f) * 2.5f);
                    }
                    return _glowGreenMat;

                default:
                    if (_defaultLitMat == null)
                    {
                        _defaultLitMat = new Material(shader) { name = "M_DefaultArena" };
                        _defaultLitMat.color = new Color(0.25f, 0.27f, 0.3f);
                    }
                    return _defaultLitMat;
            }
        }

        public static GameObject CreateCrate(Vector3 position, Vector3 size)
        {
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "Prop_Crate";
            crate.transform.position = position;
            crate.transform.localScale = size;
            crate.layer = LayerMask.NameToLayer("Props");

            var mr = crate.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.42f, 0.32f, 0.22f);
            mr.material = mat;

            return crate;
        }

        public static GameObject CreateBarrel(Vector3 position, bool explosive = true)
        {
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = explosive ? "Prop_ExplosiveBarrel" : "Prop_Barrel";
            barrel.transform.position = position;
            barrel.transform.localScale = new Vector3(0.9f, 1.2f, 0.9f);
            barrel.layer = LayerMask.NameToLayer("Props");

            var mr = barrel.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = explosive ? new Color(0.85f, 0.2f, 0.15f) : new Color(0.3f, 0.35f, 0.4f);
            if (explosive)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(0.8f, 0.2f, 0.1f) * 0.8f);
            }
            mr.material = mat;

            return barrel;
        }
    }
}
