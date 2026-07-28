using System;
using System.Collections.Generic;
using ChroniclesOfRus.Characters.Player.StateMachine.States;
using ChroniclesOfRus.Input;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Player.StateMachine
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader), typeof(PlayerMovement))]
    public sealed class PlayerStateMachine : MonoBehaviour
    {
        private readonly Dictionary<PlayerStateId, IPlayerState> states = new();

        private PlayerInputReader inputReader;
        private PlayerMovement movement;
        private IPlayerState currentState;

        public PlayerStateId CurrentStateId => currentState?.Id ?? PlayerStateId.Idle;
        public IPlayerState CurrentState => currentState;

        public event Action<PlayerStateId, PlayerStateId> StateChanged;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
            movement = GetComponent<PlayerMovement>();
            RegisterDefaultStates();
            ChangeState(PlayerStateId.Idle);
        }

        private void Update()
        {
            currentState?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            currentState?.Exit();
            currentState = null;
        }

        private void OnEnable()
        {
            if (states.Count > 0 && currentState == null)
                ChangeState(PlayerStateId.Idle);
        }

        public void ChangeState(PlayerStateId nextStateId)
        {
            if (currentState?.Id == nextStateId)
                return;

            if (!states.TryGetValue(nextStateId, out IPlayerState nextState))
                throw new InvalidOperationException($"State {nextStateId} is not registered.");

            PlayerStateId previousStateId = currentState?.Id ?? nextStateId;
            currentState?.Exit();
            currentState = nextState;
            currentState.Enter();
            Debug.Log($"Player State: {nextStateId}", this);
            StateChanged?.Invoke(previousStateId, nextStateId);
        }

        public void RegisterState(IPlayerState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            states[state.Id] = state;
        }

        private void RegisterDefaultStates()
        {
            RegisterState(new PlayerIdleState(this, inputReader, movement));
            RegisterState(new PlayerMoveState(this, inputReader, movement));
            RegisterState(new PlayerDodgeState(this, inputReader, movement));
            RegisterState(new PlayerAttackState(this, inputReader, movement));
            RegisterState(new PlayerHurtState(this, inputReader, movement));
            RegisterState(new PlayerDeathState(this, inputReader, movement));
        }
    }
}
