using System;
using System.Collections;
using UnityEngine;
using ShadowFire.Core;
using ShadowFire.Audio;
using ShadowFire.Effects;
using ShadowFire.Weapons;

namespace ShadowFire.Enemies
{
    public class BossEnemy : EnemyBase
    {
        public static BossEnemy ActiveBoss { get; private set; }

        [Header("Boss Specifics")]
        [SerializeField] private float groundSlamRadius = 12f;
        [SerializeField] private float chargeSpeed = 16f;
        [SerializeField] private float specialAttackCooldown = 5.0f;
        private float _lastSpecialTime;

        [Header("Rage Phase")]
        [SerializeField] private bool isRaged = false;
        private float _rageThreshold = 0.30f;

        public event Action<float, float> OnBossHealthChanged;
        public event Action OnBossEnraged;

        protected override void Awake()
        {
            base.Awake();
            ActiveBoss = this;
            Type = EnemyType.Boss;
            EnemyName = "SHADOW OVERLORD";
            maxHealth = 1200f;
            moveSpeed = 4.2f;
            attackDamage = 45f;
            attackRange = 3.5f;
            attackCooldown = 1.6f;
            xpReward = 400f;
            lootDropChance = 1.0f;
        }

        private void OnDestroy()
        {
            if (ActiveBoss == this) ActiveBoss = null;
        }

        public override void Initialize(float healthMultiplier = 1f, float speedMultiplier = 1f, float damageMultiplier = 1f)
        {
            base.Initialize(healthMultiplier, speedMultiplier, damageMultiplier);
            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossRoar(transform.position);
            }
        }

        public override void TakeDamage(DamageInfo damageInfo)
        {
            base.TakeDamage(damageInfo);
            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);

            // Check Rage Mode
            if (!isRaged && (currentHealth / maxHealth) <= _rageThreshold)
            {
                TriggerRageMode();
            }
        }

        private void TriggerRageMode()
        {
            isRaged = true;
            moveSpeed *= 1.45f;
            attackCooldown *= 0.65f;
            specialAttackCooldown *= 0.6f;
            if (agent != null) agent.speed = moveSpeed;

            if (bodyRenderer != null)
            {
                bodyRenderer.material = ProceduralMeshGenerator.GetMaterial("glowred");
                originalBodyColor = bodyRenderer.material.color;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossRoar(transform.position);
            }
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.AddTrauma(0.8f);
            }

            OnBossEnraged?.Invoke();
            Debug.Log("[ShadowFire] BOSS HAS ENTERED RAGE MODE!");
        }

        protected override void UpdateStateMachine()
        {
            if (isDead || targetPlayer == null) return;

            float dist = Vector3.Distance(transform.position, targetPlayer.position);

            // Periodically unleash a special attack
            if (Time.time - _lastSpecialTime >= specialAttackCooldown)
            {
                _lastSpecialTime = Time.time;
                int pick = UnityEngine.Random.Range(0, 3);
                if (pick == 0) StartCoroutine(ChargeAttackRoutine());
                else if (pick == 1) ExecuteGroundSlam();
                else ExecuteProjectileBarrage();
                return;
            }

            base.UpdateStateMachine();
        }

        private void ExecuteGroundSlam()
        {
            if (characterAnimator != null)
            {
                characterAnimator.TriggerAttack(1, 1.2f);
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion(transform.position);
            if (CameraShake.Instance != null) CameraShake.Instance.AddTrauma(0.9f);
            if (VFXManager.Instance != null) VFXManager.Instance.SpawnExplosion(transform.position, 2.0f);

            Collider[] hits = Physics.OverlapSphere(transform.position, groundSlamRadius, LayerMask.GetMask("Player"));
            foreach (var col in hits)
            {
                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    DamageInfo dInfo = new DamageInfo(attackDamage * 1.2f, col.transform.position, Vector3.up, false, gameObject, (col.transform.position - transform.position).normalized * 22f, HitType.Explosive);
                    damageable.TakeDamage(dInfo);
                }
            }
        }

        private IEnumerator ChargeAttackRoutine()
        {
            if (targetPlayer == null) yield break;

            if (characterAnimator != null)
            {
                characterAnimator.TriggerAttack(0, 1.0f);
            }

            Vector3 chargeDir = (targetPlayer.position - transform.position).normalized;
            chargeDir.y = 0;

            if (agent != null && agent.isOnNavMesh) agent.isStopped = true;

            float chargeDuration = 1.0f;
            float timer = 0;

            while (timer < chargeDuration && !isDead)
            {
                timer += Time.deltaTime;
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.Move(chargeDir * (chargeSpeed * Time.deltaTime));
                }

                // Check collision with player
                if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.position) < 2.5f)
                {
                    IDamageable pDam = targetPlayer.GetComponent<IDamageable>();
                    if (pDam != null && pDam.IsAlive)
                    {
                        DamageInfo dInfo = new DamageInfo(attackDamage * 1.5f, targetPlayer.position, Vector3.up, false, gameObject, chargeDir * 25f, HitType.Melee);
                        pDam.TakeDamage(dInfo);
                    }
                    break;
                }

                yield return null;
            }

            if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        }

        private void ExecuteProjectileBarrage()
        {
            if (targetPlayer == null) return;

            if (characterAnimator != null)
            {
                characterAnimator.TriggerAttack(2, 0.8f);
            }

            Vector3 spawnOrigin = transform.position + Vector3.up * 2.5f;
            Vector3 baseDir = (targetPlayer.position - spawnOrigin).normalized;

            int count = isRaged ? 7 : 5;
            for (int i = 0; i < count; i++)
            {
                float angleOffset = (i - count / 2) * 12f;
                Vector3 dir = Quaternion.Euler(0, angleOffset, 0) * baseDir;

                GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                projObj.transform.position = spawnOrigin;
                projObj.transform.localScale = Vector3.one * 0.5f;
                projObj.GetComponent<MeshRenderer>().material = ProceduralMeshGenerator.GetMaterial("glowred");
                Destroy(projObj.GetComponent<Collider>());

                var proj = projObj.AddComponent<Projectile>();
                proj.Initialize(dir, 20f, attackDamage * 0.7f, gameObject, true, 4f, 10f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGunshot(WeaponType.RocketLauncher);
            }
        }

        protected override void Die(DamageInfo damageInfo)
        {
            ActiveBoss = null;
            // Spawn multiple loot drops on boss death
            if (LootDropManager.Instance != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 offset = UnityEngine.Random.insideUnitSphere * 3f;
                    offset.y = 0;
                    LootDropManager.Instance.TryDropLoot(transform.position + offset, 1.0f);
                }
            }

            base.Die(damageInfo);
        }
    }
}
