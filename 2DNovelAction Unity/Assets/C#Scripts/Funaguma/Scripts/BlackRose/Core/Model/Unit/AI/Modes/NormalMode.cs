using BlackRose.Core.Models.Units.State;
using Cysharp.Threading.Tasks;
using HighElixir.StateMachine.Extention;
using HighElixir.Timers;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using AIStates = BlackRose.Core.Models.Units.AIController.AIStates;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public sealed class NormalMode : AIModeBase
    {
        [Header("Warp")]
        [SerializeField] private float _warpRange;
        [SerializeField] private GameObject _preWarpDemo;

        // プレイヤーの中央になるように調整
        [SerializeField] private Vector3 _warpDemoDelta = new(0, 1, 0);
        [SerializeField] private float _skillCT = 4f;
        [SerializeField] private float _intervalDelay;
        private TimerTicket _skillTicket;

        [Header("States")]
        [SerializeField] private Warp _warpState = new Warp();
        [SerializeField] private ShootForward _shoot = new ShootForward();
        [SerializeField] private ShootForward _half = new ShootForward();
        [SerializeField] private ShootForward _full = new ShootForward();

        [Header("状態管理")]
        [SerializeField] private int _shootCount = 0;
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private float _ct;
#endif
        public Vector2 WarpPreDir { get; private set; }

        public override AIStates Attach => AIStates.Normal;

        protected override void RegisterStates()
        {
            _skillTicket = Timer.CountDownRegister(_skillCT, "ワープCT", initZero: true);

            _stateMachine.RegisterState(SubState.Shoot, _shoot, "Shoot");
            _stateMachine.RegisterState(SubState.Half, _half, "Shoot");
            _stateMachine.RegisterState(SubState.Full, _full, "Shoot");

            // event
            _stateMachine.OnCompletion.SkipWhile(_ => !_stateMachine.Awaked).Where(x => x.ID == SubState.Skill).Subscribe(_ =>
            {
                Debug.Log("==================");
                if (_stateMachine.Send(Triggers.skillFinished))
                    Debug.Log("Warp Completed and State Changed");
                else
                    Debug.Log("Warp Completed but State Change Blocked");
            });

            _stateMachine.OnCompletion.SkipWhile(_ => !_stateMachine.Awaked).Where(x => x.State.HasTag("Shoot")).Subscribe(_ =>
            {
                Debug.Log("===========");
                _stateMachine.Send(Triggers.shootCompleted);
                var token = new CancellationToken();
                UniTask.Create(async () =>
                {
                    await _stateMachine.DelaySendEvtAsync(TimeSpan.FromSeconds(_intervalDelay), Triggers.watingTimeHasElapsed, token);
                    if (token.IsCancellationRequested)
                        _shootCount = 0;
                }).Forget();
            });
        }

        public override void OnSkill(InputValue value)
        {
            if (!Timer.IsFinished(_skillTicket)) return;

            if (value.isPressed)
            {
                _preWarpDemo.SetActive(true);
            }
            else
            {
                // TODO : ワープ実行
                _preWarpDemo.SetActive(false);
                _stateMachine.Send(Triggers.skillInput);
                Timer.Start(_skillTicket);
            }
        }
        public override void OnInputMove(Vector2 dir)
        {
            if (dir != Vector2.zero)
                WarpPreDir = dir;
            else
                WarpPreDir = _parent.ShootDir;
        }

        public override void FixedUpdate(float deltaTime)
        {
            _preWarpDemo.transform.position = _parent.transform.position + _warpDemoDelta + (Vector3)WarpPreDir * _warpRange;
            _warpState.SetPos(_preWarpDemo.transform.position);
        }

        public override void InvokeShoot()
        {
            if (_shootCount > 4)
            {
                Debug.Log("うああああああ");
                return;
            }
            _stateMachine.Send(Triggers.shootInput);
        }

        public override void InvokeHalfShoot()
        {
            _stateMachine.Send(Triggers.halfCharge);
        }

        public override void InvokeFullShoot()
        {
            _stateMachine.Send(Triggers.fullCharge);
        }

#if UNITY_EDITOR
        public override void Update(float deltaTime)
        {
            if (Timer.TryGetCurrentTime(_skillTicket, out var rm)) _ct = rm;

        }
#endif
        public override void ModeChange_C()
        {
            _parent.SwitchModeLight();
        }

        public override void ModeChange_V()
        {
            _parent.SwitchModeHeavy();
        }
    }
}