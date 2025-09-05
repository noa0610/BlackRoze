using UnityEngine;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using System.Collections.Generic;
using HighElixir;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_Shooter : GroundedUnit
    {
        [Header("攻撃関連")]
        [SerializeField] private Animator _anim;
        [SerializeField] private GameObject _muzzle;

        [SerializeField] private BulletData _bulletData;
        [SerializeField] private Rigidbody2D _rb2;

        private static readonly Dictionary<States, string> _states = EnumWrapper.GetDict<States>();

        public enum States
        {
            none,
            idle,           // 待機
            Encount,        // 警戒待機
            move,           // 前進
            shootReady,     // 攻撃待機
            shoot,          // ショット
            knockBack,      // ノックバック
            dead            // 死亡
        }

        private enum Triggers
        {
            None,
            MissingPlayer,
            FoundPlayer,
            AttackRange,
            shootCoolDown,
            shoot,
            Died,
            time,
            Damage,
        }

        private SearchAssistanceMono _searchAssistance;
        // shoot ステート用のカウンタとタイマー
private int shootCount = 0;                      // 撃った回数
[SerializeField] private int maxShootCount = 3;  // 最大発射数
[SerializeField] private float shootInterval = 0.5f; // 発射間隔



        // タイマー類
        private float encountTimer = 0f;
        private float encountDuration = 2.0f; // エンカウント後に射撃準備へ移るまでの時間

        private float shootReadyTimer = 0f;
        private float shootReadyDuration = 1.0f; // 攻撃待機から射撃へ

        private float shootTimer = 0f;
        private float shootDuration = 1.0f; // 1発の発射処理時間

        // 発射回数カウンタ
        private int shotCount = 0;
        private int maxShotCount = 3; // 3発撃ったらIdleに戻る

        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        protected override void OnGrounded() { }
        protected override void OnUnGrounded() { }

        protected override void RegisterStats()
        {
            // トランジション設定（図と一致させる）
            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.move),       // プレイヤー発見で前進
                (Triggers.Damage, States.knockBack)
            };

            var moveTrigger = new[]
            {
                (Triggers.FoundPlayer, States.Encount),    // プレイヤー発見で警戒
                (Triggers.MissingPlayer, States.idle),     // プレイヤー不在なら待機へ
                (Triggers.Damage, States.knockBack)
            };

            var EncountTrigger = new[]
            {
                (Triggers.AttackRange, States.shootReady), // 数秒後に攻撃待機
                (Triggers.Damage, States.knockBack)
            };

            var shootReadyTrigger = new[]
            {
                (Triggers.shootCoolDown, States.shoot),    // 射撃
                (Triggers.Damage, States.knockBack)
            };

            var shootTrigger = new[]
{
    (Triggers.shoot, States.shootReady),   // ← 1発撃ち終わったら準備へ戻る
    (Triggers.MissingPlayer, States.idle), // ← 3発撃ったらIdleへ（赤線）
    (Triggers.Damage, States.knockBack)
};

            var knockBackTrigger = new[]
            {
                (Triggers.time, States.idle),              // ノックバック終了で待機へ
                (Triggers.Died, States.dead)
            };

            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.move, moveTrigger)
                .AddTransmissions(States.Encount, EncountTrigger)
                .AddTransmissions(States.shootReady, shootReadyTrigger)
                .AddTransmissions(States.shoot, shootTrigger)
                .AddTransmissions(States.knockBack, knockBackTrigger);

            // ステート定義
            _stateMachine.AddState(States.idle, new Idle().SetAnimeTrigger("Leave"));
            _stateMachine.AddState(States.move, new MoveOnGround(_rb2, true).SetAnimeTrigger("WalkState"));
            _stateMachine.AddState(States.Encount, new Idle().SetAnimeTrigger("Contact"));
            _stateMachine.AddState(States.shootReady, new Idle());

            var attack = new ShootForward();
attack.SetBullet(_bulletData);

