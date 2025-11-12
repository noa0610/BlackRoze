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
            armpunchidle,
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
            Attack1loop,
            Attack1loopend,
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
                (Triggers.Playerdead, States.idle,""),
                (Triggers.Died, States.dead,""),
                (Triggers.Attack1, States.firewall,""),
                (Triggers.Attack2, States.firewall,""),
                (Triggers.Attack3, States.firewall,"")
            };
            var armpunchTrigger = new[]
            {
                (Triggers.Attack1loop, States.armpunchidle,""),
                (Triggers.Died, States.dead,"")
            };
            var armpunchidleTrigger = new[]
            {
                (Triggers.Attack1loopend, States.armpunch,""),
                (Triggers.Attack1end, States.attackidle,""),
                (Triggers.Died, States.dead,"")
            };
            var diffusebeamgunTrigger = new[]
            {
                (Triggers.Attack2end, States.attackidle,""),
                (Triggers.Died, States.dead,"")
            };
            var firewallTrigger = new[]
            {
                (Triggers.Attack3end, States.attackidle,""),
                (Triggers.Died, States.dead,"")
            };
            _stateMachine
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.attackidle, attackidleTrigger)
                .AddTransitions(States.armpunch, armpunchTrigger)
                .AddTransitions(States.armpunchidle, armpunchidleTrigger)
                .AddTransitions(States.diffusebeamgun, diffusebeamgunTrigger)
                .AddTransitions(States.firewall, firewallTrigger);
            // ステート登録
            _stateMachine.AddState(States.idle, new Idle());
            // 死亡
            _stateMachine.AddState(States.dead, new Idle());
            // 攻撃待機
            var attackIdle = new Idle_LazyEvent(5f);
            attackIdle.OnCompleted += Attackjudgement;
            _stateMachine.AddState(States.attackidle, attackIdle);
            // アームパンチ
            var armpunch = new ShootForward(_armpunchBulletData, _armpunchTargetLayer)
            .SetDirection(Vector2.down);
            armpunch.SetGameObject(_armpunchPoint != null ? _armpunchPoint : gameObject);
            armpunch.onShootComplete.AddListener(() =>
            {
                if (punchcount == 3)
                {
                    // 最終状態なら攻撃終了へ
                    punchcount = 0;
                    _stateMachine.LazyChange(Triggers.Attack1end);
                }
                else
                {
                    ++punchcount;
                    // 続けるなら Attack1loop を発火して armpunch に戻す（transmission で armpunch->armpunchidle に遷移）
                    _stateMachine.LazyChange(Triggers.Attack1loop);
                }
            });
            _stateMachine.AddState(States.armpunch, armpunch);
            var armpunchidle = new Idle_LazyEvent(5f);
            armpunchidle.OnCompleted += () =>
            {
                // Attack1loopend を発火して armpunch に戻す（transmission で armpunchidle->armpunch に遷移）
                _stateMachine.LazyChange(Triggers.Attack1loopend);
            };
            _stateMachine.AddState(States.armpunchidle, armpunchidle);
            // 拡散ビーム砲
            var diffusebeamgun = new ShootForward(_diffusebeamgunBulletData, _diffusebeamgunTargetLayer)
            .SetDirection(Vector2.down);
            diffusebeamgun.SetGameObject(_diffusebeamgunPoint != null ? _diffusebeamgunPoint : gameObject);
            diffusebeamgun.onShootComplete.AddListener(() =>
            {
                _stateMachine.ChangeState(Triggers.Attack2end);
            });
            _stateMachine.AddState(States.diffusebeamgun, diffusebeamgun);
            // ファイアウォール
            var firewall = new Idle_LazyEvent(5f);
            firewall.OnCompleted += () =>
            {
                Udetobasi();
                _stateMachine.ChangeState(Triggers.Attack3end);
            };
            _stateMachine.AddState(States.firewall, firewall);
        }
    }
}
