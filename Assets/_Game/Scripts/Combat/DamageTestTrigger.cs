using UnityEngine;

namespace ChroniclesOfRus.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class DamageTestTrigger : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damageAmount = 20f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private MonoBehaviour target;
        [SerializeField, Min(0f)] private float damageCooldown = 1f;
        [SerializeField] private bool enableDebugLogs = true;

        private IDamageable configuredTarget;
        private float remainingCooldown;

        private void Awake()
        {
            configuredTarget = target as IDamageable;
            if (target != null && configuredTarget == null)
                Debug.LogError("Damage Test Trigger target must implement IDamageable.", this);
        }

        private void Update()
        {
            if (remainingCooldown > 0f)
                remainingCooldown = Mathf.Max(0f, remainingCooldown - Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (remainingCooldown > 0f || damageAmount <= 0f)
                return;

            IDamageable damageable = configuredTarget;
            if (damageable == null)
            {
                damageable = other.GetComponent<IDamageable>();
                if (damageable == null)
                    damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable == null || !damageable.IsAlive)
                return;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitDirection = other.transform.position - transform.position;
            DamageInfo damageInfo = new(
                damageAmount,
                gameObject,
                hitPoint,
                hitDirection,
                damageType);
            bool canApplyDamage = damageable.CanReceiveDamage;
            damageable.ReceiveDamage(damageInfo);
            remainingCooldown = damageCooldown;

            if (enableDebugLogs && canApplyDamage)
                Debug.Log($"Test trigger damage applied: {damageAmount}", this);
        }

        private void OnValidate()
        {
            damageAmount = Mathf.Max(0f, damageAmount);
            damageCooldown = Mathf.Max(0f, damageCooldown);

            Collider trigger = GetComponent<Collider>();
            if (trigger != null)
                trigger.isTrigger = true;
        }

        private void OnDrawGizmosSelected()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                return;

            Gizmos.color = new Color(1f, 0f, 0f, 0.75f);
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = previousMatrix;
        }
    }
}
