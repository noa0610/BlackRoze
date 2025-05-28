using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose
{
    public class ActionRobot : GroundedUnit
    {
        [Header("Reference")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private List<BulletObject> _bullets = new List<BulletObject>();
        [SerializeField] private LayerMask _targetLayer;
        private Stun _stunState;

        [Header("Coyote Time")]
        [SerializeField] private float coyoteTime = 0.2f;         // 地面離れてからジャンプ猶予(sec)
        private float coyoteTimeCounter;

        private InputActionMap _Player;
        private Rigidbody2D _rigidbody;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _forceEnableJump;
#endif
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

            var forward = new ShootForward(_bullets[0], _targetLayer);
            forward.OnShootComplete += OnshootComplete;
            _stateMachine.AddState("shoot", forward);
            _stateMachine.AddState("move", new MoveOnGround(_rigidbody, "Move", _statusManager.GetStatusAmount(Status.Speed)));
            _stateMachine.AddState("dash", new DashOnGround(_rigidbody, this, _statusManager.GetStatusAmount(Status.DashSpeed)));
            _stateMachine.AddState("jump", new Jump(_rigidbody, _statusManager.GetStatusAmount(Status.SpeedInAir)));
            _stunState = new Stun(_rigidbody, 0.5f);
            _stateMachine.AddState("stun", _stunState);
        }

        private void OnEnable()
        {
            _Player.Enable();
            _Player.FindAction("Move").performed += InMove;
            _Player.FindAction("Move").canceled += InCanceledMove;
            _Player.FindAction("Attack").performed += InAttack;
            _Player.FindAction("Jump").performed += InJump;
            _Player.FindAction("Jump").canceled += InCancelJump;
            _Player.FindAction("Dash").performed += InDash;
            _Player.FindAction("Dash").canceled += InCancelDash;
        }

        private void OnDisable()
        {
            _Player.FindAction("Move").performed -= InMove;
            _Player.FindAction("Move").canceled -= InCanceledMove;
            _Player.FindAction("Attack").performed -= InAttack;
            _Player.FindAction("Jump").performed -= InJump;
            _Player.FindAction("Jump").canceled -= InCancelJump;
            _Player.FindAction("Dash").performed -= InDash;
            _Player.FindAction("Dash").canceled -= InCancelDash;
            _Player.Disable();
        }

        private void InMove(InputAction.CallbackContext ctx)
        {
            Direction = ctx.ReadValue<Vector2>();
            _stateFlags |= StateFlags.InMove;
        }

        private void InCanceledMove(InputAction.CallbackContext ctx)
        {
            _stateFlags &= ~StateFlags.InMove;
            Debug.Log("Canceled Move.");
        }

        private void InAttack(InputAction.CallbackContext ctx)
        {
            _stateFlags |= StateFlags.InShoot;
            var b = _bullets[0].Clone();
            b.currentstatus = b.bulletData.originalstatus; // 初期状態をコピー
            b.currentstatus.direction = Direction == Vector2.zero
                ? Vector2.right
                : new Vector2(Direction.x, 0);
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

        private void InDash(InputAction.CallbackContext ctx)
        {
            if (_stateFlags.HasFlag(StateFlags.InJump | StateFlags.InFall) || !IsGrounded)
                return; // 既にダッシュ中、または地面にいない場合は無視
            _stateFlags |= StateFlags.InDash;
        }
        private void InCancelDash(InputAction.CallbackContext ctx)
        {
            _stateFlags &= ~StateFlags.InDash;
        }
        protected override void OnGrounded()
        {
            coyoteTimeCounter = coyoteTime;
            _stateFlags &= ~StateFlags.InJump;

            if (_stateMachine.StateMap["jump"] is Jump jump)
                jump.HadLeapt = false;
        }
        protected override void OnUnGrounded()
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
        protected override string StateDecision()
        {
            if (_stateFlags.HasFlag(StateFlags.InStun))
            {
                return "stun";
            }
            if (_stateFlags.HasFlag(StateFlags.InShoot))
            {
                _stateFlags &= ~StateFlags.InShoot;
                return "shoot";
            }
            if (_stateFlags.HasFlag(StateFlags.InJump))
            {
                _stateFlags &= ~StateFlags.InJump;
                return "jump";
            }
            if (_stateFlags.HasFlag(StateFlags.InDash) && IsGrounded)
                return "dash";
            if (_stateFlags.HasFlag(StateFlags.InMove))
                return "move";
            return "idle";
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            _stateFlags |= StateFlags.InStun; // ダメージを受けたらスタン状態にする
        }
        private void OnshootComplete()
        {
            _stateFlags &= ~StateFlags.InShoot;
        }

        protected override void Update()
        {
            base.Update();
            if (_stateFlags.HasFlag(StateFlags.InStun) && _stunState.StunTimer <= 0f)
            {
                _stateFlags &= ~StateFlags.InStun; // スタンが終わったらフラグを下ろす
            }
        }
    }
}
//unicode