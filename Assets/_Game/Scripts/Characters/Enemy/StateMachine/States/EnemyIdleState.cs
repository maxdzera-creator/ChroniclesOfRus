namespace ChroniclesOfRus.Characters.Enemy.StateMachine.States
{
    public sealed class EnemyIdleState : EnemyState
    {
        public override EnemyStateId Id => EnemyStateId.Idle;

        public EnemyIdleState(EnemyStateMachine stateMachine, EnemyController controller)
            : base(stateMachine, controller) { }

        public override void Enter() => Controller.AnimationController.SetIdle();

        public override void Tick(float deltaTime)
        {
            Movement.Stop(deltaTime);
            if (!Controller.Health.IsAlive)
            {
                StateMachine.ChangeState(EnemyStateId.Death);
                return;
            }

            Detection.TickDetection();
            if (Detection.HasTarget)
                StateMachine.ChangeState(EnemyStateId.Chase);
        }
    }
}
