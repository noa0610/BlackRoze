using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using UnityEngine;
using HighElixir;
using System.Collections.Generic;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Objects;


namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]

    public partial class Enemy_rasubosu1 : UnitBase
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private GameObject bulletPrefab;
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
        [SerializeField] private BulletData _firewallBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _firewallTargetLayer; // 必要ならInspectorでセット
        [SerializeField] private Transform[] _firewallPoints;// 必要ならInspectorでセット
        [SerializeField] private BulletData _diffusebeamgunBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _diffusebeamgunTargetLayer;
        [SerializeField] private GameObject _diffusebeamgunPoint;// 必要ならInspectorでセット
        [SerializeField] private BulletData _armpunchBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _armpunchTargetLayer;
        [SerializeField] private GameObject _armpunchPoint;// 必要ならInspectorでセット
        private int punchcount = 0;
        [SerializeField] private DamageFloorMaker _damageFloorMaker; // Inspector でセット
    [SerializeField] private float _damageFloorDuration = 3f;    // ダメージ床の持続時間
    [SerializeField] private float _damageFloorLength = 5f;      // ダメージ床の最大長さ
    [SerializeField] private LayerMask _raycastLayer;            // Raycast用レイヤー


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
        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
        public void Attackjudgement()
        {
            int choice = Random.Range(0, 3);
            switch (choice)
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
            }
        }

        void Attack1()
        {
            Debug.Log("アームパンチ");
            _stateMachine.ChangeState(Triggers.Attack1);
        }

        void Attack2()
        {
            Debug.Log("拡散ビーム砲");
            _stateMachine.ChangeState(Triggers.Attack2);
        }
        void Attack3()
        {
            Debug.Log("ファイアウォール");
            _stateMachine.ChangeState(Triggers.Attack3);
        }

        void firewallRayCast()
        {
            if (_damageFloorMaker == null) return;

        foreach (var point in _firewallPoints)
        {
            // 下方向に Raycast を打つ
            RaycastHit2D hit = Physics2D.Raycast(
                point.position + Vector3.up * 2f, 
                Vector2.down, 
                10f, 
                _raycastLayer
            );

            if (hit.collider != null)
            {
                // 床を生成（ヒットした位置を起点に）
                _damageFloorMaker.Create(
                    hit.point,           // 生成位置
                    _damageFloorDuration,// 持続時間
                    _damageFloorLength   // 最大長さ
                );
            }
        }
        }
    }

}