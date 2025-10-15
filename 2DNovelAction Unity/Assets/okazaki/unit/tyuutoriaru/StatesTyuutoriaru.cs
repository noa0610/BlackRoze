using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_tyuutoriaru
    {

        private enum States
        {
            none,
            idle,// 待機
            dead,// 死亡
            attackidle, // 攻撃待機
            lasershot, // レーザー攻撃
            beamswordattack, // ビームソード攻撃
            beamswordattackmove, // ビームソード攻撃移動
            fixedpositionjump, // ジャンプ
            stun, // スタン
            shockwave, // ショックウェーブ
        }
        private enum Triggers
        {
            None,
            FoundPlayer,   // プレイヤーを発見した
            Attackcooldown, // 攻撃クールダウンした
            Attack1, // 攻撃１
            Attack2, // 攻撃２
            Attack1end, // 攻撃1した
            moveend, // 移動した
            Attack2end, // 攻撃1した
            Shockwaveend, // ショックウェーブした
            HalfHP, // HPが半分以下
            Landing, // 着地
            Event1, // イベント1発生
            Event2, // イベント2発生
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            // States.idle
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.attackidle, "Contact"),
                (Triggers.Died, States.dead, "")
            };
            // States.attackidle
            var attackidleTrigger = new[]
            {
                (Triggers.Attack1, States.lasershot),
                (Triggers.Attack2, States.beamswordattack),
                (Triggers.Died, States.dead)
            };
            // States.lasershot
            var lasershotTrigger = new[]
            {
                (Triggers.Attack1end, States.attackidle),
                (Triggers.Died, States.dead)
            };
            // States.beamswordattack
            var beamswordattackTrigger = new[]
            {
                (Triggers.moveend, States.beamswordattackmove),
                (Triggers.Died, States.dead)
            };
            // States.beamswordattackmove
            var beamswordattackmoveTrigger = new[]
            {
                (Triggers.Attack2end, States.attackidle),
                (Triggers.Died, States.dead)
            };
            // States.stun
            var stunTrigger = new[]
            {
                (Triggers.Landing, States.attackidle),
                (Triggers.Died, States.dead)
            };
            // States.shockwave
            var shockwaveTrigger = new[]
            {
                (Triggers.Shockwaveend, States.attackidle),
                (Triggers.Died, States.dead)
            };
            _stateMachine
            .AddTransmissions(States.idle, idleTrigger)
            .AddTransmissions(States.attackidle, attackidleTrigger)
            .AddTransmissions(States.lasershot, lasershotTrigger)
            .AddTransmissions(States.beamswordattack, beamswordattackTrigger)
            .AddTransmissions(States.beamswordattackmove, beamswordattackmoveTrigger)
            .AddTransmissions(States.stun, stunTrigger)
            .AddTransmissions(States.shockwave, shockwaveTrigger);
            // 待機
            _stateMachine.AddState(States.idle, new Idle());
            // 死亡 
            _stateMachine.AddState(States.dead, new Idle());
            // 攻撃待機
            var attackIdle = new Idle_LazyEvent(5f);
            // 遅延完了時に呼びたい処理をOnCompletedで登録  
            attackIdle.OnCompleted += Attackselect;
            _stateMachine.AddState(States.attackidle, attackIdle);
            // レーザー攻撃
            _stateMachine.AddState(States.lasershot, new Idle());
            // ビームソード攻撃
            _stateMachine.AddState(States.beamswordattack, new Idle());
            // ビームソード攻撃移動
            var freeMove = new FreeMove()
            .SetAccel(80f)
            .SetDecel(40f)
            .SetDeadZone(0.01f);
            _stateMachine.AddState(States.beamswordattackmove, freeMove);
            // スタン
            var stun = new Idle_LazyChange(Triggers.Event2.ToString(),  5, true);
            _stateMachine.AddState(States.stun, stun);
            // ショックウェーブ
            _stateMachine.AddState(States.shockwave, new Idle());

        }
    }
}