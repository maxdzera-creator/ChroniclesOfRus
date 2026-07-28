using ChroniclesOfRus.Input;

namespace ChroniclesOfRus.Characters.Player.StateMachine.States
{
    public sealed class PlayerIdleState : PlayerState
    {
        public override PlayerStateId Id => PlayerStateId.Idle;

        public PlayerIdleState(PlayerStateMachine stateMachine, PlayerInputReader input, PlayerMovement movement)
            : base(stateMachine, input, movement)
        {
        }

        public override void Tick(float deltaTime)
        {
            Movement.TickMovement(Input.Move, deltaTime);

            if (Input.Move.sqrMagnitude > 0.001f)
                StateMachine.ChangeState(PlayerStateId.Move);
        }
    }
}
