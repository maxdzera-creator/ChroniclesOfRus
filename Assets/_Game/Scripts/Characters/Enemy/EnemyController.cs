using System;
using ChroniclesOfRus.Characters.Enemy.StateMachine;
using ChroniclesOfRus.Combat;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(CharacterController),
        typeof(HealthComponent),
        typeof(EnemyMovement))]
    [RequireComponent(
        typeof(EnemyDetection),
        typeof(EnemyCombat),
        typeof(EnemyStateMachine))]
    [RequireComponent(typeof(EnemyAnimationController))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private HealthComponent health;
        [SerializeField] private EnemyMovement movement;
        [SerializeField] private EnemyDetection detection;
        [SerializeField] private EnemyCombat combat;
        [SerializeField] private EnemyStateMachine stateMachine;
        [SerializeField] private EnemyAnimationController animationController;
        [SerializeField] private bool enableDebugLogs = true;

        private bool diedRaised;
        private bool isListeningToHealth;

        public Transform Target => target;
        public CharacterController CharacterController => characterController;
        public HealthComponent Health => health;
        public EnemyMovement Movement => movement;
        public EnemyDetection Detection => detection;
        public EnemyCombat Combat => combat;
        public EnemyStateMachine StateMachine => stateMachine;
        public EnemyAnimationController AnimationController => animationController;

        public event Action<DamageInfo, float> EnemyHurt;
        public event Action EnemyDied;

        private void Awake()
        {
            EnsureReferences();
        }

        public void EnsureReferences()
        {
            characterController ??= GetComponent<CharacterController>();
            health ??= GetComponent<HealthComponent>();
            movement ??= GetComponent<EnemyMovement>();
            detection ??= GetComponent<EnemyDetection>();
            combat ??= GetComponent<EnemyCombat>();
            stateMachine ??= GetComponent<EnemyStateMachine>();
            animationController ??= GetComponent<EnemyAnimationController>();

            if (target != null)
                detection.SetTarget(target);
            else
                target = detection.Target;
        }

        private void OnEnable()
        {
            if (health == null)
                return;
            health.DamageReceived += OnDamageReceived;
            health.Died += OnDied;
            isListeningToHealth = true;
        }

        private void OnDisable()
        {
            StopHealthListening();
        }

        public void StopHealthListening()
        {
            if (health == null || !isListeningToHealth)
                return;
            health.DamageReceived -= OnDamageReceived;
            health.Died -= OnDied;
            isListeningToHealth = false;
        }

        public void SetTarget(Transform value)
        {
            target = value;
            detection.SetTarget(value);
        }

        public void RaiseEnemyDied()
        {
            if (diedRaised)
                return;
            diedRaised = true;
            EnemyDied?.Invoke();
        }

        public void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log(message, this);
        }

        private void OnDamageReceived(DamageInfo info, float appliedDamage)
        {
            if (appliedDamage <= 0f || !health.IsAlive)
                return;
            Log($"Enemy received damage: {appliedDamage}");
            EnemyHurt?.Invoke(info, appliedDamage);
            stateMachine.HandleDamageReceived();
        }

        private void OnDied(DamageInfo? finalDamage)
        {
            stateMachine.ChangeState(EnemyStateId.Death);
        }
    }
}
