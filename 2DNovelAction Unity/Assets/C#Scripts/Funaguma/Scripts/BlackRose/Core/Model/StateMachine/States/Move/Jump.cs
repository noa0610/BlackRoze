using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class Jump : StateWithAnime
    {
        [SerializeField] protected Rigidbody2D _rb;
        [SerializeField] protected bool _hasLeapt = false;
        [SerializeField] protected float _cutMultiplier = 0.5f;  // カット時に垂直速度を何割にするか

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
        public Jump(Rigidbody2D rb)
        {
            _rb = rb;
        }
        public Jump() { }
        // 前のIStateから切り替わった瞬間に呼ばれる
        public override void Enter(IState previousIState, UnitBase parent)
        {
            if (!_hasLeapt)
            {
                if (!parent.StatusManager.TryReadValue(Status.JumpPower, out var amount))
                {
                    Debug.LogError("JumpPowerが登録されてないよ！");
                }
                // 一度だけ上方向にインパルス
                _rb.AddForce(Vector2.up * amount, ForceMode2D.Impulse);
                _hasLeapt = true;
            }
        }

        // ジャンプ中ずっと毎フレーム呼ばれる
        public override void Stay(UnitBase parent)
        {
            // 横移動入力を取り出し
            float h = parent.Direction.x;
            float speed = parent.StatusManager.ReadValue(Status.SpeedInAir);
            if (h == 0) return;
            // 現在の上方向速度はキープしつつ、横速度だけ書き換え
            Vector2 vel = _rb.velocity;
            vel.x += h * speed * Time.deltaTime;
            _rb.AddForce(vel);
        }
        public void Cut()
        {
            if (_rb.velocity.y > 0f)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _cutMultiplier);
            }
        }
    }
}