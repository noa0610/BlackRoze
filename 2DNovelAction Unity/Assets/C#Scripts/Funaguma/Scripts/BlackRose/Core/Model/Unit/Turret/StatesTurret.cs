using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_Turret
    {
        [SerializeField] private ShootForward _shoot;
        private Idle_Looking _looking;
        private static Dictionary<States, string> _states = EnumWrapper.GetDict<States>();
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
            IntervalIsFinished, // インターバル完了
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
                (Triggers.FindPlayer, States.inVigilance, "Contact"),
                (Triggers.Died, States.dead, "")
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
            var intervalTrigger = new[]
            {
                (Triggers.IntervalIsFinished, States.shoot),
                (Triggers.Died, States.dead)
            };
            _stateMachine
             .AddTransitions(States.idle, idleTrigger)
             .AddTransitions(States.shoot, shootTrigger)
             .AddTransitions(States.inVigilance, vigilanceTrigger)
             .AddTransitions(States.shootInterval, intervalTrigger);

            // 待機
            //_stateMachine.AddState(States.idle, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));
            _stateMachine.AddState(States.idle, new Idle());

            // 発射
            _shoot.onShootComplete.AsObservable().Subscribe(_ => OnShootComplete()).AddTo(this);
            _shoot.SetBullet(_bulletData);
            _stateMachine.AddState(States.shoot, _shoot);

            // インターバル
            var interval = new Idle_LazyChange(Triggers.ShootReady.ToString(), _trishootInterval);
            _stateMachine .AddState(States.shootInterval, interval);
            // 警戒
            //_looking = new Idle_Looking(_turret).SetAnimeTrigger("idle").SetCancelableProgress(0);
            _looking = new Idle_Looking(_turret);
            _stateMachine.AddState(States.inVigilance, _looking);

            // 死亡
            _stateMachine.AddState(States.dead, new Idle());
        }
    }
}