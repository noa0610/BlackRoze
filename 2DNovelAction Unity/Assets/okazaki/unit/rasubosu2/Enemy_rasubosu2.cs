using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using HighElixir;
using System.Collections.Generic;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_rasubosu2 : UnitBase
    {
        [SerializeField] private float closeRangeDistance = 5f; // 近距離判定の距離
        [SerializeField] private Transform[] _MissileFallPoint;    // ミサイル落下地点
        [SerializeField] private float _WarpIntervalTime = 1f;  // ワープ移動間隔時間
        [SerializeField] private GameObject _ShotPoint;         // 連続ワープショット発射位置
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



        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }

        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (IsMatchingState(States.idle) && _searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.Event1);
            }
        }
        
        protected override void AfterFixedUpdate()
        {
            SearchPlayer();
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
            // beamswordattackステート中のみ判定

        }

        private void Attackjudgement()
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
    }
}
