using UnityEngine;

namespace BlackRose
{
    public class Enemy_Test_1 : UnitBase
    {
        [SerializeField] private BulletObject _bulletObject;//#Y------------------------------------------------------ 弾のプレハブ
        [SerializeField] private Rigidbody2D _rigidbody2D;//#Y-------------------------------------------------------- Rigidbody2Dコンポーネント
        [SerializeField] private float _jumpInterval = 2f; //#Y------------------------------------------------------- ジャンプ間隔
        [SerializeField] private float _JumpTimer = 0f; //#Y---------------------------------------------------------- ジャンプタイマー
        [SerializeField] private ISearch _searchAssistance; //#Y------------------------------------------------------ 検知支援
        [SerializeField] private float _detectionDistance; //#Y------------------------------------------------------- 検知距離
        [SerializeField] private LayerMask _targetLayer; //#Y--------------------------------------------------------- ターゲットレイヤー
        private Jump _jumpState; //#Y--------------------------------------------------------------------------------- ジャンプ状態

        protected override void RegisterStats()// #Y------------------------------------------------------------------ ステータスの登録
        {

            _searchAssistance = new SearchAssistance();// #Y---------------------------------------------------------- 検知支援の初期化

            var shoot = new ShootForward(_bulletObject, _targetLayer, "");// #Y--------------------------------------- 弾を前方に発射する状態
            var status = statusManager.GetStatusAmount(Status.Speed);// #Y-------------------------------------------- スピードステータスの取得
            var state = new MoveOnGround(_rigidbody2D, "ここにアニメーション", status);// #Y---------------------------- 地上での移動状態
            _searchAssistance.AddComp("tag", new FilterByTag(UnitTags.Player)); //#Y---------------------------------- タグによるフィルタリングを追加

            _jumpState = new Jump(_rigidbody2D, statusManager.GetStatusAmount(Status.JumpPower)); //#Y---------------- ジャンプ状態の初期化
            _stateMachine.AddState("move", state);// #Y--------------------------------------------------------------- 移動状態をステートマシンに追加
            _stateMachine.AddState("Jump", _jumpState);// #Y---------------------------------------------------------- ジャンプ状態をステートマシンに追加
            _searchAssistance.AddComp("range", new FilterByXDistance(this, _detectionDistance));// #Y----------------- 検知支援の範囲を追加
            _stateMachine.AddState("shoot", shoot);// #Y-------------------------------------------------------------- 発射状態をステートマシンに追加

        }

        protected override string StateDecision()//#Y----------------------------------------------------------------- ステート決定
        {
            if (_JumpTimer >= _jumpInterval)//#Y---------------------------------------------------------------------- もしジャンプタイマーがジャンプ間隔以上なら
            {
                _JumpTimer = 0f;// #Y--------------------------------------------------------------------------------- ジャンプタイマーをリセット
                _jumpState.HadLeapt = false;// #Y ジャンプ状態を更新
                return "Jump";// #Y ジャンプ状態に遷移
            }

            // if (_stateFlags.HasFlag(StateFlags.InShoot))
            //     return "shoot";
            // 3) それ以外は回転待機(idle)
            // return "idle";

            return "move";// #Y デフォルトは移動状態
        }

        protected override void Update()// #Y 更新処理
        {
            _JumpTimer = Mathf.Min(_JumpTimer + Time.deltaTime, _jumpInterval);// #Y ジャンプタイマーを更新
            base.Update();// #Y 基底クラスの更新処理を呼び出す
            SearchPlayer();// #Y プレイヤーを検索
        }

        protected override void Awake()// #Y 初期化処理
        {
            base.Awake();// #Y 基底クラスのAwakeを呼び出す
            _rigidbody2D = GetComponent<Rigidbody2D>();// #Y Rigidbody2Dコンポーネントを取得
            if (_rigidbody2D == null)// #Y もし、Rigidbody2Dが見つからない場合
            {
                Debug.LogError("Rigidbody2Dが見つかりません。Enemy_TestスクリプトをアタッチしたオブジェクトにRigidbody2Dコンポーネントを追加してください。");
            }
            Direction = Vector2.right; // #Y 初期方向を右に設定
        }

        private void SearchPlayer()// #Y プレイヤーを検索
        {
            _stateFlags &= ~StateFlags.InShoot;// #Y 発射状態フラグをクリア
            var list = UnitManager.instance.GetUnitList();// #Y ユニットマネージャーからユニットリストを取得
            var result = _searchAssistance.Execute(list);// #Y 検知支援を実行して結果を取得
            if (result != null && result.Count > 0)// #Y もし、検知結果が存在する場合
            {
                _stateFlags |= StateFlags.InShoot;// #Y 発射状態フラグをセット
                // 最短距離のプレイヤーを狙う
                result.Sort((a, b) =>// #Y 検知結果を距離でソート
                {
                    var diffA = a.Transform.position - transform.position;// #Y aの位置と敵の位置の差
                    var diffB = b.Transform.position - transform.position;// #Y bの位置と敵の位置の差
                    return diffA.sqrMagnitude// #Y aの距離の二乗と
                        .CompareTo(diffB.sqrMagnitude);// #Y bの距離の二乗を比較
                });
                if (result[0].Transform.position.x < transform.position.x)// #Y もし、最短距離のプレイヤーのX座標が敵のX座標より小さい場合
                {
                    Direction = new Vector2(-1, 0); //#Y 左向き
                }
                else
                {
                    Direction = new Vector2(1, 0); // 右向き
                }
            }
        }
    }
}


