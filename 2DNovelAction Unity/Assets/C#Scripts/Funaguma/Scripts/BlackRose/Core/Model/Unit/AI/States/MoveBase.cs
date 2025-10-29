using HighElixir.StateMachine;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units.State
{
    public interface IRigidbodyUser
    {
        Rigidbody2D Rigidbody2D { get; }

        void SetRB2(Rigidbody2D rb);
    }
    public class MovingStateBase : State<AIController>
    {
        public Rigidbody2D Rigidbody2D => Cont.Rigidbody2D;
        protected virtual Vector2 GetDirection() => Cont.MoveDirection.normalized;
    }

    public class HolizontalMovingStates : MovingStateBase
    {
        protected override Vector2 GetDirection() => base.GetDirection() * new Vector2(1, 0);
    }

    // =======================
    // Move（移動）状態
    // =======================
    [Serializable]
    public abstract class AccelMoveBase : HolizontalMovingStates
    {
        [SerializeField] private bool _isStopInExit = false;
        [SerializeField] protected float _accel = 20f;           // 横方向の加速（m/s^2 想定）
        [SerializeField] protected float _friction = 1.0f;
        
        protected abstract Status Status { get; }
        public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }
        public virtual float GetAccel(UnitBase parent)
        {
            if (parent.StatusManager.TryGetCurrentValue(Status, out var c))
            {
                return c;
            }
            else
            {
                return _accel;
            }
        }
        public virtual float GetFriction(UnitBase parent)
        {
            return _accel;
        }
        public AccelMoveBase(bool isStopInExit = false)
            : base()
        {
            _isStopInExit = isStopInExit;
        }
        public override void Exit()
        {
            if (_isStopInExit)
                Rigidbody2D.velocity = Vector2.zero;
        }

        public override void Update(float deltaTime)
        {
            if (Rigidbody2D == null) return;

            // 横入力（例：-1〜1）と空中速度上限
            var input = GetDirection(); // Vector2 なら x 成分を使う
            if (input.x == 0f) return;
            float desiredDir = Mathf.Sign(input.x);
            float absInput = Mathf.Abs(input.x);

            float maxSpeed = Cont.StatusManager.ReadValue(Status);
            float targetVx = desiredDir * maxSpeed;

            // 目標Vxへ滑らかに近づける
            float newVx = absInput > 0.0001f ?
                Mathf.MoveTowards(Rigidbody2D.velocity.x, targetVx, _accel * deltaTime) :
                Mathf.MoveTowards(Rigidbody2D.velocity.x, 0f, _friction * deltaTime);

            // 縦速度は保持、横だけ更新
            Rigidbody2D.velocity = new Vector2(newVx, Rigidbody2D.velocity.y);
        }

        public AccelMoveBase SetAccel(float accel)
        {
            _accel = accel;
            return this;
        }

        public AccelMoveBase SetFriction(float friction)
        {
            _friction = friction;
            return this;
        }
    }
}