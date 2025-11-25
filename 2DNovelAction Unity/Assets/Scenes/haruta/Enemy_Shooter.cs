using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using HighElixir;
using HighElixir.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        private enum StartDirection
        {
            Left,
            Right
        }
        [SerializeField] private StartDirection _StartDirection = StartDirection.Left;


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
        [SerializeField] private Vector2 _offset; // 地面判定距離
        [SerializeField] private Transform wallCheck;              // 壁を確認する位置
        [SerializeField] private float wallCheckHeight = 0.6f;     // 壁判定高度
        [SerializeField] private LayerMask groundLayer;            // 地面レイヤー


        [Header("死亡状態")]
        [SerializeField] private float _DeadEndwaitTime = 0.2f;
        [SerializeField] private GameObject _DeadPartecl; // 死亡時のエフェクト
        [SerializeField] private float _DeadParteclTime = 4f;  // エフェクト発生時間


        [Header("SE")]
        [SerializeField] private string _EncountSEName = "発見";
        [SerializeField] private float _EncountSEVolume = 0.5f;
        [SerializeField] private string _ShotSEName = "シューター・ショット";
        [SerializeField] private float _ShotSEVolume = 0.5f;
        [SerializeField] private string _DamageSEName = "敵ダメージ1";
        [SerializeField] private float _DamageSEVolume = 0.2f;
        [SerializeField] private string _DeadSEName = "敵ダメージ2";
        [SerializeField] private float _DeadSEVolume = 0.4f;

        private UnitBase _player;
        private bool canAttack = false;
        private float firstAttackDelay = 1.0f; // 最初の攻撃までの待機秒数
        private float firstAttackTimer = 0f;

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
            attack.onShootComplete.AddListener(() =>
            {
                PlaySE(_ShotSEName, _ShotSEVolume);
            });
            attack.SetGameObject(_muzzle);
            _stateMachine.AddState(States.shoot, attack);

            var knockBack = new Stun(_rb2, 0.6f, false).SetAnimeTrigger("Damage");
            knockBack.KnockbackForce = 1f;
            _stateMachine.AddState(States.knockBack, knockBack);

            var dead = new Idle_LazyEvent(_DeadEndwaitTime);
            dead.OnAnimationCompleted.AddListener(() =>
            {
                Dead();
            });
            _stateMachine.AddState(States.dead, dead);
        }


        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        protected override void Start()
        {
            base.Start();
            InitDirection();
        }


        /// <summary>
        /// 外部から呼び出されるダメージ処理
        /// </summary>
        protected override void OnTakeDamage(IUnit from, float damage)
        {
            PlaySE(_DamageSEName, _DamageSEVolume);
            if (statusManager.ReadValue(Status.HP) <= 0)
            {
                PlaySE(_DeadSEName, _DeadSEVolume);
                _stateMachine.ChangeState(Triggers.Died);
            }
            _stateMachine.ChangeState(Triggers.Damage);
            _anim?.SetTrigger("Damage"); // 被弾アニメがあるなら
        }

        private async void Dead()
        {
            if (_DeadPartecl != null)
            {
                Destroy(
                    Instantiate(_DeadPartecl, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 2), Quaternion.identity, null),
                    _DeadParteclTime);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_DeadEndwaitTime));

            UnitManager.instance.RemoveUnit(this);
            Destroy(gameObject);

            // UniTaskエラー対策
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_DeadEndwaitTime));
            }
            catch (OperationCanceledException)
            {
                // キャンセルされたら何もしない
                return;
            }
            // オブジェクトが既に破棄されていたら続行しない
            if (this == null) return;

            UnitManager.instance.RemoveUnit(this);
            if (this != null) Destroy(gameObject);
        }


        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();

            // Execute が true かつ対象が1体以上いる場合のみ found を true にする
            if (_searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
            else
            {
                _player = null;
            }


            if (IsMatchingState(States.idle) || IsMatchingState(States.move))
            {
                if (_player != null)
                {
                    PlaySE(_EncountSEName, _EncountSEVolume);
                    PlayerTurnAround();
                    _stateMachine.ChangeState(Triggers.FoundPlayer);
                }
                else
                {
                    _stateMachine.ChangeState(Triggers.MissingPlayer);
                }
            }
        }

        protected override void OnGrounded()
        {
            if (IsMatchingState(States.move))
            {
                CheckEnvironment(); // ← 壁 or 崖を判定してFlip
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

            // 発見状態
            if (IsMatchingState(States.Encount))
            {
                PlayerTurnAround();
                encountTimer += Time.fixedDeltaTime;
                if (encountTimer >= encountDuration)
                {
                    encountTimer = 0f;
                    shootCount = 0;
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
                    PlaySE(_ShotSEName, _ShotSEVolume);
                    shootCount++;
                    //Debug.Log($"Shoot 発射! ({shootCount}/{maxShootCount - 1})");

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

        private void CheckEnvironment()
        {
            // 向きに応じた水平方向ベクトル — localScale.x を使って確実に向きを取得
            var faceSign = Mathf.Sign(transform.localScale.x);
            var forward = new Vector2(faceSign, 0f);

            // 前方の壁をRayでチェック（ユニットの向きに沿って飛ばす）
            RaycastHit2D[] wallHits = Physics2D.RaycastAll(wallCheck.position, forward, wallCheckHeight, groundLayer);

            // 壁に当たった or 足元が無い → 反転
            foreach (var wallHit in wallHits)
            {
                if (wallHit.collider != null && wallHit.collider.gameObject != gameObject)
                {
                    if (Interval.Check(240))// 240フレームに1回だけDebug.Log
                        Debug.Log($"[Shooter]{name} is Fliped. because of wall ahead.");
                    Flip();
                    _rb2.velocity = Vector2.zero;
                    break;
                }
            }
            // 足元の前方をRayでチェック（崖判定）
            // groundCheck の位置から前方に少しオフセットして下方向へレイを飛ばすことで、
            // 前方の地面が存在するかを正しく判定する
            float forwardOffset = graundCheckDistance; // 前方へどれだけオフセットして落下をチェックするか
            Vector2 groundOrigin = (Vector2)groundCheck.position + forward * forwardOffset + _offset;
            RaycastHit2D groundHit = Physics2D.Raycast(groundOrigin, Vector2.down, graundCheckDistance + 0.05f, groundLayer);

            if (groundHit.collider == null)
            {
                if (Interval.Check(240))// 240フレームに1回だけDebug.Log
                    Debug.Log($"[Shooter]{name} is Fliped. because of no ground ahead.");
                Flip();
                _rb2.velocity = Vector2.zero;
            }

        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            if (groundCheck != null)
            {
                Gizmos.color = Color.blue;
                var faceSign = Mathf.Sign(transform.localScale.x);
                var forward = new Vector2(faceSign, 0f);
                float forwardOffset = graundCheckDistance;
                Vector3 groundOrigin = groundCheck.position + (Vector3)forward * forwardOffset + (Vector3)_offset;
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * graundCheckDistance);
                Gizmos.DrawLine(groundOrigin, groundOrigin + Vector3.down * (graundCheckDistance + 0.05f));
            }
            if (wallCheck != null)
            {
                Gizmos.color = Color.red;
                var faceSign = Mathf.Sign(transform.localScale.x);
                var forward = new Vector3(faceSign, 0f, 0f);
                Gizmos.DrawLine(wallCheck.position, wallCheck.position + forward * wallCheckHeight);
            }
        }
#endif
        private void InitDirection()
        {
            switch (_StartDirection)
            {
                case StartDirection.Left:
                    MoveDirection = Vector2.left;
                    Direction = Vector2.left;

                    break;
                case StartDirection.Right:
                    MoveDirection = Vector2.right;
                    Direction = Vector2.right;
                    break;
            }

            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (Direction.x >= 0f ? 1f : -1f);
            transform.localScale = scale;
        }

        private void Flip()
        {
            MoveDirection = new Vector2(-MoveDirection.x, MoveDirection.y);
            Direction = new Vector2(-Direction.x, Direction.y);
        }

        private void PlayerTurnAround()
        {
            // 見た目の向き変更など既存処理
            if (_player != null)
            {
                var toPlayer = (_player.transform.position - transform.position).normalized;
                Direction = (toPlayer.x > 0) ? Vector2.right : Vector2.left;
                MoveDirection = new Vector2(Direction.x, MoveDirection.y);

                var scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (Direction.x >= 0f ? 1f : -1f);
                transform.localScale = scale;
            }
        }


        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }
    }
}
