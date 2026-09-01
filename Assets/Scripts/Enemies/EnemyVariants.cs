using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Audio;
using ShadowFire.Effects;
using ShadowFire.Animation;
using ShadowFire.Models;

namespace ShadowFire.Enemies
{
    public class ZombieEnemy : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Zombie;
            EnemyName = "Zombie";
            maxHealth = 60f;
            moveSpeed = 3.5f;
            attackDamage = 18f;
            attackRange = 1.8f;
            attackCooldown = 1.1f;
            xpReward = 15f;
            lootDropChance = 0.20f;
        }
    }

    public class RunnerEnemy : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Runner;
            EnemyName = "Shadow Runner";
            maxHealth = 35f;
            moveSpeed = 6.8f;
            attackDamage = 12f;
            attackRange = 1.6f;
            attackCooldown = 0.75f;
            xpReward = 20f;
            lootDropChance = 0.25f;
        }

        protected override void PerformAttack()
        {
            base.PerformAttack();
            // Quick lunge
            if (agent != null && agent.isOnNavMesh)
            {
                agent.Move(transform.forward * 1.5f);
            }
        }
    }

    public class TankEnemy : EnemyBase
    {
        [SerializeField] private float slamRadius = 4.5f;
        [SerializeField] private float armorReduction = 0.35f; // Absorbs 35% damage

        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Tank;
            EnemyName = "Goliath Tank";
            maxHealth = 320f;
            moveSpeed = 2.4f;
            attackDamage = 35f;
            attackRange = 2.8f;
            attackCooldown = 2.0f;
            xpReward = 75f;
            lootDropChance = 0.60f;
        }

        public override void TakeDamage(DamageInfo damageInfo)
        {
            // Tank passive armor reduction
            damageInfo.Amount *= (1f - armorReduction);
            base.TakeDamage(damageInfo);
        }

        public override void ApplyKnockback(Vector3 force)
        {
            // Tank resists 90% knockback
            base.ApplyKnockback(force * 0.1f);
        }

        protected override void PerformAttack()
        {
            lastAttackTime = Time.time;

            if (characterAnimator != null)
            {
                characterAnimator.TriggerAttack(1, 0.8f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion(transform.position);
            }
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.AddTrauma(0.5f);
            }
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnExplosion(transform.position, 0.6f);
            }

            // Area slam
            Collider[] hits = Physics.OverlapSphere(transform.position, slamRadius, LayerMask.GetMask("Player"));
            foreach (var col in hits)
            {
                var damageable = col.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    DamageInfo dInfo = new DamageInfo(attackDamage, col.transform.position, Vector3.up, false, gameObject, (col.transform.position - transform.position).normalized * 15f, HitType.Melee);
                    damageable.TakeDamage(dInfo);
                }
            }
        }
    }

    public class ShooterEnemy : EnemyBase
    {
        [Header("Shooter Specs")]
        [SerializeField] private float preferredDistance = 14f;
        [SerializeField] private float projectileSpeed = 25f;

        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Shooter;
            EnemyName = "Shadow Spitter";
            maxHealth = 50f;
            moveSpeed = 3.2f;
            attackDamage = 14f;
            attackRange = 16f;
            attackCooldown = 1.8f;
            xpReward = 30f;
            lootDropChance = 0.35f;
        }

        protected override void UpdateStateMachine()
        {
            if (targetPlayer == null) return;
            float dist = Vector3.Distance(transform.position, targetPlayer.position);

            if (dist > preferredDistance + 3f)
            {
                // Advance closer
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(targetPlayer.position);
                }
                SetState(EnemyState.Chase);
            }
            else if (dist < preferredDistance - 4f)
            {
                // Back away to maintain standoff distance
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    Vector3 retreatDir = (transform.position - targetPlayer.position).normalized;
                    Vector3 retreatPos = transform.position + retreatDir * 5f;
                    agent.isStopped = false;
                    agent.SetDestination(retreatPos);
                }
            }
            else
            {
                // In sweet spot, shoot!
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
                SetState(EnemyState.Attack);

                Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 8f);
                }

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    PerformAttack();
                }
            }
        }

        protected override void PerformAttack()
        {
            lastAttackTime = Time.time;

            if (characterAnimator != null)
            {
                characterAnimator.TriggerAttack(2, 0.4f);
            }

            if (targetPlayer == null) return;

            Vector3 aimPos = targetPlayer.position + Vector3.up * 1.2f;
            Vector3 spawnPos = transform.position + Vector3.up * 1.5f + transform.forward * 0.8f;
            Vector3 dir = (aimPos - spawnPos).normalized;

            GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projObj.transform.position = spawnPos;
            projObj.transform.localScale = Vector3.one * 0.4f;
            projObj.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
            Destroy(projObj.GetComponent<Collider>());

            var proj = projObj.AddComponent<ShadowFire.Weapons.Projectile>();
            proj.Initialize(dir, projectileSpeed, attackDamage, gameObject, false, 0f, 6f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGunshot(WeaponType.SMG);
            }
        }
    }
}
