using UnityEngine;

namespace ChroniclesOfRus.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class DamageableDummy : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool changeScaleOnDamage = true;
        [SerializeField, Min(0f)] private float deathScaleMultiplier = 0.5f;

        private Vector3 initialScale;

        private void Awake()
        {
            if (health == null)
                health = GetComponent<HealthComponent>();
            initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (health == null)
                return;

            health.DamageReceived += OnDamageReceived;
            health.Died += OnDied;
            health.Revived += OnRevived;
        }

        private void OnDisable()
        {
            if (health == null)
                return;

            health.DamageReceived -= OnDamageReceived;
            health.Died -= OnDied;
            health.Revived -= OnRevived;
        }

        private void OnDamageReceived(DamageInfo damageInfo, float appliedDamage)
        {
            if (changeScaleOnDamage)
            {
                float multiplier = Mathf.Lerp(
                    deathScaleMultiplier, 1f, health.NormalizedHealth);
                transform.localScale = Vector3.Scale(initialScale, Vector3.one * multiplier);
            }

            if (enableDebugLogs)
                Debug.Log($"Dummy damage received: {appliedDamage}. Health: {health.CurrentHealth}/{health.MaxHealth}", this);
        }

        private void OnDied(DamageInfo? finalDamage)
        {
            if (changeScaleOnDamage)
                transform.localScale = initialScale * deathScaleMultiplier;
            if (enableDebugLogs)
                Debug.Log("Damageable Dummy died", this);
        }

        private void OnRevived(float currentHealth)
        {
            if (changeScaleOnDamage)
                transform.localScale = initialScale;
        }

        private void OnValidate()
        {
            deathScaleMultiplier = Mathf.Max(0f, deathScaleMultiplier);
        }
    }
}
