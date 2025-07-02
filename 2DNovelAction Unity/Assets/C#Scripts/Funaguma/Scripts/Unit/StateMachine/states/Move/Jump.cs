using System;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public class Jump : StateComp
    {
        [SerializeField] protected Rigidbody2D _rb;
        [SerializeField] protected bool _hasLeapt = false;
        [SerializeField] protected float _cutMultiplier = 0.5f;  // カット時に垂直速度を何割にするか
        protected StatusAmount _moveOnAir;

        public bool HadLeapt
        {
            get
            {
                return _hasLeapt;
            }
            set
            {
                _hasLeapt = value;
            }
        }

        /// <param name="amount">Status.SpeedInAir</param>
        public Jump(Rigidbody2D rb, StatusAmount amount)
        {
            _rb = rb;
            _moveOnAir = amount;
        }
        public Jump() { }
        // 前のIStateから切り替わった瞬間に呼ばれる
        public override void Enter(IState previousIState, IUnit parent)
        {
            if (!_hasLeapt)
            {
                if (!parent.StatusManager.TryGetStatus(Status.JumpPower, out var amount))
                {
                    Debug.LogError("JumpPowerが登録されてないよ！");
                }
                // 一度だけ上方向にインパルス
                _rb.AddForce(Vector2.up * amount.ChangedMax, ForceMode2D.Impulse);
                _hasLeapt = true;
            }
        }

        // ジャンプ中ずっと毎フレーム呼ばれる
        public override void Stay(IUnit parent)
        {
            // 横移動入力を取り出し
            float h = parent.Direction.x;
            if (h == 0) return;
            if (_moveOnAir == null) _moveOnAir = parent.StatusManager.GetStatusAmount(Status.SpeedInAir);
            // 現在の上方向速度はキープしつつ、横速度だけ書き換え
            Vector2 vel = _rb.velocity;
            float addSpeed = _moveOnAir.ChangedMax;
            vel.x += h * addSpeed * Time.deltaTime;
            _rb.velocity = vel;
        }

        public override void Exit(IState nextIState, IUnit parent)
        {
            if (_rb.velocity.y > 0f)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _cutMultiplier);
            }
        }
    }
}
//unicode