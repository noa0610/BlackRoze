using UnityEngine;

namespace Test
{
    public class Jump : IState
    {
        Rigidbody2D _rb;
        bool _hasLeapt = false;
        float _cutMultiplier = 0.5f;  // カット時に垂直速度を何割にするか

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

        public Jump(Rigidbody2D rb)
        {
            _rb = rb;
        }

        // 前のStateから切り替わった瞬間に呼ばれる
        public bool Enter(IState previousState, IUnit parent)
        {
            if (!_hasLeapt)
            {
                // 一度だけ上方向にインパルス
                _rb.AddForce(Vector2.up * parent.UnitStatus.jumpPower, ForceMode2D.Impulse);
                _hasLeapt = true;
            }
            return true;
        }

        // 他のStateへ行くときに呼ばれる
        public bool Exit(IState nextState, IUnit parent)
        {
            return true;
        }

        // ジャンプ中ずっと毎フレーム呼ばれる
        public bool Stay(IUnit parent)
        {
            if (!parent.StateFlags.HasFlag(StateFlags.InMove))
                return true;
            // 横移動入力を取り出し
            float h = parent.UnitStatus.direction.x;
            if (h == 0)
                return true;



            // 現在の上方向速度はキープしつつ、横速度だけ書き換え
            Vector2 vel = _rb.linearVelocity;
            float addSpeed = parent.UnitStatus.speedInAir;
            vel.x += h * addSpeed * Time.deltaTime;

            _rb.linearVelocity = vel;

            return true;
        }

        // ボタン離したときに呼ぶと、上向き速度をカットして短ジャンにできる
        public void CutJump()
        {
            if (_rb.linearVelocity.y > 0f)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * _cutMultiplier);
            }
        }
    }
}
//unicode