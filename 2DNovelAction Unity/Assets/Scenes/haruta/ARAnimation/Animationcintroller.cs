using BlackRose.Core.Models.States.Animators;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UnitAnimationController : MonoBehaviour, IAnimationDriver
{
    [SerializeField] private Animator animator;

    public string CurrentLayer { get; set; } = "Default";

    // ステート名 → Animatorトリガー名の対応表
    private readonly Dictionary<string, string> stateToTrigger = new()
    {
        { "Idle", "Idle" },
        { "Walk", "Walk" },
        { "Attack", "Attack" },
        { "Damage", "Damage" },
        { "Dead", "Dead" },
    };

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    // StateMachineから呼ばれる想定メソッド
    public void OnTransition(string from, string to, string animTrigger)
    {
        // animationTrigger が指定されている場合はそれを優先
        string trigger = string.IsNullOrEmpty(animTrigger) ? GetTriggerForState(to) : animTrigger;
        if (!string.IsNullOrEmpty(trigger))
        {
            animator.SetTrigger(trigger);
        }
    }

    public void OnSetState(string to)
    {
        // 直接状態が設定された場合もアニメを更新
        string trigger = GetTriggerForState(to);
        if (!string.IsNullOrEmpty(trigger))
        {
            animator.SetTrigger(trigger);
        }
    }

    private string GetTriggerForState(string state)
    {
        if (stateToTrigger.TryGetValue(state, out var trigger))
            return trigger;
        return null;
    }
}
