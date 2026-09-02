using System.Collections;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Audio;
using ShadowFire.Weapons;
using ShadowFire.Effects;

namespace ShadowFire.Enemies
{
    public class RunnerEnemy : EnemyBase
    {
        [Header("Runner Gun Specs")]
        [SerializeField] private float fireRange = 12f;
        [SerializeField] private float projectileSpeed = 26f;

        protected override void Awake()
        {
            base.Awake();
            Type = EnemyType.Runner;
            EnemyName = "Shadow Skirmisher";
            maxHealth = 20f;
            moveSpeed = 5.2f;
            attackDamage = 2f;
            attackRange = 12f;
            attackCooldown = 2.0f;
            xpReward = 20f;
            lootDropChance = 0.45f;
        }

        protected override void UpdateStateMachine()
        {
            if (targetPlayer == null) return;

            float horizontalDist = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(targetPlayer.position.x, 0, targetPlayer.position.z)
            );

            // Keep moving toward or flanking player
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(targetPlayer.position);
            }

            if (horizontalDist <= fireRange)
            {
                SetState(EnemyState.Attack);

                // Aim at player
                Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 12f);
                }

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    StartCoroutine(FireSmgBurstRoutine());
                }
            }
            else
            {
                SetState(EnemyState.Chase);
            }
        }

        private IEnumerator FireSmgBurstRoutine()
        {
            lastAttackTime = Time.time;

            for (int i = 0; i < 2; i++)
            {
                if (targetPlayer == null || !IsAlive) break;

                Vector3 aimPos = targetPlayer.position + Vector3.up * 0.9f;
                Vector3 muzzlePos = transform.position + Vector3.up * 0.9f + transform.forward * 0.5f + transform.right * 0.15f;
                Vector3 fireDir = (aimPos - muzzlePos).normalized;

                fireDir += new Vector3(Random.Range(-0.06f, 0.06f), Random.Range(-0.06f, 0.06f), Random.Range(-0.06f, 0.06f));
                fireDir.Normalize();

                GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.transform.position = muzzlePos;
                projObj.transform.localScale = Vector3.one * 0.28f;
                projObj.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
                var col = projObj.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var proj = projObj.AddComponent<Projectile>();
                proj.Initialize(fireDir, projectileSpeed, attackDamage, gameObject, false, 0f, 4f);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayGunshot(WeaponType.SMG);
                }

                yield return new WaitForSeconds(0.12f);
            }
        }
    }
}
