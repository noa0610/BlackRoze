using System.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace BlackRose
{
    public class StateWithAnime : StateComp
    {
        [SerializeField] public string animeTriggerName;
        /// <summary>
        /// アニメーションが完了するまで遷移をブロックするかどうか
        /// </summary>
        [SerializeField] public bool waitForAnimeEnd = false;
        /// <summary>
        /// キャンセル可能になるアニメーション進行度
        /// </summary>
        [SerializeField] public float cancelableProgress = 0.7f;
        private bool? _hasTrigger = null;
        public UnityEvent OnAnimeationCompleted { get; set; } = new();

        public float GetNormalized(UnitBase parent)
        {
            if (HasTrigger (parent))
                return parent.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            else return 1f;
        }
        public override void Enter(IState previousIState, UnitBase parent)
        {
            if (HasTrigger(parent))
                parent.Animator.SetTrigger(animeTriggerName);
        }
        public override void Stay(UnitBase parent)
        {
            if (GetNormalized(parent) >= 1f) OnAnimeationCompleted?.Invoke();
        }
        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (!HasTrigger(parent)) return true; // トリガーがない場合は遷移許可
            // 通常の待ちフラグ判定
            if (!waitForAnimeEnd)
                return true;

            // キャンセル可能進行度を越えていれば遷移許可
            return GetNormalized(parent) >= cancelableProgress;
        }

        public bool HasTrigger(UnitBase parent)
        {
            if (_hasTrigger == null)
            {
                if (string.IsNullOrEmpty(animeTriggerName))
                {
                    _hasTrigger = false; // トリガー名が空の場合は存在しない
                    return false;
                }
                _hasTrigger = parent.Animator
                    .parameters
                    .Any(p => p.type == AnimatorControllerParameterType.Trigger
                           && p.name == animeTriggerName);
            }
            return _hasTrigger.Value;
        }
        public void ResetTriggerCheck()
        {
            _hasTrigger = null; // トリガーの存在チェックをリセット
        }
    }
    public static class StateExtension
    {
        public static T SetAnimeTrigger<T>(this T state, string trigger) where T : StateWithAnime
        {
            state.animeTriggerName = trigger;
            state.ResetTriggerCheck(); // トリガーの存在チェックをリセット
            return state;
        }
        public static T SetNeedWait<T>(this T state, bool isNeed) where T : StateWithAnime
        {
            state.waitForAnimeEnd = isNeed;
            return state;
        }
        public static T SetCancelableProgress<T>(this T state, float progress) where T : StateWithAnime
        {
            state.cancelableProgress = progress;
            return state;
        }
    }
}