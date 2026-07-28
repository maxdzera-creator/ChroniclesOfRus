using UnityEngine;

namespace ChroniclesOfRus.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f);
        [SerializeField, Min(0.1f)] private float distance = 12f;
        [SerializeField, Range(15f, 75f)] private float pitch = 42f;
        [SerializeField] private float yaw = 45f;
        [SerializeField, Min(0.01f)] private float followSmoothTime = 0.18f;

        private Vector3 followVelocity;
        private Vector3 smoothedTargetPosition;

        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                if (target != null)
                    smoothedTargetPosition = target.position + targetOffset;
            }
        }

        private void Start()
        {
            if (target != null)
            {
                smoothedTargetPosition = target.position + targetOffset;
                ApplyCameraPose(smoothedTargetPosition);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desiredTargetPosition = target.position + targetOffset;
            smoothedTargetPosition = Vector3.SmoothDamp(
                smoothedTargetPosition,
                desiredTargetPosition,
                ref followVelocity,
                followSmoothTime);
            ApplyCameraPose(smoothedTargetPosition);
        }

        private void ApplyCameraPose(Vector3 lookAtPosition)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(
                lookAtPosition - rotation * Vector3.forward * distance,
                rotation);
        }

        public void AddPositionImpulse(Vector3 worldOffset)
        {
            smoothedTargetPosition += worldOffset;
        }
    }
}
