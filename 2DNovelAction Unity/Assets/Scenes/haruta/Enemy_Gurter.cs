using UnityEngine;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using System.Collections.Generic;
using HighElixir;
using Unity.VisualScripting;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_Gurter : GroundedUnit
    {
        [Header("攻撃関連")]
        [SerializeField] private Animator _anim;
        [SerializeField] private GameObject _muzzle;
        [SerializeField] private Rigidbody2D _rb2;

        private static readonly Dictionary<States, string> _states = EnumWrapper.GetDict<States>();
        private SearchAssistanceMono _searchAssistance;

        public enum States
        {
            none,
            Idle,       // 待機
            Encount,    // 接敵
            MAttack,    // 近接攻撃
            RAttack,    // 遠距離攻撃
            KnockOut,   // スタン
            dead
        }

        private enum Triggers
        {
            recover,
            None,
            cooldown,
            shoot,
            MissingPlayer, // プレイヤーを見失う
            TackleRenge,   // 近距離範囲
            AttackRange,   // 遠距離範囲内
            AttackEnd,
            Died,
            Damage,
        }

        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        protected override void OnGrounded() { }
        protected override void OnUnGrounded() { }

        protected override void RegisterStats()
        {
            _stateMachine
                .AddTransmissions(States.Idle, new[]
                {
                    (Triggers.recover, States.Encount),
                })
                .AddTransmissions(States.Encount, new[]
                {
                    (Triggers.AttackRange, States.RAttack),
                    (Triggers.TackleRenge, States.MAttack),
                    (Triggers.Damage, States.KnockOut)
                })
                .AddTransmissions(States.RAttack, new[]
                {
                    (Triggers.cooldown, States.Encount),
                    (Triggers.Damage, States.KnockOut)
                })
                .AddTransmissions(States.MAttack, new[]
                {
                    (Triggers.cooldown, States.Encount),
                    (Triggers.Damage, States.KnockOut)
                })
                .AddTransmissions(States.KnockOut, new[]
                {
                    (Triggers.None, States.Idle),
                    (Triggers.Died, States.dead)
                });

            _stateMachine.AddState(States.Idle, new Idle());
            _stateMachine.AddState(States.Encount, new Idle());
            _stateMachine.AddState(States.MAttack, new Idle());
            _stateMachine.AddState(States.RAttack, new Idle());
            _stateMachine.AddState(States.dead, new Idle());
            _stateMachine.AddState(States.KnockOut, new Stun(_rb2, 4, true));
        }

        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }

        private void SearchPlayer()
{
    var list = UnitManager.instance.GetUnitList();
    var player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) return;

    // プレイヤーが右なら 1、左なら -1
    int direction = player.transform.position.x > transform.position.x ? 1 : -1;
    Vector3 scale = transform.localScale;
            if (IsMatchingState(States.RAttack) || IsMatchingState(States.MAttack))
            {
                return;
            }
    // ===== 遠距離攻撃範囲検知 =====
            if (_searchAssistance.Execute("renge", list, out _))
            {
                // 向き反転
                if (direction == 1 && scale.x < 0 || direction == -1 && scale.x > 0)
                {
                    scale.x *= -1;
                    transform.localScale = scale;
                }

                _stateMachine.ChangeState(Triggers.AttackRange);
                Debug.Log("遠距離範囲内に入りました");
            }
            // ===== 近距離攻撃範囲検知 =====
            else if (_searchAssistance.Execute("meray", list, out _))
            {
                if (direction == 1 && scale.x < 0 || direction == -1 && scale.x > 0)
                {
                    scale.x *= -1;
                    transform.localScale = scale;
                }

                _stateMachine.ChangeState(Triggers.TackleRenge);
                Debug.Log("近距離範囲内に入りました");
            }
}

        // --- Idle待機処理 ---
        private float testTimer = 2.0f;
        [SerializeField] private float idleWaitTime = 3.0f;
        private float idleTimer;

        protected override void FixedUpdate()
        {
            SearchPlayer();
            Debug.Log("現在のステート: " + _stateMachine.CurrentState.key);

            if (IsMatchingState(States.Idle))
            {
                idleTimer += Time.fixedDeltaTime;
                if (idleTimer >= idleWaitTime)
                {
                    idleTimer = 0f;
                    _stateMachine.ChangeState(Triggers.recover); // Idle→Encountへ
                }
            }

            if (IsMatchingState(States.RAttack))
            {
                testTimer -= Time.fixedDeltaTime;
                if (testTimer <= 0)
                {
                    testTimer = 2.0f;
                    _stateMachine.ChangeState(Triggers.cooldown);
                    Debug.Log("遠距離攻撃中");
                }
            }

            if (IsMatchingState(States.MAttack))
            {
                testTimer -= Time.fixedDeltaTime;
                if (testTimer <= 0)
                {
                    testTimer = 2.0f;
                    _stateMachine.ChangeState(Triggers.cooldown);
                    Debug.Log("近距離攻撃中");
                }
            }
        }

        public void TakeDamage(int damage)
        {
            _stateMachine.ChangeState(Triggers.Damage);
        }

        // --- 反転処理 ---
        private void Flip()
        {
            Direction *= -1; // 進行方向を反転
            transform.Rotate(0, 180, 0); // 見た目を反転
        }
    }
}
