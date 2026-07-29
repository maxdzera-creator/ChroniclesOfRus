using ChroniclesOfRus.Characters.Player.StateMachine;
using ChroniclesOfRus.Combat;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent), typeof(PlayerStateMachine))]
    public sealed class PlayerDamageReceiver : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private PlayerStateMachine stateMachine;
        [SerializeField] private bool enableDebugLogs = true;

        private void Awake()
        {
            if (health == null)
                health = GetComponent<HealthComponent>();
            if (stateMachine == null)
                stateMachine = GetComponent<PlayerStateMachine>();
        }

        private void OnEnable()
        {
            if (stateMachine == null)
                return;

            stateMachine.DodgeInvulnerabilityStarted += OnInvulnerabilityStarted;
            stateMachine.DodgeInvulnerabilityEnded += OnInvulnerabilityEnded;
            health.DamageRejected += OnDamageRejected;
            ApplyInvulnerability(stateMachine.IsInvulnerable);
        }

        private void OnDisable()
        {
            if (stateMachine != null)
            {
                stateMachine.DodgeInvulnerabilityStarted -= OnInvulnerabilityStarted;
                stateMachine.DodgeInvulnerabilityEnded -= OnInvulnerabilityEnded;
            }
            if (health != null)
                health.DamageRejected -= OnDamageRejected;

            if (health != null)
                health.SetCanReceiveDamage(true);
        }

        private void OnInvulnerabilityStarted()
        {
            ApplyInvulnerability(true);
        }

        private void OnInvulnerabilityEnded()
        {
            ApplyInvulnerability(false);
        }

        private void ApplyInvulnerability(bool isInvulnerable)
        {
            if (health == null)
                return;

            health.SetCanReceiveDamage(!isInvulnerable);
        }

        private void OnDamageRejected(DamageInfo damageInfo)
        {
            if (stateMachine != null && stateMachine.IsInvulnerable && enableDebugLogs)
                Debug.Log("Damage ignored: Dodge invulnerability", this);
        }
    }
}
