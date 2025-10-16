using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    // ステート、モード管理
    public partial class AIController
    {
        public enum Mode { Normal, Light, Heavy }

        // 各モードに共通するステート
        public enum AIStates { Idle, Move, Jump, Fall, Dash, SpecialAttack, Dead, ShootInterval }
        public enum AITriggers
        {
            moveInput, cancelMove, dashInput, shootInput, halfCharge, fullCharge, jumpInput,
            shootCompleted, watingTimeHasElapsed, skillInput, skillFinished,
            landing, falling, stun, recoverFromStun,
            modeChanged
        }
        public enum Tags
        {
            Shoot, Stunned
        }
        public readonly static Dictionary<Tags, string> tags = EnumWrapper.GetValueNameMap<Tags>();
        private Mode _currentEnumMode = Mode.Normal;
        private AISpriteResolver _spriteResolver;

        // ===== State Machine =====
        public AIModeBase CurrentMode => _currentEnumMode switch
        {
            Mode.Normal => _normalMode,
            Mode.Heavy => _heavyMode,
            Mode.Light => _lightMode,
            _ => _normalMode
        };

        // 外部からのモード切替 API
        public void SwitchModeLight()
        {
            _spriteResolver.Change_L();
            ChangeMode(Mode.Light);
        }
        public void SwitchModeHeavy()
        {
            _spriteResolver.Change_H();
            ChangeMode(Mode.Heavy);
        }
        public void SwitchModeNormal()
        {
            _spriteResolver.Change_N();
            ChangeMode(Mode.Normal);
        }

        // === Private ===

        private void ChangeMode(Mode mode)
        {
            // ★ ステータス反映
            var status = CurrentMode.StatusData;
            statusManager.GetStatus(Status.MaxHP).SetDefault(status.maxHp);
            statusManager.GetStatus(Status.Speed).SetDefault(status.speed);
            statusManager.GetStatus(Status.SpeedInAir).SetDefault(status.speedInAir);
            statusManager.GetStatus(Status.JumpPower).SetDefault(status.jumpPower);
            statusManager.GetStatus(Status.DashSpeed).SetDefault(status.dashSpeed);
            statusManager.GetStatus(Status.Power).SetDefault(status.power);
            statusManager.GetStatus(Status.DamageRatio).SetDefault(status.damageTakeScale);

            _currentEnumMode = mode;

            _stateMachine.SetLayer(mode.ToString());

            _stateMachine.ChangeState(AITriggers.modeChanged);

            Debug.Log("ModeChanged");
        }
        protected void ModeRegist()
        {
            _spriteResolver = GetComponent<AISpriteResolver>();
            _normalMode.Bind(this);
            _lightMode.Bind(this);
            _heavyMode.Bind(this);
        }

        protected override void AfterAwake()
        {
            base.AfterAwake();
            ChangeMode(Mode.Normal);
        }

        protected override void RegisterStats()
        {
            _normalMode.Register();
            _lightMode.Register();
            _heavyMode.Register();

            // モード非依存の共通フォールバック

            // Idle
            _stateMachine.AddTransitionsForLayer(
                Layer.COMMON,
                AIStates.Idle,
                (AITriggers.moveInput, AIStates.Move, ""),
                (AITriggers.dashInput, AIStates.Dash, ""),
                (AITriggers.falling, AIStates.Fall, "")
                );
            _stateMachine.AddTransitionsForLayer(
                Layer.COMMON,
                AIStates.ShootInterval,
                (AITriggers.watingTimeHasElapsed, AIStates.Idle, ""),
                (AITriggers.moveInput, AIStates.Move, ""),
                (AITriggers.dashInput, AIStates.Dash, ""),
                (AITriggers.falling, AIStates.Fall, "")
                );
            // Move
            _stateMachine.AddTransitionsForLayer(
                Layer.COMMON,
                AIStates.Move,
                (AITriggers.cancelMove, AIStates.Idle, ""),
                (AITriggers.dashInput, AIStates.Dash, ""),
                (AITriggers.jumpInput, AIStates.Jump, "")
                );
            // Fall
            _stateMachine.AddTransitionsForLayer(
                Layer.COMMON,
                AIStates.Fall,
                (AITriggers.landing, AIStates.Idle, "")
                );

            // 任意遷移
            _stateMachine.AddAnyTransition(AITriggers.landing, AIStates.Idle, Layer.COMMON);
            _stateMachine.AddAnyTransition(AITriggers.modeChanged, AIStates.Idle, Layer.COMMON);
            _stateMachine.AddAnyTransition(AITriggers.skillFinished, AIStates.Idle, Layer.COMMON);
            _stateMachine.AddAnyTransition(AITriggers.shootCompleted, AIStates.ShootInterval, Layer.COMMON);

            _stateMachine.AddState(AIStates.Idle, new Idle());
            _stateMachine.AddState(AIStates.Fall, new MoveOnAir());
            _stateMachine.AddState(AIStates.Move, new MoveOnGround());
            _stateMachine.AddState(AIStates.Dash, new DashOnGround(this));

            var idle = new Idle_LazyEvent();
            _stateMachine.AddState(AIStates.ShootInterval, idle);

            idle.SetTime(0.2f);
            idle.OnCompleted += () =>
            {
                _stateMachine.ChangeState(AITriggers.watingTimeHasElapsed);
            };
        }
    }
}