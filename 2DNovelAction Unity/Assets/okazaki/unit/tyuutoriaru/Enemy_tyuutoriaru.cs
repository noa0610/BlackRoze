using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using BlackRose.Core.Models.Systems;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_tyuutoriaru : GroundedUnit
    {
        [Header("デバッグ")]
        [Tooltip("攻撃選択の固定化(１，レーザーショット ２，ビームソード)")]
        [SerializeField] private int FixedAttackSelect = 0;

        [Tooltip("登場演出の省略")]
        [SerializeField] private bool cutEntry = false;


        [Header("固有設定")]
        [SerializeField] private float _AttackIntervalTime = 2f;


        [Header("登場演出")]
        [SerializeField] private float _EntryEndwaitTime = 4.5f;    // 登場アニメーション終了時間（手動必須になる）


        [Header("ジャンプ")]
        [SerializeField] private float _JumpSpeed = 7f;
        [SerializeField] private float _wallDistanse = 3f;          // 壁との距離
        [SerializeField] private float _wallChackRenge = 15f;       // 壁チェックの距離
        [SerializeField] private LayerMask _wallLayer;

        // TODO 後でSerializeFieldを消す
        [SerializeField] private List<Vector3> _junpPositions;
        [SerializeField] private Vector3 _centerPositions;


        [Header("接近ビームソード")]
        [SerializeField] private GameObject _beamswordmuzzle;
        [SerializeField] private BulletData _beamswordBulletData;
        [SerializeField] private float _Moveaccel = 20f;
        [SerializeField] private float _Movefriction = 1.0f;
        [SerializeField] private float _beamswordDistance = 5f;
        [SerializeField] private GameObject WallChackPoint;
        [SerializeField] private LayerMask _WallLayer;

        [Tooltip("プレイヤー感知 → セイバー攻撃まで")]
        [SerializeField] private float _beamswordwaitTime = 0.1f;
        private bool _wallChack;


        [Header("レーザーショット")]
        [SerializeField] private GameObject _Lasershotmuzzle;
        [SerializeField] private BulletData _LasershotbulletData;
        [SerializeField] private int _LaserShotCount = 6;

        [Tooltip("レーザーショット開始 → 発射直前待機")]
        [SerializeField] private float _LaserShotStartTime = 1f;

        [Tooltip("発射直前待機 → 発射")]
        [SerializeField] private float _LaserShotbeforeTime = 0.5f;

        [Tooltip("発射 → 発射後の後隙")]
        [SerializeField] private float _LaserShotTime = 1f;

        [Tooltip("発射後の後隙 → 次の発射")]
        [SerializeField] private float _LaserShotIntervalTime = 1f;
        private int _shotCount;


        [Header("衝撃波")]
        [SerializeField] private GameObject _shockwaveshotmuzzle;
        [SerializeField] private BulletData _shockwaveBulletData;


        [Header("死亡状態")]
        [SerializeField] private bool _wontDie = false;
        [SerializeField] private float _DeadEndwaitTime = 6.5f;


        private Rigidbody2D _RB2;
        private Animator _anim;
        private int currentAttack = 1; // 初期値は1（レーザーショットから発動）
        private int nowstate = 2;
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
        private bool _waitingForAttack1 = false;

        private bool _isAnimating = false; // アニメ再生中フラグ（AnimaSelect の重複実行防止）
        private bool _waitingForAttack2 = false;
        private SearchAssistanceMono _searchAssistance;
        private CancellationTokenSource _cancellation;

        #region === Unit ===
        protected override void BeforeAwake()
        {
            InitJumpPosition();
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }
        protected override void AfterAwake()
        {
            if (cutEntry)
            {
                _stateMachine.Awake("attackidle", false);
                _animator.SetTrigger("toIdle");
                IsInvincible = false;
            }
            else
            {
                _stateMachine.Awake("entry", false);
                IsInvincible = true;
            }
        }
        protected override void Start()
        {
            base.Start();
            _RB2 = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _cancellation = new CancellationTokenSource();
        }

        private void EntryEnd()
        {
            IsInvincible = false;
        }

        private bool _halfHpTriggered = false;

        protected override bool BeforeTakeDamage(IUnit from, ref float damage)
            => !IsInvincible;

        protected override void OnTakeDamage(IUnit from, float damage)
        {
            if (!_halfHpTriggered)
            {
                var hpStatus = statusManager.GetStatus(Status.HP);
                var maxHpStatus = statusManager.GetStatus(Status.MaxHP);

                if (hpStatus != null && maxHpStatus != null)
                {
                    float hp = hpStatus.CurrentAmount;
                    float maxHp = maxHpStatus.CurrentAmount;

                    if (hp <= maxHp / 2f)
                    {
                        _halfHpTriggered = true;
                        Debug.Log("HPが半分以下になりました");
                        _stateMachine.ChangeState(Triggers.HalfHP);
                        IsInvincible = true;
                    }
                }
            }
        }
        protected override void OnDeath()
        {
            base.OnDeath();
            if (_wontDie) return;
            _stateMachine.ChangeState(Triggers.Died);
        }
        #endregion


        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (_searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
        }

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();

            if (IsMatchingState(States.attackidle))
            {
                TurnAround();

                if (_player == null)
                {
                    _stateMachine.ChangeState(Triggers.Playerdead);
                }
            }

            if (IsMatchingState(States.beamsword_move) && _player != null)
            {
                WallChack();
                BeamSwordMoveStay();
            }
        }

        private void TurnAround()
        {
            // 見た目の向き変更など既存処理
            if (_player != null)
            {
                Direction = (_player.Transform.position - transform.position).normalized;
                Direction = (Direction.x > 0) ? Vector2.right : Vector2.left;
                if (Direction.x != 0)
                {
                    Debug.Log("振り向き");
                    var scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * (Direction.x > 0 ? 1 : -1);
                    transform.localScale = scale;
                }
            }
        }

        private void WallChack()
        {
            if (WallChackPoint == null) return;
            Ray2D ray = new Ray2D(WallChackPoint.transform.position, MoveDirection);
            RaycastHit2D rayhit = Physics2D.Raycast(ray.origin, ray.direction, 1f, _WallLayer);
            _wallChack = rayhit.collider ? true : false;
            Debug.DrawRay(WallChackPoint.transform.position, MoveDirection, Color.red);
        }

        private async void Dead()
        {
            IsInvincible = true; // 攻撃不可

            _cancellation.Cancel(); // UniTask停止

            IsInvincible = true; // 攻撃不可

            _cancellation.Cancel();  // UniTask停止
            _cancellation.Dispose(); // リソース解放

            await UniTask.Delay(TimeSpan.FromSeconds(_DeadEndwaitTime));

            GetComponent<FlowchartFirer>().Fire();

            await UniTask.Delay(TimeSpan.FromSeconds(0.1));

            UnitManager.instance.RemoveUnit(this); // UnitManagerの自データ削除

            Destroy(gameObject);
        }



        #region === Jump ===

        // 左右から壁を探してジャンプ位置をセット
        private void InitJumpPosition()
        {
            // リスト初期化
            if (_junpPositions == null)
                _junpPositions = new List<Vector3> { Vector3.zero, Vector3.zero };
            else
            {
                if (_junpPositions.Count < 2)
                {
                    _junpPositions.Clear();
                    _junpPositions.Add(Vector3.zero);
                    _junpPositions.Add(Vector3.zero);
                }
            }

            // 左右方向（最初に左方向から）
            Vector2[] directions = new Vector2[]
            {
                Vector2.left,
                Vector2.right
            };
            int mask = _wallLayer.value;

            for (int i = 0; i < 2; i++)
            {
                Vector2 dir = directions[i];
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, _wallChackRenge, mask);

                Vector2 candidate;
                if (hit.collider != null)
                {
                    Debug.Log("Hit Wall");
                    candidate = hit.point - hit.normal * _wallDistanse * Vector2.left;
                }
                else
                {
                    Debug.Log("Not Wall");
                    // 壁が見つからない場合は索敵範囲をジャンプ位置に
                    candidate = (Vector2)transform.position + dir * (_wallChackRenge - _wallDistanse);
                }

                // Y軸は現在位置
                candidate.y = transform.position.y;

                // 左右位置をセット
                _junpPositions[i] = (Vector3)candidate;
            }

            // 中央位置をセット
            _centerPositions = (_junpPositions[0] + _junpPositions[1]) / 2;
        }

        // 中央から遠い位置をジャンプ位置として設定 (1 = 右, 0 = 左)
        private int JumpSelect()
        {
            // 中心とこのオブジェクトのx座標差を取得
            float distance = transform.position.x - _centerPositions.x;

            Debug.Log("距離差: " + distance);
            // 差がプラスなら1、マイナスなら0を返す
            return distance >= 0 ? 0 : 1;
        }
        #endregion

        #region === Attack Select ===
        private void Attackselect()
        {
            if (FixedAttackSelect != 0)
            {
                switch (FixedAttackSelect)
                {
                    case 1:
                        Attack1();
                        break;
                    case 2:
                        Attack2();
                        break;
                }
                return;
            }

            if (currentAttack == 1)
            {
                TurnAround();
                Attack1();
                currentAttack = 2;
                return;
            }
            else if (currentAttack == 2)
            {
                TurnAround();
                Attack2();
                currentAttack = 1;
            }
        }

        void Attack1()
        {
            Debug.Log("レーザー攻撃開始");
            _stateMachine.ChangeState(Triggers.Attack1start);
        }

        void Attack2()
        {
            Debug.Log("ビームソード開始");
            _stateMachine.ChangeState(Triggers.Attack2start);
        }
        #endregion

        #region === LaserShot ===
        private void LaserShotStart()
        {
            _shotCount = 0;
            _lasershotState?.SetDirection(Direction);
        }

        private void LaserShotBefore()
        {

        }

        private async void LaserShotExit()
        {
            _shotCount++;
            await UniTask.Delay(TimeSpan.FromSeconds(_LaserShotTime), cancellationToken: _cancellation.Token);

            if (_shotCount >= _LaserShotCount)
            {
                _stateMachine.ChangeState(Triggers.Attack1end);
            }
            else
            {
                _stateMachine.ChangeState(Triggers.Attack1loop);
            }
        }

        private void AnimaSelect()
        {
            if (_shotCount % 3 == 1)
            {
                _anim.SetTrigger("toShot_Up");
                return;
            }
            else if (_shotCount % 3 == 2)
            {
                _anim.SetTrigger("toShot_Down");
                return;
            }
            else if (_shotCount % 3 == 0)
            {
                _anim.SetTrigger("toShot_Medium");
                return;
            }
        }

        private IEnumerator WaitForBeamswordAnimationThenFire()
        {
            if (_waitingForAttack2) yield break;
            _waitingForAttack2 = true;
            try
            {
                if (_anim == null)
                {
                    _stateMachine.LazyChange(Triggers.Attack2end);
                    yield break;
                }

                // 1フレーム待ってアニメ遷移を反映+                yield return null;
                // 目標ステートの変化を待つ（短タイムアウト）
                int startHash = _anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
                float waitTime = 0f;
                const float maxWait = 5f;
                while (_anim.GetCurrentAnimatorStateInfo(0).shortNameHash == startHash && waitTime < maxWait)
                {
                    waitTime += Time.deltaTime;
                    yield return null;
                }
                // 新しいステートの進行度が 1.0 に達するまで待つ（ループの可能性を考慮）
                waitTime = 0f;
                while (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f && waitTime < maxWait)
                {
                    waitTime += Time.deltaTime;
                    yield return null;
                }

                // アニメ完了後にステート遷移
                _stateMachine.LazyChange(Triggers.Attack2end);
                GetComponent<BoxCollider2D>().isTrigger = true;
                _positionJump?.SetTarget(JumpSelect());
            }
            finally
            {
                _waitingForAttack2 = false;
            }
        }
        #endregion

        #region === BeamSword ===
        private void BeamSwordStart()
        {
            TurnAround();
            MoveDirection = Direction;
        }

        private async void BeamSwordMoveStay()
        {
            float playerdictance = _player.Transform.position.x - transform.position.x;

            // プレイヤーの近くまで接近したら
            if (Mathf.Abs(playerdictance) <= _beamswordDistance)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_beamswordwaitTime), cancellationToken: _cancellation.Token);
                _stateMachine.ChangeState(Triggers.Attack2);
            }
            else if (_wallChack)
            {
                _stateMachine.ChangeState(Triggers.Attack2);
            }
        }
        #endregion

        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }

    }
}

