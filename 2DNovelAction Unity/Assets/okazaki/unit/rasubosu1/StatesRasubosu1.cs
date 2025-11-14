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
            armpunchanimaidle,

            armpunchidle,
            armpunchendile, 
            diffusebeamgun,
            firewall,
            firewallanimaidle,
            firewallshot,
            firewallshotidle,
        }
        public enum Triggers
        {
            None,
            FoundPlayer,
            Attackcooldown,
            firewallmoveend,
            Attack1,
            Armpunch,
            Attack1loop,
            Attack1loopend,
            Attack1idle,
            Attack2,
            Attack3,
            Firewall,
            Firewallshot,
            Attack3idle,
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
                (Triggers.FoundPlayer, States.attackidle, ""),
                (Triggers.Died, States.dead, "")
            };
            var attackidleTrigger = new[]
            {
                (Triggers.Playerdead, States.idle,""),
                (Triggers.Died, States.dead,""),
                (Triggers.Attack1, States.armpunchanimaidle,"FireRight"),
                (Triggers.Attack2, States.armpunchanimaidle,"FireRight"),
                (Triggers.Attack3, States.armpunchanimaidle,"FireRight")
            };
            var armpunchanimaidleTrigger = new[]
            {
                (Triggers.Armpunch, States.armpunch,""),
                (Triggers.Died, States.dead,"")
            };
            var armpunchTrigger = new[]
            {
                (Triggers.Attack1loop, States.armpunchidle,""),
                (Triggers.Attack1idle, States.armpunchendile,""),
                (Triggers.Died, States.dead,"")
            };
            var armpunchidleTrigger = new[]
            {
                (Triggers.Attack1loopend, States.armpunch,""),
                (Triggers.Died, States.dead,"")
            };
            var armpunchendileTrigger = new[]
            {
                (Triggers.Attack1end, States.attackidle,"ArmReturn"),
                (Triggers.Died, States.dead,"")
            };
            var diffusebeamgunTrigger = new[]
            {
                (Triggers.Attack2end, States.attackidle,""),
                (Triggers.Died, States.dead,"")
            };
            var firewallTrigger = new[]
            {
                (Triggers.Firewallshot, States.firewallshot,""),
                (Triggers.Died, States.dead,"")
            };
            var firewallanimaidleTrigger = new[]
            {
                (Triggers.Firewall, States.firewall,""),
                (Triggers.Died, States.dead,"")
            };
            var firewallshotTrigger = new[]
            {
                (Triggers.Attack3idle, States.firewallshotidle,""),
                (Triggers.Died, States.dead,"")
            };
            var firewallshotidle1Trigger = new[]
            {
                (Triggers.Attack3end, States.attackidle,"ArmReturn"),
                (Triggers.Died, States.dead,"")
            };
            _stateMachine
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.attackidle, attackidleTrigger)
                .AddTransitions(States.armpunch, armpunchTrigger)
                .AddTransitions(States.armpunchanimaidle, armpunchanimaidleTrigger)
                .AddTransitions(States.armpunchidle, armpunchidleTrigger)
                .AddTransitions(States.armpunchendile, armpunchendileTrigger)
                .AddTransitions(States.diffusebeamgun, diffusebeamgunTrigger)
                .AddTransitions(States.firewall, firewallTrigger)
                .AddTransitions(States.firewallanimaidle, firewallanimaidleTrigger) 
                .AddTransitions(States.firewallshot, firewallshotTrigger)
                .AddTransitions(States.firewallshotidle, firewallshotidle1Trigger);
            // ステート登録
            _stateMachine.AddState(States.idle, new Idle());
            // 死亡
            _stateMachine.AddState(States.dead, new Idle());
            // 攻撃待機
            var attackIdle = new Idle_LazyEvent(5f);
            attackIdle.OnCompleted += Attackjudgement;
            _stateMachine.AddState(States.attackidle, attackIdle);
            // アームパンチアニメ待機
            var armpunchanimaidle = new Idle_LazyChange(Triggers.Armpunch.ToString(), 5, true);
            _stateMachine.AddState(States.armpunchanimaidle, armpunchanimaidle);
            // アームパンチ
            var armpunch = new ShootForward(_armpunchBulletData, _armpunchTargetLayer)
            .SetDirection(Vector2.down);
            armpunch.SetGameObject(_armpunchPoint != null ? _armpunchPoint : gameObject);
            armpunch.onShootComplete.AddListener(() =>
            {
                if (punchcount == 2)
                {
                    // 最終状態なら攻撃終了へ
                    punchcount = 0;
                    Debug.Log(punchcount);
                    _stateMachine.LazyChange(Triggers.Attack1idle);
                }
                else
                {
                    ++punchcount;
                    Debug.Log(punchcount);
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
            // アームパンチ終了待機
            var armpunchendile = new Idle_LazyChange(Triggers.Attack1end.ToString(), 5, true);
            _stateMachine.AddState(States.armpunchendile, armpunchendile);
            // 拡散ビーム砲
            var diffusebeamgun = new ShootForward(_armpunchBulletData, _armpunchTargetLayer)
            .SetDirection(Vector2.left);
            diffusebeamgun.SetGameObject(_diffusebeamgunPoint != null ? _diffusebeamgunPoint : gameObject);
            diffusebeamgun.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Attack2end);
            }); 
            _stateMachine.AddState(States.diffusebeamgun, diffusebeamgun);
            // ファイアウォールアニメ待機
            var firewallanimaidle = new Idle_LazyChange(Triggers.Firewall.ToString(), 5, true);
            _stateMachine.AddState(States.firewallanimaidle, firewallanimaidle);

            // ファイアウォール
            var firewall = new Idle_LazyEvent(2f);
            firewall.OnCompleted += () =>
            {
                Udetobasi();
                _stateMachine.LazyChange(Triggers.Firewallshot);
            };
            _stateMachine.AddState(States.firewall, firewall);
            var firewallshot = new ShootForward(_firewallBulletData, _firewallTargetLayer)
            .SetDirection(Vector2.left);
            firewallshot.SetGameObject(_biribiriPoint != null ? _biribiriPoint : gameObject);
            firewallshot.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Attack3idle);
            });
            _stateMachine.AddState(States.firewallshot, firewallshot);
            var firewallshotidle = new Idle_LazyEvent(5f);
            firewallshotidle.OnCompleted += () =>
            {
                _stateMachine.LazyChange(Triggers.Attack3end);
            };
            _stateMachine.AddState(States.firewallshotidle, firewallshotidle);
        }
    }
}
