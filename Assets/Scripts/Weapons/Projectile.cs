using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Effects;
using ShadowFire.Audio;
using ShadowFire.Player;

namespace ShadowFire.Weapons
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private bool isExplosive = false;
        [SerializeField] private float explosionRadius = 6f;
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private LayerMask hitLayers;

        private Vector3 _velocity;
        private float _spawnTime;
        private GameObject _owner;

        public void Initialize(Vector3 direction, float projSpeed, float projDamage, GameObject owner, bool explosive = false, float splashRadius = 0f, float knockback = 10f)
        {
            speed = projSpeed;
            damage = projDamage;
            _owner = owner;
            isExplosive = explosive;
            explosionRadius = splashRadius;
            knockbackForce = knockback;
            _velocity = direction.normalized * speed;
            _spawnTime = Time.time;
            transform.forward = direction;
        }

        private void Update()
        {
            if (Time.time - _spawnTime > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 step = _velocity * Time.deltaTime;
            float stepDistance = step.magnitude;

            if (Physics.Raycast(transform.position, _velocity.normalized, out RaycastHit hit, stepDistance + 0.1f, ~LayerMask.GetMask("Ignore Raycast")))
            {
                // Don't collide with owner
                if (_owner != null && hit.collider.gameObject == _owner)
                {
                    transform.position += step;
                    return;
                }

                OnImpact(hit);
                return;
            }

            transform.position += step;
        }

        private void OnImpact(RaycastHit hit)
        {
            if (isExplosive)
            {
                Explode(hit.point, hit.normal);
            }
            else
            {
                // Direct Hit
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    DamageInfo info = new DamageInfo(damage, hit.point, hit.normal, false, _owner, _velocity.normalized * knockbackForce);
                    damageable.TakeDamage(info);
                    if (PlayerStats.Instance != null && _owner == PlayerStats.Instance.gameObject)
                    {
                        PlayerStats.Instance.ApplyLifesteal(damage);
                    }
                }

                if (VFXManager.Instance != null)
                {
                    VFXManager.Instance.SpawnHitSparks(hit.point, hit.normal);
                }
            }

            Destroy(gameObject);
        }

        private void Explode(Vector3 center, Vector3 normal)
        {
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnExplosion(center);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion(center);
            }
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.AddTrauma(0.7f);
            }

            Collider[] hits = Physics.OverlapSphere(center, explosionRadius);
            foreach (var col in hits)
            {
                IDamageable target = col.GetComponentInParent<IDamageable>();
                if (target != null && target.IsAlive)
                {
                    float distance = Vector3.Distance(center, col.transform.position);
                    float damageMultiplier = Mathf.Clamp01(1f - (distance / explosionRadius));
                    float actualDamage = damage * damageMultiplier;

                    Vector3 knockDir = (col.transform.position - center).normalized;
                    DamageInfo info = new DamageInfo(actualDamage, col.transform.position, normal, false, _owner, knockDir * (knockbackForce * damageMultiplier), HitType.Explosive);
                    target.TakeDamage(info);

                    if (PlayerStats.Instance != null && _owner == PlayerStats.Instance.gameObject)
                    {
                        PlayerStats.Instance.ApplyLifesteal(actualDamage);
                    }
                }

                Rigidbody rb = col.attachedRigidbody;
                if (rb != null)
                {
                    rb.AddExplosionForce(knockbackForce * 100f, center, explosionRadius, 1f, ForceMode.Impulse);
                }
            }
        }
    }
}
