using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using HighElixir;
using HighElixir.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput)), Serializable]
    public partial class ActionRobot : GroundedUnit
    {
        [Header("Reference")]
        [SerializeField] private List<BulletData> _bullets = new List<BulletData>();
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private TextThrower _thrower;
        private Stun _stunState;
        private Rigidbody2D _rigidbody;

        [Header("Option Settings")]
        [SerializeField] private float _coyoteTime = 0.2f;         // 地面離れてからジャンプ猶予(sec)
        [SerializeField] private float[] _chargeShoot = new float[2] { 1.8f, 3.4f }; // チャージ攻撃用の時間配列
        [SerializeField] private bool _canChargeCount = false;
        [SerializeField] private Vector2 _stunKnockback = Vector2.zero;
        [SerializeField] private int _maxSuccession = 3; // 最大連射回数
        [SerializeField] private float _shootBlockTime = 0.6f; // 連射をブロックする時間
        [SerializeField] private float _invincibleTime = 0.6f; // 無敵時間
        [SerializeField] private Animator _anim;
        private int _successionCount = 0; // 連射回数
        private float _shootPressTime = 0f; // 攻撃ボタンを押した時間
        private Vector2 _shootDirection = Vector2.right; // 攻撃方向



#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _forceEnableJump;
#endif
        // 地面にいるかどうか（OverlapCircle判定＆コヨーテタイム管理）
        public bool canJump => _forceEnableJump || !Timer.IsFinished(nameof(_coyoteTime));

        public List<BulletData> Bullets
        {
            get => _bullets;
            set => _bullets = value;
        }


        protected override void OnGrounded()
        {
            Timer.Reset(nameof(_coyoteTime));
            _jump.HadLeapt = false;
            _stateMachine.ChangeState(Triggers.landing);
        }
        protected override void OnFall()
        {
            Timer.Start(nameof(_coyoteTime)); // コヨーテタイム開始
            _stateMachine.ChangeState(Triggers.falling);
        }

        protected override bool BeforeTakeDamage(IUnit unit, ref float damage)
        {
            return !IsInvincible;
        }

        protected override void OnTakeDamage(IUnit s, float damage)
        {
            //Debug.Log($"Take Damage : {StatusManager.ReadValue(Status.HP)}/{StatusManager.ReadValue(Status.MaxHP)}");
            IsInvincible = true;
            Timer.Start(nameof(_invincibleTime));
            if (s is not ObjectDamageWorker)
                _stateMachine.ChangeState(Triggers.stuned.ToString());
        }
        public override void Pause()
        {
            GetComponent<UnityEngine.InputSystem.PlayerInput>().currentActionMap.Disable();
            Debug.Log("Input actions disabled for pause.");
        }
        public override void Play()
        {
            GetComponent<UnityEngine.InputSystem.PlayerInput>().currentActionMap.Enable();
            Debug.Log("Input actions enabled for play.");
        }
        // === Private ===

        // === InputAction ===
        #region
        private void OnJump(InputValue value)
        {
            if (canJump)
            {
                _stateMachine.ChangeState(Triggers.jumpInput);
                AfterJump();
                Timer.Stop(nameof(_coyoteTime)); // ジャンプしたのでコヨーテタイムを終了させる
                
            }
            if (!value.isPressed)
            {
                _jump.Cut();
            }
        }
        private void OnDash(InputValue value)
{
    // シフトキーの状態を bool で取得
    bool isPressed = value.isPressed;

    if (!isPressed)
    {
        _stateMachine.LazyChange(Triggers.cancelMove);
        return;
    }
    if (!IsGrounded) return; // 地面にいない場合は無視

    // 現在の向きを取得
    float currentFacing = transform.localScale.x > 0 ? 1f : -1f;

    // 現在向いている方向へダッシュ
    MoveDirection = new Vector2(currentFacing, 0f);
    Direction = MoveDirection;
    
    _stateMachine.ChangeState(Triggers.dashInput);
}
        private void OnMove(InputValue value)
        {
            var d = value.Get<Vector2>();
            if (d != Vector2.zero)
                Direction = d.normalized;
            MoveDirection = d.normalized;
            if (d == Vector2.zero)
            {
                _stateMachine.ChangeState(Triggers.cancelMove);
                //Debug.Log("Canceled Move.");
                return;
            }
            else if (d.x != 0)
            {
                _shootDirection = d; // 横入力がある場合は攻撃方向を更新
            }
            _stateMachine.ChangeState(Triggers.moveInput);
        }
        private void OnAttack(InputValue value)
        {_anim.SetTrigger("toShot");
            if (value.isPressed)
            {
                _normal.SetDirection(_shootDirection); // 攻撃方向を設定
                Debug.Log("Shoot");
                
                // 入力時に一度通常攻撃を行い、その後チャージを行う
                if (IsMatchState(StateKey.shootWait) && _successionCount >= _maxSuccession)
                {
                    Debug.Log("連射ブロック中");
                    return;
                }
                else if (!IsMatchState(StateKey.shootWait))
                    _successionCount = 0;

                if (Timer.IsFinished(nameof(_shootBlockTime)))
                {
                    _successionCount++;
                    if (IsGrounded)
                        _stateMachine.ChangeState(Triggers.shootInput);
                    else
                        _stateMachine.ChangeState(Triggers.shootInAir);
                    _canChargeCount = true; // 攻撃ボタンを押したのでチャージ可能状態にする
                }
                if (_successionCount >= _maxSuccession)
                {
                    _successionCount = 0;
                    Timer.Start(nameof(_shootBlockTime)); // 連射ブロックタイムを開始
                    _stateMachine.LazyChange(Triggers.watingTimeHasElapsed);
                    _thrower.Create(gameObject, "もう疲れたよ...", Color.red);
                }
            }
            else
            {
                Debug.Log("Releaced Attack Button");
                _shootPressTime = 0f; // 攻撃ボタンを離したので時間をリセット
                _canChargeCount = false; // 攻撃ボタンを離したのでチャージ不可状態にする
                if (_chargeShoot[0] <= _shootPressTime && _shootPressTime < _chargeShoot[1])
                {
                    Debug.Log("チャージ１");
                    // チャージ攻撃の状態にする
                    if (IsGrounded)
                        _stateMachine.ChangeState(Triggers.chargeShoot);
                    else
                        _stateMachine.ChangeState(Triggers.halfChargeInAir);
                    _halfCharge.SetDirection(_shootDirection); // 攻撃方向を設定
                }
                else if (_shootPressTime >= _chargeShoot[1])
                {
                    Debug.Log("フルチャージ");
                    if (IsGrounded)
                        _stateMachine.ChangeState(Triggers.fullChargeShoot);
                    else
                        _stateMachine.ChangeState(Triggers.fullChargeInAir);
                    _fullCharge.SetDirection(_shootDirection); // 攻撃方向を設定
                }
                else
                {
                    _stateMachine.ChangeState(Triggers.shootComplete);
                }
            }
        }
        #endregion

        private bool IsMatchState(StateKey state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }
        private bool IsMatchState(StateKey arg1, StateKey arg2)
        {
            return IsMatchState(arg1) || IsMatchState(arg2);
        }
        // === Unity LifeCycle ===
        protected override void BeforeAwake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            Timer.CountDownRegister(nameof(_coyoteTime), _coyoteTime);
            Timer.CountDownRegister(nameof(_shootBlockTime), _shootBlockTime, () => { Debug.Log("シュート可能"); }, initializeTimer: false);
            //Timer.CountDownRegister(nameof(_shootBlockTime), _shootBlockTime);
            Timer.CountDownRegister(nameof(_invincibleTime), _invincibleTime, () => IsInvincible = false);
        }
        protected override void Start()
        {
            base.Start();
            this.UpdateAsObservable()
                .Where(_ => _canChargeCount)
                .Subscribe(_ => _shootPressTime += Time.deltaTime)
                .AddTo(this);
            this.UpdateAsObservable()
                .Where(_ => _isPlaying)
                .Subscribe(_ => Timer.Update(Time.deltaTime))
                .AddTo(this);
            this.UpdateAsObservable()
    .Where(_ => _stateMachine != null)
    .Subscribe(_ =>
    {
        Debug.Log($"[ActionRobot] Current State: {_stateMachine.CurrentState.key}");
    })
    .AddTo(this);
            this.UpdateAsObservable()
                .Where(_ => _isPlaying && MoveDirection.x != 0) // 移動入力があるときのみ
                .Subscribe(_ =>
                {
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * Mathf.Sign(MoveDirection.x);
                    transform.localScale = scale;
                })
                .AddTo(this);
        }
    }
}