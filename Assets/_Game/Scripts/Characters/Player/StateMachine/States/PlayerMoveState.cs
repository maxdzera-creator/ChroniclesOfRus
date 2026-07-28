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

        public override void Enter()
        {
            Input.DodgePressed += OnDodgePressed;
            Input.AttackPressed += OnAttackPressed;
        }

        public override void Exit()
        {
            Input.DodgePressed -= OnDodgePressed;
            Input.AttackPressed -= OnAttackPressed;
        }

        public override void Tick(float deltaTime)
        {
            Movement.TickMovement(Input.Move, deltaTime);

            if (Input.Move.sqrMagnitude <= 0.001f)
                StateMachine.ChangeState(PlayerStateId.Idle);
        }

        private void OnDodgePressed()
        {
            if (StateMachine.CanDodge)
                StateMachine.ChangeState(PlayerStateId.Dodge);
        }

        private void OnAttackPressed()
        {
            if (StateMachine.CanAttack)
                StateMachine.ChangeState(PlayerStateId.Attack);
        }
    }
}
