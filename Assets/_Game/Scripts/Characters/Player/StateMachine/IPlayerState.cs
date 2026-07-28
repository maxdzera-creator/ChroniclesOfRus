namespace ChroniclesOfRus.Characters.Player.StateMachine
{
    public interface IPlayerState
    {
        PlayerStateId Id { get; }
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
