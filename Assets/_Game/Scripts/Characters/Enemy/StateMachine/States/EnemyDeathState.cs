using UnityEngine;

namespace ChroniclesOfRus.Characters.Enemy.StateMachine.States
{
    public sealed class EnemyDeathState : EnemyState
    {
        public override EnemyStateId Id => EnemyStateId.Death;

        public EnemyDeathState(EnemyStateMachine stateMachine, EnemyController controller)
            : base(stateMachine, controller) { }

        public override void Enter()
        {
            Movement.Stop(0f);
            Combat.DisableCombat();
            Detection.SetDetectionEnabled(false);
            Controller.Health.SetCanReceiveDamage(false);
            Controller.StopHealthListening();
            Controller.AnimationController.PlayDeath();

            Collider[] colliders = Controller.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;

            if (Controller.CharacterController != null)
                Controller.CharacterController.enabled = false;

            Controller.Log("Enemy died");
            Controller.RaiseEnemyDied();
        }

        public override void Tick(float deltaTime) { }
    }
}
