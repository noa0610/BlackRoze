using BlackRose.Core.Models.States;
using BlackRose.Core.Models.Helper;
using HighElixir;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_rasubosu1
    {
        private enum States
        {
            none,
            idle,
            dead,
            attackidle,
            armpunch,
            diffusebeamgun,
            firewall,
            firewallmove,
        }
        public enum Triggers
        {
            None,
            FoundPlayer,
            Attackcooldown,
            firewallmoveend,
            Attack1,
            Attack2,
            Attack3,
            Attack4,
            Attack1end,
            Attack2end,
            Attack3end,
            shockwaveend,
            Event1,
            Playerdead,
            Died,
        }

        protected override void RegisterStats()
        {

            // トランスミッション
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.attackidle, "Contact"),
                (Triggers.Died, States.dead, "")
            };
            var attackidleTrigger = new[]
            {
                (Triggers.Playerdead, States.idle),
                (Triggers.Died, States.dead),
                (Triggers.Attack1, States.armpunch),
                (Triggers.Attack2, States.diffusebeamgun),
                (Triggers.Attack3, States.firewallmove),
            };
            var armpunchTrigger = new[]
            {
                (Triggers.Attack1end, States.attackidle),
                (Triggers.Died, States.dead),
            };
            var diffusebeamgunTrigger = new[]
            {
                (Triggers.Attack2end, States.attackidle),
                (Triggers.Died, States.dead),
            };
            var firewallmoveTrigger = new[]
            {
                (Triggers.firewallmoveend, States.firewall),
                (Triggers.Died, States.dead),
            };
            var firewallTrigger = new[]
            {
                (Triggers.Attack3end, States.attackidle),
                (Triggers.Died, States.dead),
            };
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.armpunch, armpunchTrigger)
                .AddTransmissions(States.diffusebeamgun, diffusebeamgunTrigger)
                .AddTransmissions(States.firewall, firewallTrigger)
                .AddTransmissions(States.firewallmove, firewallmoveTrigger);
            // ステート登録
            _stateMachine.AddState(States.idle, new Idle());
            // 死亡
            _stateMachine.AddState(States.dead, new Idle());
            // 攻撃待機
            var attackIdle = new Idle_LazyEvent(5f);
            // 遅延完了時に呼びたい処理をOnCompletedで登録
            attackIdle.OnCompleted += Attackjudgement;
            _stateMachine.AddState(States.attackidle, attackIdle);
            // アームパンチ
            _stateMachine.AddState(States.armpunch, new Idle());
            // 拡散ビーム砲
            _stateMachine.AddState(States.diffusebeamgun, new Idle());
            // ファイアウォール移動
            _stateMachine.AddState(States.firewallmove, new Idle());
            // ファイアウォール
            _stateMachine.AddState(States.firewall, new Idle());
        }
    }
}
