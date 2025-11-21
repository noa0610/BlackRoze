using BlackRose.Core.Models.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class PositionJump : StateComp, IRigidbodyUser, ICompleteEmitter
    {
        [SerializeField]
        private List<Vector2> _positions;
        [SerializeField]
        private float _jumpSpeed;      // 水平方向の速度
        [SerializeField, Min(0)]
        private int _targetIdx = 0;
        private Vector2 _startPos;
        private float _savedGravity;

        public Vector2 TargetPosition { get; private set; }

        // 既存APIのため、OnCompletedにアタッチする形で実装
        public event Action OnArrived { add=> OnCompleted += value; remove => OnCompleted -= value; }
        public event Action OnCompleted;

        public Rigidbody2D Rigidbody2D { get; private set; }

        public PositionJump(List<Vector2> positions, float jumpSpeed)
           : base()
        {
            SetPositions(positions);
            _jumpSpeed = jumpSpeed;
        }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            // 初期化
            TargetPosition = _positions[_targetIdx];
            _savedGravity = Rigidbody2D.gravityScale;
            if (_savedGravity == 0) Rigidbody2D.gravityScale = 1;  // もし重力無効なら有効化
            Rigidbody2D.velocity = Vector2.zero;
            _startPos = Rigidbody2D.position;

            // 放物線用の初速計算
            Vector2 delta = TargetPosition - _startPos;
            float time = Mathf.Abs(delta.x) / _jumpSpeed;
            if (time <= 0f) return;

            // Physics2D.gravity.y はマイナス（下方向）なので、そのまま使う
            float g = Physics2D.gravity.y * Rigidbody2D.gravityScale;
            float vy = (delta.y - 0.5f * g * time * time) / time;
            Vector2 launchVelocity = new Vector2(Mathf.Sign(delta.x) * _jumpSpeed, vy);

            Rigidbody2D.velocity = launchVelocity;
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            base.Stay(parent, deltaTime);
            if (Rigidbody2D == null) return;
            // 目標位置に到達したかチェック
            if (Vector2.Distance(Rigidbody2D.position, TargetPosition) < 0.1f)
            {
                Rigidbody2D.velocity = Vector2.zero; // 到達時の速度をゼロにする
                OnCompleted?.Invoke(); // 到達時のコールバックを呼び出す
            }
        }
        public override void Exit(IState nextIState, UnitBase parent)
        {
            base.Exit(nextIState, parent);
            if (Rigidbody2D != null)
                Rigidbody2D.gravityScale = _savedGravity;
        }

        public void SetTarget(int index)
        {
            if (index < 0 || index >= _positions.Count)
            {
                Debug.LogError("Index out of range for positions list.");
                return;
            }
            _targetIdx = index;
        }

        public void SetPositions(List<Vector2> positions)
        {
            _positions = new List<Vector2>(positions);
            TargetPosition = _positions.Count > 0 ? _positions[0] : Vector2.zero;
        }

        public void SetRB2(Rigidbody2D rb)
        {
            Rigidbody2D = rb;
        }
    }
}
