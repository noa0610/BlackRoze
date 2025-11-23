using UnityEngine;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using System.Collections.Generic;
using HighElixir;
using System;
using BlackRose.Core.UI;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_Shooter : GroundedUnit
    {
        private static readonly Dictionary<States, string> _states = EnumWrapper.GetValueNameMap<States>();

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

        
        [SerializeField] private Animator _anim;
        [SerializeField] private Rigidbody2D _rb2;

        [Header("移動")]
        [SerializeField] private float _Accel = 4f;      // 加速度
        [SerializeField] private float _Friction = 0.3f; // 摩擦度


        [Header("ショット")]
        [SerializeField] private BulletData _bulletData;       
        [SerializeField] private float shootInterval = 0.5f;
        [SerializeField] private int maxShootCount = 3;
        [SerializeField] private float shootReadyDuration = 0.15f;
        private SearchAssistanceMono _searchAssistance;
        private ShootForward attack;
        private float shootTimer = 0f;
        private int shootCount = 0;
        private float shootReadyTimer = 0f;


        [Header("接敵")]
        [SerializeField] private float encountDuration = 0.15f;
        private float encountTimer = 0f;



        [Header("環境判定")]
        [SerializeField] private Transform groundCheck;            // 足元の前方を確認する位置
        [SerializeField] private float graundCheckDistance = 0.2f; // 地面判定距離
        [SerializeField] private Transform wallCheck;              // 壁を確認する位置
        [SerializeField] private float wallCheckHeight = 0.6f;     // 壁判定高度
        [SerializeField] private LayerMask groundLayer;            // 地面レイヤー

        private int moveDirection = 1; // 左向きスタート


        private bool canAttack = false;
        private float firstAttackDelay = 1.0f; // 最初の攻撃までの待機秒数
        private float firstAttackTimer = 0f;


        protected override void OnGrounded()
        {
            if (IsMatchingState(States.move))
            {
                CheckEnvironment(); // ← 壁 or 崖を判定してFlip
            }
        }
        protected override void OnUnGrounded() { }

        protected override void RegisterStats()
        {
            _stateMachine
                .AddTransitions(States.idle, new[]
                {
                    (Triggers.MissingPlayer, States.move),
                    (Triggers.FoundPlayer, States.Encount),
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransitions(States.move, new[]
                {
                    (Triggers.FoundPlayer, States.Encount),
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransitions(States.Encount, new[]
                {
                    (Triggers.AttackRange, States.shootReady),
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransitions(States.shootReady, new[]
                {
                    (Triggers.shoot,States.shoot),
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransitions(States.shoot, new[]
                {
                    (Triggers.AttackEnd, States.idle),   // ← 3発撃ち終わったらIdleに戻る
                    (Triggers.Damage, States.knockBack)
                })
                .AddTransitions(States.knockBack, new[]
                {
                    (Triggers.None, States.idle),
                    (Triggers.Died, States.dead)
                });

            var idle = new Idle().SetAnimeTrigger("idle");
            _stateMachine.AddState(States.idle, idle);

            var move = new MoveOnGround().SetAnimeTrigger("WalkState");
            move.SetAccel(_Accel).SetFriction(_Friction);
            _stateMachine.AddState(States.move, move);

            var encount = new Idle().SetAnimeTrigger("Contact");
            _stateMachine.AddState(States.Encount, encount);

            var shootready = new Idle();
            _stateMachine.AddState(States.shootReady, shootready); // アニメーション専用

            attack = new ShootForward(_bulletData, AttackLayer);
            attack.SetGameObject(_muzzle);
            _stateMachine.AddState(States.shoot, attack);

            var knockBack = new Stun(_rb2, 0.6f, false).SetAnimeTrigger("Damage");
            knockBack.KnockbackForce = 1f;
            _stateMachine.AddState(States.knockBack, knockBack);

            var dead = new Idle();
            _stateMachine.AddState(States.dead, dead);
        }
        

        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
            MoveDirection = new Vector2(moveDirection, 0);
        }


        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();

            // Execute が true かつ対象が1体以上いる場合のみ found を true にする
            bool found = _searchAssistance.Execute("yellow", list, out _);


            if (IsMatchingState(States.idle))//今のステートがidleかつYellowの中に当てはまるオブジェクトが一つでもある
            {
                if (_searchAssistance.Execute("yellow", list, out _))
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        float dir = player.transform.position.x - transform.position.x;

                        // 向きが違っていたら反転
                        if (dir > 0 && moveDirection < 0)
                        { // プレイヤーが右側
                            Flip();
                        }// ← これでMoveOnGroundの移動方向も変わる
                        else if (dir < 0 && moveDirection > 0) // プレイヤーが左側
                        {
                            Flip();
                        }
                    }
                    _stateMachine.ChangeState(Triggers.FoundPlayer);
                    //ismatchingStatesがidleではないため

                }
                else
                    _stateMachine.ChangeState(Triggers.MissingPlayer);
            }
            else if (IsMatchingState(States.move))//いまのすてーとがmove
            {
                if (found)
                {

                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        float dir = player.transform.position.x - transform.position.x;

                        // 向きが違っていたら反転
                        if (dir > 0 && moveDirection < 0)
                        { // プレイヤーが右側
                            Flip();
                        }// ← これでMoveOnGroundの移動方向も変わる
                        else if (dir < 0 && moveDirection > 0) // プレイヤーが左側
                        {
                            Flip();
                        }
                    }

                    _stateMachine.ChangeState(Triggers.FoundPlayer);
                }
                else
                {
                    _stateMachine.ChangeState(Triggers.MissingPlayer);
                }
            }
        }
        
        public void OnAttackEnd()
        {
            // アニメーションが終わったタイミングでのみ Idle へ戻す
            _stateMachine.ChangeState(Triggers.AttackEnd);
        }

        // ノックバックアニメーション終了時に呼ばれる
        public void OnKnockBackEnd()
        {
            if (statusManager.ReadValue(Status.HP) <= 0)
            {
                // HPが0以下 → Deadステートへ
                _stateMachine.ChangeState(Triggers.Died);

            }
            _stateMachine.ChangeState(Triggers.None); // KnockBack → Idle に戻る
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            // 最初の攻撃待機
            if (!canAttack)
            {
                firstAttackTimer += Time.fixedDeltaTime;
                if (firstAttackTimer >= firstAttackDelay)
                {
                    canAttack = true;
                }
                return; // 攻撃サイクルに入らない
            }

            SearchPlayer();

            // 既存の処理（攻撃や死亡処理）
            if (IsMatchingState(States.Encount))
            {
                encountTimer += Time.fixedDeltaTime;
                if (encountTimer >= encountDuration)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        float dir = player.transform.position.x - transform.position.x;
                        if (dir > 0 && moveDirection < 0) Flip();
                        else if (dir < 0 && moveDirection > 0) Flip();
                    }
                    encountTimer = 0f;

                    // ★ ここでshootCountをリセット
                    shootCount = 0; // プレイヤー発見後、攻撃開始直前にリセット
                    _stateMachine.ChangeState(Triggers.AttackRange);
                }
            }
            else encountTimer = 0f;

            // shootReadyではリセットしない
            if (IsMatchingState(States.shootReady))
            {
                shootReadyTimer += Time.fixedDeltaTime;
                if (shootReadyTimer >= shootReadyDuration)
                {
                    shootReadyTimer = 0f;
                    _anim.SetTrigger("Attack_OneShot");
                    attack.SetDirection(Direction);
                    _stateMachine.ChangeState(Triggers.shoot);
                }
            }


            if (IsMatchingState(States.shoot) && shootCount <= maxShootCount - 1)//ここで一回
            {

                shootTimer += Time.fixedDeltaTime;
                if (shootTimer >= shootInterval)//ここで3回打っている
                {

                    shootTimer = 0f;
                    var current = _stateMachine.CurrentState;
                    if (current.state is ShootForward shoot)
                        shoot.SetDirection(Direction).Enter(current.state, this);
                    ;

                    shootCount++;
                    Debug.Log($"🔫 Shoot 発射! ({shootCount}/{maxShootCount - 1})");

                    if (shootCount >= maxShootCount - 1)
                    {
                        shootCount = 0;
                        _stateMachine.ChangeState(Triggers.AttackEnd);
                        _anim?.SetTrigger("AttackEnd");
                        return; // ここで即座に処理終了
                    }
                }
            }

            if (IsMatchingState(States.dead))
            {
                UnitManager.instance.RemoveUnit(this);
                Destroy(gameObject);
            }

        }

        /// <summary>
        /// 外部から呼び出されるダメージ処理
        /// </summary>
        protected override void OnTakeDamage(IUnit from, float damage)
        {
            if (IsMatchingState(States.dead)) return; // すでに死亡していたら無視
                                                      // HPが残っている → KnockBackステートへ
            _stateMachine.ChangeState(Triggers.Damage);
            _anim?.SetTrigger("Damage"); // 被弾アニメがあるなら

        }
        private void CheckEnvironment()
        {
            // 前方の壁をRayでチェック
            RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.up, wallCheckHeight, groundLayer);

            // 足元の前方をRayでチェック（崖判定）
            RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, graundCheckDistance, groundLayer);

            // 壁に当たった or 足元が無い → 反転
            if (wallHit.collider != null)
            {
                Debug.Log("wallhit Flip");
                Flip();
            }

            if (groundHit.collider == null)
            {
                Debug.Log("groundlost Flip");
                Flip();
            }

            // デバッグ表示
            Debug.DrawRay(wallCheck.position, Vector2.up * wallCheckHeight, Color.red);
            Debug.DrawRay(groundCheck.position, Vector2.down * graundCheckDistance, Color.blue);
        }

        private void Flip()
        {
            moveDirection *= -1; // 方向を反転
            transform.Rotate(0, 180, 0); // 見た目を反転
            Direction = new Vector2(moveDirection, 0); // ← これでMoveOnGroundの移動方向も変わる
            MoveDirection = Direction;
        }


        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }
    }
}
