using System;
using System.Collections.Generic;
using ChroniclesOfRus.Characters.Enemy.StateMachine.States;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Enemy.StateMachine
{
    [DisallowMultipleComponent]
    public sealed class EnemyStateMachine : MonoBehaviour
    {
        [Header("Reactions")]
        [SerializeField, Min(0.01f)] private float hurtDuration = 0.25f;
        [SerializeField] private bool allowHurtDuringAttack = true;
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private readonly Dictionary<EnemyStateId, IEnemyState> states = new();
        private EnemyController controller;
        private IEnemyState currentState;
        private bool started;

        public EnemyStateId CurrentStateId => currentState?.Id ?? EnemyStateId.Idle;
        public float HurtDuration => hurtDuration;
        public bool IsDead => CurrentStateId == EnemyStateId.Death;

        public event Action<EnemyStateId, EnemyStateId> StateChanged;

        private void Awake()
        {
            controller = GetComponent<EnemyController>();
            controller.EnsureReferences();
            RegisterStates();
        }

        private void Start()
        {
            started = true;
            ChangeState(controller.Health.IsAlive ? EnemyStateId.Idle : EnemyStateId.Death);
        }

        private void Update() => currentState?.Tick(Time.deltaTime);

        private void OnDisable()
        {
            currentState?.Exit();
            currentState = null;
        }

        private void OnEnable()
        {
            if (started && states.Count > 0 && currentState == null)
                ChangeState(controller.Health.IsAlive ? EnemyStateId.Idle : EnemyStateId.Death);
        }

        public void ChangeState(EnemyStateId next)
        {
            if (CurrentStateId == EnemyStateId.Death || currentState?.Id == next)
                return;
            if (!states.TryGetValue(next, out IEnemyState state))
                throw new InvalidOperationException($"Enemy state {next} is not registered.");

            EnemyStateId previous = currentState?.Id ?? next;
            currentState?.Exit();
            currentState = state;
            currentState.Enter();
            if (enableDebugLogs)
                Debug.Log($"Enemy State: {next}", this);
            StateChanged?.Invoke(previous, next);
        }

        public void HandleDamageReceived()
        {
            if (IsDead)
                return;
            if (CurrentStateId == EnemyStateId.Hurt)
            {
                ((EnemyHurtState)currentState).Restart();
                return;
            }
            if (CurrentStateId != EnemyStateId.Attack || allowHurtDuringAttack)
                ChangeState(EnemyStateId.Hurt);
        }

        private void RegisterStates()
        {
            states[EnemyStateId.Idle] = new EnemyIdleState(this, controller);
            states[EnemyStateId.Chase] = new EnemyChaseState(this, controller);
            states[EnemyStateId.Attack] = new EnemyAttackState(this, controller);
            states[EnemyStateId.Hurt] = new EnemyHurtState(this, controller);
            states[EnemyStateId.Death] = new EnemyDeathState(this, controller);
        }

        private void OnValidate() => hurtDuration = Mathf.Max(0.01f, hurtDuration);
    }
}
