using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using UnityEngine;
using HighElixir;
using System.Collections.Generic;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Objects;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]

    // 拡散ショットの弾のUnit
    public partial class Diifusebeamgun : UnitBase
    {
        [Tooltip("同時シュートする際の最大角度")]
        [SerializeField, Min(0)] private float _range;
        [Tooltip("同時に発射する弾数")]
        [SerializeField, Min(1)] private int _shootCount;
        [SerializeField] private MultiShoot _multiShoot;

        [Header("SE")]
        [SerializeField] private string _SpreadSEName = "拡散";
        [SerializeField] private float _SpreadSEVolume = 0.5f;

        // private SearchAssistanceMono _searchAssistance;
        // private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
        protected override void AfterFixedUpdate()
        {
            // SearchPlayer();
            // beamswordattackステート中のみ判定
            if(IsMatchingState(States.dead))
            {
                OnDeath();
            }
        }
        // private void SearchPlayer()
        // {
        //     // var list = UnitManager.instance.GetUnitList();
        //     // if (IsMatchingState(States.idle) && _searchAssistance.Execute("yellow", list, out var units))
        //     // {
        //     //     _player = units.GetUnitNearest(transform.position);
        //     //     _stateMachine.ChangeState(Triggers.FoundPlayer);
        //     // }
        // }
        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
        // protected override void BeforeAwake()
        // {
        //     _searchAssistance = GetComponent<SearchAssistanceMono>();
        // }

        protected override void OnDeath()
        {
            base.OnDeath();
            UnitManager.instance.RemoveUnit(this);
            // await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            Debug.Log($"{gameObject.name} : RemoveUnit");
            Destroy(gameObject);
        }

        // Start is called before the first frame update
    }
}
