using UnityEngine;

namespace BlackRose
{
    public class Enemy_huyuurobo_1 : UnitBase
    {
        // === Data ===
        [SerializeField] private BulletObject _bulletObject;
        [SerializeField] private float _shootInterval = 100f;
        [SerializeField] private int _shootFireCount;       // 1サイクルあたりの連射数
        [SerializeField] private float _detectionDistance;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        // === Reference ===
        private ISearch _searchAssistance;

        // === Internal ===
        private float _shootIntervalCount = 0f;  // 待機タイマー
        private float _trishootInterval; // インターバルカウント
        private int _shootCount;            // 現在までに撃ったカウント

        // === StateMachine ===
        protected override IState DefaultState => new Idle();

        // ステート登録
        protected override void RegisterStats()
        {
            _searchAssistance = new SearchAssistance();

            var shoot = new ShootForward(_bulletObject, _targetLayer, "");
            shoot.onShootComplete += OnShootComplete;
            _stateMachine.AddState("shoot", shoot);


            _stateMachine.AddState("shootInterval", new Idle());
            _searchAssistance.AddComp("tag", new FilterByTag(UnitTags.Player));
            _searchAssistance.AddComp("range", new FilterByXDistance(this, _detectionDistance));
            var status = statusManager.GetStatusAmount(Status.Speed);// このキャラクターの移動速度を取得
            var state = new MoveOnGround(_rigidbody2D, "ここにアニメーション", status); // 移動状態を定義
            _stateMachine.AddState("move", state); // ステートマシンに移動状態を追加
        }

        protected override void Update()
        {
            // ■ インターバルカウントダウン ■
            if (_shootIntervalCount > 0f)
                _shootIntervalCount = Mathf.Max(0f, _shootIntervalCount - Time.deltaTime);
            if (_trishootInterval > 0f)
                _trishootInterval = Mathf.Max(0f, _trishootInterval - Time.deltaTime);

            SearchPlayer();           // プレイヤー検出＆セットアップ
            base.Update();
        }

        private void SearchPlayer()
        {
            _stateFlags &= ~StateFlags.InShoot;
            var list = UnitManager.instance.GetUnitList();
            var result = _searchAssistance.Execute(list);
            if (result != null && result.Count > 0)
            {
                _stateFlags |= StateFlags.InShoot;
                // 最短距離のプレイヤーを狙う
                result.Sort((a, b) =>
                {
                    var diffA = a.Transform.position - transform.position;
                    var diffB = b.Transform.position - transform.position;
                    return diffA.sqrMagnitude
                        .CompareTo(diffB.sqrMagnitude);
                });
                if (result[0].Transform.position.x < transform.position.x)
                {
                    Direction = new Vector2(-1, 0); // 左向き
                }
                else
                {
                    Direction = new Vector2(1, 0); // 右向き
                }
                SetBullet(result[0]);
            }
        }

        private void SetBullet(IUnit target)
        {
            // 弾をセット
            var clone = _bulletObject.Clone();
            clone.currentstatus = clone.bulletData.originalstatus;
            clone.currentstatus.direction =
                (target.Transform.position - transform.position).normalized;

            var shootState = (ShootForward)_stateMachine.StateMap["shoot"];
            shootState.SetBullet(clone);
        }
        // ステート遷移判定
        protected override string StateDecision()
        {
            // 1) インターバル中は必ずshootInterval
            if (_shootIntervalCount > 0f || _trishootInterval > 0f)
                return "shootInterval";

            // 2) プレイヤー見つかってて、インターバル終了ならshoot
            if (_stateFlags.HasFlag(StateFlags.InShoot))
                return "shoot";

            // 3) それ以外は回転待機(idle)
            // return "idle";
            return "move";
        }

        // ShootForward が１発撃ち終わるたびに呼ばれる
        private void OnShootComplete()
        {
            _trishootInterval = 0.3f;
            _shootCount++;
            if (_shootCount >= _shootFireCount)
            {
                // n発撃ったらインターバルスタート
                _shootIntervalCount = _shootInterval;
                _shootCount = 0;
            }
        }
        protected override void Awake()
        {
            base.Awake();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            if (_rigidbody2D == null)
            {
                Debug.LogError("Rigidbody2D component is missing on the Enemy_Test GameObject.");
            }
            Direction = Vector2.right; // 初期方向を右に設定
        }
    }
}