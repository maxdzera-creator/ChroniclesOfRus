using System;
using ChroniclesOfRus.Combat;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyCombat : MonoBehaviour
    {
        private const int HitBufferSize = 16;

        [SerializeField, Min(0f)] private float attackDamage = 20f;
        [SerializeField, Min(0.01f)] private float attackDuration = 0.8f;
        [SerializeField, Min(0f)] private float attackActiveStart = 0.35f;
        [SerializeField, Min(0f)] private float attackActiveEnd = 0.5f;
        [SerializeField, Min(0f)] private float attackCooldown = 1f;
        [SerializeField, Min(0f)] private float attackRange = 1.6f;
        [SerializeField, Min(0.01f)] private float attackRadius = 0.7f;
        [SerializeField, Min(0f)] private float attackRotationSpeed = 720f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool enableDebugLogs = true;

        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private float elapsedTime;
        private float remainingCooldown;
        private bool hitWindowOpen;
        private bool hitAttempted;

        public float AttackRange => attackRange;
        public float AttackRotationSpeed => attackRotationSpeed;
        public bool CanAttack => remainingCooldown <= 0f;
        public bool IsAttacking { get; private set; }

        public event Action AttackStarted;
        public event Action AttackHitWindowOpened;
        public event Action<DamageInfo> AttackHit;
        public event Action AttackHitWindowClosed;
        public event Action AttackEnded;

        private void Update()
        {
            if (remainingCooldown > 0f)
                remainingCooldown = Mathf.Max(0f, remainingCooldown - Time.deltaTime);
        }

        public void BeginAttack()
        {
            if (IsAttacking || !CanAttack)
                return;

            IsAttacking = true;
            elapsedTime = 0f;
            hitAttempted = false;
            SetHitWindow(false);
            Log("Enemy attack started");
            AttackStarted?.Invoke();
        }

        public bool TickAttack(float deltaTime, Transform target, IDamageable damageable)
        {
            if (!IsAttacking)
                return true;

            elapsedTime = Mathf.Min(elapsedTime + deltaTime, attackDuration);
            bool shouldBeOpen = elapsedTime >= attackActiveStart && elapsedTime < attackActiveEnd;
            SetHitWindow(shouldBeOpen);

            if (hitWindowOpen && !hitAttempted)
                PerformHitDetection(target, damageable);

            if (elapsedTime < attackDuration)
                return false;

            FinishAttack();
            return true;
        }

        public void CancelAttack()
        {
            if (!IsAttacking)
                return;
            FinishAttack();
        }

        public void DisableCombat()
        {
            CancelAttack();
            enabled = false;
        }

        private void PerformHitDetection(Transform target, IDamageable damageable)
        {
            if (target == null || damageable == null || !damageable.IsAlive)
                return;

            Vector3 center = transform.position + Vector3.up * attackRadius +
                transform.forward * attackRange;
            int count = Physics.OverlapSphereNonAlloc(
                center, attackRadius, hitBuffer, Physics.AllLayers, QueryTriggerInteraction.Collide);

            Collider targetCollider = null;
            for (int i = 0; i < count; i++)
            {
                Collider candidate = hitBuffer[i];
                if (candidate != null && candidate.transform.root == target.root)
                {
                    targetCollider = candidate;
                    break;
                }
            }

            if (targetCollider == null)
                return;

            hitAttempted = true;
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            DamageInfo info = new(
                attackDamage,
                gameObject,
                targetCollider.ClosestPoint(transform.position),
                direction,
                damageType);
            bool canApply = damageable.CanReceiveDamage && attackDamage > 0f;
            damageable.ReceiveDamage(info);

            if (canApply)
            {
                Log($"Enemy attack hit Player: {attackDamage}");
                AttackHit?.Invoke(info);
            }
        }

        private void FinishAttack()
        {
            SetHitWindow(false);
            IsAttacking = false;
            remainingCooldown = attackCooldown;
            Log("Enemy attack finished");
            AttackEnded?.Invoke();
        }

        private void SetHitWindow(bool value)
        {
            if (hitWindowOpen == value)
                return;
            hitWindowOpen = value;
            if (value)
            {
                Log("Enemy hit window opened");
                AttackHitWindowOpened?.Invoke();
            }
            else
            {
                Log("Enemy hit window closed");
                AttackHitWindowClosed?.Invoke();
            }
        }

        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log(message, this);
        }

        private void OnValidate()
        {
            attackDamage = Mathf.Max(0f, attackDamage);
            attackDuration = Mathf.Max(0.01f, attackDuration);
            attackActiveStart = Mathf.Clamp(attackActiveStart, 0f, attackDuration);
            attackActiveEnd = Mathf.Clamp(attackActiveEnd, attackActiveStart, attackDuration);
            attackCooldown = Mathf.Max(0f, attackCooldown);
            attackRange = Mathf.Max(0f, attackRange);
            attackRadius = Mathf.Max(0.01f, attackRadius);
            attackRotationSpeed = Mathf.Max(0f, attackRotationSpeed);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * attackRadius;
            Vector3 center = origin + transform.forward * attackRange;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, center);
            Gizmos.DrawWireSphere(center, attackRadius);
        }
    }
}
