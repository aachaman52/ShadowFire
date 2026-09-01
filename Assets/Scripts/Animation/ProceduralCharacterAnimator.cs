using System.Collections;
using UnityEngine;
using ShadowFire.Core;

namespace ShadowFire.Animation
{
    public class ProceduralCharacterAnimator : MonoBehaviour
    {
        [Header("Skeletal Hierarchy")]
        public Transform RootBone;
        public Transform Pelvis;
        public Transform Spine;
        public Transform Chest;
        public Transform Head;

        public Transform LeftShoulder;
        public Transform LeftUpperArm;
        public Transform LeftForearm;
        public Transform LeftHand;

        public Transform RightShoulder;
        public Transform RightUpperArm;
        public Transform RightForearm;
        public Transform RightHand;

        public Transform LeftThigh;
        public Transform LeftShin;
        public Transform LeftFoot;

        public Transform RightThigh;
        public Transform RightShin;
        public Transform RightFoot;

        // Optional Wings for Boss
        public Transform LeftWing;
        public Transform RightWing;

        [Header("Animation State")]
        [SerializeField] private float speed = 0f;
        [SerializeField] private bool isAttacking = false;
        [SerializeField] private bool isDead = false;
        [SerializeField] private EnemyType enemyType = EnemyType.Zombie;

        private float _animTimer = 0f;
        private float _attackTimer = 0f;
        private float _attackDuration = 0.6f;
        private int _attackVariant = 0;
        private float _flinchTimer = 0f;
        private Vector3 _flinchOffset;
        private Vector3 _initialRootPos;

        private void Start()
        {
            if (RootBone != null) _initialRootPos = RootBone.localPosition;
        }

        public void SetSpeed(float currentSpeed)
        {
            speed = currentSpeed;
        }

        public void SetEnemyType(EnemyType type)
        {
            enemyType = type;
        }

        public void TriggerAttack(int variant = 0, float duration = 0.6f)
        {
            isAttacking = true;
            _attackTimer = 0f;
            _attackDuration = Mathf.Max(0.2f, duration);
            _attackVariant = variant;
        }

        public void TriggerFlinch(Vector3 hitDirection)
        {
            _flinchTimer = 0.15f;
            _flinchOffset = -hitDirection.normalized * 0.15f;
        }

        public void TriggerDeath()
        {
            isDead = true;
            StartCoroutine(DeathCollapseRoutine());
        }

