using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    // =======================
    // FreeMove（自由移動）状態 — 改善版
    // =======================
    [Serializable]
    public class FreeMove : MovingStateBase
    {
        [SerializeField] private bool _isStopInExit = false;

        [Header("Tuning")]
        [SerializeField, Min(0f)] protected float _accel = 60f;         // 入力ありのときの加速（速度ベクトルの変更量 [m/s^2]）
        [SerializeField, Min(0f)] protected float _decel = 30f;         // 入力なしのときの減速（速度ベクトルの変更量 [m/s^2]）
        [SerializeField, Range(0f, 0.1f)] protected float _deadZone = 0.001f; // 入力無視しきい値

        public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }

        [Obsolete]
        public FreeMove(Rigidbody2D rigidbody2D, bool isStopInExit = false)
            : base()
        {
            _isStopInExit = isStopInExit;
        }
        public FreeMove(bool isStopInExit = false)
            : base()
        {
            _isStopInExit = isStopInExit;
        }
        public override void Exit(IState nextIState, UnitBase parent)
        {
            base.Enter(nextIState, parent);
            if (_isStopInExit && Rigidbody2D != null)
                Rigidbody2D.velocity = Vector2.zero;
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            base.Stay(parent, deltaTime);
            if (Rigidbody2D == null) return;

            var input = parent.Direction;                   // 期待：(-1..1, -1..1)
            var hasInput = input.sqrMagnitude > (_deadZone * _deadZone);

            var maxSpeed = parent.StatusManager.ReadValue(Status.Speed);
            var targetVel = hasInput ? input.normalized * maxSpeed : Vector2.zero;

            // 速度ベクトルをターゲットに滑らかに寄せる（ベクトル版 MoveTowards）
            var changePerSec = hasInput ? _accel : _decel;  // 入力時は加速、無入力時は減速
            var maxDelta = changePerSec * Mathf.Max(deltaTime, 0f);
            Rigidbody2D.velocity = Vector2.MoveTowards(Rigidbody2D.velocity, targetVel, maxDelta);
        }

        public FreeMove SetAccel(float accel)
        {
            _accel = Mathf.Max(0f, accel);
            return this;
        }

        public FreeMove SetDecel(float decel)
        {
            _decel = Mathf.Max(0f, decel);
            return this;
        }

        public FreeMove SetDeadZone(float dz)
        {
            _deadZone = Mathf.Clamp(dz, 0f, 0.1f);
            return this;
        }
    }
}
