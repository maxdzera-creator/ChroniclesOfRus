using System;
using ChroniclesOfRus.Combat;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyDetection : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float detectionRadius = 8f;
        [SerializeField, Min(0f)] private float loseTargetRadius = 10f;
        [SerializeField] private bool requireTargetAlive = true;
        [SerializeField] private bool enableDebugLogs = true;

        private IDamageable targetDamageable;
        private bool isDetected;
        private bool detectionEnabled = true;

        public Transform Target => target;
        public IDamageable TargetDamageable => targetDamageable;
        public bool HasTarget => detectionEnabled && isDetected && IsTargetValid();
        public float DetectionRadius => detectionRadius;
        public float LoseTargetRadius => loseTargetRadius;

        public event Action<Transform> TargetDetected;
        public event Action<Transform> TargetLost;

        private void Awake() => CacheTargetDamageable();

        public void SetTarget(Transform value)
        {
            if (target == value)
                return;
            if (isDetected)
                LoseTarget();

            target = value;
            CacheTargetDamageable();
        }

        public void TickDetection()
        {
            if (!detectionEnabled || target == null)
                return;

            float distance = HorizontalDistanceToTarget();
            if (!isDetected && IsTargetValid() && distance <= detectionRadius)
                DetectTarget();
            else if (isDetected && (!IsTargetValid() || distance > loseTargetRadius))
                LoseTarget();
        }

        public float HorizontalDistanceToTarget()
        {
            if (target == null)
                return float.PositiveInfinity;
            Vector3 delta = target.position - transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        public void SetDetectionEnabled(bool value)
        {
            detectionEnabled = value;
            if (!value && isDetected)
                LoseTarget();
        }

        private bool IsTargetValid()
        {
            return target != null &&
                (!requireTargetAlive || targetDamageable == null || targetDamageable.IsAlive);
        }

        private void CacheTargetDamageable()
        {
            targetDamageable = target != null ? target.GetComponent<IDamageable>() : null;
            if (target != null && targetDamageable == null)
                targetDamageable = target.GetComponentInParent<IDamageable>();
        }

        private void DetectTarget()
        {
            isDetected = true;
            if (enableDebugLogs)
                Debug.Log("Player detected", this);
            TargetDetected?.Invoke(target);
        }

        private void LoseTarget()
        {
            isDetected = false;
            if (enableDebugLogs)
                Debug.Log("Player lost", this);
            TargetLost?.Invoke(target);
        }

        private void OnValidate()
        {
            detectionRadius = Mathf.Max(0f, detectionRadius);
            loseTargetRadius = Mathf.Max(detectionRadius, loseTargetRadius);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            DrawCircle(transform.position, detectionRadius);
            Gizmos.color = new Color(1f, 0.5f, 0f);
            DrawCircle(transform.position, loseTargetRadius);

            if (target != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }

        private static void DrawCircle(Vector3 center, float radius)
        {
            const int segments = 48;
            Vector3 previous = center + Vector3.forward * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 next = center + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
