namespace ChroniclesOfRus.Characters.Enemy.StateMachine.States
{
    public sealed class EnemyHurtState : EnemyState
    {
        private float remainingTime;

        public override EnemyStateId Id => EnemyStateId.Hurt;

        public EnemyHurtState(EnemyStateMachine stateMachine, EnemyController controller)
            : base(stateMachine, controller) { }

        public override void Enter()
        {
            Restart();
            Combat.CancelAttack();
            Controller.AnimationController.PlayHurt();
        }

        public override void Tick(float deltaTime)
        {
            Movement.Stop(deltaTime);
            if (!Controller.Health.IsAlive)
            {
                StateMachine.ChangeState(EnemyStateId.Death);
                return;
            }

            remainingTime -= deltaTime;
            if (remainingTime > 0f)
                return;

            Detection.TickDetection();
            StateMachine.ChangeState(
                Detection.HasTarget ? EnemyStateId.Chase : EnemyStateId.Idle);
        }

        public void Restart() => remainingTime = StateMachine.HurtDuration;
    }
}
