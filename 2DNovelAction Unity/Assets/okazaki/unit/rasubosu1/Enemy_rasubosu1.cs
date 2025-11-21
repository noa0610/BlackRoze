using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using UnityEngine;
using HighElixir;
using System.Collections.Generic;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Objects;
using System.Collections;
using UnityEditor.U2D.Animation;


namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]

    public partial class Enemy_rasubosu1 : UnitBase
    {

        [Header("デバッグ")]
        [Tooltip("攻撃選択の固定化(１，アームパンチ ２，拡散ビーム砲 ３，ファイアウォール)")]
        [SerializeField] private int FixedAttackSelect = 0;                // 攻撃選択の固定化

        [Tooltip("登場演出の省略")]
        [SerializeField] private bool cutEntry = false;

        // [Header("固有処理")]

        

        [Header("登場演出")]
        [SerializeField] private float _EntryEndwaitTime = 4.5f;           // 登場アニメーション終了時間（手動必須になる）


        [Header("アームパンチ")]
        [SerializeField] private BulletData _armpunchBulletData;
        [SerializeField] private int _armPunchCount = 2;
        
        [Tooltip("空のオブジェクトのプレハブなら何でもOK")]
        [SerializeField] private GameObject EmptyObject;          // 位置指定用の空のプレハブ
        [SerializeField] private float _ArmHeightOfFall = 10f;    // パンチを落とす高さ
        [SerializeField] private float _ArmWidthFall = 5f;        // パンチを落とす範囲

        [Tooltip("アームパンチ開始 → パンチ落下")]
        [SerializeField] private float _ArmPunchStartTime = 2.5f;

        [Tooltip("パンチ落下 → 次のパンチ")]
        [SerializeField] private float _ArmPunchWaitTime = 1.5f;

        [Tooltip("パンチ終了 → 攻撃待機へ")]
        [SerializeField] private float _ArmPunchEndTime = 4f;
        private GameObject _armpunchPoint;
        private Vector3 _startArmPunchPos;


        [Header("拡散ビーム砲")]
        [SerializeField] private BulletData _SpreadShotBulletData;
        [SerializeField] private GameObject _SpreadShotPoint;

        [Tooltip("発射 → 攻撃待機へ")]
        [SerializeField] private float _SpreadShotEndTime = 2f;


        [Header("ファイアウォール")]
        [SerializeField] private BulletData _firewallBulletData;
        [SerializeField] private GameObject _biribiriPoint;
        [SerializeField] public GameObject _fireWallArmprefab;
        [SerializeField] public Transform point;      // アーム発射位置
        [SerializeField] public float speed = 5f;     // 移動速度（右->左なので Vector3.left を使う）
        [SerializeField] public float lifetime = 10f; // 自動破棄までの時間（秒）

        /*
        課題点　ファイアウォール
        １，ファイアウォールから落下させるDamageFloorでダメージ床が生成されない（エラーが発生中）
        解決法→DamageFloorを削除する？　そもそもタイルマップと相性悪？
        　
        ２，アームとファイアウォールの生成位置の見た目上のズレ
        解決法→アーム専用のファイアウォールBulletの数値設定用スクリプトを作成？

        ３，次の生成までの時間設定
        解決法→アーム専用のファイアウォールBulletの数値設定用スクリプトを作成？
        */


        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
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
            InitArmPunchFallPoint();
        }
        protected override void AfterAwake()
        {
            if (cutEntry)
            {
                _stateMachine.Awake("attackidle", false);
                _animator.SetTrigger("toIdle");
            }
            else
            {
                _stateMachine.Awake("entry", false);
            }
        }

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();
            // beamswordattackステート中のみ判定

        }

        #region === AttackSelect ===
        public void AttackSelect()
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
                    case 3:
                        Attack3();
                        break;
                }
                return;
            }

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
        #endregion

        #region === ArmPunch ===
        private void InitArmPunchFallPoint()
        {
            _armpunchPoint = Instantiate(EmptyObject);
            Transform cameraPos = Camera.main.transform;
            float centerPosx = cameraPos.localPosition.x;
            _armpunchPoint.transform.position = new Vector2(centerPosx, _ArmHeightOfFall);
            _startArmPunchPos = _armpunchPoint.transform.localPosition;
        }

        
        // パンチの落下ポイントを決める
        private void RandomArmPunchFallPoint()
        {
            _armpunchPoint.transform.position = new Vector2(Random.Range(_startArmPunchPos.x - _ArmWidthFall, _startArmPunchPos.x + _ArmWidthFall), _ArmHeightOfFall);
        }
        #endregion

        #region === FireWall ===
        void FireWallArmMove()
        {
            if (_fireWallArmprefab == null || point == null)
            {
                Debug.LogWarning("prefab または point が設定されていません。");
                return;
            }

            // point 位置にプレハブ生成
            GameObject obj = Instantiate(_fireWallArmprefab, point.position, point.rotation);

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
        #endregion

        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }

    }
}