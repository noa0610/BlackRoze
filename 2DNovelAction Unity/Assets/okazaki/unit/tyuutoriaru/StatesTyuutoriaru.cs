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
                (Triggers.Died, States.dead, ""),
                (Triggers.HalfHP, States.stun,"")                // HPが半分以下でショックウェーブへ
            };
            // States.attackidle
            var attackidleTrigger = new[]
            {
                (Triggers.Attack2, States.lasershot),
                (Triggers.Attack1, States.beamswordattackmove),
                (Triggers.Died, States.dead),
                (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
            };
            // States.lasershot
            var lasershotTrigger = new[]
            {
                (Triggers.Attack1end, States.attackidle),
                (Triggers.Died, States.dead),
                (Triggers.HalfHP, States.stun)
            };
            // States.beamswordattack
            var beamswordattackmoveTrigger = new[]
            {
                (Triggers.moveend, States.beamswordattack),
                (Triggers.Died, States.dead),
                (Triggers.HalfHP, States.stun)
            };
            // States.beamswordattackmove
            var beamswordattackTrigger = new[]
            {
                (Triggers.Attack2end, States.attackidle),
                (Triggers.Died, States.dead),
                (Triggers.HalfHP, States.stun)
            };
            // States.fixedpositionjump
            var fixedpositionjumpTrigger = new[]
            {
                (Triggers.Landing, States.attackidle),
                (Triggers.Died, States.dead),
                (Triggers.HalfHP, States.stun)
            };
            // States.stun
            var stunTrigger = new[]
            {
                (Triggers.Landing, States.attackidle),
                (Triggers.Died, States.dead),
                (Triggers.HalfHP, States.stun)
            };
            // States.shockwave
            var shockwaveTrigger = new[]
            {
                (Triggers.Shockwaveend, States.attackidle),
                (Triggers.Died, States.dead),
                (Triggers.HalfHP, States.stun)
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
            var lasershot = new LaserShot(_RB2, _firePoints, _bulletPrefab);
            _stateMachine.AddState(States.lasershot, lasershot);
            // ビームソード攻撃
            var beamswordattack = new ShootForward(_beamswordBulletData, _beamswordTargetLayer)
            .SetDirection(Vector2.down)
            .SetMuzzle(_swordfirePoints.Length > 0 ? _swordfirePoints[0].gameObject : gameObject    );
            _stateMachine.AddState(States.beamswordattack, beamswordattack);
            // ビームソード攻撃移動
            var freeMove = new FreeMove()
            .SetAccel(80f)
            .SetDecel(40f)
            .SetDeadZone(0.01f);
            _stateMachine.AddState(States.beamswordattackmove, freeMove);
            // ジャンプ
            // ジャンプ
            var jumpPositions = _junpPositions.ConvertAll(pos => (Vector2)pos.transform.position);
            var fixedpositionjump = new PositionJump(jumpPositions, 10f);
                fixedpositionjump.OnArrived += () =>
                {
                   _stateMachine.ChangeState(Triggers.Landing); // 例：Landingトリガーで遷移
                };
            _stateMachine.AddState(States.fixedpositionjump, fixedpositionjump);
            // スタン
            var stun = new Idle_LazyChange(Triggers.Event2.ToString(),  5, true);
            _stateMachine.AddState(States.stun, stun);
            // ショックウェーブ
            var shockwave = new ShootForward(_shockwaveBulletData, _shockwaveTargetLayer)
            .SetDirection(Vector2.left)
            .SetMuzzle(_swordfirePoints.Length > 0 ? _swordfirePoints[0].gameObject : gameObject);
            // 弾発射完了時にショックウェーブ終了トリガーを発火
            shockwave.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Shockwaveend);
            });
            _stateMachine.AddState(States.shockwave, shockwave);

        }
    }
}