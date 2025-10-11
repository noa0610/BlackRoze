using UnityEngine;
using System.Collections.Generic;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;
using BlackRose.Datas.Definitions;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_tyuutoriaru : UnitBase
    {
        public enum States
        {
            none,
            idle,// 待機
            dead,// 死亡
            attackidle, // 攻撃待機
            lasershot, // レーザー攻撃
            beamswordattack, // ビームソード攻撃
            beamswordattackmove, // ビームソード攻撃移動
            fixedpositionjump, // ジャンプ
            stun, // スタン
            shockwave, // ショックウェーブ
        }
        private enum Triggers
        {
            None,
            FoundPlayer,   // プレイヤーを発見した
            Attackcooldown, // 攻撃クールダウンした
            Attack1, // 攻撃１
            Attack2, // 攻撃２
            Attack1end, // 攻撃1した
            moveend, // 移動した
            Attack2end, // 攻撃1した
            Shockwaveend, // ショックウェーブした
            HalfHP, // HPが半分以下
            Landing, // 着地
            Event1, // イベント1発生
            Event2, // イベント2発生
            Died,          // 死亡した（HPが０になった）
        }
        [SerializeField] private List<GameObject> _junpPositions;
        [SerializeField] private GameObject _centerPositions;
        [SerializeField] private GameObject _YPositions;

        [SerializeField] private float closeRangeDistance = 5f; // 近距離判定の距離
        private int currentAttack = 1; // 初期値は1（アタック1）
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetDict<States>();
        [SerializeField] private Transform[] _firePoints;
        [SerializeField] private Transform[] _swordfirePoints;
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Rigidbody2D _RB2;
        [SerializeField] private FreeMove _freeMove;
        [SerializeField] private BulletData _beamswordBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _beamswordTargetLayer; // 必要ならInspectorでセット
        [SerializeField] private BulletData _shockwaveBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _shockwaveTargetLayer; // 必要ならInspectorでセット
        [SerializeField] private PositionJump _positionJump ;

        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]                                // 待機ステートのトリガー
            {
                (Triggers.FoundPlayer, States.attackidle),              // イベント1発生で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
            };
            var attackidleTrigger = new[]                          // 攻撃待機ステートのトリガー  
            {
                (Triggers.Attack1, States.fixedpositionjump),              // 攻撃１でレーザー攻撃へ
                (Triggers.Attack2, States.beamswordattackmove),   // 攻撃２でビームソード接近へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
            };
            var lasershotTrigger = new[]                           // レーザー攻撃ステートのトリガー
            {
                (Triggers.Attack1end, States.attackidle),                // 攻撃１終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
            };
            var beamswordattackmoveTrigger = new[]                     // ビームソード攻撃ステートのトリガー
            {
                (Triggers.moveend, States.beamswordattack),                // 移動終了で攻撃２へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
            };
            var beamswordattackTrigger = new[]                     // ビームソード攻撃ステートのトリガー
            {
                (Triggers.Attack2end, States.fixedpositionjump),                // 攻撃２終了でジャンプへ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
            };
            var fixedpositionjumpTrigger = new[]                                // ジャンプステートのトリガー
            {
                (Triggers.Landing, States.attackidle),             // 着地で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
            };
            var stunTrigger = new[]                                // スタンステートのトリガー
            {
                (Triggers.Event2, States.shockwave),               // イベント2発生でショックウェーブへ
                (Triggers.Died, States.dead),                      // 死亡で死へ 
            };
            var shockwaveTrigger = new[]                           // ショックウェーブステートのトリガー
            {
                (Triggers.Shockwaveend, States.idle),              // ショックウェーブ終了で待機へ
                (Triggers.Died, States.dead)                       // 死亡で死へ
            };

            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.attackidle, attackidleTrigger)
                .AddTransitions(States.lasershot, lasershotTrigger)
                .AddTransitions(States.beamswordattackmove, beamswordattackmoveTrigger)
                .AddTransitions(States.beamswordattack, beamswordattackTrigger)
                .AddTransitions(States.fixedpositionjump, fixedpositionjumpTrigger)
                .AddTransitions(States.stun, stunTrigger)
                .AddTransitions(States.shockwave, shockwaveTrigger);

            // 死んだときに何もしないならDeadの設定はいらない

            // 待機
            var idle = new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0);
            _stateMachine.AddState(States.idle, idle);
            // 死亡
            var died = new Idle().SetAnimeTrigger("died").SetCancelableProgress(0);
            died.OnAnimationCompleted.AddListener(() =>
            {
                UnitManager.instance.RemoveUnit(this);
                Destroy(gameObject);
            });
            _stateMachine.AddState(States.dead, died);
            // ジャンプ
            var jumpPositions = _junpPositions.ConvertAll(pos => (Vector2)pos.transform.position);
            var fixedpositionjump = new PositionJump(jumpPositions, 10f)
                .SetAnimeTrigger("fixedpositionjump")
                .SetCancelableProgress(0);
                fixedpositionjump.OnArrived += () =>
                {
                   _stateMachine.ChangeState(Triggers.Landing); // 例：Landingトリガーで遷移
                };
            _stateMachine.AddState(States.fixedpositionjump, fixedpositionjump);
            // 攻撃待機
            var attackIdle = new Idle_LazyEvent(5f).SetAnimeTrigger("attackidle").SetCancelableProgress(0);
            attackIdle.LazyEvent.AddListener(Attackselect);
            _stateMachine.AddState(States.attackidle, attackIdle);
            // レーザー攻撃
            var lasershot = new LaserShot(_RB2, _firePoints, _bulletPrefab).SetAnimeTrigger("lasershot").SetCancelableProgress(0);
            _stateMachine.AddState(States.lasershot, lasershot);
            // ビームソード攻撃移動
            var beamswordattackmove = _freeMove.SetAnimeTrigger("move").SetCancelableProgress(0);
            _stateMachine.AddState(States.beamswordattackmove, beamswordattackmove);
            // ビームソード攻撃
            var beamswordattack = new ShootForward(_beamswordBulletData, _beamswordTargetLayer)
            .SetDirection(Vector2.down) // プレイヤー方向など、必要に応じてセット
            .SetMuzzle(_swordfirePoints.Length > 0 ? _swordfirePoints[0].gameObject : gameObject)
            .SetAnimeTrigger("beamswordattack")
            .SetCancelableProgress(0);
            beamswordattack.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Attack2end);
            });
            _stateMachine.AddState(States.beamswordattack, beamswordattack);
            // スタン
            var stun = new Idle_LazyChange(Triggers.Event2.ToString(), 5, true);
            _stateMachine.AddState(States.stun, stun);
            // ショックウェーブ
            var shockwave = new ShootForward(_shockwaveBulletData, _shockwaveTargetLayer)
            .SetDirection(Vector2.left)
            .SetMuzzle(_swordfirePoints.Length > 0 ? _swordfirePoints[0].gameObject : gameObject)
            .SetAnimeTrigger("beamswordattack")
            .SetCancelableProgress(0);
            // 弾発射完了時にショックウェーブ終了トリガーを発火
            shockwave.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Shockwaveend);
            });
            _stateMachine.AddState(States.shockwave, shockwave);
        }
        private SearchAssistanceMono _searchAssistance;
        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (IsMatchingState(States.idle) && _searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
        }

        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }
        private bool _halfHpTriggered = false;

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();

            // HPが半分以下になったら一度だけトリガー発火
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
                    }
                }
            }

            // beamswordattackステート中のみ判定
            if (IsMatchingState(States.beamswordattackmove) && _player != null && _YPositions != null)
            {
                // プレイヤーが_YPositionsのy座標を通過したら止める
                float targetX = _YPositions.transform.position.x;
                float playerX = _player.Transform.position.x;

                // 例えば「近い」判定（±0.5以内など）
                if (Mathf.Abs(playerX - targetX) < 0.5f)
                {
                    Debug.Log("プレイヤーがY座標を通過しました");
                    // ステート遷移（例：ジャンプや攻撃待機など）
                    _stateMachine.ChangeState(Triggers.moveend);
                }
            }

            // 見た目の向き変更など既存処理
            if (_player != null)
            {
                Direction = (_player.Transform.position - transform.position).normalized;
                if (Direction.x != 0)
                {
                    var scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * (Direction.x > 0 ? 1 : -1);
                    transform.localScale = scale;
                }
            }
        }

        private void Attackselect()
        {
            if (currentAttack == 1)
            {
                Attack1();
                currentAttack = 2;
                return;
            }
            else if (currentAttack == 2)
            {
                Attack2();
                currentAttack = 1;
            }
        }
        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
        void Attack1()
        {
            Debug.Log("レーザー攻撃");
            _stateMachine.ChangeState(Triggers.Attack1);
        }

        void Attack2()
        {
            Debug.Log("ビームソード接近");
            _stateMachine.ChangeState(Triggers.Attack2);
        }
    }

}

