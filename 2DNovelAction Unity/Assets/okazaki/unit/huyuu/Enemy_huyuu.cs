using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using HighElixir;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_huyuu : UnitBase
    {
        [SerializeField] private SuicideBombing _suicideBombing;
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();

        // 実装
        private SearchAssistanceMono _searchAssistance;


        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (IsMatchingState(States.move) && _searchAssistance.Execute("red", list, out _))
            {
                _stateMachine.ChangeState(Triggers.AttackRange);
            }
            if (IsMatchingState(States.idle) && _searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
            else if (!_searchAssistance.Execute("green", list, out _))
                _stateMachine.ChangeState(Triggers.MissingPlayer);

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

        protected override void OnDeath()
        {
            base.OnDeath();
            _stateMachine.ChangeState(Triggers.Died);
            UnitManager.instance.RemoveUnit(this);
            Destroy(gameObject);
        }

        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
    }
}