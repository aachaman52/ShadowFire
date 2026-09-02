using System.Collections;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Audio;
using ShadowFire.Weapons;
using ShadowFire.Effects;

namespace ShadowFire.Enemies
{
    public class ZombieEnemy : EnemyBase
    {
        [Header("Ranged Specs")]
        [SerializeField] private float preferredDistance = 10f;
        [SerializeField] private float projectileSpeed = 22f;
        [SerializeField] private int burstCount = 2;
        [SerializeField] private float burstInterval = 0.2f;
        private bool _isFiringBurst = false;

        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Zombie;
            EnemyName = "Shadow Rifleman";
            maxHealth = 35f;
            moveSpeed = 2.8f;
            attackDamage = 3f;
            attackRange = 16f;
            attackCooldown = 2.5f;
            xpReward = 15f;
            lootDropChance = 0.50f;
        }

        protected override void UpdateStateMachine()
        {
            if (targetPlayer == null || _isFiringBurst) return;

            float horizontalDist = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(targetPlayer.position.x, 0, targetPlayer.position.z)
            );

            if (horizontalDist > preferredDistance + 4f)
            {
                // Advance closer to combat range
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(targetPlayer.position);
                }
                SetState(EnemyState.Chase);
            }
            else if (horizontalDist < preferredDistance - 4f && horizontalDist > 3.5f)
            {
                // Back away slightly to keep firing angle
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    Vector3 retreatDir = (transform.position - targetPlayer.position).normalized;
                    retreatDir.y = 0;
                    agent.isStopped = false;
                    agent.SetDestination(transform.position + retreatDir * 4f);
                }
            }
            else
            {
                // In combat range: stop, aim, and shoot!
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
                SetState(EnemyState.Attack);

                // Aim directly at player
                Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
                }

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    StartCoroutine(FireRifleBurstRoutine());
                }
            }
        }

        private IEnumerator FireRifleBurstRoutine()
        {
            _isFiringBurst = true;
            lastAttackTime = Time.time;

            for (int i = 0; i < burstCount; i++)
            {
                if (targetPlayer == null || !IsAlive) break;

                // Face player
                Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(lookDir);

                // Spawn projectile
                Vector3 aimPos = targetPlayer.position + Vector3.up * 1.0f;
                Vector3 muzzlePos = transform.position + Vector3.up * 1.1f + transform.forward * 0.6f + transform.right * 0.2f;
                Vector3 fireDir = (aimPos - muzzlePos).normalized;

                // Add slight spread
                fireDir += new Vector3(Random.Range(-0.04f, 0.04f), Random.Range(-0.04f, 0.04f), Random.Range(-0.04f, 0.04f));
                fireDir.Normalize();

                GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.transform.position = muzzlePos;
                projObj.transform.localScale = Vector3.one * 0.35f;
                projObj.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
                var col = projObj.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var proj = projObj.AddComponent<Projectile>();
                proj.Initialize(fireDir, projectileSpeed, attackDamage, gameObject, false, 0f, 5f);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayGunshot(WeaponType.Rifle);
                }

                yield return new WaitForSeconds(burstInterval);
            }

            _isFiringBurst = false;
        }
    }
}
