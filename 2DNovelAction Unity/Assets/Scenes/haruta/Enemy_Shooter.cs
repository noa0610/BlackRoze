using Unity.VisualScripting;
using UnityEngine;

namespace BlackRose
{
    public class Enemy_Shooter : UnitBase
    {
        public enum States//状態
        {
            none,
            move, // 移動
            idle,// 待機
            shootReady,//攻撃待機
            KnockBack,//ノックバック
            dead,// 死亡
            shoot,// 攻撃
        }
        private enum Triggers//状態を遷移するためのトリガー
        {
            None,
            MissingPlayer, // プレイヤーを見失った
            FoundPlayer,   // プレイヤーを発見した
            Canshoot,      // ショットのクールタイムが明けた
            shoot,         // ショットを打った
            Damage,        //ダメージを受けた
            Died,          // 死亡した（HPが０になった）
            time,          //一定時間が経過した
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.shootReady),
                (Triggers.MissingPlayer, States.move),
                (Triggers.Damage, States.KnockBack)
            };
            var moveTrigger = new[]
            {
                (Triggers.FoundPlayer, States.shootReady),
                (Triggers.Damage, States.KnockBack)
            };
            var shootReadyTrigger = new[]
            {
                (Triggers.Canshoot, States.shoot),
                (Triggers.Damage, States.KnockBack)
            };
            var shootTrigger = new[]
            {
                (Triggers.shoot, States.idle),
                (Triggers.Damage, States.KnockBack)
            };
            var knockBackTrigger = new[]
            {
                (Triggers.time, States.idle),
                (Triggers.Died, States.dead)
            };

            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.move, moveTrigger)
                .AddTransmissions(States.shoot, shootTrigger)
                .AddTransmissions(States.shootReady, shootReadyTrigger)
                .AddTransmissions(States.KnockBack, knockBackTrigger);
            // 死んだときに何もしないならDeadの設定はいらない
            _stateMachine.AddState(States.idle, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));//格ゲーのコマンドキャンセル的なものに近く、ある程度アイメーションが進めば途中でも条件が合えばステートを移行する的なものだったはず(0.0f~1.0fの間で指定)
            // 発射クールタイム
            _stateMachine.AddState(States.shootInterval, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));
            // 爆発
            // var  = new ShootForward(_bulletData, targetLayer);//shootForwardクラスのインスタンスを生成し変数shootに格納(_bulletDataやtargetLayerはinstanceに必要な引数)
            // shoot.onShootComplete.AsObservable().Subscribe( => OnShootComplete());//shootを打ち終わったことを受け取り、subscribe（）内のメソッドを呼び出す処理
            // shoot.SetBullet(_bulletData);//shootのインスタンスの際に_bulletDataを渡す処理（追加設定や再設定がいる場合に改めて情報を渡すため
            _stateMachine.AddState(States.shoot, shoot);//ステートマシーンに新しいステートを登録するための処理



            // 死亡
            _stateMachine.AddState(States.dead, new Idle());
        }
    }
}
