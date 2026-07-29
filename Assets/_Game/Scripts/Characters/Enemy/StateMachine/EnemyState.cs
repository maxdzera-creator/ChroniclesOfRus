namespace ChroniclesOfRus.Characters.Enemy.StateMachine
{
    public abstract class EnemyState : IEnemyState
    {
        protected EnemyStateMachine StateMachine { get; }
        protected EnemyController Controller { get; }
        protected EnemyMovement Movement => Controller.Movement;
        protected EnemyDetection Detection => Controller.Detection;
        protected EnemyCombat Combat => Controller.Combat;

        public abstract EnemyStateId Id { get; }

        protected EnemyState(EnemyStateMachine stateMachine, EnemyController controller)
        {
            StateMachine = stateMachine;
            Controller = controller;
        }

        public virtual void Enter() { }
        public abstract void Tick(float deltaTime);
        public virtual void Exit() { }
    }
}
