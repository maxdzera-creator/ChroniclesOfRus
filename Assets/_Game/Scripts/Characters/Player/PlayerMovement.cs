using ChroniclesOfRus.Characters.Player.StateMachine;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField, Min(0.01f)] private float acceleration = 20f;
        [SerializeField, Min(0.01f)] private float deceleration = 28f;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;

        [Header("Grounding")]
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float groundedVerticalSpeed = -2f;

        private CharacterController characterController;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private PlayerStateMachine stateMachine;

        public float NormalizedSpeed => moveSpeed > 0f
            ? Mathf.Clamp01(horizontalVelocity.magnitude / moveSpeed)
            : 0f;

        public bool IsMoving => horizontalVelocity.sqrMagnitude > 0.0025f;
        public bool IsGrounded => characterController != null && characterController.isGrounded;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            stateMachine = GetComponent<PlayerStateMachine>();
            if (stateMachine == null)
                stateMachine = gameObject.AddComponent<PlayerStateMachine>();
        }

        public void TickMovement(Vector2 moveInput, float deltaTime)
        {
            Vector3 targetVelocity = GetCameraRelativeDirection(moveInput) * moveSpeed;
            float rate = targetVelocity.sqrMagnitude > horizontalVelocity.sqrMagnitude
                ? acceleration
                : deceleration;

            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * deltaTime);
            UpdateRotation(deltaTime);
            UpdateGravity(deltaTime);

            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            CollisionFlags flags = characterController.Move(velocity * deltaTime);
            if ((flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
                verticalVelocity = groundedVerticalSpeed;
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude <= 0f)
                return Vector3.zero;

            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
        }

        private void UpdateRotation(float deltaTime)
        {
            if (horizontalVelocity.sqrMagnitude < 0.0025f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * deltaTime);
        }

        private void UpdateGravity(float deltaTime)
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = groundedVerticalSpeed;
            else
                verticalVelocity += gravity * deltaTime;
        }

        private void OnValidate()
        {
            if (gravity > 0f)
                gravity = -gravity;
            if (groundedVerticalSpeed > 0f)
                groundedVerticalSpeed = -groundedVerticalSpeed;
        }
    }
}
