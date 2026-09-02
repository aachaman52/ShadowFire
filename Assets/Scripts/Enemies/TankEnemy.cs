using System.Collections;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Audio;
using ShadowFire.Effects;
using ShadowFire.Player;
using ShadowFire.Weapons;

namespace ShadowFire.Enemies
{
    public class TankEnemy : EnemyBase
    {
        [SerializeField] private float armorReduction = 0.35f;
        [SerializeField] private float fireRange = 15f;
        [SerializeField] private float projectileSpeed = 28f;

        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Tank;
            EnemyName = "Goliath Gunner";
            maxHealth = 150f;
            moveSpeed = 2.0f;
            attackDamage = 8f;
            attackRange = 15f;
            attackCooldown = 3.0f;
            xpReward = 75f;
            lootDropChance = 0.75f;
        }

        public override void TakeDamage(DamageInfo damageInfo)
        {
            damageInfo.Amount *= (1f - armorReduction);
            base.TakeDamage(damageInfo);
        }

        public override void ApplyKnockback(Vector3 force)
        {
            base.ApplyKnockback(force * 0.1f);
        }

        protected override void UpdateStateMachine()
        {
            if (targetPlayer == null) return;

            float horizontalDist = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(targetPlayer.position.x, 0, targetPlayer.position.z)
            );

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(targetPlayer.position);
            }

            if (horizontalDist <= fireRange)
            {
                SetState(EnemyState.Attack);

                Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 6f);
                }

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    StartCoroutine(FireHeavySpreadRoutine());
                }
            }
            else
            {
                SetState(EnemyState.Chase);
            }
        }

        private IEnumerator FireHeavySpreadRoutine()
        {
            lastAttackTime = Time.time;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGunshot(WeaponType.Shotgun);
            }
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.AddTrauma(0.3f);
            }

            Vector3 aimPos = targetPlayer.position + Vector3.up * 1.1f;
            Vector3 muzzlePos = transform.position + Vector3.up * 1.2f + transform.forward * 0.8f;
            Vector3 baseDir = (aimPos - muzzlePos).normalized;

            // Fire 4 heavy pellets
            for (int i = 0; i < 4; i++)
            {
                Vector3 spreadDir = baseDir + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.06f, 0.06f), Random.Range(-0.1f, 0.1f));
                spreadDir.Normalize();

                GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.transform.position = muzzlePos;
                projObj.transform.localScale = Vector3.one * 0.45f;
                projObj.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
                var col = projObj.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var proj = projObj.AddComponent<Projectile>();
                proj.Initialize(spreadDir, projectileSpeed, attackDamage * 0.5f, gameObject, false, 0f, 5f);
            }

            yield return null;
        }
    }
}
