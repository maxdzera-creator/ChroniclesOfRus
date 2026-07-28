using ChroniclesOfRus.Input;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Player.StateMachine.States
{
    public sealed class PlayerDodgeState : PlayerState
    {
        private Vector3 direction;
        private float elapsedTime;
        private float previousCurveValue;
        private bool finished;

        public override PlayerStateId Id => PlayerStateId.Dodge;

        public PlayerDodgeState(PlayerStateMachine stateMachine, PlayerInputReader input, PlayerMovement movement)
            : base(stateMachine, input, movement)
        {
        }

        public override void Enter()
        {
            PlayerDodgeSettings settings = StateMachine.Dodge;
            direction = Movement.GetCameraRelativeDirection(Input.Move);
            if (direction.sqrMagnitude <= 0.0001f)
                direction = Movement.transform.forward;
            direction.y = 0f;
            direction.Normalize();

            elapsedTime = 0f;
            previousCurveValue = EvaluateCurve(settings, 0f);
            finished = false;
            Movement.BeginControlledMovement();
            StateMachine.LogDodge("Dodge started");
            UpdateInvulnerability();
        }

        public override void Tick(float deltaTime)
        {
            PlayerDodgeSettings settings = StateMachine.Dodge;
            elapsedTime = Mathf.Min(elapsedTime + deltaTime, settings.dodgeDuration);
            float normalizedTime = Mathf.Clamp01(elapsedTime / settings.dodgeDuration);
            float curveValue = EvaluateCurve(settings, normalizedTime);
            float distance = Mathf.Max(0f, curveValue - previousCurveValue) * settings.dodgeDistance;
            previousCurveValue = curveValue;

            Movement.RotateTowards(direction, settings.rotationSpeed, deltaTime);
            Movement.MoveControlled(direction * distance, deltaTime);
            UpdateInvulnerability();

            if (elapsedTime >= settings.dodgeDuration)
                FinishDodge();
        }

        public override void Exit()
        {
            StateMachine.SetDodgeInvulnerable(false);
            StateMachine.StartDodgeCooldown();
        }

        private void UpdateInvulnerability()
        {
            PlayerDodgeSettings settings = StateMachine.Dodge;
            bool shouldBeInvulnerable =
                elapsedTime >= settings.invulnerabilityStartTime &&
                elapsedTime < settings.invulnerabilityEndTime;
            StateMachine.SetDodgeInvulnerable(shouldBeInvulnerable);
        }

        private void FinishDodge()
        {
            if (finished)
                return;

            finished = true;
            StateMachine.LogDodge("Dodge finished");
            PlayerStateId nextState = Input.Move.sqrMagnitude > 0.001f
                ? PlayerStateId.Move
                : PlayerStateId.Idle;
            StateMachine.ChangeState(nextState);
        }

        private static float EvaluateCurve(PlayerDodgeSettings settings, float normalizedTime)
        {
            AnimationCurve curve = settings.movementCurve;
            float start = curve.Evaluate(0f);
            float end = curve.Evaluate(1f);
            float range = end - start;
            if (Mathf.Abs(range) <= 0.0001f)
                return normalizedTime;

            return Mathf.Clamp01((curve.Evaluate(normalizedTime) - start) / range);
        }
    }
}
