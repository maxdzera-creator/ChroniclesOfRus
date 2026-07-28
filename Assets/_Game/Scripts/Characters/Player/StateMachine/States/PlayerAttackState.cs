using System.Collections.Generic;
using ChroniclesOfRus.Input;
using UnityEngine;

namespace ChroniclesOfRus.Characters.Player.StateMachine.States
{
    public sealed class PlayerAttackState : PlayerState
    {
        private const int HitBufferSize = 32;

        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<Collider> hitColliders = new();

        private Vector3 attackDirection;
        private float elapsedTime;
        private bool finished;

        public override PlayerStateId Id => PlayerStateId.Attack;
        public IReadOnlyCollection<Collider> HitColliders => hitColliders;

        public PlayerAttackState(PlayerStateMachine stateMachine, PlayerInputReader input, PlayerMovement movement)
            : base(stateMachine, input, movement)
        {
        }

        public override void Enter()
        {
            attackDirection = Movement.GetCameraRelativeDirection(Input.Move);
            if (attackDirection.sqrMagnitude <= 0.0001f)
                attackDirection = Movement.transform.forward;
            attackDirection.y = 0f;
            attackDirection.Normalize();

            elapsedTime = 0f;
            finished = false;
            hitColliders.Clear();
            Movement.BeginControlledMovement();
            StateMachine.NotifyAttackStarted();
            UpdateHitWindow();
        }

        public override void Tick(float deltaTime)
        {
            PlayerAttackSettings settings = StateMachine.Attack;
            elapsedTime = Mathf.Min(elapsedTime + deltaTime, settings.attackDuration);

            Movement.RotateTowards(attackDirection, settings.attackRotationSpeed, deltaTime);
            Movement.MoveControlled(Vector3.zero, deltaTime);
            UpdateHitWindow();

            if (StateMachine.IsAttackHitWindowOpen)
                PerformAttackHitDetection();

            if (elapsedTime >= settings.attackDuration)
                FinishAttack();
        }

        public override void Exit()
        {
            StateMachine.SetAttackHitWindow(false);
            StateMachine.StartAttackCooldown();
        }

        public void PerformAttackHitDetection()
        {
            PlayerAttackSettings settings = StateMachine.Attack;
            Vector3 center = Movement.transform.position +
                Vector3.up * settings.attackRadius +
                attackDirection * settings.attackRange;

            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                settings.attackRadius,
                hitBuffer,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider target = hitBuffer[i];
                if (target != null && target.transform.root != Movement.transform.root)
                    hitColliders.Add(target);
            }
        }

        private void UpdateHitWindow()
        {
            PlayerAttackSettings settings = StateMachine.Attack;
            bool shouldBeOpen =
                elapsedTime >= settings.attackActiveStart &&
                elapsedTime < settings.attackActiveEnd;

            if (StateMachine.IsAttackHitWindowOpen && !shouldBeOpen)
                StateMachine.LogAttack($"Targets found: {hitColliders.Count}");

            StateMachine.SetAttackHitWindow(shouldBeOpen);
        }

        private void FinishAttack()
        {
            if (finished)
                return;

            finished = true;
            StateMachine.NotifyAttackEnded();
            PlayerStateId nextState = Input.Move.sqrMagnitude > 0.001f
                ? PlayerStateId.Move
                : PlayerStateId.Idle;
            StateMachine.ChangeState(nextState);
        }
    }
}
