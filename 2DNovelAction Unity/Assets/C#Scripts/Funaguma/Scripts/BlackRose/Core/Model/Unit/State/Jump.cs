using System;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.Units.State
{
    [Serializable]
    public class Jump<T> : AccelMoveBase<T>
        where T : UnitBase
    {
        protected bool _hasLeapt = false;
        [SerializeField] protected int _enableJumped = 0; // -1で∞
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
                Debug.LogError($"{nameof(Jump<T>)}: Rigidbody2D not set.");
                return;
            }

            if (!Cont.StatusManager.TryReadValue(Status.JumpPower, out var amount))
                Debug.LogError("JumpPowerが登録されてないよ！");

            // 一度だけ上方向にインパルス
            Rigidbody2D.AddForce(Vector2.up * amount, ForceMode2D.Impulse);
            var x = Rigidbody2D.velocity.x;
            if (GetDirection().x != 0 && Mathf.Abs(x) > 0.005)
            {
                // ジャンプ直後に進行方向に対して逆向きに入力していた場合、
                // 横方向のブレーキをかける
                float pow = 0;
                if (x > 0 && GetDirection().x < 0) pow = -1;
                if (x < 0 && GetDirection().x > 0) pow = 1;
                pow *= _accel;
                if (pow > x) pow = -x;
                Rigidbody2D.velocity = new Vector2(x + pow, Rigidbody2D.velocity.y);
            }

            if (_enableJumped != -1 && _jumpCount >= _enableJumped)
                _hasLeapt = true;
            else if (_enableJumped > 0)
                _jumpCount++;

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
