using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using ShadowFire.Core;
using ShadowFire.Player;
using ShadowFire.Audio;
using ShadowFire.Effects;
using ShadowFire.Managers;

namespace ShadowFire.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBase : MonoBehaviour, IDamageable, IKnockbackable
    {
        [Header("Identity & Type")]
        public EnemyType Type = EnemyType.Zombie;
        public string EnemyName = "Zombie";

        [Header("Stats")]
        [SerializeField] protected float maxHealth = 60f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 3.8f;
        [SerializeField] protected float attackDamage = 15f;
        [SerializeField] protected float attackRange = 1.8f;
        [SerializeField] protected float attackCooldown = 1.2f;
        [SerializeField] protected float aggroRange = 35f;
        [SerializeField] protected float xpReward = 20f;
        [SerializeField] protected float lootDropChance = 0.25f;

        [Header("Visual Feedback")]
        [SerializeField] protected MeshRenderer bodyRenderer;
        protected Color originalBodyColor;

        protected NavMeshAgent agent;
        protected Transform targetPlayer;
        protected EnemyState currentState = EnemyState.Idle;
        protected float lastAttackTime;
        protected bool isDead = false;
        private Coroutine _flinchRoutine;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => !isDead && currentHealth > 0;
        public EnemyState State => currentState;
        public NavMeshAgent Agent => agent;

        public event Action<DamageInfo> OnDamaged;
        public event Action<DamageInfo> OnDied;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<MeshRenderer>();
            if (bodyRenderer != null && bodyRenderer.material != null)
            {
                originalBodyColor = bodyRenderer.material.color;
            }
        }

        public virtual void Initialize(float healthMultiplier = 1f, float speedMultiplier = 1f, float damageMultiplier = 1f)
        {
            maxHealth *= healthMultiplier;
            currentHealth = maxHealth;
            moveSpeed *= speedMultiplier;
            attackDamage *= damageMultiplier;
            isDead = false;

            if (agent != null)
            {
                agent.speed = moveSpeed;
                agent.stoppingDistance = Mathf.Max(0.5f, attackRange * 0.8f);
                agent.isStopped = false;
            }

            if (PlayerController.Instance != null)
            {
                targetPlayer = PlayerController.Instance.transform;
            }

            SetState(EnemyState.Chase);
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (targetPlayer == null && PlayerController.Instance != null)
            {
                targetPlayer = PlayerController.Instance.transform;
            }

            UpdateStateMachine();
        }

        protected virtual void UpdateStateMachine()
        {
            if (targetPlayer == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            switch (currentState)
            {
                case EnemyState.Idle:
                case EnemyState.Patrol:
                    if (distanceToPlayer <= aggroRange)
                    {
                        SetState(EnemyState.Chase);
                    }
                    break;

                case EnemyState.Chase:
                    if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(targetPlayer.position);
                    }

                    if (distanceToPlayer <= attackRange)
                    {
                        SetState(EnemyState.Attack);
                    }
                    break;

                case EnemyState.Attack:
                    if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                    {
                        agent.isStopped = true;
                    }

                    // Face player while attacking
                    Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
                    lookDir.y = 0;
                    if (lookDir.sqrMagnitude > 0.001f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
                    }

                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        PerformAttack();
                    }

                    if (distanceToPlayer > attackRange * 1.3f)
                    {
                        SetState(EnemyState.Chase);
                    }
                    break;
            }
        }

        protected virtual void PerformAttack()
        {
            lastAttackTime = Time.time;

            if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.position) <= attackRange * 1.3f)
            {
                IDamageable playerDamageable = targetPlayer.GetComponent<IDamageable>();
                if (playerDamageable != null && playerDamageable.IsAlive)
                {
                    DamageInfo dInfo = new DamageInfo(attackDamage, targetPlayer.position, Vector3.up, false, gameObject, transform.forward * 5f, HitType.Melee);
                    playerDamageable.TakeDamage(dInfo);
                }
            }
        }

        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (isDead) return;

            currentHealth -= damageInfo.Amount;
            OnDamaged?.Invoke(damageInfo);

            // Trigger floating damage number
            if (DamageNumberManager.Instance != null)
            {
                DamageNumberManager.Instance.ShowDamageNumber(damageInfo.Amount, transform.position + Vector3.up * 1.8f, damageInfo.IsCritical);
            }

            // Trigger additive visual flinch flash without freezing AI state machine
            TriggerFlinchFlash();

            // Apply knockback
            if (damageInfo.KnockbackForce.sqrMagnitude > 0.1f)
            {
                ApplyKnockback(damageInfo.KnockbackForce);
            }

            if (currentHealth <= 0)
            {
                Die(damageInfo);
            }
        }

        public virtual void Heal(float amount)
        {
            if (isDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        public virtual void ApplyKnockback(Vector3 force)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.Move(force * 0.05f);
            }
        }

        protected void TriggerFlinchFlash()
        {
            if (bodyRenderer == null) return;
            if (_flinchRoutine != null) StopCoroutine(_flinchRoutine);
            _flinchRoutine = StartCoroutine(FlinchFlashRoutine());
        }

        private IEnumerator FlinchFlashRoutine()
        {
            bodyRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            if (bodyRenderer != null && !isDead)
            {
                bodyRenderer.material.color = originalBodyColor;
            }
        }

        protected virtual void Die(DamageInfo damageInfo)
        {
            if (isDead) return;
            isDead = true;
            currentState = EnemyState.Dead;

            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            OnDied?.Invoke(damageInfo);

            // Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyDeath(transform.position);
            }

            // VFX
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.SpawnBloodSplatter(transform.position + Vector3.up, Vector3.up);
            }

            // Award XP to player
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.AddXp(xpReward);
            }

            // Notify WaveManager
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.RegisterEnemyKilled(this);
            }

            // Drop loot
            if (LootDropManager.Instance != null)
            {
                LootDropManager.Instance.TryDropLoot(transform.position, lootDropChance);
            }

            // Clean up
            StartCoroutine(DeathDisappearRoutine());
        }

        private IEnumerator DeathDisappearRoutine()
        {
            // Sink slightly or scale down
            float timer = 0;
            Vector3 origScale = transform.localScale;
            while (timer < 0.6f)
            {
                timer += Time.deltaTime;
                transform.localScale = Vector3.Lerp(origScale, Vector3.zero, timer / 0.6f);
                yield return null;
            }

            Destroy(gameObject);
        }

        public void SetState(EnemyState newState)
        {
            currentState = newState;
        }
    }
}
