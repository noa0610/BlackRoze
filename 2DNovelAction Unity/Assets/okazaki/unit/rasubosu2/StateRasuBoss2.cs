using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using Unity.VisualScripting;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_rasubosu2
    {
        private Warp warp;
        private ShootForward pointermissile;
        private ShootForward crosswave;
        private ShootForward warpShot;
        private ShootForward flashBeamSword;
        public enum States
        {
            none,
            idle,               // 待機
            dead,               // 死亡
            warpidle,           // ワープ待機
            beforewarp,         // ワープ直前
            warp,               // ワープ 
            attackidle,         // 攻撃待機
            pointermissile,     // ポインターミサイル
            crosswave,          // クロスウェーブ
            warpShot,           // ワープショット
            grappleSlash,       // グラップルスラッシュ
            flashBeamSword,     // フラッシュビームソード
        }
        private enum Triggers
        {
            None,
            Warpcooldown,       // ワープクールダウンした
            WarpStart,          // ワープ開始
            Warpend,            // ワープ移動終了
            Warpcomplete,       // ワープ完了
            Attackcooldown,     // 攻撃クールダウンした
            Attack1,            // 攻撃１
            Attack2,            // 攻撃２
            Attack3,            // 攻撃３
            Attack4,            // 攻撃４
            Attack5,            // 攻撃５
            Attack1end,         // 攻撃１した
            Attack2end,         // 攻撃２した
            Attack3end,         // 攻撃３した
            Attack4end,         // 攻撃４した
            Attack5end,         // 攻撃5した
            Event1,             // イベント1が終わった
            Playerdead,         // プレイヤーが死亡
            Died,               // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]                                    /** 待機ステートのトリガー **/
            {
                (Triggers.Event1, States.warpidle ,"toIdle"),          // イベント1発生でワープ待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var warpidleTrigger = new[]                                /** ワープ待機ステートのトリガー **/
            {
                (Triggers.Warpcooldown, States.beforewarp, ""),        // ワープクールダウンでワープ直前へ
                (Triggers.Warpcomplete, States.attackidle, ""),        // ワープ完了で攻撃待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var beforewarpTrigger = new[]                              /** ワープ直前のステートのトリガー **/
            {
                (Triggers.WarpStart, States.warp, ""),                 // ワープ開始でワープへ
                (Triggers.Died, States.dead, "")                       // 死亡で死へ
            };
            var warpTrigger = new[]                                    /** ワープステートのトリガー **/
            {
                (Triggers.Warpend, States.warpidle, ""),               // ワープ移動終了でワープ待機へ
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
                (Triggers.Died, States.dead, ""),                      // 死亡で待機
            };


            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.warpidle, warpidleTrigger)
                .AddTransmissions(States.beforewarp, beforewarpTrigger)
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
            var attackIdle = new Idle_LazyEvent(0.5f);
            // 遅延完了時に呼びたい処理をOnCompletedで登録
            attackIdle.OnCompleted += AttackSelect;
            {
                
            }
            _stateMachine.AddState(States.attackidle, attackIdle);

            /* ワープ待機 */
            var warpidle = new Idle_LazyChange(Triggers.Warpcooldown.ToString(), _WarpIntervalTime, true);
            warpidle.OnCompleted += WarpIdleExit;
            {
                
            }
            _stateMachine.AddState(States.warpidle, warpidle);

            /* ワープ直前 */
            var beforewarp = new Idle();
            _stateMachine.AddState(States.beforewarp, beforewarp);

            /* ワープ */
            warp = new Warp();
            warp.OnCompleted += WarpEnter;
            {
                
            }
            _stateMachine.AddState(States.warp, warp);

            /* ポインターミサイル */
            pointermissile = new ShootForward();
            _stateMachine.AddState(States.pointermissile, pointermissile);

            /* クロスウェーブ */
            crosswave = new ShootForward();
            _stateMachine.AddState(States.crosswave, crosswave);

            /* ワープショット */ 
            warpShot = new ShootForward();
            _stateMachine.AddState(States.warpShot, warpShot);

            /* フラッシュビームソード */ 
            flashBeamSword = new ShootForward();
            _stateMachine.AddState(States.flashBeamSword, flashBeamSword);
        }
    }
}