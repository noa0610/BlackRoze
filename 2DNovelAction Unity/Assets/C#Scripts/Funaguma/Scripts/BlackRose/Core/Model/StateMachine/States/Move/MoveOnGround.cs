using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    // =======================
    // Move（移動）状態
    // =======================
    [Serializable]
    public class MoveOnGround : HolizontalMovingStates
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private bool _isStopInExit = false; // inExitStopの代わりに使用するフラグ

        public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }
        public MoveOnGround(Rigidbody2D rigidbody2D, bool isStopInExit = false)
        {
            _rigidbody2D = rigidbody2D;
            _isStopInExit = isStopInExit; // inExitStopの代わりに使用するフラグを設定
        }

        public override void Exit(IState nextIState, UnitBase parent)
        {
            if (_isStopInExit)
                _rigidbody2D.velocity = Vector2.zero; // inExitStopの代わりに使用するフラグがtrueなら速度をゼロにする
        }

        public override void Stay(UnitBase parent)
        {
            if (parent.StatusManager.TryGetStatus(Status.Speed, out var info))
                _rigidbody2D.velocity = info.CurrentAmount * GetDirection(parent) + Vector2.up * _rigidbody2D.velocity.y;
        }
    }
}