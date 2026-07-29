namespace ChroniclesOfRus.Characters.Enemy.StateMachine
{
    public interface IEnemyState
    {
        EnemyStateId Id { get; }
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
