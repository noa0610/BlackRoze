using Unity.VisualScripting;
using UnityEngine;

namespace BlackRose
{
    public class Enemy_huyuu : UnitBase
    {
        public enum States
        {
            none,
            move, // 移動
            idle,// 待機
            dead,// 死亡
            explosion,// 爆発
        }
        private enum Triggers
        {
            None,
            MissingPlayer, // プレイヤーを見失った
            FoundPlayer,   // プレイヤーを発見した
            AttackRange,   // 攻撃範囲に入った
            Explosion,     // 爆発した
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]
            {
        (Triggers.FoundPlayer, States.move),
        (Triggers.Died, States.dead)
    };
            var moveTrigger = new[]
            {
        (Triggers.MissingPlayer, States.idle),
        (Triggers.AttackRange, States.explosion),
        (Triggers.Died, States.dead)

    };
            var explosionTrigger = new[]
                    {
        (Triggers.Explosion, States.dead),
        (Triggers.Died, States.dead)
    };
            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.move, moveTrigger)
                .AddTransmissions(States.explosion, explosionTrigger);
            // 死んだときに何もしないならDeadの設定はいらない

            // // 待機
            // _stateMachine.AddState(States.idle, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));

            // // 爆発
            // // var  = new ShootForward(_bulletData, targetLayer);
            // // shoot.onShootComplete.AsObservable().Subscribe( => OnShootComplete());
            // // shoot.SetBullet(_bulletData);
            // // _stateMachine.AddState(States.shoot, shoot);

            // // 発射クールタイム
            // _stateMachine.AddState(States.shootInterval, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));

            // // 死亡
            // _stateMachine.AddState(States.dead, new Idle());
        }
    }

}

