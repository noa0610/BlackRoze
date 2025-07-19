namespace BlackRose
{
    public partial class Enemy_Girder
    {
        public enum States {
            None,
            shield_Idle,// 待機
            WaitingForAnAttack,// 攻撃待機
            Attack,// 攻撃
            Stan,// スタン
            ShieldTackle,// シールドタックル
            ShoulderGrenade,//ショルダーグレネード
            Dead// 死亡
        }
        public enum Triggers
        {
            none,
            FoundPlayer,// プレイヤーを見つけた
            MissingPlayer,// プレイヤーを見失った
            Interval,// 待機時間経過
            CloseDistance,// 近い距離
            LongDistance,// 遠い距離
            AfterTransition,// 遷移後
            Attack,// 攻撃
            ShieldReceivedHeavyModeAttack,//盾にHモードの攻撃を受けた
            Dead// 死亡
        }
        protected override void RegisterStats()
        {
            //トランスミッショングループを作成
            var shield_IdleTrigger = new[]//#Y盾待機
            {
            (Triggers.FoundPlayer, States.WaitingForAnAttack),
            (Triggers.ShieldReceivedHeavyModeAttack, States.Stan),
            (Triggers.Dead, States.Dead)
        };
            var WaitingForAnAttackTrigger = new[]//#Y攻撃待機
            {
            (Triggers.Interval, States.Attack),
            (Triggers.ShieldReceivedHeavyModeAttack, States.Stan),
            (Triggers.Dead, States.Dead)
        };
            var StanTrigger = new[]//#Yスタン
            {
            (Triggers.AfterTransition, States.shield_Idle),
            (Triggers.Dead, States.Dead)
        };
            var AttackTrigger = new[]//#Y攻撃
            {
            (Triggers.CloseDistance, States.ShieldTackle),
            (Triggers.LongDistance, States.ShoulderGrenade),
            (Triggers.Dead, States.Dead)
        };
            var ShoulderGrenadeTrigger = new[]//#Yショルダーグレネード
        {
            (Triggers.Attack, States.WaitingForAnAttack),
            (Triggers.ShieldReceivedHeavyModeAttack, States.Stan),
            (Triggers.Dead, States.Dead)
        };


            _stateMachine
                .AddTransmissions(States.shield_Idle, shield_IdleTrigger)
                .AddTransmissions(States.WaitingForAnAttack, WaitingForAnAttackTrigger)
                .AddTransmissions(States.Attack, AttackTrigger)
                .AddTransmissions(States.Stan, StanTrigger)
                .AddTransmissions(States.ShieldTackle, ShoulderGrenadeTrigger)
                .AddTransmissions(States.ShoulderGrenade, ShoulderGrenadeTrigger);

            // 待機
            _stateMachine.AddState(States.shield_Idle, new Idle().SetAnimeTrigger("shield_Idle").SetCancelableProgress(0));
            _stateMachine.AddState(States.WaitingForAnAttack, new Idle_LazyChange(Triggers.Interval.ToString(),_AttackInterval).SetAnimeTrigger("shield_Idle").SetCancelableProgress(0));
            _stateMachine.AddState(States.Attack, new Idle().SetAnimeTrigger("Attack").SetCancelableProgress(0));
            _stateMachine.AddState(States.Stan, new Idle_LazyChange(Triggers.AfterTransition.ToString(),_AfterTransitionInterval).SetAnimeTrigger("Stan").SetCancelableProgress(0));
            _stateMachine.AddState(States.ShieldTackle, new Idle().SetAnimeTrigger("ShieldTackle").SetCancelableProgress(0));
            _stateMachine.AddState(States.ShoulderGrenade, new Idle().SetAnimeTrigger("ShoulderGrenade").SetCancelableProgress(0));

            //     // 発射
            //     var shoot = new ShootForward(_bulletData, targetLayer);
            //     shoot.onShootComplete.AsObservable().Subscribe( => OnShootComplete());
            //     shoot.SetBullet(_bulletData);
            //     _stateMachine.AddState(States.shoot, shoot);

            //     // 発射クールタイム
            //     _stateMachine.AddState(States.shootInterval, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));

            //     // 死亡
                _stateMachine.AddState(States.Dead, new Idle());
            // }

            // protected override void RegisterData()
            // {
            //     _bulletData = new BulletData
            //     {
            //         bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullets/EnemyBullet_SmallInsect"),
            //         speed = 10f,
            //         damage = 1,
            //         lifeTime = 3f,
            //         targetLayer = LayerMask.GetMask("Player")
            //     };
        }
    }
}