using System;
using System.Collections.Generic;
using UnityEngine;
using BlackRose.Core.Models.Units;
namespace BlackRose.Core.Models.States
{
    public class PositionJump : StateWithAnime
    {
        private Rigidbody2D _rb;
        private List<Vector2> _positions;
        private float _savedGravity;
        private float _jumpSpeed;      // 水平方向の速度
        private Vector2 _startPos;
        public Vector2 TargetPosition { get; private set; }
        public Action OnArrived { get; private set; } // 到達時のコールバック
        public PositionJump(List<Vector2> positions, float jumpSpeed)
        {
            SetPositions(positions);
            _jumpSpeed = jumpSpeed;
        }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            if (!parent.TryGetComponent(out _rb)) return;

            // 初期化
            _savedGravity = _rb.gravityScale;
            if (_savedGravity == 0) _rb.gravityScale = 1;  // もし重力無効なら有効化
            _rb.velocity = Vector2.zero;
            _startPos = _rb.position;

            // 放物線用の初速計算
            Vector2 delta = TargetPosition - _startPos;
            float time = Mathf.Abs(delta.x) / _jumpSpeed;
            if (time <= 0f) return;

            // Physics2D.gravity.y はマイナス（下方向）なので、そのまま使う
            float g = Physics2D.gravity.y * _rb.gravityScale;
            float vy = (delta.y - 0.5f * g * time * time) / time;
            Vector2 launchVelocity = new Vector2(Mathf.Sign(delta.x) * _jumpSpeed, vy);

            _rb.velocity = launchVelocity;
        }

        public override void Stay(UnitBase parent)
        {
            base.Stay(parent);
            if (_rb == null) return;
            // 目標位置に到達したかチェック
            if (Vector2.Distance(_rb.position, TargetPosition) < 0.1f)
            {
                _rb.velocity = Vector2.zero; // 到達時の速度をゼロにする
                OnArrived?.Invoke(); // 到達時のコールバックを呼び出す
            }
        }
        public override void Exit(IState nextIState, UnitBase parent)
        {
            base.Exit(nextIState, parent);
            if (_rb != null)
                _rb.gravityScale = _savedGravity;
        }

        public void SetTarget(int index)
        {
            if (index < 0 || index >= _positions.Count)
            {
                Debug.LogError("Index out of range for positions list.");
                return;
            }
            TargetPosition = _positions[index];
        }

        public void SetPositions(List<Vector2> positions)
        {
            _positions = new List<Vector2>(positions);
            TargetPosition = _positions.Count > 0 ? _positions[0] : Vector2.zero;
        }
    }
}
