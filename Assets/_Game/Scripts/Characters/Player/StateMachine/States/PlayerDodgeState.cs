using ChroniclesOfRus.Input;

namespace ChroniclesOfRus.Characters.Player.StateMachine.States
{
    public sealed class PlayerDodgeState : PlayerState
    {
        public override PlayerStateId Id => PlayerStateId.Dodge;

        public PlayerDodgeState(PlayerStateMachine stateMachine, PlayerInputReader input, PlayerMovement movement)
            : base(stateMachine, input, movement)
        {
        }

        public override void Tick(float deltaTime)
        {
            Movement.TickMovement(default, deltaTime);
        }
    }
}
