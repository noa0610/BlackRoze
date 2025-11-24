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
        private Warp crosswavewarp;
        private Warp warpShotwarp;
        private ShootMultiplePositions pointermissile;
        private ShootCross crosswave;
        private ShootForward warpShot;
        private ShootForward flashBeamSword;
        private Idle dead;
        public enum States
        {
            none,
            entry,                  // 登場
            idle,                   // 待機
            dead,                   // 死亡
            warpidle,               // ワープ待機
            beforewarp,             // ワープ直前
            warp,                   // ワープ 
            attackidle,             // 攻撃待機
            pointermissile_before,  // ポインタミサイル直前
            pointermissile,         // ポインターミサイル
            crosswave_beforewarp,   // クロスウェーブ発動ワープ直前
            crosswave_warp,         // クロスウェーブ発動ワープ
            crosswave,              // クロスウェーブ
            crosswave_end,          // クロスウェーブ終了ワープ直前
            warpShot_beforewarp,    // ワープショット発動ワープ直前
            warpShot_warp,          // ショット直前ワープ
            warpShot,               // ワープショット
            warpShot_chain,         // ワープショット継続ワープ直前
            flashBeamSword_before,  // フラッシュビームソード開始
            flashBeamSword_dash,    // ダッシュ
            flashBeamSword,         // フラッシュビームソード
            flashBeamSword_end      // フラッシュビームソード終了
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
            Attack3chain,       // 攻撃３継続
            Attack4dash,        // 攻撃４ダッシュ
            Attack1end,         // 攻撃１した
            Attack2end,         // 攻撃２した
            Attack3end,         // 攻撃３した
            Attack4end,         // 攻撃４した
            EntryEnd,           // 登場終了
            Event1,             // イベント1が終わった
            Playerdead,         // プレイヤーが死亡
            Died,               // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            #region === Basic Triggers ===
            var entryTrigger = new[]                                   /** 登場ステートのトリガー **/
            {
                (Triggers.EntryEnd, States.warpidle ,"EntryEnd"),      // イベント1発生でワープ待機へ
            };
            var idleTrigger = new[]                                    /** 待機ステートのトリガー **/
            {
                (Triggers.Event1, States.warpidle ,"toIdle"),          // イベント1発生でワープ待機へ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var warpidleTrigger = new[]                                /** ワープ待機ステートのトリガー **/
            {
                (Triggers.Warpcooldown, States.beforewarp, "WarpStart"),// ワープクールダウンでワープ直前へ
                (Triggers.Warpcomplete, States.attackidle, ""),        // ワープ完了で攻撃待機へ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var beforewarpTrigger = new[]                              /** ワープ直前ステートのトリガー **/
            {
                (Triggers.Warp, States.warp, "toWarp"),                // ワープ開始でワープへ
                (Triggers.Died, States.dead, "toDead")                 // 死亡で死へ
            };
            var warpTrigger = new[]                                    /** ワープステートのトリガー **/
            {
                (Triggers.Warpend, States.warpidle, "toIdle"),         // ワープ移動終了でワープ待機へ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var attackidleTrigger = new[]                              /** 攻撃待機ステートのトリガー **/
            {
                (Triggers.Playerdead, States.idle, ""),                // プレイヤーが死亡で待機へ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
                (Triggers.Attack1start, States.pointermissile_before, "MissileStart"), // ポインターミサイル直前へ 
                (Triggers.Attack2start, States.crosswave_beforewarp, "WaveStart"),     // クロスウェーブ開始ワープ直前へ
                (Triggers.Attack3start, States.warpShot_beforewarp, "ShotStart"),      // ワープショット開始ワープ直前へ
                (Triggers.Attack4start, States.flashBeamSword_before, "SwordStart"),   // フラッシュビームソード直前へ
            }; 
            #endregion
            
            #region === PointerMissile Triggers ===
            var beforepointermissileTrigger = new[]                    /** ポインターミサイル直前ステートのトリガー **/
            {
                (Triggers.Attack1, States.pointermissile, "toMissile"),// ポインターミサイルへ       
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var pointermissileTrigger = new[]                          /** ポインターミサイルステートのトリガー **/
            {
                (Triggers.Attack1end, States.warpidle, "toIdle"),      // ポインターミサイル終了で攻撃待機へ       
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            #endregion

            #region === CrossWave Triggers ===
            var crosswavebeforewarpTrigger = new[]                     /** クロスウェーブ開始ワープ直前ステートのトリガー **/     
            {
                (Triggers.Warp, States.crosswave_warp, "WaveMiddle"),  // ワープでクロスウェーブ直前のワープへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var crosswavewarpTrigger = new[]                           /** クロスウェーブ直前ステートのトリガー **/     
            {
                (Triggers.Attack2, States.crosswave, "toWave"),        // 攻撃でクロスウェーブへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var crosswaveTrigger = new[]                               /** クロスウェーブステートのトリガー **/     
            {
                (Triggers.Attack2end, States.crosswave_end, "WaveEnd"),// クロスウェーブ終了で終了ワープへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var crosswaveendTrigger = new[]                            /** クロスウェーブ終了ワープ直前ステートのトリガー **/     
            {
                (Triggers.Warp, States.warp, "toWarp"),                // ワープでワープへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            #endregion

            #region === WarpShot Triggers ===
            var warpShotbeforewarpTrigger = new[]                      /** ワープショット開始ワープステートのトリガー **/
            {
                (Triggers.Warp, States.warpShot_warp, ""),             // ワープでショット直前ワープへ（トリガーの選択は固有処理で）
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var warpShotwarpTrigger = new[]                            /** ショット直前ワープステートのトリガー **/
            {
                (Triggers.Attack3, States.warpShot, "toShot"),         // 攻撃でショットへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var warpShotTrigger = new[]                                /** ショットステートのトリガー **/
            {
                (Triggers.Attack3end, States.warpShot_chain, "ShotMiddle"), // ショット終了で継続ワープへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var warpShotchainTrigger = new[]                           /** ワープショット継続ワープ直前ステートのトリガー **/
            {
                (Triggers.Attack3chain, States.warpShot_warp, ""),     // ワープショット継続でショット直前ワープへ（トリガーの選択は固有処理で）
                (Triggers.Warp, States.warp, "ShotEnd"),               // ワープでワープへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            #endregion

            #region === FlashBeamSword Triggers ===
            var flashBeamSwordStartTrigger = new[]                     /** フラッシュビームソード開始ステートのトリガー **/                 
            {
                (Triggers.Attack4dash, States.flashBeamSword_dash, "SwordDash"),// ダッシュでダッシュへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var flashBeamSworddashTrigger = new[]                      /** ダッシュステートのトリガー **/                 
            {
                (Triggers.Attack4, States.flashBeamSword, "toSword"),  // 攻撃でフラッシュビームソードへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var flashBeamSwordTrigger = new[]                          /** フラッシュビームソードステートのトリガー **/                 
            {
                (Triggers.Attack4end, States.flashBeamSword_end, "SwordEnd"),  // フラッシュビームソード終了で終了ステートへ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            var flashBeamSwordendTrigger = new[]                       /** フラッシュビームソード終了ステートのトリガー **/                 
            {
                (Triggers.Attack4end, States.warpidle, "toIdle"),      // フラッシュビームソード終了で攻撃待機へ
                (Triggers.Died, States.dead, "toDead"),                // 死亡で死へ
            };
            #endregion

            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransitions(States.entry, entryTrigger)
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.attackidle, attackidleTrigger)
                .AddTransitions(States.warpidle, warpidleTrigger)
                .AddTransitions(States.beforewarp, beforewarpTrigger)
                .AddTransitions(States.warp, warpTrigger)
                .AddTransitions(States.pointermissile_before, beforepointermissileTrigger)
                .AddTransitions(States.pointermissile, pointermissileTrigger)
                .AddTransitions(States.crosswave, crosswaveTrigger)
                .AddTransitions(States.crosswave_beforewarp, crosswavebeforewarpTrigger)
                .AddTransitions(States.crosswave_warp, crosswavewarpTrigger)
                .AddTransitions(States.crosswave_end, crosswaveendTrigger)
                .AddTransitions(States.warpShot_beforewarp, warpShotbeforewarpTrigger)
                .AddTransitions(States.warpShot_warp, warpShotwarpTrigger)
                .AddTransitions(States.warpShot, warpShotTrigger)
                .AddTransitions(States.warpShot_chain, warpShotchainTrigger)
                .AddTransitions(States.flashBeamSword_before, flashBeamSwordStartTrigger)
                .AddTransitions(States.flashBeamSword_dash, flashBeamSworddashTrigger)
                .AddTransitions(States.flashBeamSword, flashBeamSwordTrigger)
                .AddTransitions(States.flashBeamSword_end, flashBeamSwordendTrigger);

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

            #region === Warp States ===

            /* ワープ待機 */
            var warpidle = new Idle_LazyChange(Triggers.Warpcooldown.ToString(), _WarpIntervalTime, true);
            warpidle.OnCompleted += WarpIdleStay;
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
            #endregion

            #region === PointerMissile States ===

            /* ポインターミサイル直前 */
            var pointermissilebefore = new Idle_LazyChange(Triggers.Attack1.ToString(), _MissileFallTime);
            _stateMachine.AddState(States.pointermissile_before, pointermissilebefore);

            /* ポインターミサイル */
            pointermissile = new ShootMultiplePositions(_MissileBulletDate, AttackLayer).SetFiring(_MissileFallPoint, missiledirection);
            pointermissile.onShootComplete.AddListener(MissileEnter);
            _stateMachine.AddState(States.pointermissile, pointermissile);
            #endregion

            #region === CrossWave States ===

            /* クロスウェーブ開始ワープ直前 */
            var crosswavebeforewarp = new Idle();
            _stateMachine.AddState(States.crosswave_beforewarp, crosswavebeforewarp);

            /* クロスウェーブ直前 */
            crosswavewarp = new Warp();
            crosswavewarp.OnCompleted += WarpEnter;
            _stateMachine.AddState(States.crosswave_warp, crosswavewarp);

            /* クロスウェーブ */
            crosswave = new ShootCross(_CrossWaveBulletDate, AttackLayer).SetFiring(_CrossWaveSenterPoint);
            crosswave.onShootComplete.AddListener(CrossWaveEnter);
            _stateMachine.AddState(States.crosswave, crosswave);

            /* クロスウェーブ終了 */
            var crosswaveendbeforewarp = new Idle();
            _stateMachine.AddState(States.crosswave_end, crosswaveendbeforewarp);
            #endregion

            #region === WarpShot States ===

            /* ワープショット開始ワープ直前 */
            var warpShotbeforewarp = new Idle();
            _stateMachine.AddState(States.warpShot_beforewarp, warpShotbeforewarp);

            /* ショット直前ワープ */
            warpShotwarp = new Warp();
            warpShotwarp.OnCompleted += WarpEnter;
            _stateMachine.AddState(States.warpShot_warp, warpShotwarp);

            /* ショット */
            warpShot = new ShootForward(_ShotBulletDate, AttackLayer);
            warpShot.SetGameObject(_ShotPoint); 
            warpShot.onShootComplete.AddListener(WarpShotEnter);
            _stateMachine.AddState(States.warpShot, warpShot);

            /* ワープショット継続ワープ直前 */
            var warpShotchain = new Idle();
            _stateMachine.AddState(States.warpShot_chain, warpShotchain);
            #endregion

            #region === FlashBeamSword States ===
            
            /* フラッシュビームソード開始 */
            var flashBeamSwordbefore = new Idle_LazyChange(Triggers.Attack4dash.ToString(), _FlashBeamSwordStartDashTime);
            flashBeamSwordbefore.OnCompleted += FlashBeamSwordBeforeStay;
            _stateMachine.AddState(States.flashBeamSword_before, flashBeamSwordbefore);

            /* フラッシュビームソードダッシュ */
            var flashBeamSworddash = new MoveOnGround().SetAccel(_dashaccel).SetFriction(_dashfriction);
            _stateMachine.AddState(States.flashBeamSword_dash, flashBeamSworddash);

            /* フラッシュビームソード */
            flashBeamSword = new ShootForward(_FlashBeamSwordBulletDate, AttackLayer);
            flashBeamSword.SetGameObject(_FlashBeamSwordPoint);
            flashBeamSword.onShootComplete.AddListener(FlashBeamSwordEnter);
            _stateMachine.AddState(States.flashBeamSword, flashBeamSword);

            /* フラッシュビームソード終了 */
            var flashBeamSwordend = new Idle_LazyChange(Triggers.Attack4end.ToString(), _FlashBeamSwordEndTime);
            _stateMachine.AddState(States.flashBeamSword_end, flashBeamSwordend);
            #endregion
        }
    }
}