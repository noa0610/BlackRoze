using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
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
            lasershotidle, // レーザー攻撃待機
            beamswordattack, // ビームソード攻撃
            beamswordattackmove, // ビームソード攻撃移動
            fixedpositionjump, // ジャンプ
            stun, // スタン
            shockwave, // ショックウェーブ
            shockwaveidle, // ショックウェーブ待機
            shockwaveanimaidle, // ショックウェーブアニメ待機   
        }
        private enum Triggers
        {
            None,
            FoundPlayer,   // プレイヤーを発見した
            Attack1, // 攻撃１
            Attack2, // 攻撃２
            Attack1end, // 攻撃1した
            Attack1loop, // ループ完了
            moveend, // 移動した
            Attack2end, // 攻撃1した
            Shockwaveend, // ショックウェーブした
            HalfHP, // HPが半分以下
            Landing, // 着地
            Event1, // イベント1発生
            Event2, // イベント2発生
            shockwaveidleend, // ショックウェーブ待機終了
            shockwaveanimaidleend, // ショックウェーブアニメ待機終了
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
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
                (Triggers.Attack1, States.lasershot, "toShot_Medium"),
                (Triggers.Attack2, States.beamswordattackmove,"toMove"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")                // HPが半分以下でショックウェーブへ
            };
            // States.lasershot
            var lasershotTrigger = new[]
            {
                (Triggers.Attack1end, States.attackidle,"toIdle"),
                (Triggers.Attack1loop, States.lasershotidle,""),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            var lasershotidleTrigger = new[]
            {
                (Triggers.Attack1, States.lasershot,""),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.beamswordattack
            var beamswordattackmoveTrigger = new[]
            {
                (Triggers.moveend, States.beamswordattack,"toSlash"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.beamswordattackmove
            var beamswordattackTrigger = new[]
            {
                (Triggers.Attack2end, States.fixedpositionjump,"toJump"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
            // States.fixedpositionjump
            var fixedpositionjumpTrigger = new[]
            {
                (Triggers.Landing, States.attackidle,"toIdle"),
                (Triggers.Died, States.dead,""),
                (Triggers.HalfHP, States.stun,"toStan")
            };
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
            _stateMachine
            .AddTransitions(States.idle, idleTrigger)
            .AddTransitions(States.attackidle, attackidleTrigger)
            .AddTransitions(States.lasershot, lasershotTrigger)
            .AddTransitions(States.lasershotidle, lasershotidleTrigger)
            .AddTransitions(States.beamswordattack, beamswordattackTrigger)
            .AddTransitions(States.beamswordattackmove, beamswordattackmoveTrigger)
            .AddTransitions(States.fixedpositionjump, fixedpositionjumpTrigger)
            .AddTransitions(States.stun, stunTrigger)
            .AddTransitions(States.shockwaveidle, shockwaveidleTrigger)
            .AddTransitions(States.shockwaveanimaidle, shockwaveanimaidleTrigger)
            .AddTransitions(States.shockwave, shockwaveTrigger);
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
            // インスタンスをフィールドに保持して、発射方向は実行時に設定する
            _lasershotState = new ShootForward(_LasershotbulletData, _LasershotTargetLayer);
            _lasershotState.SetGameObject(_Lasershotmuzzle != null ? _Lasershotmuzzle : gameObject);
            // 弾発射完了時にレーザー攻撃終了トリガーを発火
            _lasershotState.onShootComplete.AddListener(() =>
            {
                if (nowstate == 7)
                {
                    nowstate = 2;
                    // 最終状態なら攻撃終了へ
                    _stateMachine.LazyChange(Triggers.Attack1end);
                }
                else
                {
                    // 続けるなら Attack1loop を発火して lasershot に戻す（transmission で lasershot->lasershotidle に遷移）
                    _stateMachine.LazyChange(Triggers.Attack1loop);
                }
            });

            _stateMachine.AddState(States.lasershot, _lasershotState);
            var lasershotidle = new Idle_LazyEvent(1.0f);
            // 遅延完了時に呼びたい処理をOnCompletedで登録
            lasershotidle.OnCompleted += () =>
            {
                if (_isAnimating) return; // 既に開始済みなら無視
                _isAnimating = true;
                // アニメ開始（トリガー送信）
                AnimaSelect();
                // 非同期でアニメ進行を監視して半分になったら Attack1 を呼ぶ（fire-and-forget）
                _ = WaitAndCallAttack1();
            };

            _stateMachine.AddState(States.lasershotidle, lasershotidle);
            // ビームソード攻撃移動
            var freeMove = new FreeMove(true);
            freeMove.SetAccel(30.0f);
            freeMove.SetDecel(20f);
            _stateMachine.AddState(States.beamswordattackmove, freeMove);
            // ビームソード攻撃
            var beamswordattack = new ShootForward(_beamswordBulletData, _beamswordTargetLayer)
            .SetDirection(Vector2.down);
            beamswordattack.SetGameObject(_beamswordmuzzle != null ? _beamswordmuzzle : gameObject);
            beamswordattack.onShootComplete.AddListener(() =>
            {
                // アニメの完了を待ってから Attack2end を発火
                StartCoroutine(WaitForBeamswordAnimationThenFire());
                // 当たり判定をトリガーに切り替え・ジャンプ先セット

            });

            _stateMachine.AddState(States.beamswordattack, beamswordattack);
            // ジャンプ
            _positionJump.SetPositions(_junpPositions.ConvertAll(p => (Vector2)p.transform.position));
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
            // スタン
            var stun = new Idle_LazyChange(Triggers.Event2.ToString(), 1.3f, true);
            _stateMachine.AddState(States.stun, stun);
            // ショックウェーブ待機
            var shockwaveidle = new Idle_LazyChange(Triggers.shockwaveidleend.ToString(), 2.6f, true);
            _stateMachine.AddState(States.shockwaveidle, shockwaveidle);
            // ショックウェーブアニメ待機
            var shockwaveanimaidle = new Idle_LazyChange(Triggers.shockwaveanimaidleend.ToString(), 1.8f, true);
            _stateMachine.AddState(States.shockwaveanimaidle, shockwaveanimaidle);
            // ショックウェーブ
            var shockwave = new ShootForward(_shockwaveBulletData, _shockwaveTargetLayer)
            .SetDirection(Vector2.left);
            shockwave.SetGameObject(_Lasershotmuzzle != null ? _Lasershotmuzzle : gameObject);
            // 弾発射完了時にショックウェーブ終了トリガーを発火
            shockwave.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Shockwaveend);
                IsInvincible = false;
            });
            _stateMachine.AddState(States.shockwave, shockwave);

        }
    }
}