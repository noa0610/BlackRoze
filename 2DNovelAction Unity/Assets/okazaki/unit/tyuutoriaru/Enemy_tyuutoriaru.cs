using Unity.VisualScripting;
using UnityEngine;

namespace BlackRose
{
    public class Enemy_tyuutoriaru : UnitBase
    {
        public enum States
        {
            none,
            idle,// 待機
            dead,// 死亡
            attackidle, // 攻撃待機
            attackjudgement, // 攻撃判定
            lasershot, // レーザー攻撃
            beamswordapproaching, // ビームソード接近
            beamswordattack, // ビームソード攻撃
            jump, // ジャンプ
            stun, // スタン
            shockwave, // ショックウェーブ

        }
        private enum Triggers
        {
            None,
            Attackcooldown, // 攻撃クールダウンした
            Attack1, // 攻撃１
            Attack2, // 攻撃２
            Attack2end, // 攻撃２した
            shockwaveend, // ショックウェーブした
            Shoot, // 射撃
            Frontplayer, // 前方にプレイヤーがいる
            Backplayer, // 後方にプレイヤーがいる
            HalfHP, // HPが半分以下
            Landing, // 着地
            Event1, // イベント1発生
            Event2, // イベント2発生
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]                                // 待機ステートのトリガー
            {
                (Triggers.Event1, States.attackidle),              // イベント1発生で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.shockwave)                // HPが半分以下でショックウェーブへ
            };
            var attackidleTrigger = new[]                          // 攻撃待機ステートのトリガー  
            {
                (Triggers.Attackcooldown, States.attackjudgement), // 攻撃クールダウンで攻撃判定へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.shockwave)                // HPが半分以下でショックウェーブへ
            };
            var attackjudgementTrigger = new[]                     // 攻撃判定ステートのトリガー
            {
                (Triggers.Attack1, States.lasershot),              // 攻撃１でレーザー攻撃へ
                (Triggers.Attack2, States.beamswordapproaching),   // 攻撃２でビームソード接近へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.shockwave)                // HPが半分以下でショックウェーブへ
            };
            var lasershotTrigger = new[]                           // レーザー攻撃ステートのトリガー
            {
                (Triggers.Shoot, States.jump),                     // 射撃でジャンプへ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.shockwave)                // HPが半分以下でショックウェーブへ
            };
            var beamswordapproachingTrigger = new[]                // ビームソード接近ステートのトリガー
            {
                (Triggers.Frontplayer, States.beamswordattack),    // 前方にプレイヤーがいるでビームソード攻撃へ
                (Triggers.Backplayer, States.beamswordattack),     // 後方にプレイヤーがいるでビームソード攻撃へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.shockwave)                // HPが半分以下でショックウェーブへ
            };
            var beamswordattackTrigger = new[]                     // ビームソード攻撃ステートのトリガー
            {
                (Triggers.Attack2end, States.jump),                // 攻撃２終了でジャンプへ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.shockwave)                // HPが半分以下でショックウェーブへ
            };
            var jumpTrigger = new[]                                // ジャンプステートのトリガー
            {
                (Triggers.Landing, States.attackidle),             // 着地で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.shockwave)                // HPが半分以下でショックウェーブへ
            };
            var stunTrigger = new[]                                // スタンステートのトリガー
            {
                (Triggers.Event2, States.shockwave),               // イベント2発生でショックウェーブへ
                (Triggers.Died, States.dead),                      // 死亡で死へ 
                (Triggers.HalfHP, States.shockwave)                // HPが半分以下でショックウェーブへ
            };
            var shockwaveTrigger = new[]                           // ショックウェーブステートのトリガー
            {
                (Triggers.shockwaveend, States.idle),              // ショックウェーブ終了で待機へ
                (Triggers.Died, States.dead)                       // 死亡で死へ
            };
            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.attackjudgement, attackjudgementTrigger)
                .AddTransmissions(States.lasershot, lasershotTrigger)
                .AddTransmissions(States.beamswordapproaching, beamswordapproachingTrigger)
                .AddTransmissions(States.beamswordattack, beamswordattackTrigger)
                .AddTransmissions(States.jump, jumpTrigger)
                .AddTransmissions(States.stun, stunTrigger)
                .AddTransmissions(States.shockwave, shockwaveTrigger);
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

