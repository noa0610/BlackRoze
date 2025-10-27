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
        [SerializeField] private Animator animator;
        private bool _isStunning = false;




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
        [SerializeField, Header("スタン設定")]
        private float stunDuration = 2f;          // スタン時間（_eventTimeに渡す）
        [SerializeField] private float stunKnockbackForce = 15f; // ノックバックの強さ
        [SerializeField] private Vector2 stunKnockbackDir = new Vector2(0.78f, 0.9f); // ノックバック方向



        protected override void RegisterStats()
        {
            var idleTrigger = new[]
            {
                (Triggers.StartBattle, States.ShieldIdle,""),
            (Triggers.HitWhileShield, States.Stun,"stun"),
            (Triggers.Died, States.Dead,"")
            };
            var sieldidleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.AttackWait,""),
            (Triggers.HitWhileShield, States.Stun,"stun"),
             (Triggers.Died, States.Dead,"")
            };
            var AttackwaitTrigger = new[]
            {
                (Triggers.LostPlayer, States.ShieldIdle,""),
            (Triggers.NearAttack, States.ShieldTackle,"tackle"),
             (Triggers.FarAttack, States.ShoulderGrenade,""),
             (Triggers.HitWhileShield, States.Stun,"stun"),
             (Triggers.Died, States.Dead,"")
            };
            var ShieldTackleTrigger = new[]
            {
                (Triggers.CooldownEnd, States.AttackWait,""),
                (Triggers.HitWhileShield, States.Stun,"stun"),
                (Triggers.Died, States.Dead,"")
            };
            var SholderGrenadTrigger = new[]
            {
                (Triggers.CooldownEnd, States.AttackWait,""),
            (Triggers.HitWhileShield, States.Stun,"stun"),
            (Triggers.Died, States.Dead,"")
            };
            var stunTrigger = new[]
            {
               (Triggers.StartBattle, States.ShieldIdle,""),
             (Triggers.Died, States.Dead,"")
            };
            _stateMachine
            .AddTransmissions(States.Idle, idleTrigger)
            .AddTransmissions(States.ShieldIdle, sieldidleTrigger)
            .AddTransmissions(States.AttackWait, AttackwaitTrigger)
            .AddTransmissions(States.ShieldTackle, ShieldTackleTrigger)
            .AddTransmissions(States.ShoulderGrenade, SholderGrenadTrigger)
            .AddTransmissions(States.Stun, stunTrigger)
            .AddTransmissions(States.Idle, idleTrigger);



            // 各ステート登録
            _stateMachine.AddState(States.Idle, new Idle());
            _stateMachine.AddState(States.ShieldIdle, new Idle());
            _stateMachine.AddState(States.AttackWait, new Idle());
            _stateMachine.AddState(States.ShieldTackle, new Stun(_rb2, 1, true));
            shoot.SetBullet(_bulletData);
            shoot.SetMuzzle(_muzzle);
            shoot.SetDirection(Direction);
            _stateMachine.AddState(States.ShoulderGrenade, shoot);




            // ✅ スタンステートをインスペクタ値で設定
            var stunState = new Stun(_rb2, stunDuration, true)
                .SetKnockback(stunKnockbackDir); // ノックバック方向設定
            stunState.KnockbackForce = stunKnockbackForce; // ノックバック力設定

            _stateMachine.AddState(States.Stun, stunState);

            _stateMachine.AddState(States.Dead, new Idle());
        }


        protected override void Start()
        {
            base.Start();
            _stateMachine.ChangeState(States.Idle);

        }
        [SerializeField] private float tackletime = 0.8f;

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
                }
                return; // Idle中は他の処理をしない
            }
            if (IsMatchingState(States.Stun))
            {
                if (!_isStunning) // スタン開始時の1回だけ実行
                {
                    _isStunning = true;
                    OnStunStart?.Invoke(); // シールドを無効化

                    // ✅ 2秒後にスタン解除処理を呼ぶ
                    Invoke(nameof(AutoRecoverFromStun), stunDuration);
                }
                return;
            }


            if (IsMatchingState(States.Dead))
            {

                Destroy(gameObject);
            }

            if (hp <= 0)
            {
                _stateMachine.ChangeState(Triggers.Died);
                return;
            }
            if ( IsMatchingState(States.AttackWait)||IsMatchingState(States.ShieldIdle))
            {
                SearchPlayer();
            }



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
            }
            // シールドタックル開始時
            if(IsMatchingState(States.ShieldTackle))
            {
                tackletime -= Time.fixedDeltaTime;
                if(tackletime<0)
                {
                    StateMachine.ChangeState(Triggers.CooldownEnd);
                }
            }

        }
        private void AutoRecoverFromStun()
        {
            if (!_isStunning) return; // 二重実行防止

            
            animator.SetTrigger("stunrecover"); // ✅ リカバーアニメーション再生
            
            // _stateMachine.ChangeState(Triggers.StartBattle); // Idleへ復帰
        }

        public void tackleEnd()
        {
            if (IsMatchingState(States.ShieldTackle))
            {
                _stateMachine.ChangeState(Triggers.StartBattle);
                animator.SetTrigger("idle");
                Debug.Log($"tackleEnd 実行後: 現在のステート = {_stateMachine.CurrentState.key}");
            }
        }
        public void Recover()
        {
            Debug.Log("スタン復帰");
            _isStunning = false;
            OnStunEnd?.Invoke();                // ✅ シールド再有効化など
            _stateMachine.ChangeState(Triggers.StartBattle);
        }
        public void stanEnd()
        {
            _isStunning = false;
            animator.SetTrigger("stunrecover");
            OnStunEnd?.Invoke();  
        }

        public void shootEnd()
        {
            Debug.Log("shoot終了");
            _stateMachine.ChangeState(Triggers.CooldownEnd);
            animator.SetTrigger("idle");
        }
        private void RecoverFromStun()
        {

            OnStunEnd?.Invoke(); // シールドを再有効化
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


                Direction = facingVector;

                // --- 発射方向をセット ---
                shoot.SetDirection(shootDir);


            }



            // --- クールダウン中は攻撃しない ---
            if (IsMatchingState(States.AttackWait))
            {
                if (cooldownTimer > 0)
                    return;
                if (_searchAssistance.Execute("meray", list, out _))
                {
                    animator.SetTrigger("tackle");
                    _stateMachine.ChangeState(Triggers.NearAttack);
                    cooldownTimer = attackCooldown;
                     
                }
                else if (_searchAssistance.Execute("renge", list, out _))
                {

                    _stateMachine.ChangeState(Triggers.FarAttack);
                    animator.SetTrigger("shoot");
                    cooldownTimer = attackCooldown;
                    
                }
                else
                {
                    _stateMachine.ChangeState(Triggers.LostPlayer);
                }
            }
            if (IsMatchingState(States.ShieldIdle) && _searchAssistance.Execute("renge", list, out _))
            {
                _stateMachine.ChangeState(Triggers.FoundPlayer);
                
            }


        }



        public void TakeDamage(int damage)
        {
            hp -= damage;
            Debug.Log(hp);
            if (hp <= 0)
            {
                _stateMachine.ChangeState(Triggers.Died);

            }
        }
    }
}
