using System;
using UnityEngine;

namespace ChroniclesOfRus.Combat
{
    [DisallowMultipleComponent]
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(0.01f)] private float maxHealth = 100f;
        [SerializeField] private bool startWithFullHealth = true;
        [SerializeField, Min(0f)] private float initialHealth = 100f;
        [SerializeField] private bool destroyOnDeath;
        [SerializeField, Min(0f)] private float destroyDelay;
        [SerializeField] private bool enableDebugLogs = true;

        private float currentHealth;
        private bool canReceiveDamage = true;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float NormalizedHealth => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public bool IsAlive => currentHealth > 0f;
        public bool IsDead => !IsAlive;
        public bool CanReceiveDamage => canReceiveDamage && IsAlive;

        public event Action<float, float, float> HealthChanged;
        public event Action<DamageInfo, float> DamageReceived;
        public event Action<float, float> Healed;
        public event Action<DamageInfo?> Died;
        public event Action<float> Revived;
        public event Action<DamageInfo> DamageRejected;

        private void Awake()
        {
            currentHealth = startWithFullHealth
                ? maxHealth
                : Mathf.Clamp(initialHealth, 0f, maxHealth);
        }

        public void ReceiveDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Amount <= 0f || IsDead)
                return;
            if (!CanReceiveDamage)
            {
                DamageRejected?.Invoke(damageInfo);
                return;
            }

            float previousHealth = currentHealth;
            float appliedDamage = Mathf.Min(damageInfo.Amount, currentHealth);
            currentHealth -= appliedDamage;

            DamageReceived?.Invoke(damageInfo, appliedDamage);
            HealthChanged?.Invoke(previousHealth, currentHealth, maxHealth);
            Log($"Damage received: {appliedDamage}");
            Log($"Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0f)
                HandleDeath(damageInfo);
        }

        public void Heal(float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), "Heal amount cannot be negative.");
            if (amount <= 0f || currentHealth >= maxHealth)
                return;

            bool wasDead = IsDead;
            float previousHealth = currentHealth;
            float appliedHealing = Mathf.Min(amount, maxHealth - currentHealth);
            currentHealth += appliedHealing;

            Healed?.Invoke(appliedHealing, currentHealth);
            HealthChanged?.Invoke(previousHealth, currentHealth, maxHealth);
            Log($"Healed: {appliedHealing}");
            Log($"Health: {currentHealth}/{maxHealth}");

            if (wasDead)
            {
                Log("Revived");
                Revived?.Invoke(currentHealth);
            }
        }

        public void RestoreFullHealth()
        {
            Heal(maxHealth - currentHealth);
        }

        public void Kill()
        {
            if (IsDead)
                return;

            float previousHealth = currentHealth;
            currentHealth = 0f;
            HealthChanged?.Invoke(previousHealth, currentHealth, maxHealth);
            HandleDeath(null);
        }

        public void SetCanReceiveDamage(bool value)
        {
            canReceiveDamage = value;
        }

        private void HandleDeath(DamageInfo? finalDamage)
        {
            Log("Died");
            Died?.Invoke(finalDamage);

            if (destroyOnDeath && Application.isPlaying)
                Destroy(gameObject, destroyDelay);
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(0.01f, maxHealth);
            initialHealth = Mathf.Clamp(initialHealth, 0f, maxHealth);
            destroyDelay = Mathf.Max(0f, destroyDelay);
        }

        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log(message, this);
        }
    }
}
