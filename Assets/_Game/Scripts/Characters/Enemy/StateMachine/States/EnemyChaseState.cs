namespace ChroniclesOfRus.Characters.Enemy.StateMachine.States
{
    public sealed class EnemyChaseState : EnemyState
    {
        public override EnemyStateId Id => EnemyStateId.Chase;

        public EnemyChaseState(EnemyStateMachine stateMachine, EnemyController controller)
            : base(stateMachine, controller) { }

        public override void Enter() => Controller.AnimationController.SetMoving(true);
        public override void Exit() => Controller.AnimationController.SetMoving(false);

        public override void Tick(float deltaTime)
        {
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
            if (Detection.HorizontalDistanceToTarget() <= Combat.AttackRange)
            {
                StateMachine.ChangeState(EnemyStateId.Attack);
                return;
            }

            Movement.MoveTowards(Detection.Target.position, deltaTime);
        }
    }
}
