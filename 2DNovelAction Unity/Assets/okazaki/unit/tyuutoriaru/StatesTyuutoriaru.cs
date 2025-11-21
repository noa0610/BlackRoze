using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_tyuutoriaru
    {
        private PositionJump _positionJump;
        private ShootForward _lasershotState;

        private enum States
        {
            none,
            entry,               // 登場
            idle,                // 待機
            dead,                // 死亡
            attackidle,          // 攻撃待機
            lasershot_Start,     // レーザーショット開始
            lasershot_before,    // レーザーショット直前
            lasershot,           // レーザーショット
            lasershot_idle,      // レーザーショット待機
            beamsword_Start,     // ビームソード直前
            beamsword_move,      // ビームソード攻撃移動
            beamsword,           // ビームソード攻撃
            fixedpositionjump,   // ジャンプ
            stun,                // スタン
            shockwave,           // ショックウェーブ
            shockwaveidle,       // ショックウェーブ待機
            shockwaveanimaidle,  // ショックウェーブアニメ待機   
        }
        private enum Triggers
        {
            None,
            FoundPlayer,           // プレイヤーを発見した
            Attack1start,          // 攻撃１開始
            Attack2start,          // 攻撃２開始
            Attack1,               // 攻撃１
            Attack2,               // 攻撃２
            Attack1loop,           // 攻撃１継続
            movestart,             // 移動開始
            moveend,               // 移動した
            Attack1end,            // 攻撃１した
            Attack2end,            // 攻撃２した
            Shockwaveend,          // ショックウェーブした
            HalfHP,                // HPが半分以下
            Landing,               // 着地
            shockwaveidleend,      // ショックウェーブ待機終了
            shockwaveanimaidleend, // ショックウェーブアニメ待機終了
            EntryEnd,              // 登場終了
            Event1,                // イベント1発生
            Event2,                // イベント2発生
            Playerdead,            // プレイヤーが死亡
            Died,                  // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            #region === Basic Triggers ===
            // States.entry
            var entryTrigger = new[]                                   /** 登場ステートのトリガー **/
            {
                (Triggers.EntryEnd, States.idle ,"EntryEnd"),          // イベント1発生でワープ待機へ
            };
            // States.idle
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.attackidle, ""),
                (Triggers.Died, States.dead, ""),
                (Triggers.HalfHP, States.stun,"toStan")                // HPが半分以下でショックウェーブへ
            };
            // States.attackidle
            var attackidleTrigger = new[]
            {
                (Triggers.Attack1start, States.lasershot_Start,"toShot_Medium"),
                (Triggers.Attack2start, States.beamsword_Start,"SwordStart"),
                (Triggers.Died, States.dead,""),
                (Triggers.Playerdead, States.idle,""),
                (Triggers.HalfHP, States.stun,"toStan")                // HPが半分以下でショックウェーブへ
            };
            #endregion

            #region === LaserShot Triggers ===
            // States.lasershot_Start
            var lasershotStartTrigger = new[]
            {
                (Triggers.Attack1, States.lasershot_before,""),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.lasershot_before
            var lasershotbeforeTrigger = new[]
            {
                (Triggers.Attack1, States.lasershot,"toShot"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.lasershot
            var lasershotTrigger = new[]
            {
                (Triggers.Attack1end, States.attackidle,"toIdle"),
                (Triggers.Attack1loop, States.lasershot_idle,""),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.lasershot_Idle
            var lasershotidleTrigger = new[]
            {
                (Triggers.Attack1, States.lasershot_before,""),            // 固有処理でトリガーをセット
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            #endregion

            #region === BeamSword Triggers ===
            // States.beamsword_Start
            var beamswordStartTrigger = new[]
            {
                (Triggers.movestart, States.beamsword,"toSlash"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.beamsword_Move
            var beamswordmoveTrigger = new[]
            {
                (Triggers.moveend, States.beamsword,"toMove"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.beamsword
            var beamswordTrigger = new[]
            {
                (Triggers.Attack2end, States.fixedpositionjump,"toJump"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            #endregion

            #region === Jump Triggers ===
            // States.fixedpositionjump
            var fixedpositionjumpTrigger = new[]
            {
                (Triggers.Landing, States.attackidle,"toIdle"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            #endregion

            #region === ShockWave Triggers ===
            // States.stun
            var stunTrigger = new[]
            {
                (Triggers.Event2, States.shockwaveidle,"toIdle"),
                (Triggers.Died, States.dead,""),
            };
            // States.shockwaveidle
            var shockwaveidleTrigger = new[]
            {
                (Triggers.shockwaveidleend, States.shockwaveanimaidle,"toShockWave"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.shockwaveanimaidle
            var shockwaveanimaidleTrigger = new[]
            {
                (Triggers.shockwaveanimaidleend, States.shockwave,""),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.shockwave
            var shockwaveTrigger = new[]
            {
                (Triggers.Shockwaveend, States.idle,"toIdle"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            #endregion

            _stateMachine
            .AddTransitions(States.entry, entryTrigger)
            .AddTransitions(States.idle, idleTrigger)
            .AddTransitions(States.attackidle, attackidleTrigger)
            .AddTransitions(States.lasershot_Start, lasershotStartTrigger)
            .AddTransitions(States.lasershot_before, lasershotbeforeTrigger)
            .AddTransitions(States.lasershot, lasershotTrigger)
            .AddTransitions(States.lasershot_idle, lasershotidleTrigger)
            .AddTransitions(States.beamsword_Start, beamswordStartTrigger)
            .AddTransitions(States.beamsword, beamswordTrigger)
            .AddTransitions(States.beamsword_move, beamswordmoveTrigger)
            .AddTransitions(States.fixedpositionjump, fixedpositionjumpTrigger)
            .AddTransitions(States.stun, stunTrigger)
            .AddTransitions(States.shockwaveidle, shockwaveidleTrigger)
            .AddTransitions(States.shockwaveanimaidle, shockwaveanimaidleTrigger)
            .AddTransitions(States.shockwave, shockwaveTrigger);

            #region === Basic States ===

            /* 登場演出 */
            var entry = new Idle_LazyChange(Triggers.EntryEnd.ToString(), _EntryEndwaitTime);
            _stateMachine.AddState(States.entry, entry);

            /* 待機 */
            _stateMachine.AddState(States.idle, new Idle());

            /* 死亡 */
            _stateMachine.AddState(States.dead, new Idle());

            /* 攻撃待機 */
            var attackIdle = new Idle_LazyEvent(_AttackIntervalTime);
            attackIdle.OnCompleted += Attackselect;
            _stateMachine.AddState(States.attackidle, attackIdle);
            #endregion
            
            #region === LaserShot States ===

            /* レーザーショット開始 */
            var lasershotStart = new Idle_LazyChange(Triggers.Attack1.ToString(), _LaserShotStartTime);
            lasershotStart.OnCompleted += LaserShotStart;
            _stateMachine.AddState(States.lasershot_Start, lasershotStart);

            /* レーザーショット直前 */
            var lasershotbefore = new Idle_LazyChange(Triggers.Attack1.ToString(), _LaserShotbeforeTime);
            lasershotbefore.OnCompleted += LaserShotBefore;
            _stateMachine.AddState(States.lasershot_before, lasershotbefore);

            /* レーザーショット */
            _lasershotState = new ShootForward(_LasershotbulletData, AttackLayer);
            _lasershotState.SetGameObject(_Lasershotmuzzle != null ? _Lasershotmuzzle : gameObject);
            _lasershotState.onShootComplete.AddListener(() =>
            {
                LaserShotExit();
            });
            _stateMachine.AddState(States.lasershot, _lasershotState);

            /* レーザーショット待機 */
            var lasershotidle = new Idle_LazyChange(Triggers.Attack1.ToString(), _LaserShotIntervalTime);
            lasershotidle.OnCompleted += () =>
            {
                AnimaSelect();
            };
            _stateMachine.AddState(States.lasershot_idle, lasershotidle);
            #endregion

            #region === BeamSword States ===

            /* ビームソード開始 */
            var beamswordstart = new Idle_LazyChange(Triggers.movestart.ToString(), 0.5f);
            _stateMachine.AddState(States.beamsword_Start, beamswordstart);

            /* ビームソード接近 */
            var freeMove = new FreeMove(true);
            freeMove.SetAccel(30.0f);
            freeMove.SetDecel(20f);
            _stateMachine.AddState(States.beamsword_move, freeMove);

            /* ビームソード */
            var beamswordattack = new ShootForward(_beamswordBulletData, AttackLayer)
            .SetDirection(Vector2.down);
            beamswordattack.SetGameObject(_beamswordmuzzle != null ? _beamswordmuzzle : gameObject);
            beamswordattack.onShootComplete.AddListener(() =>
            {
                // アニメの完了を待ってから Attack2end を発火
                StartCoroutine(WaitForBeamswordAnimationThenFire());
                // 当たり判定をトリガーに切り替え・ジャンプ先セット

            });
            _stateMachine.AddState(States.beamsword, beamswordattack);
            #endregion
   
            #region === Jump States ===

            /* ジャンプ */
            _positionJump = new PositionJump(_junpPositions.ConvertAll(p => (Vector2)p), _JumpSpeed);
            var fixedpositionjump = _positionJump;
            fixedpositionjump.SetRB2(_RB2);
            _positionJump.OnArrived += () =>
            {
                GetComponent<BoxCollider2D>().isTrigger = false;
                Debug.Log("ジャンプ到達コールバックが呼ばれました。");
                _stateMachine.LazyChange(Triggers.Landing);
                Debug.Log("固定位置ジャンプに到達しました。");
            };
            _stateMachine.AddState(States.fixedpositionjump, fixedpositionjump);
            #endregion
           
            #region === ShockWave States ===

            /* スタン */
            var stun = new Idle_LazyChange(Triggers.Event2.ToString(), 1.3f, true);
            _stateMachine.AddState(States.stun, stun);

            /* ショックウェーブ待機 */
            var shockwaveidle = new Idle_LazyChange(Triggers.shockwaveidleend.ToString(), 2.6f, true);
            _stateMachine.AddState(States.shockwaveidle, shockwaveidle);

            /* ショックウェーブアニメ待機 */
            var shockwaveanimaidle = new Idle_LazyChange(Triggers.shockwaveanimaidleend.ToString(), 1.8f, true);
            _stateMachine.AddState(States.shockwaveanimaidle, shockwaveanimaidle);
            
            /* ショックウェーブ */
            var shockwave = new ShootForward(_shockwaveBulletData, AttackLayer)
            .SetDirection(Vector2.left);
            shockwave.SetGameObject(_shockwaveshotmuzzle != null ? _shockwaveshotmuzzle : gameObject);
            // 弾発射完了時にショックウェーブ終了トリガーを発火
            shockwave.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Shockwaveend);
                IsInvincible = false;
            });
            _stateMachine.AddState(States.shockwave, shockwave);
            #endregion

        }
    }
}