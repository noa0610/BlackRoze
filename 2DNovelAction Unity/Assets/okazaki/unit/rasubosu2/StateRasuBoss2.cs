using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_rasubosu2
    {
        public enum States
        {
            none,
            idle,// 待機
            dead,// 死亡
            warpidle, // ワープ待機
            warp, // ワープ 
            attackidle, // 攻撃待機
            pointermissile, // ポインターミサイル
            crosswave, // クロスウェーブ
            warpShot, // ワープショット
            grappleSlash, // グラップルスラッシュ
            flashBeamSword, // フラッシュビームソード
        }
        private enum Triggers
        {
            None,
            Warpcooldown, // ワープクールダウンした
            Warpreturn, // ワープに戻る
            Warpend, // ワープ終了
            Warpcomplete, // ワープ完了
            Attackcooldown, // 攻撃クールダウンした
            Attack1,       // 攻撃１
            Attack2,       // 攻撃２
            Attack3,       // 攻撃３
            Attack4,       // 攻撃４
            Attack5,       // 攻撃5
            Attack1end,    // 攻撃１した
            Attack2end,    // 攻撃２した
            Attack3end,    // 攻撃３した
            Attack4end,    // 攻撃４した
            Attack5end,    // 攻撃5した
            Event1,        // イベント1が終わった
            Playerdead,    // プレイヤーが死亡
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]                                    /** 待機ステートのトリガー **/
            {
                (Triggers.Event1, States.warpidle ,"Idle"),            // イベント1発生で攻撃待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var warpidleTrigger = new[]                                /** ワープ待機ステートのトリガー **/
            {
                (Triggers.Warpcooldown, States.warp, ""),              // ワープクールダウンでワープへ
                (Triggers.Warpcomplete, States.attackidle, ""),        // ワープ完了で攻撃待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var warpTrigger = new[]                                    /** ワープステートのトリガー **/
            {
                (Triggers.Warpend, States.warp, ""),                   // ワープ終了でワープへ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var attackidleTrigger = new[]                              /** 攻撃待機ステートのトリガー **/
            {
                (Triggers.Playerdead, States.idle, ""),                // プレイヤーが死亡で待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
                (Triggers.Attack1, States.pointermissile, ""),         // ポインターミサイルへ 
                (Triggers.Attack2, States.crosswave, ""),              // クロスウェーブへ
                (Triggers.Attack3, States.warpShot, ""),               // ワープショットへ
                (Triggers.Attack4, States.grappleSlash, ""),           // グラップルスラッシュへ
                (Triggers.Attack5, States.flashBeamSword, ""),         // フラッシュビームソードへ
            };
            var pointermissileTrigger = new[]                          /** ポインターミサイルステートのトリガー **/
            {
                (Triggers.Attack1end, States.warpidle, ""),            // ポインターミサイル終了で攻撃待機へ       
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var crosswaveTrigger = new[]                               /** ステートのトリガー **/     
            {
                (Triggers.Attack2end, States.warpidle, ""),            // クロスウェーブ終了で攻撃待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var warpShotTrigger = new[]                                /** ワープショットステートのトリガー **/
            {
                (Triggers.Attack3end, States.attackidle, ""),          // ワープショット終了で攻撃待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var grappleSlashTrigger = new[]                            /** グラップルスラッシュステートのトリガー **/
            {
                (Triggers.Attack4end, States.attackidle, ""),          // 
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var flashBeamSwordTrigger = new[]                          /** フラッシュビームソードステートのトリガー **/                 
            {
                (Triggers.Attack5end, States.attackidle, ""),          // フラッシュビームソード終了で攻撃待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };


            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.warpidle, warpidleTrigger)
                .AddTransmissions(States.warp, warpTrigger)
                .AddTransmissions(States.pointermissile, pointermissileTrigger)
                .AddTransmissions(States.crosswave, crosswaveTrigger)
                .AddTransmissions(States.warpShot, warpShotTrigger)
                .AddTransmissions(States.grappleSlash, grappleSlashTrigger)
                .AddTransmissions(States.flashBeamSword, flashBeamSwordTrigger);

            /* 待機 */
            _stateMachine.AddState(States.idle, new Idle());
            /* 死亡 */
            _stateMachine.AddState(States.dead, new Idle());

            // var died = new Idle().SetAnimeTrigger("died").SetCancelableProgress(0);
            // died.OnAnimationCompleted.AddListener(() =>
            // {
            //     UnitManager.instance.RemoveUnit(this); // UnitManagerの自データ削除？
            //     Destroy(gameObject);
            // });
            // _stateMachine.AddState(States.dead, died);


            /* 攻撃待機 */
            var attackIdle = new Idle_LazyEvent(5f);
            // 遅延完了時に呼びたい処理をOnCompletedで登録
            attackIdle.OnCompleted += Attackjudgement;
            {
                
            }
            _stateMachine.AddState(States.attackidle, attackIdle);

            /* ワープ待機 */
            var warpidle = new Idle_LazyChange(Triggers.Warpcooldown.ToString(), 5, true);
            _stateMachine.AddState(States.warpidle, warpidle);

            /* ワープ */
            var warp = new Idle();
            _stateMachine.AddState(States.warpidle, warp);

            /* ポインターミサイル */
            var pointermissile = new ShootForward();
            _stateMachine.AddState(States.pointermissile, pointermissile);

            /* クロスウェーブ */
            var crosswave = new Idle();
            _stateMachine.AddState(States.crosswave, crosswave);

            /* ワープショット */ 
            var warpShot = new Idle();
            _stateMachine.AddState(States.warpShot, warpShot);

            /* グラップ */ 
            var grappleSlash = new Idle();
            _stateMachine.AddState(States.grappleSlash, grappleSlash);

            /* フラッシュビームソード */ 
            var flashBeamSword = new Idle();
            _stateMachine.AddState(States.flashBeamSword, flashBeamSword);
        }
    }
}