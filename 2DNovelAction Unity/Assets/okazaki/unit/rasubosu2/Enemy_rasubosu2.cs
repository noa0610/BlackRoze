using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using HighElixir;
using System.Collections.Generic;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_rasubosu2 : UnitBase
    {
        [SerializeField] private float closeRangeDistance = 5f;            // 近距離判定の距離

        [Header("ワープ移動")]
        [SerializeField] private float _WarpMovingRange = 5f;              // ワープ移動する範囲(初期位置の前後距離)
        [SerializeField] private float _WarpIntervalTime = 2f;             // ワープ移動間隔時間
        [SerializeField] private float _WarpStartTime = 0.5f;              // ワープ遷移から実際にワープするまでの時間
        private int _WarpCount = 0;                                        // ワープ回数（初期値0）
        [SerializeField] private float _WarpCoolTime = 0;                  // ワープのクールタイム
        [SerializeField] private float _WarpTimer = 0;                     // 遷移からワープを行うまでの計測時間



        [Header("ポインタミサイル")]
        [SerializeField] private Transform[] _MissileFallPoint;            // ミサイル落下地点
        [SerializeField] private BulletData _MIssilBulletDate;

        [Header("連続ワープショット")]
        [SerializeField] private float _WarpDistance = 4f;                 // 連続ワープショットのワープ先のプレイヤーとの距離
        [SerializeField] private GameObject _ShotPoint;                    // 連続ワープショット発射位置
        [SerializeField] private BulletData _ShotBulletDate;

        [Header("クロスウェーブ")]
        [SerializeField] private Transform _CrossWaveWarpPoint;

        [Header("攻撃相手")]
        [SerializeField] private LayerMask _AttackTargetLayer;

        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetDict<States>();
        private SearchAssistanceMono _searchAssistance;
        private UnitBase _player;


        protected override void AfterAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
            _stateMachine.ChangeState(Triggers.Event1);
        }

        protected override void Start()
        {
            base.Start();
        }

        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (IsMatchingState(States.warpidle) && _searchAssistance.Execute("ShortDistance", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.Event1);
                Debug.Log($"{_player.name}");
            }
        }

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();
            if (IsMatchingState(States.warpidle) && _player != null)
            {
                TurnAround();
                if (_WarpCoolTime <= _WarpIntervalTime)
                {
                    _WarpCoolTime += Time.fixedDeltaTime;
                }
                else
                {
                    Debug.Log("Change");
                    _WarpCount++;
                    _stateMachine.ChangeState(Triggers.Warpcooldown);
                }
            }

            if (IsMatchingState(States.beforewarp))
            {
                _WarpTimer += Time.fixedDeltaTime;
                if (_WarpTimer <= _WarpStartTime) return;
                warp.SetPos(SetGroundWarpPointRandom());
                _stateMachine.ChangeState(Triggers.WarpStart);
            }
        }
        
        // ワープ位置を決定する
        private Vector2 SetGroundWarpPointRandom()
        {
            Vector2 warpPoint = new(5, 0);
            

            return warpPoint;
        }

        private void TurnAround()
        {
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

        private void AttackSelect()
        {
            if (_player == null) return;
            float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

            if (distanceToPlayer <= closeRangeDistance)
            {
                int attackIndex1 = Random.Range(0, 2); // 0〜1 の間でランダム

                switch (attackIndex1)
                {
                    case 0:
                        Attack4();
                        break;
                    case 1:
                        Attack5();
                        break;
                }
            }
            int attackIndex = Random.Range(0, 4); // 0〜3 の間でランダム

            switch (attackIndex)
            {
                case 0:
                    Attack1();
                    break;
                case 1:
                    Attack2();
                    break;
                case 2:
                    Attack3();
                    break;
                case 3:
                    Attack5();
                    break;
            }
        }

        void Attack1()
        {
            Debug.Log("ポインターミサイル");
            _stateMachine.ChangeState(Triggers.Attack1);
        }

        void Attack2()
        {
            Debug.Log("クロスウェーブ");
            _stateMachine.ChangeState(Triggers.Attack2);
        }
        void Attack3()
        {
            Debug.Log("ワープショット");
            _stateMachine.ChangeState(Triggers.Attack3);
        }
        void Attack4()
        {
            Debug.Log("グラップルスラッシュ");
            _stateMachine.ChangeState(Triggers.Attack4);
        }
        void Attack5()
        {
            Debug.Log("フラッシュビームソード");
            _stateMachine.ChangeState(Triggers.Attack5);
        }



        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
    }
}
