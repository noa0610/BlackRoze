using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public partial class Enemy_huyuu
    {
        [SerializeField] private float ExplosionDelay = 0f;
        public enum States
        {
            none,
            move, // 移動
            idle,// 待機
            dead,// 死亡
            explosion,// 爆発
        }
        private enum Triggers
        {
            None,
            MissingPlayer, // プレイヤーを見失った
            FoundPlayer,   // プレイヤーを発見した
            AttackRange,   // 攻撃範囲に入った
            Explosion,     // 爆発した
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.move, "Contact"),
                (Triggers.Died, States.dead, "")
            };
            var moveTrigger = new[]
            {
                (Triggers.MissingPlayer, States.idle,""),
                (Triggers.AttackRange, States.explosion,"Explosion"),
                (Triggers.Died, States.dead,"")
            };
            var explosionTrigger = new[]
            {
                (Triggers.Explosion, States.dead, ""),
                (Triggers.Died, States.dead,"")
            };
            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.move, moveTrigger)
                .AddTransitions(States.explosion, explosionTrigger);
            // 死んだときに何もしないならDeadの設定はいらない

            // 待機
            _stateMachine.AddState(States.idle, new Idle());

            // 移動
            var move = new FreeMove(true);
            move.SetAccel(30.0f);
            move.SetDecel(20.0f);
            _stateMachine.AddState(States.move, move);

            //爆発
            // 爆発イベント（OnExplode）で死亡トリガーを発火
            _suicideBombing.SetDelay(ExplosionDelay);
            _suicideBombing.OnCompleted += () =>
            {
                Debug.Log("Enemy_huyuu: 爆発アニメーションが完了しました。");
                _stateMachine.LazyChange(Triggers.Died);
            };

            // ステートマシンに登録
            _stateMachine.AddState(States.explosion, _suicideBombing);
            // 死亡
            var died = new Idle();
            died.OnAnimationCompleted.AddListener(() =>
            {
                Debug.Log("Enemy_huyuu: 死亡アニメーションが完了しました。");
                UnitManager.instance.RemoveUnit(this);
                Destroy(gameObject);
            });
            _stateMachine.AddState(States.dead, died);
        }
        // Update is called once per frame
    }
}
