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

    public partial class Enemy_rasubosu1 : UnitBase
    {
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
        [SerializeField] private BulletData _diffusebeamgunBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _diffusebeamgunTargetLayer;
        [SerializeField] private GameObject _diffusebeamgunPoint;// 必要ならInspectorでセット
        [SerializeField] private BulletData _armpunchBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _armpunchTargetLayer;
        [SerializeField] private GameObject _armpunchPoint;// 必要ならInspectorでセット
        [SerializeField] private BulletData _firewallBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _firewallTargetLayer;
        [SerializeField] private GameObject _firewallPoint;// 必要ならInspectorでセット
        [SerializeField] private GameObject _biribiriPoint;// 必要ならInspectorでセット
        private int punchcount = 0;
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
        public GameObject prefab;    // インスペクタで割り当てるプレハブ
    public Transform point;      // インスペクタで割り当てる発射位置（point）
    public float speed = 5f;     // 移動速度（右->左なので Vector3.left を使う）
    public float lifetime = 10f; // 自動破棄までの時間（秒）

        void Udetobasi()
        {
            if (prefab == null || point == null)
            {
                Debug.LogWarning("prefab または point が設定されていません。");
                return;
            }

            // point 位置にプレハブ生成
            GameObject obj = Instantiate(prefab, point.position, point.rotation);

            // Rigidbody2D を取得
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 右→左に進む（X軸マイナス方向）
                rb.velocity = Vector2.left * speed;
            }
            else
            {
                Debug.LogWarning("生成したプレハブに Rigidbody2D がありません。");
            }
            

            // 一定時間後に自動削除
            Destroy(obj, lifetime);
        }
    
    }


}