using System;
using UniRx;
using UnityEngine;
using HighElixir.Timers;
using BlackRose.Core.Models.EffectManagers;
using BlackRose.Core.Models.States;
using BlackRose.Core.Models.States.Animators;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SpriteEffectPlayer), typeof(Rigidbody2D)), Serializable]
    public abstract class UnitBase : MonoBehaviour, IPausable, IUnit
    {
        #region === Inspector References ===
        [Header("Datas")]
        [SerializeField] protected UnitStatusData _status;
        [SerializeField] protected LayerMask _attackLayer;
        [SerializeField] protected Animator _animator;

        [Header("StateMachine")]
        [SerializeField] public static bool _isPlaying = true;

        [Header("Objects")]
        [SerializeField] protected GameObject _muzzle;
        #endregion

        #region Dirs
        [SerializeField] private Vector2 _moveDir = Vector2.zero;
        [SerializeField] private Vector2 _shootDir = Vector2.right;

        #endregion

        #region === Components & Managers ===
        protected IStateMachine _stateMachine;
        private StatusManager _statusManager;
        private StatusEffectManager _effectManager;
        private SpriteEffectPlayer _spriteEffectPlayer;
        private Rigidbody2D _body2D;
        public SpriteEffectPlayer player; // 外部アクセス用
        #endregion

        #region === Reactive & Direction ===
        private ReactiveProperty<Vector2> _reactiveDirection = new(new(1, 0));
        public IObservable<Vector2> ReactiveDirection => _reactiveDirection;
        public Vector2 Direction
        {
            get => _reactiveDirection.Value;
            set => _reactiveDirection.Value = value;
        }
        public Vector2 MoveDirection { get => _moveDir; set => _moveDir = value; }
        public Vector2 ShootDir { get => _shootDir; set => _shootDir = value; }
        #endregion

        #region === Properties ===
        public Rigidbody2D Rigidbody2D => _body2D;
        public GameObject Muzzle => _muzzle;
        public UnitStatusData UnitStatusData => _status;
        public StatusManager statusManager => _statusManager; // 既存API互換
        public IStateMachine StateMachine => _stateMachine;
        public Transform Transform => transform;
        public Timer Timer { get; private set; }
        public StatusManager StatusManager => _statusManager;
        public StatusEffectManager StatusEffectManager => _effectManager;
        public SpriteEffectPlayer SpriteEffectPlayer => _spriteEffectPlayer;
        public Animator Animator
        {
            get => _animator;
            set => _animator = value;
        }
        public LayerMask AttackLayer => _attackLayer;
        public bool IsInvincible { get; set; }
        public bool IsArrivals { get; set; } = true;
        #endregion

#if UNITY_EDITOR
        #region === Debug ===
        [Header("Debug")]
        [SerializeField] private bool _enableVisuableInvincible;
        [SerializeField] private string _currentState;

        public virtual bool ShoudBeLogging => false;
        #endregion
#endif

        // Statusを初期値にセットする
        public virtual void Refresh()
        {
            _statusManager.Initialize(_status);
        }
        #region === Initialization ===
        protected virtual string StartState => "idle";
        protected abstract void RegisterStats();

        protected virtual void BeforeAwake() { }
        protected virtual void AfterAwake() { }
        protected virtual void BeforeRegisterStats() { }

        protected void Awake()
        {
            Timer = new Timer(gameObject.name);
            BeforeAwake();

            _spriteEffectPlayer = GetComponent<SpriteEffectPlayer>();
            if (_animator == null)
                _animator = GetComponent<Animator>();
            _body2D = GetComponent<Rigidbody2D>();

            UnitManager.instance.AddUnit(this);
            _stateMachine = new StateMachine(this, new AnimatorAnimationDriver(_animator));
            _statusManager = new StatusManager();
            _effectManager = new(this);

            BeforeRegisterStats();
            _statusManager.Initialize(_status);
            RegisterStats();

#if UNITY_EDITOR
            _stateMachine.Awake(StartState, ShoudBeLogging);
#else
            _stateMachine.Awake(StartState, false);
#endif
            AfterAwake();
        }

        protected virtual void Start()
        {
        }
        #endregion

        #region === Update Cycle ===
        protected virtual void BeforeUpdate() { }
        protected virtual void OnUpdate() { }
        protected virtual void AfterUpdate() { }
        protected virtual void BeforeFixedUpdate() { }
        protected virtual void AfterFixedUpdate() { }

        protected void Update()
        {
            BeforeUpdate();
            if (!_isPlaying) return;

            OnUpdate();

            var dt = Time.deltaTime;
            _stateMachine.UpdateMachine(dt);
            Timer.Update(dt);
            _effectManager.Update();

            AfterUpdate();
        }

        protected virtual void FixedUpdate()
        {
#if UNITY_EDITOR
            _currentState = _stateMachine.CurrentState.key;
#endif
            BeforeFixedUpdate();
            if (!_isPlaying) return;
            AfterFixedUpdate();
        }
        #endregion

        #region === Status & Damage ===
        public void TakeDamage(IUnit from, float damage)
        {
            if (!_isPlaying || !IsArrivals) return;
            if (!BeforeTakeDamage(from, ref damage)) return;

            if (_statusManager.TakeDamage(damage))
                OnDeath();

            OnTakeDamage(from, damage);
        }

        protected virtual bool BeforeTakeDamage(IUnit from, ref float damage) => true;
        protected virtual void OnTakeDamage(IUnit from, float damage) { }

        protected virtual void OnDeath()
        {
            Debug.Log($"{_status.name}が死亡した");
            IsArrivals = false;
        }
        #endregion

        #region === Pause & Play ===
        public virtual void Pause() { }
        public virtual void Play() { }
        #endregion

        public void PlaySE(string bgmName, float volume = 1f)
        {
            if (SoundManager.instance != null && bgmName != null)
            {
                SoundManager.instance.PlaySE(bgmName, volume);
            }
        }

#if UNITY_EDITOR
        #region === Gizmos ===
        private void OnDrawGizmosSelected()
        {
            if (_enableVisuableInvincible && IsInvincible)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(transform.position, new Vector3(2, 2, 0.1f));
            }
        }
        #endregion
#endif
    }
}
