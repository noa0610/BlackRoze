using UnityEngine;

namespace BlackRose
{
    public abstract class UnitBase : MonoBehaviour, IUnit, IStopableObject
    {
        // === Reference ===
        protected IStatusManager _statusManager;
        [Header("Datas")]
        [SerializeField] protected UnitStatusData _status;
        [SerializeField] protected StateFlags _stateFlags; // 現在の状態（移動中・攻撃中など）
        [SerializeField] protected Animator _animator;
        [Header("StateMachine")]
        protected IStateMachine _stateMachine; // ステートマシン本体
        private IState _defaultState;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private string _currentState;
#endif

        protected virtual IState DefaultState => _defaultState ??= new Idle();
        protected virtual string DefaultStateKey => "idle";
        public UnitStatusData UnitStatusData => _status;
        public IStateMachine StateMachine => _stateMachine; // 外部からステートマシン取得
        public StateFlags StateFlags => _stateFlags; // 状態フラグ取得
        public Transform Transform => transform;
        public IStatusManager StatusManager => _statusManager;
        public Vector2 Direction {  get; set; }
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

        protected virtual void Start()
        {
            UnitManager.instance.AddUnit(this);
            _stateMachine = new StateMachine(this, DefaultState, StateDecision, DefaultStateKey);
            _statusManager = new StatusManager();
            RegisterStatus();
            RegisterStats();
        }

        protected virtual void Update()
        {
            _stateMachine.Update();
            var s = _stateMachine.CurrentState.Item1;
            _currentState = s;

        }

        protected virtual void RegisterStatus()
        {
            _statusManager.AddStatus(Status.HP, _status.maxHp);
            _statusManager.AddStatus(Status.Speed, _status.speed);
            _statusManager.AddStatus(Status.SpeedInAir, _status.speedInAir);
            _statusManager.AddStatus(Status.JumpPower, _status.jumpPower);
            _statusManager.AddStatus(Status.Power, _status.power);
            _statusManager.DeadCallBack += DeadCallBack;
        }
        protected abstract void RegisterStats();
        protected abstract string StateDecision();
        // ====== [一時停止関連のインターフェース実装（未実装）] ======

        public virtual void GamePlay_Pose()
        {
            // ここにゲーム一時停止処理を書く予定
        }

        public virtual void Dispose()
        {
            // ゲーム終了時の後始末など
        }

        public virtual void GamePlay_Continue()
        {
            // 停止からの再開処理を書く場所
        }

        // ===== ステータス操作 =====

        public virtual void TakeDamage(float damage)
        {
            _statusManager?.TakeDamage(damage);
        }

        protected virtual void DeadCallBack()
        {
            gameObject.SetActive(false);
            Debug.Log($"{_status.name}が死亡した");
        }
    }
}
//unicode