using ChroniclesOfRus.Input;

namespace ChroniclesOfRus.Characters.Player.StateMachine.States
{
    public sealed class PlayerMoveState : PlayerState
    {
        public override PlayerStateId Id => PlayerStateId.Move;

        public PlayerMoveState(PlayerStateMachine stateMachine, PlayerInputReader input, PlayerMovement movement)
            : base(stateMachine, input, movement)
        {
        }

        public override void Tick(float deltaTime)
        {
            Movement.TickMovement(Input.Move, deltaTime);

            if (Input.Move.sqrMagnitude <= 0.001f)
                StateMachine.ChangeState(PlayerStateId.Idle);
        }
    }
}
