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
            idle,
            Encount,
            move,
            shootReady,
            shoot,
            knockBack,
            dead
        }

        private enum Triggers
        {
            None,
            shootcooldown,
            shoot,
            MissingPlayer,
            FoundPlayer,
            AttackRange,
            AttackEnd,   // ← 追加
            Died,
            Damage,
        }


        private SearchAssistanceMono _searchAssistance;

        private int shootCount = 0;
        [SerializeField] private int maxShootCount = 3;
        [SerializeField] private float shootInterval = 0.5f;
        private float shootTimer = 0f;

        private float encountTimer = 0f;
        [SerializeField] private float encountDuration = 0.15f;

        private float shootReadyTimer = 0f;
        [SerializeField] private float shootReadyDuration = 0.15f;

        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        protected override void OnGrounded() { }
        protected override void OnUnGrounded() { }

        protected override void RegisterStats()
        {
            _stateMachine
                .AddTransmissions(States.idle, new[]
                {
                    (Triggers.MissingPlayer, States.move),
                    (Triggers.FoundPlayer, States.Encount),
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransmissions(States.move, new[]
                {
                    (Triggers.FoundPlayer, States.Encount),
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransmissions(States.Encount, new[]
                {
                    (Triggers.AttackRange, States.shootReady),
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransmissions(States.shootReady, new[]
                {
                    (Triggers.shoot,States.shoot),
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransmissions(States.shoot, new[]
{
    (Triggers.AttackEnd, States.idle),   // ← 3発撃ち終わったらIdleに戻る
    (Triggers.Damage, States.knockBack)
})

                .AddTransmissions(States.knockBack, new[]
                {
                    (Triggers.None, States.idle),
                    (Triggers.Died, States.dead)
                });

            _stateMachine.AddState(States.idle, new Idle().SetAnimeTrigger("idle"));
            _stateMachine.AddState(States.move, new MoveOnGround(_rb2, true).SetAnimeTrigger("WalkState"));
            _stateMachine.AddState(States.Encount, new Idle().SetAnimeTrigger("Contact"));
            _stateMachine.AddState(States.shootReady, new Idle()); // アニメーション専用

            var attack = new ShootForward();
            attack.SetBullet(_bulletData);
            attack.SetMuzzle(_muzzle);
            attack.SetCancelableProgress(0);

            _stateMachine.AddState(States.shoot, attack);
            _stateMachine.AddState(States.knockBack, new Stun(_rb2, 0.6f, false).SetAnimeTrigger("Damage"));
            _stateMachine.AddState(States.dead, new Idle());
        }

        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();

            // Execute が true かつ対象が1体以上いる場合のみ found を true にする
            bool found = _searchAssistance.Execute("yellow", list, out _);


            if (IsMatchingState(States.idle))//今のステートがidleかつYellowの中に当てはまるオブジェクトが一つでもある
            {
                if (_searchAssistance.Execute("yellow", list, out _))
                    _stateMachine.ChangeState(Triggers.FoundPlayer);//ismatchingStatesがidleではないため
                else
                    _stateMachine.ChangeState(Triggers.MissingPlayer);
            }
            else if (IsMatchingState(States.move))//いまのすてーとがmove
            {
                if (found)
                    _stateMachine.ChangeState(Triggers.FoundPlayer);//ismatchingStatesがidleではないため
                else
                    _stateMachine.ChangeState(Triggers.MissingPlayer);
            }
            Debug.Log(found);
}

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();

            // Encount → ShootReady
            if (IsMatchingState(States.Encount))
            {
                encountTimer += Time.fixedDeltaTime;
                if (encountTimer >= encountDuration)
                {
                    encountTimer = 0f;
                    _stateMachine.ChangeState(Triggers.AttackRange);

                    // ShootReady アニメーション発火
                    if (_anim != null)
                        _anim.SetTrigger("Attack_OneShot");
                }
            }
            else encountTimer = 0f;

            // ShootReady → Shoot（溜め時間が終わったら1回だけShootへ）
            if (IsMatchingState(States.shootReady))
            {
                shootReadyTimer += Time.fixedDeltaTime;
                if (shootReadyTimer >= shootReadyDuration)
                {
                    shootReadyTimer = 0f;

                    // Shoot へ遷移（1回だけ）
                    _stateMachine.ChangeState(Triggers.shoot);
                }
            }


            // Shoot処理
            if (IsMatchingState(States.shoot))
            {
                shootTimer += Time.fixedDeltaTime;
                if (shootTimer >= shootInterval)
                {
                    shootTimer = 0f;

                    // ShootForward を取得して弾を発射
                    var current = _stateMachine.CurrentState;
                    if (current.state is ShootForward shoot)
                        shoot.Enter(current.state, this);

                    shootCount++;
                    Debug.Log($"🔫 Shoot 発射! ({shootCount}/{maxShootCount})");

                    if (shootCount >= maxShootCount)
                    {
                        shootCount = 0;

                        // AttackEnd トリガー発火
                        if (_anim != null)
                            _anim.SetTrigger("AttackEnd");

                        // Idle へ遷移
                        _stateMachine.ChangeState(Triggers.AttackEnd);
                    }


                }
            }


        }


        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }
    }
}
