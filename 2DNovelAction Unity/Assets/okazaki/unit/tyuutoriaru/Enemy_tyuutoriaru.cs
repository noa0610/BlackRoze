using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using System;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_tyuutoriaru : UnitBase
    {

        [SerializeField] private List<GameObject> _junpPositions;
        [SerializeField] private GameObject _centerPositions;
        [SerializeField] private GameObject _YPositions;
        [SerializeField] private Animator _anim;
        [SerializeField] private float closeRangeDistance = 5f; // 近距離判定の距離
        private int currentAttack = 1; // 初期値は1（アタック1）
        private int nowstate = 2;
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
        [SerializeField] private GameObject _Lasershotmuzzle;
        [SerializeField] private GameObject _beamswordmuzzle;
        [SerializeField] private BulletData _LasershotbulletData;
        [SerializeField] private LayerMask _LasershotTargetLayer; // 必要ならInspectorでセット
        [SerializeField] private Rigidbody2D _RB2;
        [SerializeField] private FreeMove _freeMove;
        [SerializeField] private BulletData _beamswordBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _beamswordTargetLayer; // 必要ならInspectorでセット
        [SerializeField] private BulletData _shockwaveBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _shockwaveTargetLayer; // 必要ならInspectorでセット
        [SerializeField] private PositionJump _positionJump;
        private bool _waitingForAttack1 = false;
        // アニメ再生中フラグ（AnimaSelect の重複実行防止）
        private bool _isAnimating = false;
        private bool _waitingForAttack2 = false;
        // lasershot ステート参照（方向を動的にセットするために保持）
        private ShootForward _lasershotState;
        #region 

        // protected override void RegisterStats()
        // {
        //     // トランスミッショングループを作成
        //     var idleTrigger = new[]                                // 待機ステートのトリガー
        //     {
        //         (Triggers.FoundPlayer, States.attackidle),              // イベント1発生で攻撃待機へ
        //         (Triggers.Died, States.dead),                      // 死亡で死へ
        //         (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
        //     };
        //     var attackidleTrigger = new[]                          // 攻撃待機ステートのトリガー  
        //     {
        //         (Triggers.Attack1, States.fixedpositionjump),              // 攻撃１でレーザー攻撃へ
        //         (Triggers.Attack2, States.beamswordattackmove),   // 攻撃２でビームソード接近へ
        //         (Triggers.Died, States.dead),                      // 死亡で死へ
        //         (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
        //     };
        //     var lasershotTrigger = new[]                           // レーザー攻撃ステートのトリガー
        //     {
        //         (Triggers.Attack1end, States.attackidle),                // 攻撃１終了で攻撃待機へ
        //         (Triggers.Died, States.dead),                      // 死亡で死へ
        //         (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
        //     };
        //     var beamswordattackmoveTrigger = new[]                     // ビームソード攻撃ステートのトリガー
        //     {
        //         (Triggers.moveend, States.beamswordattack),                // 移動終了で攻撃２へ
        //         (Triggers.Died, States.dead),                      // 死亡で死へ
        //         (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
        //     };
        //     var beamswordattackTrigger = new[]                     // ビームソード攻撃ステートのトリガー
        //     {
        //         (Triggers.Attack2end, States.fixedpositionjump),                // 攻撃２終了でジャンプへ
        //         (Triggers.Died, States.dead),                      // 死亡で死へ
        //         (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
        //     };
        //     var fixedpositionjumpTrigger = new[]                                // ジャンプステートのトリガー
        //     {
        //         (Triggers.Landing, States.attackidle),             // 着地で攻撃待機へ
        //         (Triggers.Died, States
        //         (Triggers.HalfHP, States.stun)                // HPが半分以下でショックウェーブへ
        //     };
        //     var stunTrigger = new[]                                // スタンステートのトリガー
        //     {
        //         (Triggers.Event2, States.shockwave),               // イベント2発生でショックウェーブへ
        //         (Triggers.Died, States.dead),                      // 死亡で死へ 
        //     };
        //     var shockwaveTrigger = new[]                           // ショックウェーブステートのトリガー
        //     {
        //         (Triggers.Shockwaveend, States.idle),              // ショックウェーブ終了で待機へ
        //         (Triggers.Died, States.dead)                       // 死亡で死へ
        //     };

        //     // ステートマシンにStatesの移動先の追加
        //     _stateMachine
        //         .AddTransitions(States.idle, idleTrigger)
        //         .AddTransitions(States.attackidle, attackidleTrigger)
        //         .AddTransitions(States.lasershot, lasershotTrigger)
        //         .AddTransitions(States.beamswordattackmove, beamswordattackmoveTrigger)
        //         .AddTransitions(States.beamswordattack, beamswordattackTrigger)
        //         .AddTransitions(States.fixedpositionjump, fixedpositionjumpTrigger)
        //         .AddTransitions(States.stun, stunTrigger)
        //         .AddTransitions(States.shockwave, shockwaveTrigger);

        //     // 死んだときに何もしないならDeadの設定はいらない

        //     // 待機
        //     var idle = new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0);
        //     _stateMachine.AddState(States.idle, idle);
        //     // 死亡
        //     var died = new Idle().SetAnimeTrigger("died").SetCancelableProgress(0);
        //     died.OnAnimationCompleted.AddListener(() =>
        //     {
        //         UnitManager.instance.RemoveUnit(this);
        //         Destroy(gameObject);
        //     });
        //     _stateMachine.AddState(States.dead, died);
        //     // ジャンプ
        // var jumpPositions = _junpPositions.ConvertAll(pos => (Vector2)pos.transform.position);
        // var fixedpositionjump = new PositionJump(jumpPositions, 10f)
        //     .SetAnimeTrigger("fixedpositionjump")
        //     .SetCancelableProgress(0);
        //     fixedpositionjump.OnArrived += () =>
        //     {
        //        _stateMachine.ChangeState(Triggers.Landing); // 例：Landingトリガーで遷移
        //     };
        // _stateMachine.AddState(States.fixedpositionjump, fixedpositionjump);
        //     // 攻撃待機
        //     var attackIdle = new Idle_LazyEvent(5f).SetAnimeTrigger("attackidle").SetCancelableProgress(0);
        //     attackIdle.LazyEvent.AddListener(Attackselect);
        //     _stateMachine.AddState(States.attackidle, attackIdle);
        //     // レーザー攻撃
        //     var lasershot = new LaserShot(_RB2, _firePoints, _bulletPrefab).SetAnimeTrigger("lasershot").SetCancelableProgress(0);
        //     _stateMachine.AddState(States.lasershot, lasershot);
        //     // ビームソード攻撃移動
        //     var beamswordattackmove = _freeMove.SetAnimeTrigger("move").SetCancelableProgress(0);
        //     _stateMachine.AddState(States.beamswordattackmove, beamswordattackmove);
        //     // ビームソード攻撃
        //     var beamswordattack = new ShootForward(_beamswordBulletData, _beamswordTargetLayer)
        //     .SetDirection(Vector2.down) // プレイヤー方向など、必要に応じてセット
        //     .SetMuzzle(_swordfirePoints.Length > 0 ? _swordfirePoints[0].gameObject : gameObject)
        //     .SetAnimeTrigger("beamswordattack")
        //     .SetCancelableProgress(0);
        //     beamswordattack.onShootComplete.AddListener(() =>
        //     {
        //         _stateMachine.LazyChange(Triggers.Attack2end);
        //     });
        //     _stateMachine.AddState(States.beamswordattack, beamswordattack);
        //     // スタン
        //     var stun = new Idle_LazyChange(Triggers.Event2.ToString(), 5, true);
        //     _stateMachine.AddState(States.stun, stun);
        //     // ショックウェーブ
        //     var shockwave = new ShootForward(_shockwaveBulletData, _shockwaveTargetLayer)
        //     .SetDirection(Vector2.left)
        //     .SetMuzzle(_swordfirePoints.Length > 0 ? _swordfirePoints[0].gameObject : gameObject)
        //     .SetAnimeTrigger("beamswordattack")
        //     .SetCancelableProgress(0);
        //     // 弾発射完了時にショックウェーブ終了トリガーを発火
        //     shockwave.onShootComplete.AddListener(() =>
        //     {
        //         _stateMachine.LazyChange(Triggers.Shockwaveend);
        //     });
        //     _stateMachine.AddState(States.shockwave, shockwave);
        // }
        #endregion
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
        private bool _halfHpTriggered = false;

        protected override void AfterFixedUpdate()
        {
            base.AfterFixedUpdate();
            SearchPlayer();

            // HPが半分以下になったら一度だけトリガー発火
            if (!_halfHpTriggered)
            {
                var hpStatus = statusManager.GetStatus(Status.HP);
                var maxHpStatus = statusManager.GetStatus(Status.MaxHP);

                if (hpStatus != null && maxHpStatus != null)
                {
                    float hp = hpStatus.CurrentAmount;
                    float maxHp = maxHpStatus.CurrentAmount;

                    if (hp <= maxHp / 2f)
                    {
                        _halfHpTriggered = true;
                        Debug.Log("HPが半分以下になりました");
                        _stateMachine.ChangeState(Triggers.HalfHP);
                    }
                }
            }

            // beamswordattackステート中のみ判定
            if (IsMatchingState(States.beamswordattackmove) && _player != null && _YPositions != null)
            {
                // プレイヤーが_YPositionsのy座標を通過したら止める
                float targetX = _YPositions.transform.position.x;
                float playerX = _player.Transform.position.x;

                // 例えば「近い」判定（±0.5以内など）
                if (Mathf.Abs(playerX - targetX) < 0.5f)
                {
                    Debug.Log("プレイヤーがY座標を通過しました");
                    // ステート遷移（例：ジャンプや攻撃待機など）
                    _stateMachine.ChangeState(Triggers.moveend);
                }
            }

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

        private void Attackselect()
        {
            if (currentAttack == 1)
            {
                Attack2();
                currentAttack = 2;
                return;
            }
            else if (currentAttack == 2)
            {
                Attack2();
                currentAttack = 1;
            }
        }
        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
        void Attack1()
        {
            // プレイヤーの位置に合わせて射撃方向をセット（左右のみ）
            Vector2 shootDir = Vector2.left; // デフォルト
            if (_player != null)
            {
                var dir = (_player.Transform.position - transform.position).normalized;
                if (Mathf.Abs(dir.x) > 0f) shootDir = new Vector2(Mathf.Sign(dir.x), 0f);
            }
            _lasershotState?.SetDirection(shootDir);

            Debug.Log("レーザー攻撃 (direction=" + shootDir + ")");
            _stateMachine.ChangeState(Triggers.Attack1);
        }

        void Attack2()
        {
            Debug.Log("ビームソード接近");
            _stateMachine.ChangeState(Triggers.Attack2);
        }

        private int JumpSelect()
        {
            // 中心とこのオブジェクトのx座標差を取得
            float distance = transform.position.x - _centerPositions.transform.position.x;
            Debug.Log("距離差: " + distance);
            // 差がプラスなら1、マイナスなら0を返す
            return distance >= 0 ? 0 : 1;

        }
        private void AnimaSelect()
        {

            Debug.Log("Enemy_tyuutoriaru: AnimaSelect 呼び出し nowstate=" + nowstate);
            if (!_isAnimating) _isAnimating = true;

            if (nowstate == 2)
            {
                Debug.Log("Enemy_tyuutoriaru: アニメーション状態2からの遷移");
                _anim.SetTrigger("toShot_Up");
                nowstate = 3;
                return;
            }
            else if (nowstate == 3)
            {
                Debug.Log("Enemy_tyuutoriaru: アニメーション状態3からの遷移");
                _anim.SetTrigger("toShot_Down");
                nowstate = 4;
                return;
            }
            else if (nowstate == 4)
            {
                Debug.Log("Enemy_tyuutoriaru: アニメーション状態4からの遷移");
                _anim.SetTrigger("toShot_Medium");
                nowstate = 5;
                return;
            }
            else if (nowstate == 5)
            {
                Debug.Log("Enemy_tyuutoriaru: アニメーション状態5からの遷移");
                _anim.SetTrigger("toShot_Up");
                nowstate = 6;
                return;
            }
            else if (nowstate == 6)
            {
                Debug.Log("Enemy_tyuutoriaru: アニメーション状態6からの遷移");
                _anim.SetTrigger("toShot_Down");
                nowstate = 7;
                return;
            }
        }
        // アニメ遷移→進行度監視して一度だけ Attack1 を呼ぶ
        private async UniTaskVoid WaitAndCallAttack1()
        {
            // 二重起動防止
            if (_waitingForAttack1) return;
            _waitingForAttack1 = true;

            try
            {
                // Animator が無ければ即呼ぶ
                if (_anim == null)
                {
                    Attack1();
                    return;
                }
                // 1フレーム待って Animator の遷移を反映させる
                await UniTask.Yield(PlayerLoopTiming.Update);

                // 現在のステートハッシュを取得して、ステートが変わるのを待つ
                int startHash = _anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
                int attempts = 0;
                const int maxTransitionFrames = 300; // 約5秒（60FPS想定）
                while (_anim.GetCurrentAnimatorStateInfo(0).shortNameHash == startHash && attempts++ < maxTransitionFrames)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
                // 新しいステートの進行度が 0.5 以上になるまで待つ（タイムアウト付き）
                attempts = 0;
                const int maxProgressFrames = 600; // 約10秒                
                while (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f && attempts++ < maxProgressFrames)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }

                // 進行度到達後に一度だけ Attack1 を呼ぶ
                Attack1();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                // 問題があっても攻撃継続
                Attack1();
            }
            finally
            {
                _waitingForAttack1 = false;
                _isAnimating = false;
            }
        }
        private IEnumerator WaitForBeamswordAnimationThenFire()
        {
            if (_waitingForAttack2) yield break;
            _waitingForAttack2 = true;
            try
            {
                if (_anim == null)
                {
                    _stateMachine.LazyChange(Triggers.Attack2end);
                    yield break;
                }

                // 1フレーム待ってアニメ遷移を反映+                yield return null;
                // 目標ステートの変化を待つ（短タイムアウト）
                int startHash = _anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
                float waitTime = 0f;
                const float maxWait = 5f;
                while (_anim.GetCurrentAnimatorStateInfo(0).shortNameHash == startHash && waitTime < maxWait)
                {
                    waitTime += Time.deltaTime;
                    yield return null;
                }
                // 新しいステートの進行度が 1.0 に達するまで待つ（ループの可能性を考慮）
                waitTime = 0f;
                while (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f && waitTime < maxWait)
                {
                    waitTime += Time.deltaTime;
                    yield return null;
                }

                // アニメ完了後にステート遷移
                _stateMachine.LazyChange(Triggers.Attack2end);
                GetComponent<BoxCollider2D>().isTrigger = true;
                _positionJump?.SetTarget(JumpSelect());
            }
            finally
            {              
                _waitingForAttack2 = false;
            }
        }
    }
}

