using System;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.Units.State
{
    [Serializable]
    public class Jump : AccelMoveBase
    {
        protected bool _hasLeapt = false;
        [SerializeField] protected int _enableJumped = -1;
        [SerializeField] protected float _cutMultiplier = 0.5f;  // 上昇中にカットする倍率

        // 連続ジャンプ回数
        protected int _jumpCount = 0;
        public bool HadLeapt
        {
            get => _hasLeapt;
            set => _hasLeapt = value;
        }

        protected override Status Status => Status.SpeedInAir;

        public override bool AllowEnter()
        {
            return !_hasLeapt;
        }
        public override void Enter()
        {
            if (Rigidbody2D == null)
            {
                Debug.LogError($"{nameof(Jump)}: Rigidbody2D not set.");
                return;
            }

            if (!Cont.StatusManager.TryReadValue(Status.JumpPower, out var amount))
                Debug.LogError("JumpPowerが登録されてないよ！");

            // 一度だけ上方向にインパルス
            Rigidbody2D.AddForce(Vector2.up * amount, ForceMode2D.Impulse);
            if (_enableJumped == -1 || _jumpCount >= _enableJumped)
                _hasLeapt = true;
            else if (_enableJumped != -1)
                _jumpCount++;

            Cont.OnCanceledJump.Take(1).Subscribe(_ => Cut());
        }

        public override void Exit()
        {
            Cont.OnAirToGround.Take(1).Subscribe(_ =>
            {
                ResetLeaptFlag();
            });
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
