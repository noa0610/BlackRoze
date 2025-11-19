using BlackRose.Core.Models.Units.State;
using HighElixir.StateMachine.Extention;
using HighElixir.Timers;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using static BlackRose.Core.Models.Units.AIController;
using AIStates = BlackRose.Core.Models.Units.AIController.AIStates;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public sealed class NormalMode : AIModeBase
    {
        [Header("Warp")]
        [SerializeField] private float _warpRange;

        // プレイヤーの中央になるように調整
        [SerializeField] private Vector3 _warpDemoDelta = new(0, 1, 0);
        [SerializeField] private float _skillCT = 4f;
        [SerializeField] private float _intervalDelay;

        [Header("States")]
        [SerializeField] private Warp _warpState = new Warp();
        [SerializeField] private ShootForward<AIController> _shoot = new();
        [SerializeField] private ShootForward<AIController> _half = new();
        [SerializeField] private ShootForward<AIController> _full = new();

        [Header("状態管理")]
        [SerializeField] private int _shootCount = 0;
        private TimerTicket _skillTicket;
        private TimerTicket _intervalTicket;
        private TimerTicket _elapsedTicket;

        public Vector2 WarpPreDir { get; private set; }
        public GameObject WarpPre => _parent.PreWarp;
        public override AIStates Attach => AIStates.Normal;

        protected override void RegisterStates()
        {
            _intervalTicket = Timer.CountDownRegister(0.7f, "射撃CT", () =>
            {
                _shootCount = 0;
                Debug.Log("タイマー完了");
                _stateMachine.Send(Triggers.watingTimeHasElapsed);
            }, initZero: true);
            _skillTicket = Timer.CountDownRegister(_skillCT, "ワープCT", () =>
            {
                _shootCount = 0;
            }, initZero: true);
            _elapsedTicket = Timer.CountDownRegister(_intervalDelay, "射撃制限", initZero: true);

            _stateMachine.RegisterState(SubState.Shoot, _shoot, "Shoot");
            _stateMachine.RegisterState(SubState.Half, _half, "Shoot");
            _stateMachine.RegisterState(SubState.Full, _full, "Shoot");
            _stateMachine.RegisterState(SubState.Skill, _warpState, "Skill");

            // Skill
            _stateMachine.RegisterAnyTransition(AITriggers.skillFinished, SubState.Idle, "toIdle");
            // event
            //_stateMachine.OnTransition.SkipWhile(_ => !_stateMachine.Awaked).Where(x => x.ToState != SubState.ShootInterval).Subscribe(x => Timer.Stop(_intervalTicket));

            _stateMachine.OnCompletion.SkipWhile(_ => !_stateMachine.Awaked).Where(x => x.ID == SubState.Skill).Subscribe(_ =>
            {
                if (_stateMachine.Send(Triggers.skillFinished))
                    Debug.Log("Warp Completed and State Changed");
                else
                    Debug.Log("Warp Completed but State Change Blocked");
            });

            _stateMachine.OnCompletion.SkipWhile(_ => !_stateMachine.Awaked).Where(x => x.State.HasTag("Shoot")).Subscribe(_ =>
            {
                Timer.Restart(_intervalTicket);
            });
        }

        public override void OnSkill(InputValue value)
        {
            if (!Timer.IsFinished(_skillTicket)) return;

            if (value.isPressed)
            {
                WarpPre.SetActive(true);
            }
            else
            {
                // TODO : ワープ実行
                WarpPre.SetActive(false);
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
            WarpPre.transform.position = _parent.transform.position + _warpDemoDelta + (Vector3)WarpPreDir * _warpRange;
        }

        public override void InvokeShoot()
        {
            if (_shootCount > 4)
            {
                Debug.Log("うああああああ");
                if (Timer.IsRunning(_elapsedTicket))
                    Timer.Start(_elapsedTicket, init: true, isLazy: true);
                return;
            }
            _shootCount++;
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