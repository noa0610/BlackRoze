using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using Fungus;
using HighElixir;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_rasubosu2 : GroundedUnit
    {
        [Header("攻撃固定")]
        [Tooltip("攻撃選択の固定化(１，ポインタミサイル ２，クロスウェーブ ３，連続ワープショット ４，一閃ビームソード)")]
        [SerializeField] private int FixedAttackSelect = 0;                // 攻撃選択の固定化

        [Header("固有設定")]
        [SerializeField] private float closeRangeDistance = 5f;            // 近距離判定の距離

        [Header("ワープ移動")]
        [SerializeField] private float _WarpMovingRange = 5f;              // ワープ移動する範囲(初期位置の前後距離)
        [SerializeField] private float _WarpIntervalTime = 2f;             // ワープ移動間隔時間

        [Tooltip("ワープ移動開始 → 無敵時間開始")]
        [SerializeField] private float _WarpInvincibleSettingTime = 0.5f;

        [Tooltip("無敵時間開始 → ワープ実行")]
        [SerializeField] private float _WarpStartTime = 0.5f;

        [Tooltip("ワープ実行 → 無敵時間解除")]
        [SerializeField] private float _WarpInvincibleRemovedTime = 0.5f;

        [Tooltip("無敵解除 → ワープ待機に戻る")]
        [SerializeField] private float _WarpEndTime = 0.5f;

        [SerializeField] private int _WarpCount = 3;
        private int _CurrentWarpCount = 0;

        /* メモ：現在のワープの処理動作 */
        /*  
         * 
         */


        [Header("ポインタミサイル")]
        [SerializeField] private GameObject[] _MissileFallPoint;            // ミサイル落下地点
        [SerializeField] private BulletData _MissileBulletDate;
        [SerializeField] private float _MissileFallTime = 1f;
        [SerializeField] private float _MissileEndTime = 1.2f;
        private Vector2 missiledirection = Vector2.down;



        [Header("クロスウェーブ")]
        [SerializeField] private GameObject _CrossWaveSenterPoint;
        [SerializeField] private BulletData _CrossWaveBulletDate;
        [SerializeField] private int _CrossWaveShootCount = 4;
        [SerializeField] private float _CrossWaveShootIntervalTime = 0.3f;
        [SerializeField] private float _CrossWaveWarpPosY = 3f;

        [Tooltip("無敵解除 → クロスウェーブ攻撃発動")]
        [SerializeField] private float _CrossWaveStartTime = 1.5f;

        [Tooltip("クロスウェーブ攻撃終了 → ワープで戻る")]
        [SerializeField] private float _CrossWaveEndTime = 1.2f;



        [Header("連続ワープショット")]
        [SerializeField] private float _WarpCloseDistance = 3f;            // 連続ワープショットのワープ先のプレイヤーとの距離
        [SerializeField] private GameObject _ShotPoint;                    // 連続ワープショット発射位置
        [SerializeField] private BulletData _ShotBulletDate;

        [Tooltip("無敵解除 → ショット攻撃発動")]
        [SerializeField] private float _ShotStartTime = 0.5f;
        [Tooltip("ショット攻撃発動 → ワープまで")]
        [SerializeField] private float _WarpShotEndTime = 0.6f;

        [SerializeField] private int _WarpShotCount = 4;
        [Range(0, 1)]
        [SerializeField] private float _ShotForwardprobability = 0.6f;
        private CapsuleCollider2D capcol2D;
        private int _currentWarpShotCount = 0;
        private bool forwardShot;
        private Vector2 warpshootDirection;
        private string animeTrigger;



        [Header("一閃ビームソード")]
        [SerializeField] private float _FlashBeamSwordDistance = 2f;            // ソード攻撃に派生する距離
        [SerializeField] private GameObject _FlashBeamSwordPoint;               // ソード攻撃中心位置
        [SerializeField] private BulletData _FlashBeamSwordBulletDate;

        [Tooltip("無敵解除 → ショット攻撃発動")]
        [SerializeField] private float _FlashBeamSwordStartTime = 0.5f;
        [Tooltip("ショット攻撃発動 → ワープまで")]
        [SerializeField] private float _FlashBeamSwordEndTime = 0.6f;

        private Rigidbody2D _rb2d;
        private float gravity;

        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
        private SearchAssistanceMono _searchAssistance;
        private UnitBase _player;
        // 直前ステートを保持して「遷移した瞬間」を検知する
        private string _prevStateKey;
        // beforewarp 入場時の一度だけ処理を安全に行うガード（保険）
        private bool _didBeforeWarpStartThisEntry = false;


        protected override void AfterAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();

            // TODO 動作確認用のコード
            _stateMachine.ChangeState(Triggers.Event1);
            _prevStateKey = _stateMachine.CurrentState.key;
        }

        protected override void Start()
        {
            base.Start();
            _rb2d = GetComponent<Rigidbody2D>();
            capcol2D = GetComponent<CapsuleCollider2D>();
            gravity = _rb2d.gravityScale;
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

            // 現在ステートがワープ直前のステートに 変わった瞬間 を検知して一度だけ実行
            var curKey = _stateMachine.CurrentState.key;
            if (_prevStateKey != curKey)
            {
                if (curKey == _stateNames[States.beforewarp] ||
                    curKey == _stateNames[States.crosswave_beforewarp] ||
                    curKey == _stateNames[States.crosswave_end] ||
                    curKey == _stateNames[States.warpShot_beforewarp] ||
                    curKey == _stateNames[States.warpShot_chain])
                {
                    _didBeforeWarpStartThisEntry = false; // 新しい入場なのでガードをリセット
                    // Enter と同タイミングで一回だけ呼ぶ
                    if (!_didBeforeWarpStartThisEntry)
                    {
                        _didBeforeWarpStartThisEntry = true;
                        WarpBeforeStart();
                    }
                }
                // 状態が変わったら prev を更新
                _prevStateKey = curKey;
            }
        }

        protected override void OnTakeDamage(IUnit from, float damage)
        {
            Debug.Log("TakeDamage");
        }

        // ワープの前隙のディレイ⇒無敵時間のコルーチン開始⇒Warpに遷移
        private async void WarpIdleStay()
        {
            Debug.Log($"CurrentWarpCount : {_CurrentWarpCount}");
            // ワープが終わったら攻撃へ
            if (_CurrentWarpCount >= _WarpCount)
            {
                _stateMachine.ChangeState(Triggers.Warpcomplete);
            }
        }

        private async void WarpBeforeStart()
        {
            TurnAround();
            await UniTask.Delay(TimeSpan.FromSeconds(_WarpInvincibleSettingTime));

            // 無敵時間開始
            IsInvincible = true;
            _rb2d.gravityScale = 0;
            capcol2D.isTrigger = true;
            await UniTask.Delay(TimeSpan.FromSeconds(_WarpStartTime));

            if (IsMatchingState(States.beforewarp))
            {
                // 位置をセット
                warp.SetPos(SetGroundWarpPointRandom());
                _CurrentWarpCount++;
                // ワープで移動
                _stateMachine.ChangeState(Triggers.Warp);
            }
            else if (IsMatchingState(States.crosswave_beforewarp))
            {
                // 画面中央へ位置セット
                crosswavewarp.SetPos(SetAirWarpPointCenter());
                _stateMachine.ChangeState(Triggers.Warp);
            }
            else if (IsMatchingState(States.crosswave_end))
            {
                // 地上を目指して位置をセット
                crosswavewarp.SetPos(SetGroundWarpPointRandom());
                _stateMachine.ChangeState(Triggers.Warp);
            }
            else if (IsMatchingState(States.warpShot_beforewarp))
            {
                _currentWarpShotCount = 0;

                // プレイヤー左右へ位置セット
                warpShotwarp.SetPos(SetGroundWarpPointClose());
                // 前方か斜方か
                forwardShot = (UnityEngine.Random.value < _ShotForwardprobability) ? true : false;
                animeTrigger = (forwardShot == true) ? "ShotForward" : "ShotOblique";
                _animator.SetTrigger(animeTrigger);
                _stateMachine.ChangeState(Triggers.Warp);

            }
            else if (IsMatchingState(States.warpShot_chain))
            {
                
                if (_currentWarpShotCount >= _WarpShotCount)
                {
                    // ワープ後にワープ待機に戻る
                    warp.SetPos(SetGroundWarpPointRandom());
                    _stateMachine.ChangeState(Triggers.Warp);
                }
                else
                {
                    // ワープショット継続
                    warpShotwarp.SetPos(SetGroundWarpPointClose());
                    forwardShot = (UnityEngine.Random.value < _ShotForwardprobability) ? true : false;
                    animeTrigger = (forwardShot == true) ? "ShotForward" : "ShotOblique";
                    _animator.SetTrigger(animeTrigger);
                    _stateMachine.ChangeState(Triggers.Attack3chain);
                }
            }
        }

        private async void WarpEnter()
        {
            TurnAround();
            await UniTask.Delay(TimeSpan.FromSeconds(_WarpInvincibleRemovedTime));
            // 無敵時間解除
            IsInvincible = false;
            _rb2d.gravityScale = gravity;
            capcol2D.isTrigger = false;

            if (IsMatchingState(States.warp))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_WarpEndTime));
                _stateMachine.ChangeState(Triggers.Warpend);
            }
            else if (IsMatchingState(States.crosswave_warp))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_CrossWaveStartTime));
                // 上下左右か斜め方向か
                Vector2 shootDirection = (UnityEngine.Random.value < 0.5) ? Vector2.down : new Vector2(1, 1).normalized;
                // 発射方向をセット
                crosswave.SetDirection(shootDirection);
                _stateMachine.ChangeState(Triggers.Attack2);
            }
            else if (IsMatchingState(States.warpShot_warp))
            {
                warpshootDirection = (forwardShot == true) ? Direction.normalized : new Vector2(Direction.x, 1).normalized;
                // 発射方向をセット
                warpShot.SetDirection(warpshootDirection);
                await UniTask.Delay(TimeSpan.FromSeconds(_ShotStartTime));
                _stateMachine.ChangeState(Triggers.Attack3);
            }
        }


        // 地上のワープ先を決定
        // TODO 空中からのワープにも対応できるよう設計変更
        // TODO プレイヤーから少し離れてワープするよう変更
        private Vector2 SetGroundWarpPointRandom()
        {
            Transform cameraPos = Camera.main.transform;
            float warpPosX = UnityEngine.Random.Range(cameraPos.localPosition.x - _WarpMovingRange, cameraPos.localPosition.x + _WarpMovingRange);
            float warpPosY;

            if (IsGrounded)
            {
                warpPosY = transform.position.y;
            }
            else
            {
                Ray2D ray = new Ray2D(transform.position, -transform.up); // Rayを生成、-transform.upは進行方向
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 1f);//Raycastを生成
                warpPosY = hit.collider.transform.position.y;
            }

            Vector2 warpPoint = new Vector2(warpPosX, warpPosY);
            return warpPoint;
        }

        // 画面中央のワープ先を決定
        private Vector2 SetAirWarpPointCenter()
        {
            Transform cameraPos = Camera.main.transform;
            float warpPosX = cameraPos.transform.localPosition.x;
            float warpPosY = cameraPos.transform.localPosition.y + _CrossWaveWarpPosY;
            Vector2 warpPoint = new Vector2(warpPosX, warpPosY);
            return warpPoint;
        }

        // プレイヤーの近くにワープ先を決定
        private Vector2 SetGroundWarpPointClose()
        {
            if (_player == null)
            {
                Debug.Log("プレイヤー未発見");
                return transform.position;
            }

            Vector2 playerPos = _player.transform.position;

            // 左右方向にセット（初期は左方向）
            Vector2[] targetOffsets = new Vector2[]
            {
                new Vector2(-_WarpCloseDistance, 0),  // 左
                new Vector2(_WarpCloseDistance, 0)    // 右
            };
            Vector2[] warpCandidates = new Vector2[2];
            bool[] isValid = new bool[2];

            var coll = capcol2D;
            Vector2 size = coll.size;
            float radius = size.x * 0.5f;
            float angle = 0f;
            int mask = LayerMask.GetMask("Ground");

            for (int i = 0; i < 2; i++)
            {
                Vector2 targetPos = playerPos + targetOffsets[i];

                // ===== カプセルキャストで経路をチェック =====
                RaycastHit2D hit = Physics2D.CapsuleCast(
                    playerPos,
                    size,
                    CapsuleDirection2D.Vertical,
                    angle,
                    targetOffsets[i].normalized,
                    targetOffsets[i].magnitude,
                    mask
                );

                if (hit.collider != null)
                {
                    targetPos = hit.point - targetOffsets[i].normalized * 0.05f;
                }
                // ===== 最終チェック (OverlapCircle) =====
                // 目的地が空いてるか調べる
                bool blocked = Physics2D.OverlapCircle(targetPos, radius, mask);

                if (!blocked)
                {
                    warpCandidates[i] = targetPos;
                    isValid[i] = true;
                }
            }

            Ray2D ray = new Ray2D(transform.position, -transform.up);
            RaycastHit2D rayhit = Physics2D.Raycast(ray.origin, ray.direction, 1f);
            float warpPosY = rayhit.collider.transform.position.y;

            // 左を指定
            if (isValid[0] && !isValid[1])
            {
                Debug.Log("左");
                return new Vector2(warpCandidates[0].x, warpPosY);
            }
            // 右を指定
            if (isValid[1] && !isValid[0])
            {
                Debug.Log("右");
                return new Vector2(warpCandidates[1].x, warpPosY);
            }
            // 左右どちらかを選ぶ
            if (isValid[0] && isValid[1])
            {
                Debug.Log("左右どっちか");
                return UnityEngine.Random.value< 0.5 ? new Vector2(warpCandidates[0].x, warpPosY) : new Vector2(warpCandidates[1].x, warpPosY);
            }

            // いずれも不可な場合は画面中央へ移動
            Debug.Log("プレイヤー左右へのワープ不可能");
            Transform cameraPos = Camera.main.transform;
            return new Vector2(cameraPos.transform.localPosition.x, warpPosY);
        }

        private async void MissileEnter()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_MissileEndTime));
            _stateMachine.ChangeState(Triggers.Attack1end);
        }

        private async void CrossWaveEnter()
        {
            for (int i = 1; i <= _CrossWaveShootCount; i++)
            {
                crosswave.DirectShoot(this);
                await UniTask.Delay(TimeSpan.FromSeconds(_CrossWaveShootIntervalTime));
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_CrossWaveEndTime));
            _stateMachine.ChangeState(Triggers.Attack2end);
        }

        private async void WarpShotEnter()
        {
            _currentWarpShotCount++;
            await UniTask.Delay(TimeSpan.FromSeconds(_WarpShotEndTime));
            _stateMachine.ChangeState(Triggers.Attack3end);
        }


        // 無敵時間開始コルーチン
        // private async void Invincible(float invincibleTime)
        // {
        //     Debug.Log("Invincible Start");
        //     IsInvincible = true;
        //     await UniTask.Delay(TimeSpan.FromSeconds(invincibleTime));
        //     Debug.Log("Invincible End");
        //     IsInvincible = false;
        // }

        private void TurnAround()
        {
            // 見た目の向き変更など既存処理
            if (_player != null)
            {
                Direction = (_player.Transform.position - transform.position).normalized;
                Direction = (Direction.x > 0) ? Vector2.right : Vector2.left;
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
            _CurrentWarpCount = 0;
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
            Debug.Log("ポインターミサイル開始");
            _stateMachine.ChangeState(Triggers.Attack1start);
        }

        void Attack2()
        {
            Debug.Log("クロスウェーブ開始");
            _stateMachine.ChangeState(Triggers.Attack2start);
        }
        void Attack3()
        {
            Debug.Log("ワープショット開始");
            _stateMachine.ChangeState(Triggers.Attack3start);
        }
        void Attack4()
        {
            Debug.Log("フラッシュビームソード開始");
            _stateMachine.ChangeState(Triggers.Attack4start);
        }



        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
    }
}
