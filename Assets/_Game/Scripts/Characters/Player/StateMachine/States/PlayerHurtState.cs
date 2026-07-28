using ChroniclesOfRus.Input;

namespace ChroniclesOfRus.Characters.Player.StateMachine.States
{
    public sealed class PlayerHurtState : PlayerState
    {
        public override PlayerStateId Id => PlayerStateId.Hurt;

        public PlayerHurtState(PlayerStateMachine stateMachine, PlayerInputReader input, PlayerMovement movement)
            : base(stateMachine, input, movement)
        {
        }

        public override void Tick(float deltaTime)
        {
            Movement.TickMovement(default, deltaTime);
        }
    }
}
