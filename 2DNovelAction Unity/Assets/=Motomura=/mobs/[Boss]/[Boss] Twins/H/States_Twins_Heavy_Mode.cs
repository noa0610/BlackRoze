namespace BlackRose
{
    public partial class Boss_Twins_Heavy_Mode
    {
        public enum States
        {
            None,
            Idle,// 待機
            PartnerWaiting,// パートナー待機
            DiagonalMove,// 対角移動
            Random,// ランダム
            Shot_1,// ブラストショット（溜め）
            Shot_2,// ブラストショット（発射）
            Burst_1,//カウンターバースト（防御）
            Burst_2,//カウンターバースト（反撃）
            Laser_1,//ギガレーザー（溜め）
            Laser_2,//ギガレーザー（攻撃）
            Stan,// スタン
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
            Waiting_to_attack,// 攻撃待機
            Missing_cursor, // 命中なし
            The_player_attack_hits,// プレイヤーの攻撃がヒット
            Special_Attacks, // 特殊攻撃
            After_Counter_Burst, // カウンターバースト後
            After_the_blast_shot, // ブラストショット後
            After_Stun, // スタン後
            Wait_for_n_seconds, // n秒待機
            After_Giga_Laser, // ギガレーザー後
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
            var DiagonalMove_Trigger = new[]//#Y対角移動
        {
            (Triggers.Moved_n_times, States.Random),
            (Triggers.Dead, States.Dead)
        };
            var Random_Trigger = new[]//#YRandom
        {
            (Triggers.Attack_1, States.Shot_1),
            (Triggers.Attack_2, States.Burst_1),
            (Triggers.Attack_3, States.Laser_1),
            (Triggers.Dead, States.Dead)
        };
            var Shot_1_Trigger = new[]//#Yブラストショット（溜め）
            {
            (Triggers.Waiting_to_attack, States.Shot_2),
            (Triggers.Dead, States.Dead)
        };
            var Shot_2_Trigger = new[]//#Yブラストショット（発射）
            {
            (Triggers.After_the_blast_shot, States.PartnerWaiting),
            (Triggers.Dead, States.Dead)
        };
            var Burst_1_Trigger = new[]//##Yカウンターバースト（防御）
            {
                (Triggers.Missing_cursor, States.PartnerWaiting),
                (Triggers.The_player_attack_hits, States.Burst_2),
                (Triggers.Special_Attacks, States.Stan),
                (Triggers.Dead, States.Dead)
            };
            var Burst_2_Trigger = new[]//##Yカウンターバースト（反撃）
            {
                (Triggers.After_Counter_Burst, States.PartnerWaiting),
                (Triggers.Dead, States.Dead)
            };
            var Stan_Trigger = new[]//#Yスタン
            {
                (Triggers.After_Stun, States.PartnerWaiting),
                (Triggers.Dead, States.Dead)
            };
            var Laser_1_Trigger = new[]//#Yギガレーザー（溜め）
            {
            (Triggers.Waiting_to_attack, States.Laser_2),
            (Triggers.Dead, States.Dead)
            };
            var Laser_2_Trigger = new[]//#Yギガレーザー（攻撃）
            {
            (Triggers.After_Giga_Laser, States.PartnerWaiting),
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
