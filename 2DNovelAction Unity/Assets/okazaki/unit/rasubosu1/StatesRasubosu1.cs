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
        private Idle dead;
        private enum States
        {
            none,
            entry,
            idle,
            dead,
            attackidle,
            armpunch,
            armpunch_Start,
            armpunch_Idle,
            armpunch_end,
            diffusebeamgun,
            diffusebeamgunidle,
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
            Attack2idle,
            Attack3end,
            EntryEnd,           // 登場終了
            Event1,
            Playerdead,
            Died,
        }

        protected override void RegisterStats()
        {

            // トランスミッション
            #region === Basic Triggers ===
            var entryTrigger = new[]                                   /** 登場ステートのトリガー **/
            {
                (Triggers.EntryEnd, States.attackidle ,"toIdle"),      // 演出終了で攻撃待機へ
            };
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.attackidle, ""),
                (Triggers.Died, States.dead, "toDead")
            };
            var attackidleTrigger = new[]
            {
                (Triggers.Playerdead, States.idle,""),
                (Triggers.Died, States.dead,"toDead"),
                (Triggers.Attack1, States.armpunch_Start,"FireRight"),
                (Triggers.Attack2, States.diffusebeamgun,"toShot"),
                (Triggers.Attack3, States.firewallanimaidle,"FireRight")
            };
            #endregion

            #region === ArmPunch Triggers ===
            var armpunchStartTrigger = new[]                                   /** アームパンチ開始ステートのトリガー **/
            {
                (Triggers.Armpunch, States.armpunch,""),
                (Triggers.Died, States.dead,"toDead")
            };
            var armpunchTrigger = new[]                                        /** アームパンチステートのトリガー **/
            {
                (Triggers.Attack1loop, States.armpunch_Idle,""),
                (Triggers.Attack1idle, States.armpunch_end,""),
                (Triggers.Died, States.dead,"toDead")
            };
            var armpunchidleTrigger = new[]                                    /** アームパンチ待機ステートのトリガー **/
            {
                (Triggers.Attack1loopend, States.armpunch,""),
                (Triggers.Died, States.dead,"toDead")
            };
            var armpunchendTrigger = new[]                                  /** アームパンチ終了ステートのトリガー **/
            {
                (Triggers.Attack1end, States.attackidle,"ArmReturn"),
                (Triggers.Died, States.dead,"toDead")
            };
            #endregion

            #region === SpreadShot Triggers ===
            var diffusebeamgunTrigger = new[]
            {
                (Triggers.Attack2idle, States.diffusebeamgunidle,""),
                (Triggers.Died, States.dead,"toDead")
            };
            var diffusebeamgunidleTrigger = new[]
            {
                (Triggers.Attack2end, States.attackidle,""),
                (Triggers.Died, States.dead,"toDead")
            };
            #endregion

            #region === FireWall Triggers ===
            var firewallTrigger = new[]
            {
                (Triggers.Firewallshot, States.firewallshot,""),
                (Triggers.Died, States.dead,"toDead")
            };
            var firewallanimaidleTrigger = new[]
            {
                (Triggers.Firewall, States.firewall,""),
                (Triggers.Died, States.dead,"toDead")
            };
            var firewallshotTrigger = new[]
            {
                (Triggers.Attack3idle, States.firewallshotidle,""),
                (Triggers.Died, States.dead,"toDead")
            };
            var firewallshotidle1Trigger = new[]
            {
                (Triggers.Attack3end, States.attackidle,"ArmReturn"),
                (Triggers.Died, States.dead,"toDead")
            };
            #endregion

            _stateMachine
                .AddTransitions(States.entry, entryTrigger)
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.attackidle, attackidleTrigger)
                .AddTransitions(States.armpunch, armpunchTrigger)
                .AddTransitions(States.armpunch_Start, armpunchStartTrigger)
                .AddTransitions(States.armpunch_Idle, armpunchidleTrigger)
                .AddTransitions(States.armpunch_end, armpunchendTrigger)
                .AddTransitions(States.diffusebeamgun, diffusebeamgunTrigger)
                .AddTransitions(States.diffusebeamgunidle, diffusebeamgunidleTrigger)
                .AddTransitions(States.firewall, firewallTrigger)
                .AddTransitions(States.firewallanimaidle, firewallanimaidleTrigger)
                .AddTransitions(States.firewallshot, firewallshotTrigger)
                .AddTransitions(States.firewallshotidle, firewallshotidle1Trigger);

            #region === Basic States ===

            /* 登場演出 */
            var entry = new Idle_LazyChange(Triggers.EntryEnd.ToString(), _EntryEndwaitTime);
            entry.OnCompleted += EntryEnd;
            _stateMachine.AddState(States.entry, entry);

            /* 待機 */
            _stateMachine.AddState(States.idle, new Idle());

            /* 死亡 */
            dead = new Idle_LazyEvent(_DeadEndwaitTime);
            dead.OnAnimationCompleted.AddListener(() =>
            {
                Dead();
            });
            _stateMachine.AddState(States.dead, dead);

            /* 攻撃待機 */
            var attackIdle = new Idle_LazyEvent(_AttackIntervalTime);
            attackIdle.OnCompleted += AttackSelect;
            _stateMachine.AddState(States.attackidle, attackIdle);
            #endregion

            #region === ArmPunch States ===

            /* アームパンチ開始 */
            var armpunchStart = new Idle_LazyChange(Triggers.Armpunch.ToString(), _ArmPunchStartTime);
            armpunchStart.OnCompleted += RandomArmPunchFallPoint;
            _stateMachine.AddState(States.armpunch_Start, armpunchStart);

            /* アームパンチ */
            var armpunch = new ShootForward(_armpunchBulletData, AttackLayer)
            .SetDirection(Vector2.down);
            armpunch.SetGameObject(_armpunchPoint != null ? _armpunchPoint : gameObject);
            armpunch.onShootComplete.AddListener(() =>
            {
                PlaySE(_ArmFallSEName, _ArmFallSEVolume);
                // 攻撃終了
                if (punchcount >= _armPunchCount)
                {
                    punchcount = 0;
                    Debug.Log(punchcount);
                    _stateMachine.LazyChange(Triggers.Attack1idle);
                }
                else
                {
                    ++punchcount;
                    Debug.Log(punchcount);
                    RandomArmPunchFallPoint();
                    _stateMachine.LazyChange(Triggers.Attack1loop);
                }
            });
            _stateMachine.AddState(States.armpunch, armpunch);

            /* アームパンチ待機 */
            var armpunchidle = new Idle_LazyEvent(_ArmPunchWaitTime);
            armpunchidle.OnCompleted += () =>
            {
                // Attack1loopend を発火して armpunch に戻す（transmission で armpunchidle->armpunch に遷移）
                _stateMachine.LazyChange(Triggers.Attack1loopend);
            };
            _stateMachine.AddState(States.armpunch_Idle, armpunchidle);

            /* アームパンチ終了 */
            var armpunchendile = new Idle_LazyChange(Triggers.Attack1end.ToString(), _ArmPunchEndTime);
            _stateMachine.AddState(States.armpunch_end, armpunchendile);
            #endregion
            
            #region === SpreadShot States ===

            /* 拡散ビーム砲 */
            var diffusebeamgun = new ShootForward(_SpreadShotBulletData, AttackLayer)
            .SetDirection(Vector2.left);
            diffusebeamgun.SetGameObject(_SpreadShotPoint != null ? _SpreadShotPoint : gameObject);
            diffusebeamgun.onShootComplete.AddListener(() =>
            {
                PlaySE(_ShotSEName, _ShotSEVolume);
                _stateMachine.LazyChange(Triggers.Attack2idle);
            });
            _stateMachine.AddState(States.diffusebeamgun, diffusebeamgun);

            /* 拡散ビーム砲待機 */
            var diffusebeamgunidle = new Idle_LazyChange(Triggers.Attack2end.ToString(), _SpreadShotEndTime);
            _stateMachine.AddState(States.diffusebeamgunidle, diffusebeamgunidle);
            #endregion

            #region === FireWall States ===

            /* ファイアウォール開始 */
            var firewallanimaidle = new Idle_LazyChange(Triggers.Firewall.ToString(), _fireWallStartTime);
            _stateMachine.AddState(States.firewallanimaidle, firewallanimaidle);

            /* ファイアウォール生成 */
            var firewall = new Idle_LazyChange(Triggers.Firewallshot.ToString(), _fireWallWaitTime);
            firewall.OnCompleted += () =>
            {
                FireWallArmMove();
            };
            _stateMachine.AddState(States.firewall, firewall);

            /* ファイアウォール照射 */
            var firewallshot = new ShootForward(_firewallBulletData, AttackLayer)
            .SetDirection(Vector2.left);
            firewallshot.SetGameObject(gameObject);
            firewallshot.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Attack3idle);
            });
            _stateMachine.AddState(States.firewallshot, firewallshot);

            /* ファイアウォール終了 */
            var firewallshotidle = new Idle_LazyChange(Triggers.Attack3end.ToString(), _fireWallEndTime);
            _stateMachine.AddState(States.firewallshotidle, firewallshotidle);
            #endregion
        }
    }
}
