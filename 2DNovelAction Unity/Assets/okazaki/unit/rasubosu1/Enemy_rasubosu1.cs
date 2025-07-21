using Unity.VisualScripting;
using UnityEngine;

namespace BlackRose
{
    public class Enemy_rasubosu1 : UnitBase
    {
        public enum States
        {
            none,
            idle,// 待機
            dead,// 死亡
            attackidle, // 攻撃待機
            attackjudgement, // 攻撃判定
            armpunch, // アームパンチ
            diffusebeamgun, // 拡散ビーム砲
            firewall, // ファイアウォール
            headSeparationShot, // 頭部分離ショット


        }
        private enum Triggers
        {
            None,
            Attackcooldown, // 攻撃クールダウンした
            Attack1, // 攻撃１
            Attack2, // 攻撃２
            Attack3, // 攻撃３
            Attack4, // 攻撃４
            Attack1end, // 攻撃１した
            Attack2end, // 攻撃２した
            Attack3end, // 攻撃３した
            Attack4end, // 攻撃４した
            shockwaveend, // ショックウェーブした
            Event1, // イベント1が終わった
            Playerdead, // プレイヤーが死亡
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]                                // 待機ステートのトリガー
            {
                (Triggers.Event1, States.attackidle),              // イベント1が終わったで攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var attackidleTrigger = new[]                          // 攻撃待機ステートのトリガー
            {

                (Triggers.Attackcooldown, States.attackjudgement), // 攻撃クールダウンで攻撃判定へ
                (Triggers.Playerdead, States.dead),                // プレイヤーが死亡で死へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var attackjudgementTrigger = new[]                     // 攻撃判定ステートのトリガー              
            {
                (Triggers.Attack1, States.armpunch),               // アームパンチでアームパンチへ
                (Triggers.Attack2, States.diffusebeamgun),         // 拡散ビーム砲で拡散ビーム砲へ
                (Triggers.Attack3, States.firewall),               // ファイアウォールでファイアウォールへ
                (Triggers.Attack4, States.headSeparationShot),     // 頭分離ショットで頭分離ショットへ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var armpunchTrigger = new[]                            // アームパンチステートのトリガー        
            {
                (Triggers.Attack1end, States.attackidle),          // アームパンチ終了で攻撃待機へ         
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var diffusebeamgunTrigger = new[]                      //拡散ビーム砲ステートのトリガー          
            {
                (Triggers.Attack2end, States.attackidle),          // 拡散ビーム砲終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var firewallTrigger = new[]                            // ファイアウォールステートのトリガー
            {
                (Triggers.Attack3end, States.attackidle),          // ファイアウォール終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var headSeparationShotTrigger = new[]                  // 頭分離ショットステートのトリガー           
            {
                (Triggers.Attack4end, States.attackidle),          // 頭分離ショット終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.attackjudgement, attackjudgementTrigger)
                .AddTransmissions(States.armpunch, armpunchTrigger)
                .AddTransmissions(States.diffusebeamgun, diffusebeamgunTrigger)
                .AddTransmissions(States.firewall, firewallTrigger)
                .AddTransmissions(States.headSeparationShot, headSeparationShotTrigger);
        }
    }
}