using BlackRose.Core.Models.States.Helpers;
using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    // =======================
    // Move（移動）状態
    // =======================
    [Serializable]
    public class MoveOnAir : HolizontalMovingStates
    {
        [SerializeField] private bool _isStopInExit = false;
        [SerializeField] protected float _accel = 60f;           // 横方向の加速（m/s^2 想定）
        [SerializeField] protected float _airFriction = 30f;   // 入力がないときの減速

        public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }

        [Obsolete]
        public MoveOnAir(Rigidbody2D rigidbody2D, bool isStopInExit = false)
        {
            _isStopInExit = isStopInExit; 
        }
        public MoveOnAir(bool isStopInExit = false)
        {
            _isStopInExit = isStopInExit; 
        }
        public override void Exit(IState nextIState, UnitBase parent)
        {
            if (_isStopInExit)
                Rigidbody2D.velocity = Vector2.zero;
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            if (Rigidbody2D == null) return;

            // 横入力（例：-1〜1）と空中速度上限
            var input = parent.X(); // Vector2 なら x 成分を使う
            if (input.x == 0f) return;
            float desiredDir = Mathf.Sign(input.x);
            float absInput = Mathf.Abs(input.x);

            float maxSpeed = parent.StatusManager.ReadValue(Status.SpeedInAir);
            float targetVx = desiredDir * maxSpeed;

            // 目標Vxへ滑らかに近づける
            float newVx = absInput > 0.0001f ? 
                Mathf.MoveTowards(Rigidbody2D.velocity.x, targetVx, _accel * deltaTime) : 
                Mathf.MoveTowards(Rigidbody2D.velocity.x, 0f, _airFriction * deltaTime);

            // 縦速度は保持、横だけ更新
            Rigidbody2D.velocity = new Vector2(newVx, Rigidbody2D.velocity.y);
        }

        public MoveOnAir SetAccel(float accel)
        {
            _accel = accel;
            return this;
        }

        public MoveOnAir SetAirFriction(float airFriction)
        {
            _airFriction = airFriction;
            return this;
        }
    }
}