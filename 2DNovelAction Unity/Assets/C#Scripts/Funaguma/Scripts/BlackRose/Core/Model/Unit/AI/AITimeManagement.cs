using HighElixir.Timers;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public partial class AIController
    {
        // 時間管理
        [SerializeField] private float _shootBlockTime = 0.6f;
        [SerializeField] private float _coyoteTime = 0.2f;

        
        private TimerTicket _chargeTicket;// 射撃のチャージ
        private TimerTicket _coyoteTicket;
        private TimerTicket _shootTicket;
        private TimerTicket _shootIntervalDelay;

        public TimerTicket ChargeTime => _chargeTicket;
        protected void TimerRegist()
        {
            _coyoteTicket = Timer.CountDownRegister(_coyoteTime, "AI Coyote");
            _shootTicket = Timer.CountDownRegister(_shootBlockTime, "AI Shoot Block");
            _chargeTicket = Timer.CountUpRegister("AI ChargeTime");
            _shootIntervalDelay = Timer.CountDownRegister(0.2f, "ShootIntervalDelay");
            _blockFlip = Timer.CountDownRegister(3, "Flip", initZero:true);
            ModeRegist();
        }
        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            var dt = Time.deltaTime;
            Timer.Update(dt);
            CurrentMode.Update(dt);
        }
        protected override void AfterFixedUpdate()
        {
            base.AfterFixedUpdate();
            CurrentMode?.FixedUpdate(Time.fixedDeltaTime);
        }
    }
}