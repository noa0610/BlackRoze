using BlackRose.Core.Models.EffectManagers;
using BlackRose.Core.Models.States;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SpriteEffectPlayer)), Serializable]
    public abstract class UnitBase : MonoBehaviour, IPausable
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
        [Header("Debug")]
        [SerializeField] private string _currentState;
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

        protected virtual void RegisterStatus()
        {
            statusManager.AddStatus(Status.HP, _status.maxHp);
            statusManager.AddStatus(Status.Speed, _status.speed);
            statusManager.AddStatus(Status.SpeedInAir, _status.speedInAir);
            statusManager.AddStatus(Status.JumpPower, _status.jumpPower);
            statusManager.AddStatus(Status.DashSpeed, _status.dashSpeed);
            statusManager.AddStatus(Status.Power, _status.power);
            statusManager.AddStatus(Status.DamageRatio, 1);
        }
        protected abstract void RegisterStats();

        // ===== ステータス操作 =====

        public virtual void TakeDamage(float damage)
        {
            IsInvincible = true;
            statusManager?.TakeDamage(damage);
        }

        protected virtual void DeadCallBack()
        {
            Debug.Log($"{_status.name}が死亡した");
        }

        public virtual void Pause()
        {
        }

        public virtual void Play()
        {
        }

        // 子クラスで行いたい処理に合わせてBase.Awake()の位置は調整すること
        protected virtual void Awake()
        {
            UnitManager.instance.AddUnit(this);
            _stateMachine = new StateMachine(this);
            statusManager = new StatusManager();
            effectManager = new(this);
            RegisterStatus();
            RegisterStats();
            _stateMachine.Awake(StartState);
        }

        // UnitBaseではUnityコンポーネントではないクラスのアップデート呼び出しを行っている
        protected virtual void Update()
        {
            if (!_isPlaying) return;
            _stateMachine.UpdateMachine();
            effectManager.Update();
# if UNITY_EDITOR
            var s = _stateMachine.CurrentState.key;
            _currentState = s; // インスペクターからの監視用変数
# endif
        }
    }
}