attack.SetMuzzle(_muzzle); // ← 銃口を渡す
attack.SetAnimeTrigger("Attack_shoot");
attack.SetCancelableProgress(0);
_stateMachine.AddState(States.shoot, attack);

            _stateMachine.AddState(States.shoot, attack);

            _stateMachine.AddState(States.knockBack, new Stun(_rb2, 0.6f, false).SetAnimeTrigger("Damage"));
            _stateMachine.AddState(States.dead, new Idle());
        }

        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            bool found = _searchAssistance.Execute("yellow", list, out var units);

            // 状態ごとに処理
            if (IsMatchingState(States.idle))
            {
                if (found)
                {
                    Debug.Log("Idleでプレイヤー発見 → Moveへ");
                    _stateMachine.ChangeState(Triggers.FoundPlayer);
                }
            }
            else if (IsMatchingState(States.move))
            {
                if (found)
                {
                    Debug.Log("Moveでプレイヤー発見 → Encountへ");
                    _stateMachine.ChangeState(Triggers.FoundPlayer);
                }
                else
                {
                    Debug.Log("Moveでプレイヤー不在 → Idleへ");
                    _stateMachine.ChangeState(Triggers.MissingPlayer);
                }
            }
            // shoot ステート用タイマー
// shoot ステート用タイマー
if (IsMatchingState(States.shoot))
{
    shootTimer += Time.fixedDeltaTime;

    if (shootTimer >= shootDuration)
    {
        shootTimer = 0f;

        // 1発撃ち終わったタイミング
        shotCount++;

        if (shotCount >= maxShotCount)
        {
            // 3発撃ったら Idle に強制戻り（図の赤線）
            shotCount = 0;
            Debug.Log("🔴 3発撃ったので Idle に戻ります");
            _stateMachine.ChangeState(Triggers.MissingPlayer);
        }
        else
        {
            // AttackEnd 発火確認
            Debug.Log("✅ AttackEnd トリガー送信！");
            if (_anim != null) _anim.SetTrigger("AttackEnd");

            // shoot → shootReady に戻す
            _stateMachine.ChangeState(Triggers.shoot);
        }
    }
}
else
{
    shootTimer = 0f;
}


        }

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();

            // Encount ステート用タイマー
            if (IsMatchingState(States.Encount))
            {
                encountTimer += Time.fixedDeltaTime;
                if (encountTimer >= encountDuration)
                {
                    encountTimer = 0f;
                    _stateMachine.ChangeState(Triggers.AttackRange);
                }
            }
            else encountTimer = 0f;

            // shootReady ステート用タイマー
            if (IsMatchingState(States.shootReady))
            {
                shootReadyTimer += Time.fixedDeltaTime;
                if (shootReadyTimer >= shootReadyDuration)
                {
                    shootReadyTimer = 0f;
                    _stateMachine.ChangeState(Triggers.shootCoolDown);
                }
            }
            else shootReadyTimer = 0f;

            // shoot ステート中の処理
if (IsMatchingState(States.shoot))
{
    shootTimer += Time.fixedDeltaTime;

    if (shootTimer >= shootInterval)
    {
        shootTimer = 0f;

        // ShootForward の Shoot を実行
        var state = _stateMachine.CurrentState as ShootForward;
        if (state != null)
        {
            state.Shoot(this);
            shootCount++;
            Debug.Log($"🔫 Shoot 発射! ({shootCount}/{maxShootCount})");
        }

        // 規定回数に達したら終了処理
        if (shootCount >= maxShootCount)
        {
            shootCount = 0;
            Debug.Log("🏁 AttackEnd 発火 → shootReadyへ");
            _anim.SetTrigger("AttackEnd");
            _stateMachine.ChangeState(Triggers.shoot); 
        }
    }
}
else
{
    shootTimer = 0f; // shoot 以外ではリセット
    shootCount = 0;
}

        }

        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }
    }
}
