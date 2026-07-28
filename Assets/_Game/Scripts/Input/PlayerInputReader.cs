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

        public Vector2 Move => IsInputEnabled && moveAction != null
            ? Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f)
            : Vector2.zero;

        public bool IsInputEnabled { get; private set; } = true;

        public event Action DodgeRequested;
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
            Subscribe("Dodge", OnDodge);
            Subscribe("LightAttack", OnLightAttack);
            Subscribe("HeavyAttack", OnHeavyAttack);
            Subscribe("Ability", OnAbility);
            Subscribe("Interact", OnInteract);
            Subscribe("Pause", OnPause);
        }

        private void OnEnable()
        {
            if (IsInputEnabled)
                playerMap?.Enable();
        }

        private void OnDisable()
        {
            playerMap?.Disable();
        }

        private void OnDestroy()
        {
            Unsubscribe("Dodge", OnDodge);
            Unsubscribe("LightAttack", OnLightAttack);
            Unsubscribe("HeavyAttack", OnHeavyAttack);
            Unsubscribe("Ability", OnAbility);
            Unsubscribe("Interact", OnInteract);
            Unsubscribe("Pause", OnPause);
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

        private void Subscribe(string actionName, Action<InputAction.CallbackContext> callback)
        {
            playerMap.FindAction(actionName, true).performed += callback;
        }

        private void Unsubscribe(string actionName, Action<InputAction.CallbackContext> callback)
        {
            if (playerMap == null)
                return;

            InputAction action = playerMap.FindAction(actionName, false);
            if (action != null)
                action.performed -= callback;
        }

        private void OnDodge(InputAction.CallbackContext _) => InvokeIfEnabled(DodgeRequested);
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
