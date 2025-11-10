using HighElixir.Timers;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    // TimeManagement
    public partial class ActionRobot 
    {
        [Header("時間設定")]
        [SerializeField] private float _coyoteTime = 0.2f;         // 地面離れてからジャンプ猶予(sec)
        [SerializeField] private float _shootBlockTime = 0.6f; // 連射をブロックする時間
        [SerializeField] private float _invincibleTime = 0.6f; // 無敵時間
        [SerializeField] private float _intervalTime = 0.4f; // shootIntervel => idleへの時間

        [Header("回数設定")]
        [SerializeField] private int _maxSuccession = 3; // 最大連射回数
        [SerializeField] private float[] _chargeShoot = new float[2] { 1.8f, 3.4f }; // チャージ攻撃用の時間配列
        private int _successionCount = 0; // 連射回数


        #region Ticket
        private TimerTicket _coyoteTicket;
        private TimerTicket _shootBlockTicket;
        private TimerTicket _invincibleTicket;
        private TimerTicket _chargeTicket;

        #endregion

        protected override void BeforeAwake()
        {
            _coyoteTicket = Timer.CountDownRegister(_coyoteTime, nameof(_coyoteTime));
            _shootBlockTicket = Timer.CountDownRegister(_shootBlockTime, nameof(_shootBlockTime), () => 
            { 
                Debug.Log("シュート可能");
                _successionCount = 0;
            });
            _invincibleTicket = Timer.CountDownRegister(_invincibleTime, nameof(_invincibleTime), () => IsInvincible = false);
            _chargeTicket = Timer.CountUpRegister("チャージ");
        }
    }
}