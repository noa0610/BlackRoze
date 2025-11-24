using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using HighElixir;
using System.Collections.Generic;
using System;
using BlackRose.Core.Models.Objects;
using System.Collections;
using BlackRose.Core.Models.Systems;


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

        [Tooltip("攻撃遷移の停止")]
        [SerializeField] private bool attackStop = false;

        [Header("固有処理")]
        [SerializeField] private float _AttackIntervalTime = 2f;

        [Header("登場演出")]
        [SerializeField] private float _EntryEndwaitTime = 4.5f;           // 登場アニメーション終了時間（手動必須になる）
        [SerializeField] private FlowchartFirer _EntryEventFlowFirer;


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
        [SerializeField] public GameObject _fireWallArmprefab;
        [SerializeField] public Transform _ArmInitpoint;      // アーム発射位置
        [SerializeField] public float _fireWallArmMovespeed = 5f;
        [SerializeField] public float lifetime = 10f; // 自動破棄までの時間（秒）

        [Tooltip("ファイアウォール開始 → 攻撃直前へ")]
        [SerializeField] private float _fireWallStartTime = 2f;

        [Tooltip("攻撃直前 → アーム生成")]
        [SerializeField] private float _fireWallWaitTime = 0.5f;

        [Tooltip("アーム生成 → ファイアウォール終了")]
        [SerializeField] private float _fireWallEndTime = 6f;


        [Header("死亡状態")]
        [SerializeField] private float _DeadEndwaitTime = 6.5f;           // 死亡アニメーション終了時間（手動必須になる）
        [SerializeField] private GameObject _DeadPartecl; // 死亡時のエフェクト
        [SerializeField] private float _DeadParteclTime = 4f;  // エフェクト発生時間
        [SerializeField] private GameObject _DeadExplosionPartecl; // 死亡時の爆破エフェクト
        [SerializeField] private Transform _LeftArmPoint;
        [SerializeField] private Transform _RightArmPoint;

        [Header("SE")]
        [SerializeField] private string _EntrySEName = "ロボット起動";
        [SerializeField] private float _EntrySEVolume = 0.5f;
        [SerializeField] private string _ArmFireSEName = "アーム発射";
        [SerializeField] private float _ArmFireSEVolume = 0.5f;
        [SerializeField] private string _ArmReturnSEName = "アーム戻り";
        [SerializeField] private float _ArmReturnSEVolume = 0.5f;
        [SerializeField] private string _ArmFallSEName = "アーム落下";
        [SerializeField] private float _ArmFallSEVolume = 0.5f;
        [SerializeField] private string _ShotSEName = "ビーム砲";
        [SerializeField] private float _ShotSEVolume = 0.5f;
        [SerializeField] private string _FireWallSEName = "レーザー";
        [SerializeField] private float _FireWallSEVolume = 0.5f;
        [SerializeField] private string _DamageSEName = "敵ダメージ1";
        [SerializeField] private float _DamageSEVolume = 0.2f;
        [SerializeField] private string _DeadSEName = "撃破";
        [SerializeField] private float _DeadSEVolume = 0.4f;
        [SerializeField] private string _ExplosionSEName = "爆発";
        [SerializeField] private float _ExplosionSEVolume = 0.4f;
        [SerializeField] private string _ShutDownSEName = "シャットアウト";
        [SerializeField] private float _ShutDownSEVolume = 0.4f;


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
                IsInvincible = false;
            }
            else
            {
                _stateMachine.Awake("entry", false);
                IsInvincible = true;
            }
        }

        public override void Pause()
        {
            attackStop = true;
        }

        public override void Play()
        {
            attackStop = false;
        }

        private void EntryEnd()
        {
            IsInvincible = false;
            if (_EntryEventFlowFirer)
            {
                _EntryEventFlowFirer.Fire();
            }
        }

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();
        }

        protected override void OnTakeDamage(IUnit from, float damage)
        {
            PlaySE(_DamageSEName, _DamageSEVolume);
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            PlaySE(_DeadSEName, _DeadSEVolume);
            if (_DeadPartecl != null)
            {
                Destroy(
                    Instantiate(_DeadPartecl, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 2), Quaternion.identity, null),
                    _DeadParteclTime);
            }
            _stateMachine.ChangeState(Triggers.Died);
        }

        private async void Dead()
        {
            IsInvincible = true; // 攻撃不可

            await UniTask.Delay(TimeSpan.FromSeconds(_DeadEndwaitTime));

            UnitManager.instance.RemoveUnit(this); // UnitManagerの自データ削除

            Destroy(gameObject);

            // UniTaskエラー対策
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_DeadEndwaitTime));
            }
            catch (OperationCanceledException)
            {
                // キャンセルされたら何もしない
                return;
            }
            // オブジェクトが既に破棄されていたら続行しない
            if (this == null) return;

            UnitManager.instance.RemoveUnit(this);
            if (this != null) Destroy(gameObject);
        }

        #region === AttackSelect ===
        public void AttackSelect()
        {
            if (attackStop) return;

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

            int choice = UnityEngine.Random.Range(0, 3);
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
        // パンチ落下範囲の中央位置の初期化設定
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
            if(_player != null)
            {
                _armpunchPoint.transform.position = new Vector2(_player.transform.position.x, _ArmHeightOfFall);
            }
            _armpunchPoint.transform.position = new Vector2(UnityEngine.Random.Range(_startArmPunchPos.x - _ArmWidthFall, _startArmPunchPos.x + _ArmWidthFall), _ArmHeightOfFall);
        }
        #endregion

        #region === FireWall ===
        // アームを移動
        void FireWallArmMove()
        {
            if (_fireWallArmprefab == null || _ArmInitpoint == null)
            {
                Debug.LogWarning("prefab または point が設定されていません。");
                return;

            }

            // point 位置にプレハブ生成
            GameObject obj = Instantiate(_fireWallArmprefab, _ArmInitpoint.position, _ArmInitpoint.rotation);
            PlaySE(_FireWallSEName, _FireWallSEVolume);

            // Rigidbody2D を取得
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 右に進む
                rb.velocity = Vector2.right * _fireWallArmMovespeed;
            }
            else
            {
                Debug.LogWarning("生成したプレハブに Rigidbody2D がありません。");
            }
            // 一定時間後に自動削除
            Destroy(obj, lifetime);
        }
        #endregion

        private void EntrySE()
        {
            PlaySE(_EntrySEName, _EntrySEVolume);
        }

        private void ArmFireSE()
        {
            PlaySE(_ArmFireSEName, _ArmFireSEVolume);
        }

        private void ArmReturnSE()
        {
            PlaySE(_ArmReturnSEName, _ArmReturnSEVolume);
        }

        private void ExplosionSE()
        {
            PlaySE(_ExplosionSEName, _ExplosionSEVolume);
        }

        private void ShatDownSE()
        {
            PlaySE(_ShutDownSEName, _ShutDownSEVolume);
        }


        private void ArmLeftExplosionEffect()
        {
            if (_DeadExplosionPartecl != null)
            {
                Destroy(
                    Instantiate(_DeadExplosionPartecl, new Vector3(_LeftArmPoint.position.x, _LeftArmPoint.position.y, _DeadExplosionPartecl.transform.position.z), Quaternion.identity, null),
                    _DeadParteclTime);
            }
        }

        private void ArmRightExplosionEffect()
        {
            if (_DeadExplosionPartecl != null)
            {
                Destroy(
                    Instantiate(_DeadExplosionPartecl, new Vector3(_RightArmPoint.position.x, _RightArmPoint.position.y, _DeadExplosionPartecl.transform.position.z), Quaternion.identity, null),
                    _DeadParteclTime);
            }
        }

        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
    }
}