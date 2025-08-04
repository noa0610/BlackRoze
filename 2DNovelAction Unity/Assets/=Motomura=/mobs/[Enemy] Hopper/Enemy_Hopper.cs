using UnityEngine;

namespace BlackRose
{
    // 変更点：継承元を UnitBase から GroundedUnit に変更
    public partial class Enemy_Hopper : GroundedUnit 
    {
        [Header("AI Intervals")] // Header名を分かりやすく変更（任意）
        [SerializeField] private float _IdleInterval;
        [SerializeField] private float _ApproachInterval = 1.0f;
        [SerializeField] private float _AttackInterval = 1.0f;

        // ▼▼▼ Enemy_Hopper_AI.cs から全てのコードをここに移動 ▼▼▼
        [Header("AI Parameters")]
        [SerializeField] private float _searchRadius = 10f;
        [SerializeField] private float _closeDistance = 3f;
        [SerializeField] private LayerMask _targetLayer;

        [Header("Jump Power")]
        [SerializeField] private float _moveJumpPower = 5f;
        [SerializeField] private float _smallJumpPower = 4f;
        [SerializeField] private float _attackJumpPower = 7f;

        // --- 内部参照 ---
        private Rigidbody2D _rb;
        private Transform _playerTransform;
        private string _previousStateKey;

        // --- 抽象メソッドの実装 ---
        // GroundedUnitから継承したため、これらの実装が必須
        protected override void OnGrounded() { }
        protected override void OnUnGrounded() { }

        // --- 既存メソッドのオーバーライド ---
        protected override void Awake()
        {
            base.Awake(); // GroundedUnitのAwakeを呼び出す
            _rb = GetComponent<Rigidbody2D>();
            OnAirToGround += HandleLanding;
        }

        protected override void Update()
        {
            base.Update(); // UnitBaseのUpdateを呼び出す
            if (!_isPlaying) return;
            DetectStateChangeAndExecuteJump();
        }

        // GroundedUnitのFixedUpdateはprivateなので、こちらで新しく定義する
        // (Unityが両方を呼び出してくれる)
        private void FixedUpdate()
        {
            if (!_isPlaying) return;
            SearchAndJudgePlayer();
        }

        // --- AIロジックメソッド ---
        private void SearchAndJudgePlayer()
        {
            if (_playerTransform == null)
            {
                var hit = Physics2D.OverlapCircle(transform.position, _searchRadius, _targetLayer);
                if (hit != null)
                {
                    _playerTransform = hit.transform;
                    _stateMachine.ChangeState(Triggers.FoundPlayer.ToString());
                }
            }
            else
            {
                float distance = Vector2.Distance(transform.position, _playerTransform.position);
                if (distance > _searchRadius)
                {
                    _playerTransform = null;
                    _stateMachine.ChangeState(Triggers.MissingPlayer.ToString());
                    return;
                }

                var currentStateKey = _stateMachine.CurrentState.key;
                if (currentStateKey == States.WaitingForEnemyContact.ToString())
                {
                    if (distance <= _closeDistance)
                        _stateMachine.ChangeState(Triggers.CloseDistance.ToString());
                }
            }
        }
        
        private void DetectStateChangeAndExecuteJump()
        {
            string currentStateKey = _stateMachine.CurrentState.key;
            if (currentStateKey != _previousStateKey)
            {
                if (currentStateKey == States.MoveJump.ToString()) Jump(_moveJumpPower);
                else if (currentStateKey == States.SmallJump.ToString()) Jump(_smallJumpPower);
                else if (currentStateKey == States.AttackJump.ToString()) Jump(_attackJumpPower);
            }
            _previousStateKey = currentStateKey;
        }

        private void Jump(float power)
        {
            if (!IsGrounded) return;

            if (_playerTransform != null)
            {
                float direction = (_playerTransform.position.x > transform.position.x) ? 1f : -1f;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
            }

            _rb.velocity = new Vector2(_rb.velocity.x, 0);
            _rb.AddForce(Vector2.up * power, ForceMode2D.Impulse);

            AfterJump();
        }

        private void HandleLanding()
        {
            _stateMachine.ChangeState(Triggers.Landing.ToString());
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _searchRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _closeDistance);
        }
    }
}