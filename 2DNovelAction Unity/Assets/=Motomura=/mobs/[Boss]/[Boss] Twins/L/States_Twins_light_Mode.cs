namespace BlackRose
{
    public partial class Boss_Twins_light_Mode
    {
        public enum States
        {
            None,
            Idle,// 待機
            PartnerWaiting,// パートナー待機
            DiagonalMove,// 対角移動
            Random,// ランダム
            Continuous_Shot,// 連続ショット
            LockOn_Missile_1,// ロックオンミサイル溜め
            LockOn_Missile_2,// ロックオンミサイル発射
            First_high_speed_tackle, // 1回目高速タックル
            Second_high_speed_tackle, // 2回目高速タックル
            Third_high_speed_tackle, // 3回目高速タックル

            Dead// 死亡
        }
        public enum Triggers 
        {
            None,
            ConversationEnded,// 会話終了
            DefeatedAPlayer,// プレイヤーを倒した
            My_partner_is_now_on_standby,// パートナーが待機中
            There_is_no_partner,// パートナーがいない
            Moved_n_times,// n回移動
            Attack_1,// 攻撃1
            Attack_2,// 攻撃2
            Attack_3,// 攻撃3
            After_continuous_shots,// 連続ショット後
            Missing_cursor, // 命中なし
            Hit_the_player,// プレイヤーにヒット
            After_the_lockOn_missile_ends, // ロックオンミサイル終了後
            After_the_First_high_speed_tackle, // 1回目高速タックル後
            After_the_Second_high_speed_tackle, // 2回目高速タックル後
            After_a_high_speed_tackle,// 高速タックル後
            Dead// 死亡
        }
        protected override void RegisterStats()//TODO 次回、登録
        {
            //トランスミッショングループを作成
            var Idle_Trigger = new[]//#Y待機
            {
            (Triggers.ConversationEnded, States.PartnerWaiting),
            (Triggers.Dead, States.Dead)
        };
            var PartnerWaiting_Trigger = new[]//#Yパートナー待機
            {
            (Triggers.DefeatedAPlayer, States.Idle),
            (Triggers.There_is_no_partner, States.DiagonalMove),
            (Triggers.Moved_n_times, States.Random),
            (Triggers.Dead, States.Dead)
        };
            var Random_Trigger = new[]//#YRandom
            {
            (Triggers.Attack_1, States.Continuous_Shot),
            (Triggers.Attack_2, States.LockOn_Missile_1),
            (Triggers.Attack_3, States.First_high_speed_tackle),
            (Triggers.Dead, States.Dead)
        };
            var Continuous_Shot_Trigger = new[]//#Y連続ショット
            {
            (Triggers.After_continuous_shots, States.PartnerWaiting),
            (Triggers.Dead, States.Dead)
        };
            var LockOn_Missile_1_Trigger = new[]//#Yロックオンミサイル(狙う)
        {
            (Triggers.Missing_cursor, States.PartnerWaiting),
            (Triggers.Hit_the_player, States.LockOn_Missile_2),
            (Triggers.Dead, States.Dead)
        };
            var LockOn_Missile_2_Trigger = new[]//#Yロックオンミサイル(発射)
        {
            (Triggers.After_the_lockOn_missile_ends, States.PartnerWaiting),
            (Triggers.Dead, States.Dead)
        };
            var First_high_speed_tackle_Trigger = new[]//#Y1回目高速タックル
        {
            (Triggers.After_the_First_high_speed_tackle, States.Second_high_speed_tackle),
            (Triggers.Dead, States.Dead)
        };
            var Second_high_speed_tackle_Trigger = new[]//#Y2回目高速タックル
        {
            (Triggers.After_the_Second_high_speed_tackle, States.Third_high_speed_tackle),
            (Triggers.Dead, States.Dead)
        };
            var Third_high_speed_tackle_Trigger = new[]//#Y3回目高速タックル
        {
            (Triggers.After_a_high_speed_tackle, States.PartnerWaiting),
            (Triggers.Dead, States.Dead)
        };
        //     _stateMachine
            //         .AddTransmissions(States.shield_Idle, shield_IdleTrigger)
            //         .AddTransmissions(States.WaitingForAnAttack, WaitingForAnAttackTrigger)
            //         .AddTransmissions(States.Attack, AttackTrigger)
            //         .AddTransmissions(States.Stan, StanTrigger)
            //         .AddTransmissions(States.ShieldTackle, ShoulderGrenadeTrigger)
            //         .AddTransmissions(States.ShoulderGrenade, ShoulderGrenadeTrigger);

            //     // 待機
            //     _stateMachine.AddState(States.shield_Idle, new Idle().SetAnimeTrigger("shield_Idle").SetCancelableProgress(0));
            //     _stateMachine.AddState(States.WaitingForAnAttack, new Idle_LazyChange(Triggers.Interval.ToString(),_AttackInterval).SetAnimeTrigger("shield_Idle").SetCancelableProgress(0));
            //     _stateMachine.AddState(States.Attack, new Idle().SetAnimeTrigger("Attack").SetCancelableProgress(0));
            //     _stateMachine.AddState(States.Stan, new Idle_LazyChange(Triggers.AfterTransition.ToString(),_AfterTransitionInterval).SetAnimeTrigger("Stan").SetCancelableProgress(0));
            //     _stateMachine.AddState(States.ShieldTackle, new Idle().SetAnimeTrigger("ShieldTackle").SetCancelableProgress(0));
            //     _stateMachine.AddState(States.ShoulderGrenade, new Idle().SetAnimeTrigger("ShoulderGrenade").SetCancelableProgress(0));

            //     //     // 発射
            //     //     var shoot = new ShootForward(_bulletData, targetLayer);
            //     //     shoot.onShootComplete.AsObservable().Subscribe( => OnShootComplete());
            //     //     shoot.SetBullet(_bulletData);
            //     //     _stateMachine.AddState(States.shoot, shoot);

            //     //     // 発射クールタイム
            //     //     _stateMachine.AddState(States.shootInterval, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));

            //     //     // 死亡
            //         _stateMachine.AddState(States.Dead, new Idle());
            //     // }

            //     // protected override void RegisterData()
            //     // {
            //     //     _bulletData = new BulletData
            //     //     {
            //     //         bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullets/EnemyBullet_SmallInsect"),
            //     //         speed = 10f,
            //     //         damage = 1,
            //     //         lifeTime = 3f,
            //     //         targetLayer = LayerMask.GetMask("Player")
            //     //     };
        }
    }
}