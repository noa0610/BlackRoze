using System;
using UnityEngine;

namespace BlackRose
{
    [RequireComponent(typeof(SpriteEffectPlayer)), Serializable]
    public abstract class UnitBase : MonoBehaviour, IUnit, IStopableObject
    {
        // === Reference ===
        public SpriteEffectPlayer player;
        public StatusManager statusManager;
        public StatusEffectManager effectManager;
        [Header("Datas")]
        [SerializeField] protected UnitStatusData _status;
        [SerializeField] protected Animator _animator;
        [Header("StateMachine")]
        protected IStateMachine _stateMachine; // ステートマシン本体
        private StateComp _defaultState;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private string _currentState;
#endif

        protected virtual StateComp DefaultState => _defaultState ??= new Idle();
        protected virtual string DefaultStateKey => "idle";
        public UnitStatusData UnitStatusData => _status;
        public IStateMachine StateMachine => _stateMachine; // 外部からステートマシン取得
        public Transform Transform => transform;
        public StatusManager StatusManager => statusManager;
        public StatusEffectManager StatusEffectManager => effectManager;
        public SpriteEffectPlayer Player => player;
        public Vector2 Direction { get; set; } = new Vector2(1, 0); // ユニットの向き（右方向が1,0）
        public Animator Animator
        {
            get
            {
                return _animator;
            }
            set
            {
                _animator = value;
            }
        }

        // 子クラスで行いたい処理に合わせてBase.Awake()の位置は調整すること
        protected virtual void Awake()
        {
            UnitManager.instance.AddUnit(this);
            _stateMachine = new StateMachine(this, DefaultState, StateDecision, DefaultStateKey);
            statusManager = new StatusManager();
            effectManager = new(this);
            RegisterStatus();
            RegisterStats();
        }

        // UnitBaseではUnityコンポーネントではないクラスのアップデート呼び出しを行っている
        protected virtual void Update()
        {
            _stateMachine.UpdateMachine();
            effectManager.Update();
            var s = _stateMachine.CurrentState.Key;
            _currentState = s; // インスペクターからの監視用変数
        }

        protected virtual void RegisterStatus()
        {
            statusManager.AddStatus(Status.HP, _status.maxHp);
            statusManager.AddStatus(Status.Speed, _status.speed);
            statusManager.AddStatus(Status.SpeedInAir, _status.speedInAir);
            statusManager.AddStatus(Status.JumpPower, _status.jumpPower);
            statusManager.AddStatus(Status.DashSpeed, _status.dashSpeed);
            statusManager.AddStatus(Status.Power, _status.power);
            statusManager.AddStatus(Status.DamageRatio, 1);
            statusManager.DeadCallBack += DeadCallBack;
        }
        protected abstract void RegisterStats();
        protected abstract string StateDecision();
        // ====== [一時停止関連のインターフェース実装（未実装）] ======

        public virtual void GamePlay_Pose()
        {
            // ここにゲーム一時停止処理を書く予定
        }
        public virtual void GamePlay_Continue()
        {
            // 停止からの再開処理を書く場所
        }

        // ===== ステータス操作 =====

        public virtual void TakeDamage(float damage)
        {
            statusManager?.TakeDamage(damage);
        }

        protected virtual void DeadCallBack()
        {
            gameObject.SetActive(false);
            Debug.Log($"{_status.name}が死亡した");
        }
    }
}
//unicode