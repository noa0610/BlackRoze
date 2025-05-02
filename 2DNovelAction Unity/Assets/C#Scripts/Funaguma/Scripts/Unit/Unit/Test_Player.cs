using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;

namespace Test
{
    public class Test_Player : UnitBase
    {
        [Header("Reference")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private List<BulletObject> _bullets = new List<BulletObject>();

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;           // 足元チェック用のTransform
        [SerializeField] private float groundCheckRadius = 0.1f;  // チェック半径
        [SerializeField] private LayerMask groundLayer;           // 地面Layer

        [Header("Coyote Time")]
        [SerializeField] private float coyoteTime = 0.2f;         // 地面離れてからジャンプ猶予(sec)
        private float coyoteTimeCounter;

        private InputActionMap _Player;
        private Rigidbody2D _rigidbody;

        [Header("Debug")]
        [SerializeField]private bool _forceEnableJump;

        // 地面にいるかどうか（OverlapCircle判定＆コヨーテタイム管理）
        public bool canJump => _forceEnableJump || coyoteTimeCounter > 0f;

        public List<BulletObject> Bullets
        {
            get => _bullets;
            set => _bullets = value;
        }
        protected virtual void Awake()
        {
            _Player = _inputActions.FindActionMap("Player");
        }
        protected override void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            base.Start();
        }

        protected override void RegisterStats()
        {
            _stateMachine.SetCondition(StateDecision);

            _stateMachine.AddState("move", new MoveHorizontal(_rigidbody));

            var forward = new ShootForward(_bullets[0]);
            forward.OnShootComplete += OnshootComplete;
            _stateMachine.AddState("shoot", forward);

            _stateMachine.AddState("jump", new Jump(_rigidbody));
        }

        private void OnEnable()
        {
            _Player.Enable();
            _Player.FindAction("Move").performed += InMove;
            _Player.FindAction("Move").canceled += InCanceledMove;
            _Player.FindAction("Attack").performed += InAttack;
            _Player.FindAction("Jump").performed += InJump;
            _Player.FindAction("Jump").canceled += InCancelJump;
        }

        private void OnDisable()
        {
            _Player.FindAction("Move").performed -= InMove;
            _Player.FindAction("Move").canceled -= InCanceledMove;
            _Player.FindAction("Attack").performed -= InAttack;
            _Player.FindAction("Jump").performed -= InJump;
            _Player.FindAction("Jump").canceled -= InCancelJump;
            _Player.Disable();
        }

        private void InMove(InputAction.CallbackContext ctx)
        {
            status.direction = ctx.ReadValue<Vector2>();
            _stateFlags |= StateFlags.InMove;
        }

        private void InCanceledMove(InputAction.CallbackContext ctx)
        {
            _stateFlags &= ~StateFlags.InMove;
            status.direction = Vector2.zero;
        }

        private void InAttack(InputAction.CallbackContext ctx)
        {
            _stateFlags |= StateFlags.InShoot;
            var b = _bullets[0].Clone();
            b.currentstatus.direction = status.direction == Vector2.zero
                ? Vector2.right
                : new Vector2(status.direction.x, 0);
            var state = (ShootForward)_stateMachine.StateMap["shoot"];
            state.SetBullet(b);
        }

        private void InJump(InputAction.CallbackContext ctx)
        {
            if (canJump)
            {
                _stateFlags |= StateFlags.InJump;
                coyoteTimeCounter = 0f;  // ジャンプしたら猶予リセット
            }
        }

        private void InCancelJump(InputAction.CallbackContext ctx)
        {
            if (_stateMachine.StateMap["jump"] is Jump jump)
                jump.CutJump();
        }

        private void Update()
        {
            
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            GroundCheck();              // 毎フレーム地面判定＆コヨーテタイム更新
        }
        private void GroundCheck()
        {
            // OverlapCircleはCollider2Dを返すから、Collider2D型で受けるとわかりやすい！
            Collider2D hit = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer.value       // LayerMaskをIntに変換して渡す
            );

            bool grounded = hit != null;

            if (grounded)
            {
                coyoteTimeCounter = coyoteTime;
                _stateFlags &= ~StateFlags.InJump;

                if (_stateMachine.StateMap["jump"] is Jump jump)
                    jump.HadLeapt = false;
            }
            else
            {
                coyoteTimeCounter -= Time.fixedDeltaTime;
            }
        }

        protected override string StateDecision()
        {
            if (_stateFlags.HasFlag(StateFlags.InShoot))
            {
                _stateFlags &= ~StateFlags.InShoot;
                return "shoot";
            }
            if (_stateFlags.HasFlag(StateFlags.InJump))
                return "jump";
            if (_stateFlags.HasFlag(StateFlags.InMove))
                return "move";
            return "idle";
        }

        private void OnshootComplete()
        {
            _stateFlags &= ~StateFlags.InShoot;
        }

        // デバッグ用にGizmos表示
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
//unicode