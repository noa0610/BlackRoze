using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    public class AIController : GroundedUnit
    {
        private enum Mode
        {
            Normal,
            Right,
            Heavy
        }
        // 現在のステート、トリガー、次のステート
        // string => State, string => Trigger, string => State
        private Dictionary<string, Dictionary<string, string>> _translation = new();
        private string _trigger;

        // モードごとの登録処理
        [SerializeField] private UnitStatusData _normalStatus;
        [SerializeField] private UnitStatusData _rightStatus;
        [SerializeField] private UnitStatusData _heavyStatus;
        private IAIState _normalMode;
        private IAIState _rightMode;
        private IAIState _heavyMode;
        private IAIState _currentMode;
        public Dictionary<string, Dictionary<string, string>> Translation => _translation;
        public string Trigger => _trigger;
        protected override void OnGrounded()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnUnGrounded()
        {
            throw new System.NotImplementedException();
        }

        protected override void RegisterStats()
        {
            _normalMode.Register();
            _rightMode.Register();
            _heavyMode.Register();
        }

        //protected override string StateDecision()
        //{
        //    if (Translation.TryGetValue(_stateMachine.CurrentState.Key, out var to))
        //    {
        //        if (to.TryGetValue(_trigger, out var res)) return res;
        //    }
        //    return _stateMachine.DefaultStateKey;
        //}

        protected override void Awake()
        {
            base.Awake();
            InitAIState();
        }
        // === Private ===
        private void InitAIState()
        {
            _normalMode = new NormalMode(this);
        }
        private void ChangeMode(Mode mode)
        {
            var status = mode switch
            {
                Mode.Normal => _normalStatus,
                Mode.Right => _rightStatus,
                Mode.Heavy => _heavyStatus,
                _ => _normalStatus
            };
            statusManager.UpdateBaseStatus(Status.HP, status.maxHp);
            statusManager.UpdateBaseStatus(Status.Speed, status.speed);
            statusManager.UpdateBaseStatus(Status.SpeedInAir, status.speedInAir);
            statusManager.UpdateBaseStatus(Status.JumpPower, status.jumpPower);
            statusManager.UpdateBaseStatus(Status.DashSpeed, status.dashSpeed);
            statusManager.UpdateBaseStatus(Status.Power, status.power);
            statusManager.UpdateStatus(Status.DamageRatio, status.damageTakeScale);
            _currentMode = mode switch
            {
                Mode.Normal => _normalMode,
                Mode.Right => _rightMode,
                Mode.Heavy => _heavyMode,
                _ => _normalMode
            };
        }
    }
}