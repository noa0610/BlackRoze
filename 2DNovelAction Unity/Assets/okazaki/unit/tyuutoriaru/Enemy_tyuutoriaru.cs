using UnityEngine;
using System.Collections.Generic;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;


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
            beamswordapproaching, // ビームソード接近
            beamswordattack, // ビームソード攻撃
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

        [SerializeField] private float closeRangeDistance = 5f; // 近距離判定の距離
        private Transform playerTransform;
        private int currentAttack = 1; // 初期値は1（アタック1）
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetDict<States>();
        [SerializeField] private Transform[] _firePoints;
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Rigidbody2D _RB2;
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
                (Triggers.Attack1, States.lasershot),              // 攻撃１でレーザー攻撃へ
                (Triggers.Attack2, States.beamswordattack),   // 攻撃２でビームソード接近へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
            };
            var lasershotTrigger = new[]                           // レーザー攻撃ステートのトリガー
            {
                (Triggers.Attack1end, States.attackidle),                // 攻撃１終了で攻撃待機へ
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
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.lasershot, lasershotTrigger)
                .AddTransmissions(States.beamswordattack, beamswordattackTrigger)
                .AddTransmissions(States.fixedpositionjump, fixedpositionjumpTrigger)
                .AddTransmissions(States.stun, stunTrigger)
                .AddTransmissions(States.shockwave, shockwaveTrigger);

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
            var fixedpositionjump = new PositionJump(jumpPositions, 1.0f)
                .SetAnimeTrigger("fixedpositionjump")
                .SetCancelableProgress(0);
            _stateMachine.AddState(States.fixedpositionjump, fixedpositionjump);
            // 攻撃待機
            var attackIdle = new Idle_LazyEvent(5f).SetAnimeTrigger("attackidle").SetCancelableProgress(0);
            attackIdle.LazyEvent.AddListener(Attackselect);
            _stateMachine.AddState(States.attackidle, attackIdle);
            // レーザー攻撃
            var lasershot = new LaserShot(_RB2, _firePoints, _bulletPrefab).SetAnimeTrigger("lasershot").SetCancelableProgress(0);
            _stateMachine.AddState(States.lasershot, lasershot);
            // ビームソード攻撃
            var beamswordattack = new Idle().SetAnimeTrigger("beamswordattack").SetCancelableProgress(0);
            _stateMachine.AddState(States.beamswordattack, beamswordattack);
            // スタン
            var stun = new Idle_LazyChange(Triggers.Event2.ToString(), 5, true);
            _stateMachine.AddState(States.stun, stun);
            // ショックウェーブ
            var shockwave = new Idle().SetAnimeTrigger("shockwave").SetCancelableProgress(0);
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
        protected override void AfterFixedUpdate()
        {
            SearchPlayer();
            if (_player != null)
            {
                Direction = (_player.Transform.position - transform.position).normalized;

                // 見た目の向きを変更（左右反転）
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
        // private void Junpjudgement()
        // {
        //     if (playerTransform.position.x > _centerPositions.transform.position.x)
        //     {
        //         _stateMachine.ChangeState(Triggers.[0].name);
        //     }
        //     else
        //     {
        //         _stateMachine.ChangeState(Triggers.Landing);
        //     }

        // }

    }
}

