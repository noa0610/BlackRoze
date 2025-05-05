using TMPro;
using UnityEngine;

namespace Test
{
    public abstract class UnitBase : MonoBehaviour, IUnit, IStopableObject
    {
        [Header("Datas")]
        [SerializeField] protected UnitStatus _status;
        [SerializeField] protected StateFlags _stateFlags; // 現在の状態（移動中・攻撃中など）
        [SerializeField] protected Animator _animator;
        [Header("StateMachine")]
        protected StateMachine _stateMachine; // ステートマシン本体
        private IState _defaultState;

        protected virtual IState DefaultState => _defaultState ??= new Idle();
        protected virtual string DefaultStateKey => "idle";
        public StateMachine StateMachine => _stateMachine; // 外部からステートマシン取得
        public StateFlags StateFlags => _stateFlags; // 状態フラグ取得
        public Transform Transform => transform;
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
        public UnitStatus UnitStatus
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }


        protected virtual void Start()
        {
            UnitManager.instance.AddUnit(this);
            _stateMachine = new StateMachine(this, DefaultState, StateDecision, DefaultStateKey);
            RegisterStats();
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
    }
}
//unicode