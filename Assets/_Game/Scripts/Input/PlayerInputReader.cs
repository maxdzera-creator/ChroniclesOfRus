using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChroniclesOfRus.Input
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";

        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction dodgeAction;
        private InputAction attackAction;
        private bool isSubscribed;

        public Vector2 Move => IsInputEnabled && moveAction != null
            ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
            : Vector2.zero;

        public bool IsInputEnabled { get; private set; } = true;

        public event Action DodgePressed;
        public event Action AttackPressed;
        public event Action LightAttackRequested;
        public event Action HeavyAttackRequested;
        public event Action AbilityRequested;
        public event Action InteractRequested;
        public event Action PauseRequested;

        private void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError($"{nameof(PlayerInputReader)} on {name} needs an Input Action Asset.", this);
                enabled = false;
                return;
            }

            playerMap = inputActions.FindActionMap(actionMapName, true);
            moveAction = playerMap.FindAction("Move", true);
            dodgeAction = playerMap.FindAction("Dodge", true);
            attackAction = playerMap.FindAction("Attack", true);
        }

        private void OnEnable()
        {
            SubscribeCallbacks();
            if (IsInputEnabled)
                playerMap?.Enable();
        }

        private void OnDisable()
        {
            playerMap?.Disable();
            UnsubscribeCallbacks();
        }

        public void SetInputEnabled(bool value)
        {
            IsInputEnabled = value;
            if (!isActiveAndEnabled || playerMap == null)
                return;

            if (value)
                playerMap.Enable();
            else
                playerMap.Disable();
        }

        private void SubscribeCallbacks()
        {
            if (playerMap == null || isSubscribed)
                return;

            dodgeAction.performed += OnDodge;
            attackAction.performed += OnAttack;
            Subscribe("LightAttack", OnLightAttack);
            Subscribe("HeavyAttack", OnHeavyAttack);
            Subscribe("Ability", OnAbility);
            Subscribe("Interact", OnInteract);
            Subscribe("Pause", OnPause);
            isSubscribed = true;
        }

        private void UnsubscribeCallbacks()
        {
            if (playerMap == null || !isSubscribed)
                return;

            dodgeAction.performed -= OnDodge;
            attackAction.performed -= OnAttack;
            Unsubscribe("LightAttack", OnLightAttack);
            Unsubscribe("HeavyAttack", OnHeavyAttack);
            Unsubscribe("Ability", OnAbility);
            Unsubscribe("Interact", OnInteract);
            Unsubscribe("Pause", OnPause);
            isSubscribed = false;
        }

        private void Subscribe(string actionName, Action<InputAction.CallbackContext> callback) =>
            playerMap.FindAction(actionName, true).performed += callback;

        private void Unsubscribe(string actionName, Action<InputAction.CallbackContext> callback) =>
            playerMap.FindAction(actionName, true).performed -= callback;

        private void OnDodge(InputAction.CallbackContext _) => InvokeIfEnabled(DodgePressed);
        private void OnAttack(InputAction.CallbackContext _) => InvokeIfEnabled(AttackPressed);
        private void OnLightAttack(InputAction.CallbackContext _) => InvokeIfEnabled(LightAttackRequested);
        private void OnHeavyAttack(InputAction.CallbackContext _) => InvokeIfEnabled(HeavyAttackRequested);
        private void OnAbility(InputAction.CallbackContext _) => InvokeIfEnabled(AbilityRequested);
        private void OnInteract(InputAction.CallbackContext _) => InvokeIfEnabled(InteractRequested);
        private void OnPause(InputAction.CallbackContext _) => InvokeIfEnabled(PauseRequested);

        private void InvokeIfEnabled(Action action)
        {
            if (IsInputEnabled)
                action?.Invoke();
        }
    }
}
