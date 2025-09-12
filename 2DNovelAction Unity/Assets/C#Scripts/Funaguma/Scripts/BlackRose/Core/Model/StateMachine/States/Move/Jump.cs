using BlackRose.Core.Models.States.Helpers;
using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class Jump : MovingStateBase
    {
        [SerializeField] protected bool _hasLeapt = false;
        [SerializeField] protected int _enableJumped = -1;
        [SerializeField] protected float _cutMultiplier = 0.5f;  // 上昇中にカットする倍率
        [SerializeField] protected float _accel = 60f;           // 横方向の加速（m/s^2 想定）
        [SerializeField] protected float _airFriction = 30f;   // 入力がないときの減速

        // 連続ジャンプ回数
        protected int _jumpCount = 0;
        public bool HadLeapt
        {
            get => _hasLeapt;
            set => _hasLeapt = value;
        }
        public Jump() { }

        public override bool AllowEnter(IState previousState, UnitBase parent)
        {
            if (base.AllowEnter(previousState, parent))
            {
                return !_hasLeapt;
            }
            return false;
        }
        public override void Enter(IState previousIState, UnitBase parent)
        {
            if (Rigidbody2D == null)
            {
                Debug.LogError($"{nameof(Jump)}: Rigidbody2D not set.");
                return;
            }

            if (!parent.StatusManager.TryReadValue(Status.JumpPower, out var amount))
                Debug.LogError("JumpPowerが登録されてないよ！");

            // 一度だけ上方向にインパルス
            Rigidbody2D.AddForce(Vector2.up * amount, ForceMode2D.Impulse);
            if (_enableJumped == -1 || _jumpCount >= _enableJumped)
                _hasLeapt = true;
            else if (_enableJumped != -1)
                _jumpCount++;
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

        // いわゆる「ジャンプカット」：入力離しで上昇を弱める
        public void Cut()
        {
            if (Rigidbody2D == null) return;

            if (Rigidbody2D.velocity.y > 0f)
                Rigidbody2D.velocity = new Vector2(Rigidbody2D.velocity.x, Rigidbody2D.velocity.y * _cutMultiplier);
        }

        // どこか（例：着地イベントやExit）で呼んでリセットする想定
        public void ResetLeaptFlag()
        {
            _hasLeapt = false;
            _jumpCount = 0;
        }

        public void SetEnableJumped(int count)
        {
            _enableJumped = count;
        }
    }
}
