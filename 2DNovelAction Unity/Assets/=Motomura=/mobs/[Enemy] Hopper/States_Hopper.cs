namespace BlackRose
{
    public partial class Enemy_Hopper
    {
        public enum States {
            None,
            Idle,// 待機
            WaitingForEnemyContact,// 敵接待機
            WaitingForAnAttack,// 攻撃待機
            MoveJump,//ジャンプ
            SmallJump,// 小ジャンプ
            AttackJump,//ジャンプ体当たり
            Dead// 死亡
        }
        public enum Triggers
        {
            none,
            FoundPlayer,// プレイヤーを見つけた
            MissingPlayer,// プレイヤーを見失った
            Landing,// 着地した
            CloseDistance,// 近い距離
            LongDistance,// 遠い距離
            IdleInterval,// 待機時間経過
            approachInterval,// 接近時間経過
            AttackInterval,// 攻撃時間経過
            Dead
        }
        protected override void RegisterStats()
        {
            //トランスミッショングループを作成
            var idleTrigger = new[]//#Y待機
            {
            (Triggers.FoundPlayer, States.Idle),
            (Triggers.IdleInterval, States.MoveJump),
            (Triggers.Dead, States.Dead)
        };
            var MoveJumpTrigger = new[]//#Yジャンプ
            {
            (Triggers.Landing, States.Idle),
            (Triggers.Dead, States.Dead)
        };
            var WaitingForEnemyContactTrigger = new[]//#Y敵接待機
            {
            (Triggers.approachInterval, States.SmallJump),
            (Triggers.CloseDistance, States.WaitingForAnAttack),
            (Triggers.Dead, States.Dead)
        };
            var WaitingForAnAttackTrigger = new[]//#Y攻撃待機
            {
            (Triggers.AttackInterval, States.AttackJump),
            (Triggers.IdleInterval, States.WaitingForAnAttack),
            (Triggers.Dead, States.Dead)
        };
            var SmallJumpTrigger = new[]//#Y接近小ジャンプ
            {
            (Triggers.Landing, States.WaitingForEnemyContact),
            (Triggers.Dead, States.Dead)
        };
            var AttackJumpTrigger = new[]//#Yジャンプ体当たり
            {
            (Triggers.Landing, States.WaitingForAnAttack),
            (Triggers.Dead, States.Dead)
        };


            _stateMachine
                .AddTransmissions(States.Idle, idleTrigger)
                .AddTransmissions(States.MoveJump, MoveJumpTrigger)
                .AddTransmissions(States.WaitingForEnemyContact, WaitingForEnemyContactTrigger)
                .AddTransmissions(States.WaitingForAnAttack, WaitingForAnAttackTrigger)
                .AddTransmissions(States.SmallJump, SmallJumpTrigger)
                .AddTransmissions(States.AttackJump, AttackJumpTrigger);

            // 待機
            _stateMachine.AddState(States.Idle, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));
            _stateMachine.AddState(States.WaitingForEnemyContact, new Idle().SetAnimeTrigger("waitingForEnemyContact").SetCancelableProgress(0));
            _stateMachine.AddState(States.WaitingForAnAttack, new Idle().SetAnimeTrigger("waitingForAnAttack").SetCancelableProgress(0));
            _stateMachine.AddState(States.MoveJump, new Idle().SetAnimeTrigger("moveJump").SetCancelableProgress(0));
            _stateMachine.AddState(States.SmallJump, new Idle().SetAnimeTrigger("smallJump").SetCancelableProgress(0));
            _stateMachine.AddState(States.AttackJump, new Idle().SetAnimeTrigger("attackJump").SetCancelableProgress(0));

            // // 発射
            // var shoot = new ShootForward(_bulletData, targetLayer);
            // shoot.onShootComplete.AsObservable().Subscribe( => OnShootComplete());
            // shoot.SetBullet(_bulletData);
            // _stateMachine.AddState(States.shoot, shoot);

            // // 発射クールタイム
            // _stateMachine.AddState(States.shootInterval, new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0));

            // // 死亡
            // _stateMachine.AddState(States.Dead, new Idle());
        }
    }
}