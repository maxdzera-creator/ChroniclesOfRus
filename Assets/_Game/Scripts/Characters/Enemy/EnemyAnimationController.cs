using UnityEngine;

namespace ChroniclesOfRus.Characters.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        public Animator Animator => animator;

        public void SetIdle() { }
        public void SetMoving(bool value) { }
        public void PlayAttack() { }
        public void PlayHurt() { }
        public void PlayDeath() { }
    }
}
