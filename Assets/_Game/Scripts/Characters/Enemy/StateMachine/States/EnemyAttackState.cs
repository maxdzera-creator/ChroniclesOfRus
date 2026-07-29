using UnityEngine;

namespace ChroniclesOfRus.Characters.Enemy.StateMachine.States
{
    public sealed class EnemyAttackState : EnemyState
    {
        public override EnemyStateId Id => EnemyStateId.Attack;

        public EnemyAttackState(EnemyStateMachine stateMachine, EnemyController controller)
            : base(stateMachine, controller) { }

        public override void Enter()
        {
            Movement.Stop(0f);
            TryBeginAttack();
            Controller.AnimationController.PlayAttack();
        }

        public override void Tick(float deltaTime)
        {
            Movement.Stop(deltaTime);
            if (!Controller.Health.IsAlive)
            {
                StateMachine.ChangeState(EnemyStateId.Death);
                return;
            }

            Detection.TickDetection();
            if (!Detection.HasTarget)
            {
                StateMachine.ChangeState(EnemyStateId.Idle);
                return;
            }

            if (Combat.IsAttacking)
            {
                Vector3 direction = Detection.Target.position - Controller.transform.position;
                Movement.RotateTowards(direction, Combat.AttackRotationSpeed, deltaTime);
                if (!Combat.TickAttack(deltaTime, Detection.Target, Detection.TargetDamageable))
                    return;
            }

            if (Detection.HorizontalDistanceToTarget() > Combat.AttackRange)
            {
                StateMachine.ChangeState(EnemyStateId.Chase);
                return;
            }

            TryBeginAttack();
        }

        public override void Exit() => Combat.CancelAttack();

        private void TryBeginAttack()
        {
            if (Combat.CanAttack && !Combat.IsAttacking)
                Combat.BeginAttack();
        }
    }
}
