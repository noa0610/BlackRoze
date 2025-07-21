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

        public UnityEvent OnAnimeationCompleted { get; set; } = new();

        public float GetNormalized(UnitBase parent) => parent.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        public override void Enter(IState previousIState, UnitBase parent)
        {
            parent.Animator.SetTrigger(animeTriggerName);
        }
        public override void Stay(UnitBase parent)
        {
            if (GetNormalized(parent) >= 1f)OnAnimeationCompleted?.Invoke();
        }
        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (!waitForAnimeEnd) return true;
            return GetNormalized(parent) >= cancelableProgress;
        }
    }
    public static class StateExtension
    {
        public static T SetAnimeTrigger<T>(this T state, string trigger) where T : StateWithAnime
        {
            state.animeTriggerName = trigger;
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