using UnityEngine;

namespace Test
{
    public class Enemy_Turret : UnitBase
    {
        // === Data ===
        [SerializeField] private BulletObject _bulletObject;
        [SerializeField] private float _rotateSpeed;
        [SerializeField] private float _shootInterval;
        [SerializeField] private int _shootFireCount;       // 1サイクルあたりの連射数
        [SerializeField] private float _detectionDistance;

        // === Reference ===
        private ISearch _searchAssistance;

        // === Internal ===
        private float _shootIntervalCount = 0f;  // 待機タイマー
        private int _shootCount;            // 現在までに撃ったカウント

        // === StateMachine ===
        protected override IState DefaultState => new Idle_Rotate(transform, _rotateSpeed);

        // ステート登録
        protected override void RegisterStats()
        {
            _searchAssistance = new SearchAssistance();

            // 「shoot」ステート…弾を１発撃つ (ShootForward内でOnShootComplete呼ぶ)
            var shoot = new ShootForward(_bulletObject, 0);
            shoot.OnShootComplete += OnShootComplete;
            _stateMachine.AddState("shoot", shoot);

            // インターバル用ステート…何もしないIdle
            _stateMachine.AddState("shootInterval", new Idle());

            // デフォルト状態はIdle_Rotate("idle")のまま

            _searchAssistance.AddComp("tag", new FilterByTag(UnitTags.Player));
            _searchAssistance.AddComp("range", new FilterByXDistance(this, _detectionDistance));
        }

        private void Update()
        {
            // ■ インターバルカウントダウン ■
            if (_shootIntervalCount > 0f)
                _shootIntervalCount = Mathf.Max(0f, _shootIntervalCount - Time.deltaTime);

            SearchPlayer();           // プレイヤー検出＆セットアップ
            _stateMachine.Update();   // ステートを更新
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
                SetBullet(result[0]);
            }
        }

        private void SetBullet(IUnit target)
        {
            // 弾をセット
            var clone = _bulletObject.Clone();
            clone.currentstatus.direction =
                (target.Transform.position - transform.position).normalized;

            var shootState = (ShootForward)_stateMachine.StateMap["shoot"];
            shootState.SetBullet(clone);
        }
        // ステート遷移判定
        protected override string StateDecision()
        {
            // 1) インターバル中は必ずshootInterval
            if (_shootIntervalCount > 0f)
                return "shootInterval";

            // 2) プレイヤー見つかってて、インターバル終了ならshoot
            if (_stateFlags.HasFlag(StateFlags.InShoot))
                return "shoot";

            // 3) それ以外は回転待機(idle)
            return "idle";
        }

        // ShootForward が１発撃ち終わるたびに呼ばれる
        private void OnShootComplete()
        {
            _shootCount++;
            if (_shootCount >= _shootFireCount)
            {
                // n発撃ったらインターバルスタート
                _shootIntervalCount = _shootInterval;
                _shootCount = 0;
            }
        }
    }
}//unicode
