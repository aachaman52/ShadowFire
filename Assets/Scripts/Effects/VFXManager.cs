using System.Collections;
using UnityEngine;

namespace ShadowFire.Effects
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        public void SpawnMuzzleFlash(Vector3 position, Quaternion rotation, Color color)
        {
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "VFX_MuzzleFlash";
            flash.transform.position = position;
            flash.transform.localScale = Vector3.one * 0.22f;
            Destroy(flash.GetComponent<Collider>());

            var mr = flash.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * 3.5f);
            mr.material = mat;

            GameObject lightObj = new GameObject("Flash_Light");
            lightObj.transform.position = position;
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = 5f;
            l.intensity = 3f;

            Destroy(flash, 0.05f);
            Destroy(lightObj, 0.05f);
        }

        public void SpawnTracer(Vector3 start, Vector3 end, Color color)
        {
            GameObject tracerObj = new GameObject("VFX_Tracer");
            LineRenderer lr = tracerObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.04f;
            lr.endWidth = 0.02f;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * 3f);
            lr.material = mat;

            StartCoroutine(FadeAndDestroyLine(tracerObj, lr, 0.08f));
        }

        private IEnumerator FadeAndDestroyLine(GameObject obj, LineRenderer lr, float duration)
        {
            float timer = 0;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = 1f - (timer / duration);
                if (lr != null && lr.material != null)
                {
                    Color c = lr.material.color;
                    c.a = alpha;
                    lr.material.color = c;
                }
                yield return null;
            }
            if (obj != null) Destroy(obj);
        }

        public void SpawnHitSparks(Vector3 position, Vector3 normal)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spark.name = "VFX_Sparks";
            spark.transform.position = position;
            spark.transform.localScale = Vector3.one * 0.12f;
            Destroy(spark.GetComponent<Collider>());

            var mr = spark.GetComponent<MeshRenderer>();
            Material mat = ProceduralMeshGenerator.GetMaterial("glowgold");
            mr.material = mat;

            Destroy(spark, 0.1f);
        }

        public void SpawnBloodSplatter(Vector3 position, Vector3 normal)
        {
            GameObject blood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blood.name = "VFX_Blood";
            blood.transform.position = position;
            blood.transform.localScale = Vector3.one * 0.28f;
            Destroy(blood.GetComponent<Collider>());

            var mr = blood.GetComponent<MeshRenderer>();
            Material mat = ProceduralMeshGenerator.GetMaterial("glowred");
            mr.material = mat;

            Destroy(blood, 0.12f);
        }

        public void SpawnExplosion(Vector3 position, float scaleMultiplier = 1f)
        {
            GameObject exp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            exp.name = "VFX_Explosion";
            exp.transform.position = position;
            exp.transform.localScale = Vector3.one * 0.5f;
            Destroy(exp.GetComponent<Collider>());

            var mr = exp.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.4f, 0.1f);
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * 4f);
            mr.material = mat;

            GameObject expLight = new GameObject("Explosion_Light");
            expLight.transform.position = position;
            Light l = expLight.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.6f, 0.2f);
            l.range = 14f * scaleMultiplier;
            l.intensity = 6f;

            StartCoroutine(AnimateExplosion(exp, expLight, scaleMultiplier * 5.5f, 0.45f));
        }

        private IEnumerator AnimateExplosion(GameObject sphere, GameObject lightObj, float targetScale, float duration)
        {
            float timer = 0;
            Vector3 initScale = Vector3.one * 0.5f;
            Vector3 endScale = Vector3.one * targetScale;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                if (sphere != null)
                {
                    sphere.transform.localScale = Vector3.Lerp(initScale, endScale, Mathf.Sin(t * Mathf.PI * 0.5f));
                }
                yield return null;
            }

            if (sphere != null) Destroy(sphere);
            if (lightObj != null) Destroy(lightObj);
        }
    }
}
