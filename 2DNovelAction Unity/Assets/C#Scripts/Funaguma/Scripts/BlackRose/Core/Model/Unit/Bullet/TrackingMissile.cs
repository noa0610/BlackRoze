using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// ターゲットを外部から指定して追尾するミサイル
    /// 急角度では曲がれない（旋回速度制限付き）
    /// </summary>
    public class TrackingMissile : Bullet
    {
        [SerializeField, Tooltip("旋回速度（度/秒)")]
        private float _turnSpeed = 180f;

        [Header("Initial straight settings")]
        [SerializeField, Tooltip("最初にY座標を固定して直進する時間（秒）")]
        private float _straightDuration = 0.6f;
        [SerializeField, Tooltip("最初の直進中にY座標を固定するかどうか（true = 固定）")]
        private bool _lockYDuringStraight = true;

        private UnitBase _target;

        // internal state
        private bool _initialized = false;
        private float _straightTimer = 0f;
        private float _initialY = 0f;

        /// <summary>外部からターゲットを設定</summary>
        public void SetTarget(UnitBase target) => _target = target;

        /// <summary>
        /// プールから取得した際に内部状態をリセットする
        /// </summary>
        public void ResetForPool()
        {
            _initialized = false;
            _straightTimer = 0f;
            _initialY = transform.position.y;
            _target = null;
            // reset direction to current right vector if available
            _direction = transform.right;
        }

        protected override void Move(float deltaTime)
        {
            // Bullet.Invoke() から渡る deltaTime が負の可能性もあるので絶対値を使う
            float dt = Mathf.Abs(deltaTime);

            if (!_initialized)
            {
                _initialized = true;
                _straightTimer = 0f;
                _initialY = transform.position.y;
            }

            // If we're still in the initial straight phase, move straight keeping Y fixed
            if (_straightTimer < _straightDuration)
            {
                _straightTimer += dt;

                // ensure direction exists
                if (_direction.sqrMagnitude < 1e-6f)
                    _direction = Vector2.right;

                var moveDir = _direction.normalized;

                // compute new position: advance along X component; if not locking Y, advance Y as well
                Vector3 pos = transform.position;
                pos.x += moveDir.x * _status.speed * deltaTime;
                if (_lockYDuringStraight)
                    pos.y = _initialY;
                else
                    pos.y += moveDir.y * _status.speed * deltaTime;

                transform.position = pos;

                // rotate to face movement direction (if locked Y, face purely left/right)
                float rotY = _lockYDuringStraight ? 0f : moveDir.y;
                float z = Mathf.Atan2(rotY, moveDir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, z);

                return;
            }

            // After straight phase, perform homing if target exists
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
