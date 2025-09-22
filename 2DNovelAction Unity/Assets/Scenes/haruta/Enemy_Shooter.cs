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
        // Enemy_Shooter.cs に追記
        public void OnAttackEnd()
        {
            Debug.Log("🎬 Attack Animation End → Idleへ遷移");

            // アニメーションが終わったタイミングでのみ Idle へ戻す
            _stateMachine.ChangeState(Triggers.AttackEnd);
        }
        // ノックバックアニメーション終了時に呼ばれる
        public void OnKnockBackEnd()
        {
            Debug.Log("🌀 KnockBack Animation End → Idleへ遷移");
            if (currentHP <= 0)
            {
                // HPが0以下 → Deadステートへ
                _stateMachine.ChangeState(Triggers.Died);

            }
            _stateMachine.ChangeState(Triggers.None); // KnockBack → Idle に戻る
        }



        protected override void FixedUpdate()
        {
            SearchPlayer();
            if (IsMatchingState(States.move))

                if (IsMatchingState(States.move))
                {
                    CheckEnvironment(); // ← 壁 or 崖を判定してFlip
                }

            // 既存の処理（攻撃や死亡処理）


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

                    // ★ 撃つ直前にプレイヤーの位置を確認して向きを固定
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        float dir = player.transform.position.x - transform.position.x;

                        // 向きが違っていたら反転
                        if (dir > 0 && moveDirection < 0) // プレイヤーが右側
                            Flip();
                        else if (dir < 0 && moveDirection > 0) // プレイヤーが左側
                            Flip();
                    }

                    // Shoot ステートへ遷移
                    _stateMachine.ChangeState(Triggers.shoot);
                }
            }



            if (IsMatchingState(States.shoot) && shootCount < maxShootCount)
            {
                shootTimer += Time.fixedDeltaTime;
                if (shootTimer >= shootInterval)
                {
                    shootTimer = 0f;
                    var current = _stateMachine.CurrentState;
                    if (current.state is ShootForward shoot)
                        shoot.SetDirection(Direction).Enter(current.state, this);
                    ;

                    shootCount++;
                    Debug.Log($"🔫 Shoot 発射! ({shootCount}/{maxShootCount})");

                    if (shootCount >= maxShootCount)
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
                Destroy(gameObject);
            }

        }
        [SerializeField] private int maxHP = 10;   // ScriptableObjectから読み込むなら差し替え
        private int currentHP = 10;

        /// <summary>
        /// 外部から呼び出されるダメージ処理
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (IsMatchingState(States.dead)) return; // すでに死亡していたら無視
                                                      // HPが残っている → KnockBackステートへ
            _stateMachine.ChangeState(Triggers.Damage);
            _anim?.SetTrigger("Damage"); // 被弾アニメがあるなら
            currentHP -= damage;
            Debug.Log($"💥 Enemy HP: {currentHP}/{maxHP}");

        }
        [Header("環境判定")]
        [SerializeField] private Transform groundCheck;   // 足元の前方を確認する位置
        [SerializeField] private Transform wallCheck;     // 壁を確認する位置
        [SerializeField] private float checkDistance = 0.2f; // 判定距離
        [SerializeField] private LayerMask groundLayer;   // 地面レイヤー

        private int moveDirection = 1; // 右向きスタート


        private void CheckEnvironment()
        {
            // 前方の壁をRayでチェック
            RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * moveDirection, checkDistance, groundLayer);

            // 足元の前方をRayでチェック（崖判定）
            RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, checkDistance, groundLayer);

            // 壁に当たった or 足元が無い → 反転
            if (wallHit.collider != null || groundHit.collider == null)
            {
                Flip();
            }

            // デバッグ表示
            Debug.DrawRay(wallCheck.position, Vector2.right * moveDirection * checkDistance, Color.red);
            Debug.DrawRay(groundCheck.position, Vector2.down * checkDistance, Color.blue);
        }
        private void Flip()
        {
            moveDirection *= -1; // 方向を反転
            transform.Rotate(0, 180, 0); // 見た目を反転
            Direction = new Vector2(moveDirection, 0); // ← これでMoveOnGroundの移動方向も変わる
        }





        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Playerbullet"))
            {
                TakeDamage(2);
            }
        }

    }
}
