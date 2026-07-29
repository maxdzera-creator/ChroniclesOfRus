using UnityEngine;

namespace ChroniclesOfRus.Characters.Enemy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class EnemyMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 3f;
        [SerializeField, Min(0f)] private float rotationSpeed = 540f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float groundedVerticalVelocity = -2f;

        private CharacterController controller;
        private float verticalVelocity;

        public bool IsGrounded => controller != null && controller.enabled && controller.isGrounded;

        private void Awake() => controller = GetComponent<CharacterController>();

        public void MoveTowards(Vector3 position, float deltaTime)
        {
            Vector3 direction = position - transform.position;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
            RotateTowards(direction, rotationSpeed, deltaTime);
            Move(direction * moveSpeed, deltaTime);
        }

        public void Stop(float deltaTime) => Move(Vector3.zero, deltaTime);

        public void RotateTowards(Vector3 direction, float speed, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            Quaternion target = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, speed * deltaTime);
        }

        private void Move(Vector3 horizontalVelocity, float deltaTime)
        {
            if (controller == null || !controller.enabled)
                return;

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = groundedVerticalVelocity;
            else
                verticalVelocity += gravity * deltaTime;

            controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime);
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
            if (gravity > 0f)
                gravity = -gravity;
            if (groundedVerticalVelocity > 0f)
                groundedVerticalVelocity = -groundedVerticalVelocity;
        }
    }
}
