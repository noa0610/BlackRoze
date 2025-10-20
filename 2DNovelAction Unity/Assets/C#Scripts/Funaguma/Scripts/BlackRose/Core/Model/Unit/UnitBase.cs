using BlackRose.Core.Models.EffectManagers;
using BlackRose.Core.Models.States;
using BlackRose.Core.Models.States.Animators;
using HighElixir.Timers;
using System;
using UniRx;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SpriteEffectPlayer), typeof(Animator)), Serializable]
    public abstract class UnitBase : MonoBehaviour, IPausable, IUnit
    {
        // === Reference ===
        public SpriteEffectPlayer player;
        public StatusManager statusManager;
        public StatusEffectManager effectManager;
        [Header("Datas")]
        [SerializeField] protected UnitStatusData _status;
        protected Animator _animator;
        [Header("StateMachine")]
        [SerializeField] public static bool _isPlaying = true;
        protected IStateMachine _stateMachine; // ステートマシン本体
        private ReactiveProperty<Vector2> _reactiveDirection = new(new(1, 0));
        private SpriteEffectPlayer _spriteEffectPlayer;
#if UNITY_EDITOR
        // エディタからの監視用
        [Header("Debug")]
        [SerializeField] private string _currentState;
        [SerializeField] private string _currentMode;
        [SerializeField] private Vector2 _currentDirection;

        public string CurrentState => _currentState;
#endif
        public UnitStatusData UnitStatusData => _status;
        public IStateMachine StateMachine => _stateMachine; // 外部からステートマシン取得
        public Transform Transform => transform;
        public Timer Timer { get; private set; }
        public StatusManager StatusManager => statusManager;
        public StatusEffectManager StatusEffectManager => effectManager;
        public SpriteEffectPlayer SpriteEffectPlayer => _spriteEffectPlayer;
        public IObservable<Vector2> ReactiveDirection => _reactiveDirection;

        // 向き（1か-1の値をとる。外部から設定される）
        public Vector2 Direction
        {
            get
            {
                return _reactiveDirection.Value;
            }
            set
            {
                _reactiveDirection.Value = value;
            }
        }

        // 移動方向（外部から設定される）
        public Vector2 MoveDirection { get; set; }
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
        public bool IsInvincible { get; set; }

        // 初期状態のステート
        protected virtual string StartState => "idle";
        protected abstract void RegisterStats();

        // ===== ステータス操作 =====

        public void TakeDamage(IUnit from, float damage)
        {
            if (!BeforeTakeDamage(from, ref damage)) return;
            if (statusManager.TakeDamage(damage))
                OnDeath();
            OnTakeDamage(from, damage);
        }

        protected virtual bool BeforeTakeDamage(IUnit from, ref float damage)
        {
            return true;
        }
        protected virtual void OnTakeDamage(IUnit from, float damage)
        {
        }

        protected virtual void OnDeath()
        {
            Debug.Log($"{_status.name}が死亡した");
        }

        public virtual void Pause()
        {
        }

        public virtual void Play()
        {
        }

        protected void Awake()
        {
            Timer = new Timer(gameObject.name);
            BeforeAwake();
            _spriteEffectPlayer = GetComponent<SpriteEffectPlayer>();
            _animator = GetComponent<Animator>();
            UnitManager.instance.AddUnit(this);
            _stateMachine = new StateMachine(this, new AnimatorAnimationDriver(_animator));
            statusManager = new StatusManager();
            effectManager = new(this);

            // ステートとステータス登録
            BeforeRegisterStats();
            statusManager.Initialize(_status);
            RegisterStats();

            // ステートマシン起動
            _stateMachine.Awake(StartState);
            AfterAwake();
        }

        protected virtual void BeforeAwake() { }
        protected virtual void AfterAwake() { }
        protected virtual void BeforeRegisterStats() { }

        protected virtual void Start()
        {
            BeforeStart();
#if UNITY_EDITOR
            ReactiveDirection.Subscribe(v => _currentDirection = v).AddTo(this);
#endif
            AfterStart();
        }

        protected virtual void BeforeStart() { }
        protected virtual void AfterStart() { }
        // UnitBaseではUnityコンポーネントではないクラスのアップデート呼び出しを行っている
        protected void Update()
        {
            BeforeUpdate();
            if (!_isPlaying) return;
            OnUpdate();
            var dt = Time.deltaTime;
            _stateMachine.UpdateMachine(dt);
            Timer.Update(dt);
            effectManager.Update();
            AfterUpdate();
# if UNITY_EDITOR
            _currentState = _stateMachine.CurrentState.key;
            _currentMode = _stateMachine.CurrentLayer;
#endif
        }
        // _isPlayingの判定の前に呼ばれる（常に呼ばれる）
        protected virtual void BeforeUpdate() { }
        // ステートマシンのアップデートの前に呼ばれる（_isPlayingがtrueのときのみ呼ばれる）
        protected virtual void OnUpdate() { }

        // ステートマシンのアップデートの後に呼ばれる（_isPlayingがtrueのときのみ呼ばれる）
        protected virtual void AfterUpdate() { }
        protected virtual void FixedUpdate()
        {
            BeforeFixedUpdate();
            if (!_isPlaying) return;
            AfterFixedUpdate();
        }
        // _isPlayingの判定の前に呼ばれる（常に呼ばれる）
        protected virtual void BeforeFixedUpdate() { }

        // _isPlayingがtrueのときのみ呼ばれる
        protected virtual void AfterFixedUpdate() { }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var pos = transform.position + new Vector3(0, 1, 0);
            Handles.Label(pos, _currentState);
        }
#endif
    }
}