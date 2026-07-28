using ChroniclesOfRus.Input;

namespace ChroniclesOfRus.Characters.Player.StateMachine
{
    public abstract class PlayerState : IPlayerState
    {
        protected PlayerStateMachine StateMachine { get; }
        protected PlayerInputReader Input { get; }
        protected PlayerMovement Movement { get; }

        public abstract PlayerStateId Id { get; }

        protected PlayerState(
            PlayerStateMachine stateMachine,
            PlayerInputReader input,
            PlayerMovement movement)
        {
            StateMachine = stateMachine;
            Input = input;
            Movement = movement;
        }

        public virtual void Enter()
        {
        }

        public abstract void Tick(float deltaTime);

        public virtual void Exit()
        {
        }
    }
}
