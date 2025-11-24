using BlackRose.Core.Models.Units.State;
using Cysharp.Threading.Tasks;
using HighElixir.StateMachine;
using HighElixir.StateMachine.Extention;
using HighElixir.Unity.Loggings;
using System;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    // ステート登録を担当するpartial
    public partial class ActionRobot
    {
        public enum StateKey
        {
            none,

            // 移動系
            idle,
            landing,
            move,
            dash,
            fall,
            jump,

            // 射撃関連
            shoot,
            halfChargeShoot,
            fullChargeShoot,
            shootInterval,

            // 被弾、死亡
            stun,
            dead,
        }
        public enum Triggers
        {
            none,

            // 移動関連
            moveInput,
            dashInput,
            cancelMove,
            cancelDash,
            falling,
            jumpInput,
            landing,
            landed,

            // 攻撃関連
            shootInput,
            halfChargeShoot,
            fullChargeShoot,
            shootComplete,
            shootInAir,
            halfChargeInAir,
            fullChargeInAir,
            waitingTimeHasElapsed,

            // 被弾関連
            death,
            stuned,
            finishedStun,
        }

        private StateMachine<UnitBase, Triggers, StateKey> _fms;

        #region ステート
        [Header("States")]
        //
        [SerializeField] private Jump<UnitBase> _jump;
        [SerializeField] private MoveOnGround<UnitBase> _moveOnGround;
        [SerializeField] private MoveOnAir<UnitBase> _air;
        [SerializeField] private DashOnGround<UnitBase> _dash;

        //
        [SerializeField] private ShootForward<UnitBase> _normal;
        [SerializeField] private ShootForward<UnitBase> _halfCharge;
        [SerializeField] private ShootForward<UnitBase> _fullCharge;

        //
        [SerializeField] private Stun<UnitBase> _stun;

        [SerializeField] private ObservableStateMachineTrigger _landing;
        #endregion

        public StateMachine<UnitBase, Triggers, StateKey> FMS => _fms;

        #region 非同期、時間管理系
        private CancellationTokenSource _cancellableActionToken;

        #endregion

#if UNITY_EDITOR
        [SerializeField] private string _currentState_fms = "idle";
#endif
        protected override void RegisterStats()
        {
            #region FMS初期化
            StateMachineOption<UnitBase, Triggers, StateKey> option = new(this);
            option.Logger = new UnityLogger();
#if UNITY_EDITOR
            option.LogLevel = RequiredLoggerLevel.ERROR;
            _fms = new(option);
            _fms.OnTransition.Subscribe(info => _currentState_fms = info.ToState.ToString()).AddTo(this);
            //var last = 0;
            //_fms.OnTransition.Subscribe(info =>
            //{
            //    var t = Time.frameCount;
            //    //Debug.Log($"[{last}->{t}]{info.ToString()}");
            //    last = t;
            //}).AddTo(this);
#else
            option.LogLevel = RequiredLoggerLevel.Fatal;
            _fms = new(option);
#endif
            #endregion
            //var ac = _playerInput.actions.FindActionMap("Player").FindAction("Move");
            //_fms.OnTransition.Where(_ => _playerInput != null).Subscribe(_ =>
            //{
            //    OnMove(ac.ReadValue<Vector2>());
            //}).AddTo(this);
            #region ステートマシン、一括追加
            _fms.RegisterProcessor = new StateProcessor<UnitBase, Triggers, StateKey>((id, info) =>
            {
                if (id == StateKey.dead) return;
                if (id == StateKey.stun) return;

                if (info.HasTagOnChild("Tokened"))
                {
                    info.OnExit.Subscribe(_ => _cancellableActionToken?.Cancel());
                }
                if (info.HasTagOnChild("Shoot"))
                    info.RegisterTransition(Triggers.shootComplete, StateKey.shootInterval, "foo");

                if (info.HasTagOnChild("Cancelable"))
                {
                    if (info.HasTagOnChild("InAir"))
                    {
                        info.RegisterTransition(Triggers.shootInAir, StateKey.shoot, "toShot");
                        info.RegisterTransition(Triggers.halfChargeInAir, StateKey.halfChargeShoot, "toShot");
                        info.RegisterTransition(Triggers.fullChargeInAir, StateKey.fullChargeShoot, "toShot");
                    }
                    else
                    {
                        if (id != StateKey.dash)
                            info.RegisterTransition(Triggers.moveInput, StateKey.move, "toWalk");
                        info.RegisterTransition(Triggers.dashInput, StateKey.dash, "toDash");
                        info.RegisterTransition(Triggers.jumpInput, StateKey.jump, "toJump");
                    }
                    info.RegisterTransition(Triggers.falling, StateKey.fall, "toFall");
                    info.RegisterTransition(Triggers.shootInput, StateKey.shoot, "toShot");
                    info.RegisterTransition(Triggers.halfChargeShoot, StateKey.halfChargeShoot, "toShot");
                    info.RegisterTransition(Triggers.fullChargeShoot, StateKey.fullChargeShoot, "toShot");
                }
                info.RegisterTransition(Triggers.stuned, StateKey.stun, "toStun");
                info.RegisterTransition(Triggers.landing, StateKey.landing, "foo");
            });
            #endregion


            #region === 各ステートのトリガー一覧定義 ===

            // ShootWait : Cancelable
            _fms.RegisterTransition(StateKey.shootInterval, Triggers.waitingTimeHasElapsed, StateKey.idle, "toIdle");

            // Move : Cancelable
            _fms.RegisterTransition(StateKey.move, Triggers.cancelMove, StateKey.idle, "toIdle");
            _fms.RegisterTransitions(StateKey.dash,
                (Triggers.cancelDash, StateKey.move, "toWalk"),
                (Triggers.cancelMove, StateKey.idle, "toIdle"));


            _fms.RegisterTransition(StateKey.stun, Triggers.finishedStun, StateKey.idle, "toIdle");

            _fms.RegisterAnyTransition(Triggers.landing, StateKey.landing, "toLand");
            _fms.RegisterTransition(StateKey.landing, Triggers.landed, StateKey.idle, "toIdle");

            _fms.RegisterAnyTransition(Triggers.death, StateKey.dead, "toDead");
            #endregion

            #region === ステートコンポーネント登録 ===
            // idle
            _fms.RegisterState(StateKey.idle, new Idle<UnitBase>(), "Cancelable");

            _fms.RegisterState(StateKey.landing, new Idle<UnitBase>(), "Cancelable", "Tokened")
            .OnEnter.Subscribe(_ =>
            {
                _fms.LazySend(Triggers.landed);
            });

            _fms.RegisterState(StateKey.shootInterval, new Idle<UnitBase>(), "Cancelable", "Tokened")
            .OnEnter.Subscribe(info =>
            {
                var token = Take();
                UniTask.Create(async () =>
                {
                    if (await _fms.SendEventWithDelayAsync(TimeSpan.FromSeconds(_intervalTime), Triggers.waitingTimeHasElapsed, token))
                    {
                        //Debug.Log("AAAAAA");
                        _successionCount = 0;
                    }
                });

            });

            // shoot
            _fms.RegisterState(StateKey.shoot, _normal, "Shoot")
            .OnEnter.Subscribe(_ => PlaySE("ビームライフル", 0.7f)).AddTo(this);
            _fms.RegisterState(StateKey.halfChargeShoot, _halfCharge, "Shoot")
            .OnEnter.Subscribe(_ => PlaySE("ビーム砲", 0.7f)).AddTo(this);
            _fms.RegisterState(StateKey.fullChargeShoot, _fullCharge, "Shoot")
            .OnEnter.Subscribe(_ => PlaySE("ビーム砲", 0.7f)).AddTo(this);
            _fms.OnCompletion.Where(x => x.HasTagOnChild("Shoot")).Subscribe(_ =>
            {
                _fms.LazySend(Triggers.shootComplete);
            }).AddTo(this);

            // move
            _fms.RegisterState(StateKey.move, _moveOnGround, "Cancelable");
            _fms.RegisterState(StateKey.fall, _air, "Cancelable", "InAir");
            _fms.RegisterState(StateKey.dash, _dash, "Cancelable")
            .OnEnter.Subscribe(_ =>
            {
                _trailRenderer.emitting = true;
                PlaySE("バックブースター", 0.4f);
            }).AddTo(this);
            _fms.OnTransition.Where(res => res.FromState == StateKey.dash && res.ToState != StateKey.jump).Subscribe(res =>
            {
                _trailRenderer.emitting = false;
            }).AddTo(this);

            // jump
            _fms.RegisterState(StateKey.jump, _jump, "InAir", "Cancelable");

            // stun
            _fms.RegisterState(StateKey.stun, _stun, "Tokened")
            .OnEnter.Subscribe(res =>
            {
                _fms.SendEventWithDelayAsync(TimeSpan.FromSeconds(1.2f), Triggers.finishedStun, Take()).AsUniTask().Forget();
            });
            // dead
            _fms.RegisterState(StateKey.dead, new Idle<UnitBase>());

            #endregion

            // 起動
            _fms.Awake(StateKey.idle);


            // UnitBase側のエラーを防ぐための仮登録
            _stateMachine.AddState(StateKey.idle, new States.Idle());
        }

        protected override void AfterUpdate()
        {
            _fms.Update(Time.deltaTime);
        }

        private CancellationToken Take()
        {
            _cancellableActionToken?.Cancel();
            _cancellableActionToken?.Dispose();
            _cancellableActionToken = new();
            return _cancellableActionToken.Token;
        }
    }
}