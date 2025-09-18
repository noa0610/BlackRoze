using BlackRose.Core.Models.EffectManagers;
using BlackRose.Core.Models.States;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SpriteEffectPlayer)), Serializable]
    public abstract class UnitBase : MonoBehaviour, IPausable, IUnit
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
        [SerializeField] public static bool _isPlaying = true;

#if UNITY_EDITOR
        // エディタからの監視用
        [Header("Debug")]
        [SerializeField] private string _currentState;
        [SerializeField] private Vector2 _currentDirection;
#endif
        public UnitStatusData UnitStatusData => _status;
        public IStateMachine StateMachine => _stateMachine; // 外部からステートマシン取得
        public Transform Transform => transform;
        public StatusManager StatusManager => statusManager;
        public StatusEffectManager StatusEffectManager => effectManager;
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
        public bool IsInvincible { get; set; }

        // 初期状態のステート
        protected virtual string StartState => "idle";
        protected abstract void RegisterStats();

        // ===== ステータス操作 =====

        public void TakeDamage(IUnit source, float damage)
        {
            if (!BeforeTakeDamage(ref damage)) return;
            if (statusManager.TakeDamage(damage))
                OnDeath();
            OnTakeDamage(source, damage);
        }

        protected virtual bool BeforeTakeDamage(ref float damage)
        {
            return true;
        }
        protected virtual void OnTakeDamage(IUnit s, float damage)
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
            BeforeAwake();
            UnitManager.instance.AddUnit(this);
            _stateMachine = new StateMachine(this);
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
        // UnitBaseではUnityコンポーネントではないクラスのアップデート呼び出しを行っている
        protected void Update()
        {
            BeforeUpdate();
            if (!_isPlaying) return;
            OnUpdate();
            _stateMachine.UpdateMachine(Time.deltaTime);
            effectManager.Update();
            AfterUpdate();
# if UNITY_EDITOR
            _currentState = _stateMachine.CurrentState.key;
            _currentDirection = Direction;
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
    }
}