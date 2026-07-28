using System;
using System.Collections.Generic;
using ChroniclesOfRus.Characters.Player.StateMachine.States;
using ChroniclesOfRus.Input;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Player.StateMachine
{
    [Serializable]
    public sealed class PlayerDodgeSettings
    {
        [Min(0.01f)] public float dodgeDistance = 4f;
        [Min(0.01f)] public float dodgeDuration = 0.45f;
        [Min(0f)] public float dodgeCooldown = 0.35f;
        [Min(0f)] public float rotationSpeed = 1080f;
        [Min(0f)] public float invulnerabilityStartTime = 0.08f;
        [Min(0f)] public float invulnerabilityEndTime = 0.32f;
        public AnimationCurve movementCurve = new(
            new Keyframe(0f, 0f, 0f, 3f),
            new Keyframe(1f, 1f, 0f, 0f));
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader), typeof(PlayerMovement))]
    public sealed class PlayerStateMachine : MonoBehaviour
    {
        [Header("Dodge")]
        [SerializeField] private PlayerDodgeSettings dodge = new();
        [SerializeField] private bool enableDebugLogs = true;

        private readonly Dictionary<PlayerStateId, IPlayerState> states = new();

        private PlayerInputReader inputReader;
        private PlayerMovement movement;
        private IPlayerState currentState;

        public PlayerStateId CurrentStateId => currentState?.Id ?? PlayerStateId.Idle;
        public IPlayerState CurrentState => currentState;
        public PlayerDodgeSettings Dodge => dodge;
        public bool CanDodge => remainingDodgeCooldown <= 0f && CurrentStateId != PlayerStateId.Dodge;
        public float RemainingDodgeCooldown => remainingDodgeCooldown;
        public bool IsInvulnerable { get; private set; }

        public event Action<PlayerStateId, PlayerStateId> StateChanged;
        public event Action DodgeInvulnerabilityStarted;
        public event Action DodgeInvulnerabilityEnded;

        private float remainingDodgeCooldown;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
            movement = GetComponent<PlayerMovement>();
            RegisterDefaultStates();
            ChangeState(PlayerStateId.Idle);
        }

        private void Update()
        {
            if (remainingDodgeCooldown > 0f)
                remainingDodgeCooldown = Mathf.Max(0f, remainingDodgeCooldown - Time.deltaTime);
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

        public void StartDodgeCooldown()
        {
            remainingDodgeCooldown = dodge.dodgeCooldown;
        }

        public void SetDodgeInvulnerable(bool value)
        {
            if (IsInvulnerable == value)
                return;

            IsInvulnerable = value;
            if (value)
            {
                LogDodge("Dodge invulnerability started");
                DodgeInvulnerabilityStarted?.Invoke();
            }
            else
            {
                LogDodge("Dodge invulnerability ended");
                DodgeInvulnerabilityEnded?.Invoke();
            }
        }

        public void LogDodge(string message)
        {
            if (enableDebugLogs)
                Debug.Log(message, this);
        }

        private void OnValidate()
        {
            dodge ??= new PlayerDodgeSettings();
            dodge.dodgeDistance = Mathf.Max(0.01f, dodge.dodgeDistance);
            dodge.dodgeDuration = Mathf.Max(0.01f, dodge.dodgeDuration);
            dodge.dodgeCooldown = Mathf.Max(0f, dodge.dodgeCooldown);
            dodge.rotationSpeed = Mathf.Max(0f, dodge.rotationSpeed);
            dodge.invulnerabilityStartTime = Mathf.Clamp(
                dodge.invulnerabilityStartTime, 0f, dodge.dodgeDuration);
            dodge.invulnerabilityEndTime = Mathf.Clamp(
                dodge.invulnerabilityEndTime,
                dodge.invulnerabilityStartTime,
                dodge.dodgeDuration);
            if (dodge.movementCurve == null || dodge.movementCurve.length == 0)
                dodge.movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
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
