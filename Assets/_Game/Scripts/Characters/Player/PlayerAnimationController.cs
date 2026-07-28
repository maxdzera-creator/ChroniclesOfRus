using UnityEngine;

namespace ChroniclesOfRus.Characters.Player
{
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField, Min(0f)] private float dampTime = 0.1f;

        private int speedHash;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (movement == null)
                movement = GetComponent<PlayerMovement>();
            speedHash = Animator.StringToHash(speedParameter);
        }

        private void Update()
        {
            if (animator == null || movement == null)
                return;

            animator.SetFloat(speedHash, movement.NormalizedSpeed, dampTime, Time.deltaTime);
        }

        public void TriggerAttack(string triggerName) => SetTrigger(triggerName);
        public void TriggerDodge(string triggerName) => SetTrigger(triggerName);
        public void TriggerHit(string triggerName) => SetTrigger(triggerName);

        private void SetTrigger(string triggerName)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
                animator.SetTrigger(Animator.StringToHash(triggerName));
        }
    }
}
