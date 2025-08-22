using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using UniRx;

namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_Turret
    {
        private Idle_Looking _looking;

        private enum States
        {
            none = 0,
            idle,           // 待機
            inVigilance,    // 接敵
            shoot,          // 発射
            shootInterval,  // 発射間の短い遅延
            dead,           // 死亡
        }
        private enum Triggers
        {
            None,
            MissingPlayer,  // プレイヤーを見失った
            FindPlayer,     // プレイヤーを発見した
            ShootReserve,   // 発射待機に入った
            ShootReady,     // 発射準備完了
            ShootComplete,  // 発射完了(短い遅延用)
            Died,           // 死亡した（HPが０になった）
        }
        // ステート登録
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            // States.idle
            var idleTrigger = new[]
            {
                (Triggers.FindPlayer, States.inVigilance),
                (Triggers.Died, States.dead)
            };

            // States.inVigilance
            var vigilanceTrigger = new[]
            {
                (Triggers.ShootReady, States.shoot),
                (Triggers.MissingPlayer, States.idle),
                (Triggers.Died, States.dead)
            };

            // States.shoot
            var shootTrigger = new[]
            {
                (Triggers.ShootReserve, States.inVigilance),
                (Triggers.ShootComplete, States.shootInterval),
                (Triggers.Died, States.dead)
            };

            // States.shootInterval
            var intervalTriger = new[]
            {
                (Triggers.ShootReady, States.shoot),
                (Triggers.Died, States.dead)
            };
            _stateMachine
             .AddTransmissions(States.idle, idleTrigger)
             .AddTransmissions(States.shoot, shootTrigger)
             .AddTransmissions(States.inVigilance, vigilanceTrigger)
             .AddTransmissions(States.shootInterval, intervalTriger);

            // 待機
            //_stateMachine.AddState(States.idle, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));
            _stateMachine.AddState(States.idle, new Idle().SetCancelableProgress(0));

            // 発射
            var shoot = new ShootForward(_bulletData, _targetLayer);
            shoot.onShootComplete.AsObservable().Subscribe(_ => OnShootComplete()).AddTo(this);
            shoot.SetBullet(_bulletData);
            _stateMachine.AddState(States.shoot, shoot);

            // インターバル
            var interval = new Idle_LazyChange(Triggers.ShootReady.ToString(), _trishootInterval).SetAnimeTrigger("idle");
            _stateMachine .AddState(States.shootInterval, interval);
            // 警戒
            //_looking = new Idle_Looking(_turret).SetAnimeTrigger("idle").SetCancelableProgress(0);
            _looking = new Idle_Looking(_turret).SetCancelableProgress(0);
            _stateMachine.AddState(States.inVigilance, _looking);

            // 死亡
            _stateMachine.AddState(States.dead, new Idle());
        }
    }
}