        private void Update()
        {
            if (isDead) return;

            _animTimer += Time.deltaTime * (speed > 0.1f ? (speed * 1.6f) : 2.5f);

            AnimateBreathingAndLocomotion();

            if (isAttacking)
            {
                AnimateAttack();
            }

            if (_flinchTimer > 0)
            {
                _flinchTimer -= Time.deltaTime;
                if (Spine != null)
                {
                    Spine.localPosition = Vector3.Lerp(Spine.localPosition, _flinchOffset, Time.deltaTime * 18f);
                }
            }
            else if (Spine != null)
            {
                Spine.localPosition = Vector3.Lerp(Spine.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }

        private void AnimateBreathingAndLocomotion()
        {
            float stride = Mathf.Sin(_animTimer);
            float strideCos = Mathf.Cos(_animTimer);
            float isMoving = speed > 0.1f ? 1f : 0f;

            // 1. Spine / Pelvis bounce & lean
            if (Pelvis != null)
            {
                float bobY = Mathf.Abs(stride) * 0.08f * isMoving;
                Pelvis.localPosition = new Vector3(0, bobY, 0);
            }

            if (Spine != null)
            {
                float forwardLean = isMoving * (enemyType == EnemyType.Runner ? 25f : 12f);
                float breathTilt = Mathf.Sin(_animTimer * 0.8f) * 2.5f;
                Spine.localRotation = Quaternion.Euler(forwardLean + breathTilt, strideCos * 4f * isMoving, 0);
            }

            if (Head != null)
            {
                float headBob = Mathf.Sin(_animTimer * 1.2f) * 4f;
                Head.localRotation = Quaternion.Euler(-stride * 3f * isMoving + headBob, 0, 0);
            }

            // 2. Legs (Thighs & Shins)
            float legSwingAngle = (enemyType == EnemyType.Runner ? 40f : 28f) * isMoving;

            if (LeftThigh != null) LeftThigh.localRotation = Quaternion.Euler(stride * legSwingAngle, 0, 0);
            if (RightThigh != null) RightThigh.localRotation = Quaternion.Euler(-stride * legSwingAngle, 0, 0);

            if (LeftShin != null)
            {
                float shinBend = Mathf.Clamp(-stride * 30f * isMoving, 0, 45f);
                LeftShin.localRotation = Quaternion.Euler(shinBend, 0, 0);
            }
            if (RightShin != null)
            {
                float shinBend = Mathf.Clamp(stride * 30f * isMoving, 0, 45f);
                RightShin.localRotation = Quaternion.Euler(shinBend, 0, 0);
            }

            // 3. Arms (Locomotion swing if not attacking)
            if (!isAttacking)
            {
                float armSwingAngle = (enemyType == EnemyType.Zombie ? 12f : 30f) * isMoving;
                float zombieRaise = enemyType == EnemyType.Zombie ? -35f : 0f;

                if (LeftUpperArm != null) LeftUpperArm.localRotation = Quaternion.Euler(zombieRaise - stride * armSwingAngle, 0, -10f);
                if (RightUpperArm != null) RightUpperArm.localRotation = Quaternion.Euler(zombieRaise + stride * armSwingAngle, 0, 10f);

                if (LeftForearm != null) LeftForearm.localRotation = Quaternion.Euler(-20f + Mathf.Abs(stride) * 15f * isMoving, 0, 0);
                if (RightForearm != null) RightForearm.localRotation = Quaternion.Euler(-20f + Mathf.Abs(stride) * 15f * isMoving, 0, 0);
            }

            // 4. Wings (Boss Titan)
            if (LeftWing != null && RightWing != null)
            {
                float wingFlap = Mathf.Sin(_animTimer * 1.5f) * 25f;
                LeftWing.localRotation = Quaternion.Euler(15f, -30f + wingFlap, 0);
                RightWing.localRotation = Quaternion.Euler(15f, 30f - wingFlap, 0);
            }
        }

        private void AnimateAttack()
        {
            _attackTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_attackTimer / _attackDuration);
            float attackCurve = Mathf.Sin(progress * Mathf.PI);

            switch (enemyType)
            {
                case EnemyType.Zombie:
                case EnemyType.Runner:
                    // Melee Swipe / Lunge
                    if (RightUpperArm != null)
                    {
                        RightUpperArm.localRotation = Quaternion.Euler(-80f * attackCurve, 30f * attackCurve, 30f);
                    }
                    if (RightForearm != null)
                    {
                        RightForearm.localRotation = Quaternion.Euler(-45f * attackCurve, -40f * attackCurve, 0);
                    }
                    if (LeftUpperArm != null)
                    {
                        LeftUpperArm.localRotation = Quaternion.Euler(-40f * attackCurve, -20f * attackCurve, -20f);
                    }
                    break;

                case EnemyType.Tank:
                case EnemyType.Boss:
                    // Two-Handed Overhead Slam
                    float overheadAngle = Mathf.Lerp(0f, -120f, attackCurve);
                    if (LeftUpperArm != null) LeftUpperArm.localRotation = Quaternion.Euler(overheadAngle, 20f, -20f);
                    if (RightUpperArm != null) RightUpperArm.localRotation = Quaternion.Euler(overheadAngle, -20f, 20f);
                    if (Spine != null) Spine.localRotation = Quaternion.Euler(attackCurve * -25f, 0, 0);
                    break;

                case EnemyType.Shooter:
                    // Aim & Fire Recoil
                    if (RightUpperArm != null) RightUpperArm.localRotation = Quaternion.Euler(-70f + attackCurve * 20f, 15f, 0);
                    if (LeftUpperArm != null) LeftUpperArm.localRotation = Quaternion.Euler(-65f + attackCurve * 15f, -25f, 0);
                    break;
            }

            if (_attackTimer >= _attackDuration)
            {
                isAttacking = false;
            }
        }

        private IEnumerator DeathCollapseRoutine()
        {
            float timer = 0f;
            float duration = 0.8f;
            Vector3 initPelvis = Pelvis != null ? Pelvis.localPosition : Vector3.zero;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;

                if (Pelvis != null)
                {
                    Pelvis.localPosition = Vector3.Lerp(initPelvis, initPelvis - new Vector3(0, 0.8f, 0.4f), t);
                    Pelvis.localRotation = Quaternion.Slerp(Pelvis.localRotation, Quaternion.Euler(45f, 15f, 0), t);
                }

                if (Spine != null) Spine.localRotation = Quaternion.Slerp(Spine.localRotation, Quaternion.Euler(-30f, 20f, 0), t);
                if (LeftUpperArm != null) LeftUpperArm.localRotation = Quaternion.Slerp(LeftUpperArm.localRotation, Quaternion.Euler(30f, 0, -40f), t);
                if (RightUpperArm != null) RightUpperArm.localRotation = Quaternion.Slerp(RightUpperArm.localRotation, Quaternion.Euler(40f, 0, 50f), t);
                if (LeftThigh != null) LeftThigh.localRotation = Quaternion.Slerp(LeftThigh.localRotation, Quaternion.Euler(-60f, 20f, 0), t);
                if (RightThigh != null) RightThigh.localRotation = Quaternion.Slerp(RightThigh.localRotation, Quaternion.Euler(-40f, -30f, 0), t);

                yield return null;
            }
        }
    }
}
