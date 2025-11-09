using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using HighElixir;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_rasubosu2 : UnitBase
    {
        [Tooltip("攻撃選択の固定化(１，ポインタミサイル ２，クロスウェーブ ３，連続ワープショット ４，一閃ビームソード)")]
        [SerializeField] private int FixedAttackSelect = 0;                // 攻撃選択の固定化

        [Header("固有設定")]
        [SerializeField] private float closeRangeDistance = 5f;            // 近距離判定の距離

        [Header("ワープ移動")]
        [SerializeField] private float _WarpMovingRange = 5f;              // ワープ移動する範囲(初期位置の前後距離)
        [SerializeField] private float _WarpIntervalTime = 2f;             // ワープ移動間隔時間
        [SerializeField] private float _WarpBecomeInvincibleTime = 0.5f;   // ワープ遷移から無敵時間に移行するまでの時間
        [SerializeField] private float _WarpStartTime = 0.5f;              // 無敵時間以降から実際にワープするまでの時間
        [SerializeField] private float _WarpInvincibleTime = 2f;           // ワープの無敵時間
        [SerializeField] private int _WarpCount = 3;                       // ワープする回数
        private int _CurrentWarpCount = 0;

        // [SerializeField] private float _WarpCoolTime = 0;                  // ワープのクールタイム
        // [SerializeField] private float _WarpTimer = 0;                     // 遷移からワープを行うまでの計測時間
        // private string _prevStateKey;

        private int _PreviousAttackTipe = 0;                               // 前回の攻撃の種類



        [Header("ポインタミサイル")]
        [SerializeField] private GameObject[] _MissileFallPoint;            // ミサイル落下地点
        [SerializeField] private BulletData _MissileBulletDate;
        private Vector2 direction = Vector2.down;

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

        protected async override void AfterFixedUpdate()
        {
            SearchPlayer();

            if (IsMatchingState(States.beforewarp))
            {

            }

            if (IsMatchingState(States.warp))
            {
                WarpUpdate();
            }
        }

        protected override void OnTakeDamage(IUnit from, float damage)
        {
            Debug.Log("TakeDamage");
        }

        // ワープの前隙のディレイ⇒無敵時間のコルーチン開始⇒Warpに遷移
        private async void WarpIdleExit()
        {
            Debug.Log($"CurrentWarpCount : {_CurrentWarpCount}");
            if (_CurrentWarpCount >= _WarpCount)
            {
                _stateMachine.ChangeState(Triggers.Warpcomplete);
            }

            TurnAround();
            await UniTask.Delay(TimeSpan.FromSeconds(_WarpBecomeInvincibleTime));
            Invincible(_WarpInvincibleTime);
            await UniTask.Delay(TimeSpan.FromSeconds(_WarpStartTime));
            warp.SetPos(SetGroundWarpPointRandom());
            _CurrentWarpCount++;
            _stateMachine.ChangeState(Triggers.WarpStart);
        }

        private void WarpEnter()
        {
            TurnAround();
        }
        // ワープ後無敵時間解除でワープ待機に戻る
        private void WarpUpdate()
        {
            if (IsInvincible == false)
            {
                _stateMachine.ChangeState(Triggers.Warpend);
            }
        }

        // 地上のワープ先を決定
        private Vector2 SetGroundWarpPointRandom()
        {
            Transform cameraPos = Camera.main.transform;
            float warpPosX = UnityEngine.Random.Range(cameraPos.localPosition.x - _WarpMovingRange, cameraPos.localPosition.x + _WarpMovingRange);
            Vector2 warpPoint = new Vector2(warpPosX, Transform.position.y);
            return warpPoint;
        }

        // 無敵時間開始コルーチン
        private async void Invincible(float invincibleTime)
        {
            Debug.Log("Invincible Start");
            IsInvincible = true;
            await UniTask.Delay(TimeSpan.FromSeconds(invincibleTime));
            Debug.Log("Invincible End");
            IsInvincible = false;
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
                    case 4:
                        Attack4();
                        break;
                }
                return;
            }

            if (distanceToPlayer <= closeRangeDistance)
            {
                int attackIndex1 = UnityEngine.Random.Range(0, 2); // 0〜1 の間でランダム

                switch (attackIndex1)
                {
                    case 0:
                        Attack2();
                        break;
                    case 1:
                        Attack4();
                        break;
                }
            }
            int attackIndex = UnityEngine.Random.Range(0, 4); // 0〜3 の間でランダム

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
                    Attack4();
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
            Debug.Log("フラッシュビームソード");
            _stateMachine.ChangeState(Triggers.Attack4);
        }



        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
    }
}
