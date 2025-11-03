using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// ターゲットを外部から指定して追尾するミサイル
    /// 急角度では曲がれない（旋回速度制限付き）
    /// </summary>
    public class TrackingMissile : Bullet
    {
        [SerializeField, Tooltip("旋回速度（度/秒）")]
        private float _turnSpeed = 180f;

        private UnitBase _target;

        /// <summary>外部からターゲットを設定</summary>
        public void SetTarget(UnitBase target) => _target = target;

        protected override void Move(float deltaTime)
        {
            // Bullet.Invoke() から渡る deltaTime が負の可能性もあるので絶対値を使う
            float dt = Mathf.Abs(deltaTime);

            if (_target != null && _target.Transform != null)
            {
                var myPos = (Vector2)transform.position;
                var targetPos = (Vector2)_target.Transform.position;
                var toTarget = (targetPos - myPos).normalized;

                // ゼロ割回避
                if (_direction.sqrMagnitude < 1e-6f)
                    _direction = Vector2.right;

                var currentDir = _direction.normalized;

                // いまの方向→目標方向の角度差（-180～+180）
                float signed = Vector2.SignedAngle(currentDir, toTarget);
                // 1フレームで回せる最大角度
                float maxStep = _turnSpeed * dt;
                // その範囲にクランプ
                float step = Mathf.Clamp(signed, -maxStep, maxStep);

                // Z軸回転で currentDir を回す（2Dなので forward 回り）
                var rotated = Quaternion.Euler(0f, 0f, step) * new Vector3(currentDir.x, currentDir.y, 0f);
                _direction = new Vector2(rotated.x, rotated.y).normalized;

                // 見た目も進行方向へ
                float z = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, z);
            }

            // 位置更新（基底の挙動に合わせて直接移動）
            transform.position += (Vector3)_direction * _status.speed * deltaTime;
        }
    }
}
