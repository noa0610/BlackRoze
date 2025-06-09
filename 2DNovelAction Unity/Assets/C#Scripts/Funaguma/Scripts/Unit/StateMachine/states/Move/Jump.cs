using UnityEngine;

namespace BlackRose
{
    public class Jump : IState
    {
        protected Rigidbody2D _rb;
        protected bool _hasLeapt = false;
        protected float _cutMultiplier = 0.5f;  // カット時に垂直速度を何割にするか
        protected StatusAmount _statusAmount;

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
            _statusAmount = amount;
        }

        // 前のStateから切り替わった瞬間に呼ばれる
        public virtual bool Enter(IState previousState, IUnit parent)
        {
            if (!_hasLeapt)
            {
                if(!parent.StatusManager.TryGetStatus(Status.JumpPower, out var amount))
                {
                    Debug.LogError("JumpPowerが登録されてないよ！");
                    return false;
                }
                // 一度だけ上方向にインパルス
                _rb.AddForce(Vector2.up * amount.ChangedMax, ForceMode2D.Impulse);
                _hasLeapt = true;
            }
            return true;
        }

        // 他のStateへ行くときに呼ばれる
        public virtual bool Exit(IState nextState, IUnit parent)
        {
            return true;
        }

        // ジャンプ中ずっと毎フレーム呼ばれる
        public virtual bool Stay(IUnit parent)
        {
            if (!parent.StateFlags.HasFlag(StateFlags.InMove))
                return true;
            // 横移動入力を取り出し
            float h = parent.Direction.x;
            if (h == 0)
                return true;



            // 現在の上方向速度はキープしつつ、横速度だけ書き換え
            Vector2 vel = _rb.velocity;
            float addSpeed = _statusAmount.ChangedMax;
            vel.x += h * addSpeed * Time.deltaTime;

            _rb.velocity = vel;

            return true;
        }

        // ボタン離したときに呼ぶと、上向き速度をカットして短ジャンにできる
        public void CutJump()
        {
            if (_rb.velocity.y > 0f)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _cutMultiplier);
            }
        }
    }
}
//unicode