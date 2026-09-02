using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Audio;
using ShadowFire.Effects;

namespace ShadowFire.Enemies
{
    public class ShooterEnemy : EnemyBase
    {
        [Header("Shooter Specs")]
        [SerializeField] private float preferredDistance = 14f;
        [SerializeField] private float projectileSpeed = 25f;

        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Shooter;
            EnemyName = "Shadow Sniper";
            maxHealth = 30f;
            moveSpeed = 3.0f;
            attackDamage = 5f;
            attackRange = 18f;
            attackCooldown = 3.2f;
            xpReward = 30f;
            lootDropChance = 0.55f;
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
            var col = projObj.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }

            var proj = projObj.AddComponent<ShadowFire.Weapons.Projectile>();
            proj.Initialize(dir, projectileSpeed, attackDamage, gameObject, false, 0f, 6f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGunshot(WeaponType.SMG);
            }
        }
    }
}
