using System.Collections.Generic;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.Weapons;
using ShadowFire.Audio;
using ShadowFire.Effects;

namespace ShadowFire.Managers
{
    public class LootDropManager : MonoBehaviour
    {
        public static LootDropManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        public void TryDropLoot(Vector3 position, float dropChance = 0.35f)
        {
            if (UnityEngine.Random.value > dropChance) return;

            float roll = UnityEngine.Random.value;
            PickupType type = PickupType.XpOrb;

            if (roll < 0.35f) type = PickupType.XpOrb;
            else if (roll < 0.60f) type = PickupType.AmmoBox;
            else if (roll < 0.85f) type = PickupType.HealthPack;
            else type = PickupType.ArmorPlate;

            SpawnPickup(type, position + Vector3.up * 0.5f);
        }

        public void SpawnPickup(PickupType type, Vector3 position)
        {
            GameObject pickupObj = new GameObject($"Pickup_{type}");
            pickupObj.transform.position = position;
            pickupObj.layer = LayerMask.NameToLayer("Pickup");

            // Visual geometry
            GameObject meshObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshObj.transform.SetParent(pickupObj.transform, false);
            meshObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Destroy(meshObj.GetComponent<Collider>());

            var mr = meshObj.GetComponent<MeshRenderer>();
            Material mat = null;

            switch (type)
            {
                case PickupType.HealthPack:
                    mat = ProceduralMeshGenerator.GetMaterial("glowgreen");
                    break;
                case PickupType.AmmoBox:
                    mat = ProceduralMeshGenerator.GetMaterial("glowgold");
                    break;
                case PickupType.ArmorPlate:
                    mat = ProceduralMeshGenerator.GetMaterial("glowcyan");
                    break;
                case PickupType.XpOrb:
                    mat = ProceduralMeshGenerator.GetMaterial("glowcyan");
                    meshObj.transform.localScale = Vector3.one * 0.35f;
                    break;
            }

            mr.material = mat;

            // Add light beacon
            GameObject lightObj = new GameObject("Pickup_Light");
            lightObj.transform.SetParent(pickupObj.transform, false);
            Light pLight = lightObj.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.range = 3.5f;
            pLight.intensity = 2f;
            pLight.color = mat.color;

            // Add Pickup Item Component
            PickupItem item = pickupObj.AddComponent<PickupItem>();
            item.Initialize(type);
        }
    }

    public class PickupItem : MonoBehaviour
    {
        private PickupType _type;
        private Transform _playerTransform;
        private float _spawnTime;
        private float _magnetRadius = 6f;
        private float _collectRadius = 1.2f;

        public void Initialize(PickupType type)
        {
            _type = type;
            _spawnTime = Time.time;
            if (PlayerController.Instance != null)
            {
                _playerTransform = PlayerController.Instance.transform;
            }
        }

        private void Update()
        {
            // Bob and spin
            transform.Rotate(Vector3.up * (90f * Time.deltaTime));
            float yOffset = Mathf.Sin((Time.time - _spawnTime) * 4f) * 0.12f;

            if (_playerTransform == null && PlayerController.Instance != null)
            {
                _playerTransform = PlayerController.Instance.transform;
            }

            if (_playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTransform.position);

                // Magnet pull
                if (dist <= _magnetRadius)
                {
                    transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position + Vector3.up, 12f * Time.deltaTime);
                }

                // Collection
                if (dist <= _collectRadius)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            switch (_type)
            {
                case PickupType.HealthPack:
                    if (PlayerStats.Instance != null) PlayerStats.Instance.Heal(35f);
                    break;
                case PickupType.AmmoBox:
                    if (WeaponManager.Instance != null) WeaponManager.Instance.RefillAllAmmo();
                    break;
                case PickupType.ArmorPlate:
                    if (PlayerStats.Instance != null) PlayerStats.Instance.AddArmor(20f);
                    break;
                case PickupType.XpOrb:
                    if (PlayerStats.Instance != null) PlayerStats.Instance.AddXp(35f);
                    break;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPickup();
            }

            Destroy(gameObject);
        }
    }
}
