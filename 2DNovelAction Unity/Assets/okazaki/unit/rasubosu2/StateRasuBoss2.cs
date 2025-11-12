using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using Fungus;
using Unity.VisualScripting;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_rasubosu2
    {
        private Warp warp;
        private ShootMultiplePositions pointermissile;
        private ShootForward crosswave;
        private ShootForward warpShot;
        private ShootForward flashBeamSword;
        public enum States
        {
            none,
            idle,                 // 待機
            dead,                 // 死亡
            warpidle,             // ワープ待機
            beforewarp,           // ワープ直前
            warp,                 // ワープ 
            attackidle,           // 攻撃待機
            pointermissilebefore, // ポインタミサイル直前
            pointermissile,       // ポインターミサイル
            crosswave,            // クロスウェーブ
            warpShot,             // ワープショット
            flashBeamSword,       // フラッシュビームソード
        }
        private enum Triggers
        {
            None,
            Warpcooldown,       // ワープクールダウンした
            Warp,               // ワープ
            Warpend,            // ワープ移動終了
            Warpcomplete,       // ワープ完了
            Attackcooldown,     // 攻撃クールダウンした
            Attack1start,       // 攻撃１開始
            Attack2start,       // 攻撃２開始
            Attack3start,       // 攻撃３開始
            Attack4start,       // 攻撃４開始
            Attack1,            // 攻撃１
            Attack2,            // 攻撃２
            Attack3,            // 攻撃３
            Attack4,            // 攻撃４
            Attack1end,         // 攻撃１した
            Attack2end,         // 攻撃２した
            Attack3end,         // 攻撃３した
            Attack4end,         // 攻撃４した
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
                (Triggers.Warpcooldown, States.beforewarp, "WarpStart"),// ワープクールダウンでワープ直前へ
                (Triggers.Warpcomplete, States.attackidle, ""),        // ワープ完了で攻撃待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var beforewarpTrigger = new[]                              /** ワープ直前ステートのトリガー **/
            {
                (Triggers.Warp, States.warp, "toWarp"),                // ワープ開始でワープへ
                (Triggers.Died, States.dead, "")                       // 死亡で死へ
            };
            var warpTrigger = new[]                                    /** ワープステートのトリガー **/
            {
                (Triggers.Warpend, States.warpidle, "toIdle"),         // ワープ移動終了でワープ待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var attackidleTrigger = new[]                              /** 攻撃待機ステートのトリガー **/
            {
                (Triggers.Playerdead, States.idle, ""),                // プレイヤーが死亡で待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
                (Triggers.Attack1start, States.pointermissilebefore, "MissileStart"), // ポインターミサイルへ 
                (Triggers.Attack2start, States.crosswave, ""),              // クロスウェーブへ
                (Triggers.Attack3start, States.warpShot, ""),               // ワープショットへ
                (Triggers.Attack4start, States.flashBeamSword, ""),         // フラッシュビームソードへ
            }; 
            var beforepointermissileTrigger = new[]                    /** ポインターミサイル直前ステートのトリガー **/
            {
                (Triggers.Attack1, States.pointermissile, "toMissile"), // ポインターミサイルへ       
                (Triggers.Died, States.dead, ""),                      // 死亡で死へ
            };
            var pointermissileTrigger = new[]                          /** ポインターミサイルステートのトリガー **/
            {
                (Triggers.Attack1end, States.warpidle, "toIdle"),      // ポインターミサイル終了で攻撃待機へ       
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
            var flashBeamSwordTrigger = new[]                          /** フラッシュビームソードステートのトリガー **/                 
            {
                (Triggers.Attack4end, States.attackidle, ""),          // フラッシュビームソード終了で攻撃待機へ
                (Triggers.Died, States.dead, ""),                      // 死亡で待機
            };


            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.attackidle, attackidleTrigger)
                .AddTransitions(States.warpidle, warpidleTrigger)
                .AddTransitions(States.beforewarp, beforewarpTrigger)
                .AddTransitions(States.warp, warpTrigger)
                .AddTransitions(States.pointermissilebefore, beforepointermissileTrigger)
                .AddTransitions(States.pointermissile, pointermissileTrigger)
                .AddTransitions(States.crosswave, crosswaveTrigger)
                .AddTransitions(States.warpShot, warpShotTrigger)
                .AddTransitions(States.flashBeamSword, flashBeamSwordTrigger);

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
            warpidle.OnCompleted += WarpIdleStay;
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

            var pointermissilebefore = new Idle_LazyChange(Triggers.Attack1.ToString(), _MissileFallTime);
            _stateMachine.AddState(States.pointermissilebefore, pointermissilebefore);

            /* ポインターミサイル */
            pointermissile = new ShootMultiplePositions(_MissileBulletDate, _AttackTargetLayer).SetFiring(_MissileFallPoint, direction);
            pointermissile.onShootComplete.AddListener(MissileEnter);
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