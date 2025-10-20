using UnityEngine;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Datas.Definitions;
using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using System.Collections.Generic;
using HighElixir;
using System;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_Gurter : GroundedUnit
    {
        [Header("攻撃関連")]
        [SerializeField] private Animator _anim;
        [SerializeField] private GameObject _muzzle;
        [SerializeField] private Rigidbody2D _rb2;
        [SerializeField] private int hp = 10;
        [SerializeField] private float attackCooldown = 3f;
        [SerializeField] private float idleStartWait = 2f; // ← 新規追加：最初の待機時間

        private float idleStartTimer = 0f;
        private bool hasStarted = false;
        private float cooldownTimer = 0f;
        private SearchAssistanceMono _searchAssistance;
        private SieldBlock _shieldDefense;

        [SerializeField] private BulletData _bulletData;
        [SerializeField] private ShootForward shoot;
        private static readonly Dictionary<States, string> _states = EnumWrapper.GetDict<States>();
        // ★クラスの最初あたり（GroundedUnit継承直後）に追加
        public event Action OnStunStart;
        public event Action OnStunEnd;
        public event Action OnAttackStart;
        public event Action OnAttackEnd;



        public enum States
        {
            Idle,            // 初期待機 ← 新規追加
            ShieldIdle,      // 盾待機
            AttackWait,      // 攻撃待機
            ShieldTackle,    // 近距離攻撃
            ShoulderGrenade, // 遠距離攻撃
            Stun,            // スタン
            Dead             // 死亡
        }

        private enum Triggers
        {
            StartBattle,     // Idle → ShieldIdle
            FoundPlayer,
            LostPlayer,
            CooldownEnd,
            NearAttack,
            FarAttack,
            HitWhileShield,
            Died
        }

        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
            _shieldDefense = GetComponentInChildren<SieldBlock>();

            if (_shieldDefense != null)
                _shieldDefense.OnPenetrate += HandlePenetrate; // イベント購読
        }

        private void OnDestroy()
        {
            if (_shieldDefense != null)
                _shieldDefense.OnPenetrate -= HandlePenetrate; // イベント解除
        }


        private void HandlePenetrate(float damage)
        {
            Debug.Log($"防御貫通 → HPを{damage}減らします");
            TakeDamage((int)damage); // 必要ならfloat→int変換
            _stateMachine.ChangeState(Triggers.HitWhileShield);
        }


        protected override void RegisterStats()
        {
            _stateMachine
                .AddTransmissions(States.Idle, new[]
                {
                    (Triggers.StartBattle, States.ShieldIdle),
(Triggers.HitWhileShield, States.Stun), // ← ここでスタン遷移
(Triggers.Died,States.Dead)
                })
                .AddTransmissions(States.ShieldIdle, new[]
                {
                    (Triggers.FoundPlayer, States.AttackWait),
                    (Triggers.HitWhileShield, States.Stun), // ← ここでスタン遷移
                    (Triggers.Died,States.Dead)
                })
                .AddTransmissions(States.AttackWait, new[]
                {
                    (Triggers.LostPlayer, States.ShieldIdle),
                    (Triggers.NearAttack, States.ShieldTackle),
                    (Triggers.FarAttack, States.ShoulderGrenade),
(Triggers.HitWhileShield, States.Stun), // ← ここでスタン遷移
(Triggers.Died,States.Dead)
                })
                .AddTransmissions(States.ShieldTackle, new[]
                {
                    (Triggers.CooldownEnd, States.AttackWait),
                    (Triggers.HitWhileShield, States.Stun), // ← ここでスタン遷移
                    (Triggers.Died,States.Dead)
                })
                .AddTransmissions(States.ShoulderGrenade, new[]
                {
                    (Triggers.CooldownEnd, States.AttackWait),
                    (Triggers.HitWhileShield, States.Stun), // ← ここでスタン遷移
                    (Triggers.Died,States.Dead)
                })
                .AddTransmissions(States.Stun, new[]
                {

                    (Triggers.StartBattle, States.ShieldIdle), // ← ここでスタン遷移
                    (Triggers.Died,States.Dead)
                })
                 .AddTransmissions(States.ShieldIdle, new[]
        {
            (Triggers.HitWhileShield, States.Stun) // ← ここでスタン遷移
        })
                .AddTransmissions(States.AttackWait, new[]
                {
                    (Triggers.Died, States.Dead)

                });

            // 各ステート登録
            _stateMachine.AddState(States.Idle, new Idle());
            _stateMachine.AddState(States.ShieldIdle, new Idle());
            _stateMachine.AddState(States.AttackWait, new Idle());
            _stateMachine.AddState(States.ShieldTackle, new Stun(_rb2, 1, true));

            shoot.SetBullet(_bulletData);
            shoot.SetMuzzle(_muzzle);
            shoot.SetDirection(Direction);
            _stateMachine.AddState(States.ShoulderGrenade, shoot);
            _stateMachine.AddState(States.Stun, new Stun(_rb2, 1, true));
            _stateMachine.AddState(States.Dead, new Idle());
        }

        protected override void Start()
        {
            base.Start();
            _stateMachine.ChangeState(States.Idle);
            Debug.Log("初期状態: Idle（待機中）");
        }

        protected override void FixedUpdate()
        {
            // --- Idle状態からの待機処理 ---
            if (IsMatchingState(States.Idle))
            {
                idleStartTimer += Time.fixedDeltaTime;
                if (idleStartTimer >= idleStartWait && !hasStarted)
                {
                    hasStarted = true;
                    _stateMachine.ChangeState(Triggers.StartBattle);
                    Debug.Log("Idle待機完了 → ShieldIdleへ移行");
                }
                return; // Idle中は他の処理をしない
            }
            if (IsMatchingState(States.Stun))
            {
                Debug.Log("ガーターがスタン → シールドOFFイベント発火");
                OnStunStart?.Invoke(); // シールドを無効化

                // 2秒後にスタン解除
                Invoke(nameof(RecoverFromStun), 2f);
                return;
            }

            if (IsMatchingState(States.Dead))
            {
                Debug.Log("死亡しました");
                Destroy(gameObject);
            }

            if (hp <= 0)
            {
                _stateMachine.ChangeState(Triggers.Died);
                return;
            }
            SearchPlayer();

            Debug.Log("現在のステート: " + _stateMachine.CurrentState.key);

            if (cooldownTimer > 0)
                cooldownTimer -= Time.fixedDeltaTime;

            if (IsMatchingState(States.ShieldTackle) || IsMatchingState(States.ShoulderGrenade))
            {
                if (IsMatchingState(States.ShieldTackle))
                {
                    // 攻撃判定ONイベント
                    _shieldDefense?.SetAttack(true);
                }
                else
                {
                    // 攻撃判定OFFイベント
                    _shieldDefense?.SetAttack(false);
                }
                cooldownTimer = attackCooldown;
                _stateMachine.ChangeState(Triggers.CooldownEnd);
            }
            // シールドタックル開始時

        }
        private void RecoverFromStun()
        {
            Debug.Log("スタン終了 → シールドONイベント発火");
            OnStunEnd?.Invoke(); // シールドを再有効化
            _stateMachine.ChangeState(Triggers.StartBattle);
        }


        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _states[state];
        }
        [SerializeField] private float angleDeg = 30f; // ここで角度を変更してください（例：30度）
        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            int direction = player.transform.position.x > transform.position.x ? 1 : -1;
            Vector3 scale = transform.localScale;

            // --- 向き変更処理（見た目＋ロジック両方） ---
            if ((direction == 1 && scale.x < 0) || (direction == -1 && scale.x > 0))
            {
                scale.x *= -1;
                transform.localScale = scale;

                // ローカルな向きベクトル（水平）を作る
                Vector2 facingVector = new Vector2(direction, 0f);

                // --- 角度指定 ---

                float angleRad = angleDeg * Mathf.Deg2Rad;

                // 角度を考慮した発射方向（向きに応じてX符号を反転）
                Vector2 shootDir = new Vector2(direction * Mathf.Cos(angleRad), Mathf.Sin(angleRad)).normalized;

                // --- 安全にクラスの Direction フィールドを更新する ---
                // もしクラス側の Direction が int 型なら（1/-1 を期待している）：
                //    Direction = direction;
                // もしクラス側の Direction が Vector2 型なら：
                Direction = facingVector;

                // 例: Direction が int の場合（一般的にはこれが多い）
                // Direction = direction;

                // 例: Direction が Vector2 の場合
                // Direction = facingVector;

                // --- 発射方向をセット ---
                shoot.SetDirection(shootDir);

                Debug.Log($"向きを変更しました。発射方向: {shootDir} (角度: {angleDeg}°)");
            }



            // --- クールダウン中は攻撃しない ---
            if (cooldownTimer > 0)
                return;
            if (_searchAssistance.Execute("meray", list, out _))
            {
                _stateMachine.ChangeState(Triggers.FoundPlayer);
                _stateMachine.ChangeState(Triggers.NearAttack);
                cooldownTimer = attackCooldown;
            }
            else if (_searchAssistance.Execute("renge", list, out _))
            {
                _stateMachine.ChangeState(Triggers.FoundPlayer);
                _stateMachine.ChangeState(Triggers.FarAttack);
                cooldownTimer = attackCooldown;
            }
            else
            {
                _stateMachine.ChangeState(Triggers.LostPlayer);
            }


        }



        public void TakeDamage(int damage)
        {
            hp -= damage;
            Debug.Log(hp);
            if (hp <= 0)
            {
                _stateMachine.ChangeState(Triggers.Died);
                Debug.Log("敵死亡");
            }
        }
    }
}

