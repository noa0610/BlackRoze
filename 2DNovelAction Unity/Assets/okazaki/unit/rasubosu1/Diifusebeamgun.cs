using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using UnityEngine;
using HighElixir;
using System.Collections.Generic;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Objects;
using System.Collections;


namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]

    public partial class Diifusebeamgun : UnitBase
    {
        [Tooltip("同時シュートする際の最大角度")]
        [SerializeField, Min(0)] private float _range;
        [Tooltip("同時に発射する弾数")]
        [SerializeField, Min(1)] private int _shootCount;
        [SerializeField] private MultiShoot _multiShoot;
        private SearchAssistanceMono _searchAssistance;
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
        protected override void AfterFixedUpdate()
        {
            SearchPlayer();
            // beamswordattackステート中のみ判定

        }
        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (IsMatchingState(States.idle) && _searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
        }
        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        // Start is called before the first frame update
    }
}
