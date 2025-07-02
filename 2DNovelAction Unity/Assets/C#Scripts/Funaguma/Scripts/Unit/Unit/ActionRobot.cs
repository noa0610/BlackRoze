using HighElixir.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose
{
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput)), Serializable]
    public class ActionRobot : GroundedUnit
    {
        [Flags]
        private enum StateKey
        {
            none = 0, // 状態なし
            idle = 1 << 0,
            shoot = 1 << 1,
            chargeShoot = 1 << 2,
            fullChargeShoot = 1 << 3,
            move = 1 << 4,
            dash = 1 << 5,
            stun = 1 << 6,
            jump = 1 << 7,
            onGround = 1 << 8, // 地面にいる状態   
        }
        [Header("Reference")]
        [SerializeField] private List<BulletData> _bullets = new List<BulletData>();
        [SerializeField] private LayerMask _targetLayer;
        private Stun _stunState;
        private Rigidbody2D _rigidbody;

        [Header("Option Settings")]
        [SerializeField] private float coyoteTime = 0.2f;         // 地面離れてからジャンプ猶予(sec)
        [SerializeField] private float[] _chargeShoot = new float[2] { 1.8f, 3.4f }; // チャージ攻撃用の時間配列
        [SerializeField] private bool _canChargeCount = false;
        [SerializeField] private Vector2 _stunKnockback = Vector2.zero;
        private float coyoteTimeCounter;
        private float _shootPressTime = 0f; // 攻撃ボタンを押した時間

        // StateMachine
        private Dictionary<string, Dictionary<StateKey, StateKey>> _stateMap = new();
        private StateKey _stateKey = StateKey.idle; // 状態フラグ


#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _forceEnableJump;
#endif
        // 地面にいるかどうか（OverlapCircle判定＆コヨーテタイム管理）
        public bool canJump => _forceEnableJump || coyoteTimeCounter > 0f;

        public List<BulletData> Bullets
        {
            get => _bullets;
            set => _bullets = value;
        }

        protected override void RegisterStats()
        {
            // 現在のステート、トリガー、遷移先のステート
            _stateMachine.SetCondition(StateDecision);
            _stateMap.Add(StateKey.idle.ToString(), new Dictionary<StateKey, StateKey>()
            {
                { StateKey.idle, StateKey.idle },
                { StateKey.shoot, StateKey.shoot },
                { StateKey.chargeShoot, StateKey.chargeShoot },
                { StateKey.fullChargeShoot, StateKey.fullChargeShoot },
                { StateKey.move, StateKey.move },
                { StateKey.dash, StateKey.dash },
                { StateKey.jump, StateKey.jump },
                { StateKey.stun, StateKey.stun },
            });

            var normal = new ShootForward(_bullets[0], _targetLayer, "Attack");
            normal.onShootComplete += OnshootComplete;
            _stateMachine.AddState(StateKey.shoot.ToString(), normal);
            _stateMap.Add(StateKey.shoot.ToString(), new Dictionary<StateKey, StateKey>()
            {
                { StateKey.idle, StateKey.idle },
                { StateKey.chargeShoot, StateKey.chargeShoot },
                { StateKey.fullChargeShoot, StateKey.fullChargeShoot },
                { StateKey.stun, StateKey.stun },
            });

            var charge = new ShootForward(_bullets[1], _targetLayer, "Attack");
            charge.onShootComplete += OnshootComplete;
            _stateMachine.AddState(StateKey.chargeShoot.ToString(), charge);
            _stateMap.Add(StateKey.chargeShoot.ToString(), new Dictionary<StateKey, StateKey>()
            {
                { StateKey.idle, StateKey.idle },
                { StateKey.stun, StateKey.stun },
            });
            var full = new ShootForward(_bullets[2], _targetLayer, "Attack");
            full.onShootComplete += OnshootComplete;
            _stateMachine.AddState(StateKey.fullChargeShoot.ToString(), full);
            _stateMap.Add(StateKey.fullChargeShoot.ToString(), new Dictionary<StateKey, StateKey>()
            {
                { StateKey.idle, StateKey.idle },
                { StateKey.stun, StateKey.stun },
            });

            _stateMachine.AddState(StateKey.move.ToString(), new MoveOnGround(_rigidbody, "Move", statusManager.GetStatusAmount(Status.Speed)));
            _stateMap.Add(StateKey.move.ToString(), new Dictionary<StateKey, StateKey>()
            {
                { StateKey.move, StateKey.move },
                { StateKey.idle, StateKey.idle },
                { StateKey.shoot, StateKey.shoot },
                { StateKey.chargeShoot, StateKey.chargeShoot },
                { StateKey.fullChargeShoot, StateKey.fullChargeShoot },
                { StateKey.dash, StateKey.dash },
                { StateKey.jump, StateKey.jump },
                { StateKey.stun, StateKey.stun },
            });
            _stateMachine.AddState(StateKey.dash.ToString(), new DashOnGround(_rigidbody, this, statusManager.GetStatusAmount(Status.DashSpeed)));
            _stateMap.Add(StateKey.dash.ToString(), new Dictionary<StateKey, StateKey>()
            {
                { StateKey.idle, StateKey.idle },
                { StateKey.move, StateKey.move },
                { StateKey.shoot, StateKey.shoot },
                { StateKey.chargeShoot, StateKey.chargeShoot },
                { StateKey.fullChargeShoot, StateKey.fullChargeShoot },
                { StateKey.jump, StateKey.jump },
                { StateKey.stun, StateKey.stun },
            });
            _stateMachine.AddState(StateKey.jump.ToString(), new Jump(_rigidbody, statusManager.GetStatusAmount(Status.SpeedInAir)));
            _stateMap.Add(StateKey.jump.ToString(), new Dictionary<StateKey, StateKey>()
            {
                { StateKey.idle, StateKey.idle },
                { StateKey.move, StateKey.move },
                { StateKey.shoot, StateKey.shoot },
                { StateKey.chargeShoot, StateKey.chargeShoot },
                { StateKey.fullChargeShoot, StateKey.fullChargeShoot },
                { StateKey.dash, StateKey.dash },
                { StateKey.stun, StateKey.stun },
            });

            _stunState = new Stun(_rigidbody, GetComponent<PopText>(), 1.5f, _stunKnockback);
            _stateMachine.AddState(StateKey.stun.ToString(), _stunState);
            _stateMap.Add(StateKey.stun.ToString(), new Dictionary<StateKey, StateKey>()
            {
                { StateKey.idle, StateKey.idle },
            });
        }
        protected override string StateDecision()
        {
            var c = _stateMachine.CurrentState;
            var key = Enum.Parse<StateKey>(c.Key);
            if (_stunState.StunTimer > 0f)
                return StateKey.stun.ToString(); // スタン状態が続いている場合はスタン状態を返す
            if (key == StateKey.none)
                return key.ToString();
            if (_stateMap[c.Key].TryGetValue(_stateKey, out var res))
            {
                return res.ToString();
            }
            return key.ToString();
            //if (_stateFlags.HasFlag(StateFlags.InStun))
            //{
            //    return StateKey.stun.ToString();
            //}
            //if (_stateFlags.HasFlag(StateFlags.InShoot))
            //{
            //    //_stateFlags &= ~StateFlags.InShoot;
            //    if (_fullCharge)
            //    {
            //        _fullCharge = false;
            //        _charge = false;
            //        return StateKey.fullChargeShoot.ToString();
            //    }
            //    if (_charge)
            //    {
            //        _charge = false;
            //        return StateKey.chargeShoot.ToString();
            //    }
            //    return StateKey.shoot.ToString();
            //}
            //if (_stateFlags.HasFlag(StateFlags.InJump))
            //{
            //    _stateFlags &= ~StateFlags.InJump;
            //    return StateKey.jump.ToString();
            //}
            //if (_stateFlags.HasFlag(StateFlags.InDash) && IsGrounded)
            //    return StateKey.dash.ToString();
            //if (_stateFlags.HasFlag(StateFlags.InMove))
            //    return StateKey.move.ToString();
            //return StateKey.idle.ToString();
        }
        protected override void OnGrounded()
        {
            coyoteTimeCounter = coyoteTime;

            if (_stateMachine.StateMap["jump"] is Jump jump)
                jump.HadLeapt = false;
            if (_stateMachine.CurrentState.Key == StateKey.jump.ToString())
                _stateKey = StateKey.idle;
        }
        protected override void OnUnGrounded()
        {
            coyoteTimeCounter -= Mathf.Max(0, Time.fixedDeltaTime);
        }


        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            _stateMachine.ChangeRequest(StateKey.stun.ToString());
        }

        // === Private ===

        private void OnshootComplete()
        {
            Debug.Log("Shoot completed.");
            _stateKey = StateKey.idle; // 攻撃が終わったらアイドル状態に戻す
        }

        // === InputAction ===
        #region
        private void OnJump(InputValue value)
        {
            if (canJump)
            {
                _stateKey = StateKey.jump; // ジャンプ状態にする
                coyoteTimeCounter = 0f;  // ジャンプしたら猶予リセット
            }
            if (!value.isPressed && _stateMachine.StateMap[StateKey.jump.ToString()] is Jump jump)
                jump.CutJump();
        }
        private void OnDash(InputValue value)
        {
            if (!value.isPressed)
            {
                _stateKey = StateKey.idle; // ダッシュをキャンセルしてアイドル状態に戻す
                return;
            }
            if (!IsGrounded) return; // 地面にいない場合は無視
            _stateKey = StateKey.dash; // ダッシュ状態にする

        }
        private void OnMove(InputValue value)
        {
            var d = value.Get<Vector2>();
            if (d == Vector2.zero)
            {
                _stateKey = StateKey.idle; // 移動をキャンセルしてアイドル状態に戻す
                Debug.Log("Canceled Move.");
                return;
            }
            Direction = d.normalized;
            _stateKey = StateKey.move; // 移動状態にする
        }
        private void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("Shoot");
                // 入力時に一度通常攻撃を行い、その後チャージを行う
                _stateKey = StateKey.shoot; // 通常攻撃状態にする
                _canChargeCount = true; // 攻撃ボタンを押したのでチャージ可能状態にする
                _bullets[0].originalstatus.direction = Direction; // 攻撃方向を設定
            }
            else
            {
                _canChargeCount = false; // 攻撃ボタンを離したのでチャージ不可状態にする
                if (_chargeShoot[0] <= _shootPressTime && _shootPressTime < _chargeShoot[1])
                {
                    Debug.Log("チャージ１");
                    // チャージ攻撃の状態にする
                    _stateKey = StateKey.chargeShoot; // チャージ攻撃状態にする
                    _bullets[1].originalstatus.direction = Direction; // 攻撃方向を設定
                }
                else if (_shootPressTime >= _chargeShoot[1])
                {
                    Debug.Log("フルチャージ");
                    _stateKey = StateKey.fullChargeShoot; // フルチャージ攻撃状態にする
                    _bullets[2].originalstatus.direction = Direction; // 攻撃方向を設定
                }
                _shootPressTime = 0f; // 攻撃ボタンを離したので時間をリセット
            }
        }
        #endregion
        // === Unity LifeCycle ===
        protected override void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            base.Awake();
        }
        protected virtual void Start()
        {
            this.UpdateAsObservable()
                .Where(_ => Enum.Parse<StateKey>(_stateMachine.CurrentState.Key) == StateKey.stun && _stunState.StunTimer <= 0f)
                .Subscribe(_ => _stateKey = StateKey.idle)
                .AddTo(this);
            this.UpdateAsObservable()
                .Where(_ => _canChargeCount)
                .Subscribe(_ => _shootPressTime += Time.deltaTime)
                .AddTo(this);
        }
    }
}
//unicode