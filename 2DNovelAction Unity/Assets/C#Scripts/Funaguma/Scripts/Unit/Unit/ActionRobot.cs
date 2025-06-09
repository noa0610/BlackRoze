using HighElixir.Pool;
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

        [Header("Option Settings")]
        [SerializeField] private float coyoteTime = 0.2f;         // 地面離れてからジャンプ猶予(sec)
        [SerializeField] private float[] _chargeShoot = new float[2] { 1.8f, 3.4f}; // チャージ攻撃用の時間配列
        [SerializeField] private bool _canChargeCount = false;
        [SerializeField] private Vector2 _stunKnockback = Vector2.zero;

        private float coyoteTimeCounter;
        private InputActionMap _Player;
        private Rigidbody2D _rigidbody;
        private float _shootPressTime = 0f; // 攻撃ボタンを押した時間


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
        protected override void Awake()
        {
            _Player = _inputActions.FindActionMap("Player");
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        protected override void RegisterStats()
        {
            _stateMachine.SetCondition(StateDecision);

            var normal = new ShootForward(_bullets[0], _targetLayer);
            normal.onShootComplete += OnshootComplete;
            _stateMachine.AddState("shoot", normal);
            var charge = new ShootForward(_bullets[1], _targetLayer);
            charge.onShootComplete += OnshootComplete;
            _stateMachine.AddState("chargeShoot", charge);
            var full = new ShootForward(_bullets[2], _targetLayer);
            full.onShootComplete += OnshootComplete;
            _stateMachine.AddState("fullChargeShoot", full);

            _stateMachine.AddState("move", new MoveOnGround(_rigidbody, "Move", statusManager.GetStatusAmount(Status.Speed)));
            _stateMachine.AddState("dash", new DashOnGround(_rigidbody, this, statusManager.GetStatusAmount(Status.DashSpeed)));
            _stateMachine.AddState("jump", new Jump(_rigidbody, statusManager.GetStatusAmount(Status.SpeedInAir)));
            _stunState = new Stun(_rigidbody, GetComponent<PopText>(), 0.5f, _stunKnockback);
            _stateMachine.AddState("stun", _stunState);
        }

        private void OnEnable()
        {
            _Player.Enable();
            _Player.FindAction("Move").performed += InMove;
            _Player.FindAction("Move").canceled += InCanceledMove;
            _Player.FindAction("Attack").performed += InAttack;
            _Player.FindAction("Attack").canceled += CanceledAttack; 
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
            _Player.FindAction("Attack").canceled -= CanceledAttack;
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

        private void InCanceledMove(InputAction.CallbackContext _)
        {
            _stateFlags &= ~StateFlags.InMove;
            Debug.Log("Canceled Move.");
        }

        private void InAttack(InputAction.CallbackContext _)
        {
            // 入力時に一度通常攻撃を行い、その後チャージを行う
            _stateFlags |= StateFlags.InShoot;
            SetBulletToShootstate<ShootForward>("shoot", _bullets[0].Clone());
            _canChargeCount = true; // 攻撃ボタンを押したのでチャージ可能状態にする
        }

        private void CanceledAttack(InputAction.CallbackContext _)
        {
            _canChargeCount = false; // 攻撃ボタンを離したのでチャージ不可状態にする
            if (_chargeShoot[0] <= _shootPressTime && _shootPressTime < _chargeShoot[1])
            {
                Debug.Log("チャージ１");
                // チャージ攻撃の状態にする
                SetBulletToShootstate<ShootForward>("chargeShoot", _bullets[1].Clone());
            }
            else if (_shootPressTime >= _chargeShoot[1])
            {
                Debug.Log("フルチャージ");
                // フルチャージ攻撃の状態にする
                SetBulletToShootstate<ShootForward>("fullChargeShoot", _bullets[2].Clone());
            }
            _shootPressTime = 0f; // 攻撃ボタンを離したので時間をリセット
        }
        private void SetBulletToShootstate<T>(string targetState, BulletObject bullet) where T : ShootStateBase
        {
            bullet.currentstatus = bullet.bulletData.originalstatus; // 初期状態をコピー
            bullet.currentstatus.direction = Direction == Vector2.zero
                ? Vector2.right
                : new Vector2(Direction.x, 0);
            var state = (T)_stateMachine.StateMap[targetState];
            state.SetBullet(bullet);
        }
        private void InJump(InputAction.CallbackContext _)
        {
            if (canJump)
            {
                _stateFlags |= StateFlags.InJump;
                coyoteTimeCounter = 0f;  // ジャンプしたら猶予リセット
            }
        }

        private void InCancelJump(InputAction.CallbackContext _)
        {
            if (_stateMachine.StateMap["jump"] is Jump jump)
                jump.CutJump();
        }

        private void InDash(InputAction.CallbackContext _)
        {
            if (_stateFlags.HasFlag(StateFlags.InJump | StateFlags.InFall) || !IsGrounded)
                return; // 既にダッシュ中、または地面にいない場合は無視
            _stateFlags |= StateFlags.InDash;
        }
        private void InCancelDash(InputAction.CallbackContext _)
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
            if (_canChargeCount)
                _shootPressTime += Time.deltaTime; // 攻撃ボタンを押している間、時間をカウント
            
            if (_stateFlags.HasFlag(StateFlags.InStun) && _stunState.StunTimer <= 0f)
            {
                _stateFlags &= ~StateFlags.InStun; // スタンが終わったらフラグを下ろす
            }
        }
    }
}
//unicode