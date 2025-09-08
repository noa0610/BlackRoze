using BlackRose.Core.Models.States;
using System;
using System.Collections.Generic;

namespace BlackRose.Core.Models.Helper
{
    public static class StateMachineHelper
    {
        // =========================
        // 非モード（Enum）: 既存（そのまま）
        // =========================
        /// <summary>
        /// 非モード版：from に対して複数 (trigger -> to) を一括追加
        /// 既存キーがあっても例外を投げず、安全に上書きします。
        /// </summary>
        public static IStateMachine AddTransmissions<TTrigger, TState>(
            this IStateMachine machine,
            TState from,
            params (TTrigger trigger, TState to)[] triggers)
            where TTrigger : Enum
            where TState : Enum
        {
            if (machine is null) return machine;
            var fromKey = from.ToString();
            foreach (var (trigger, to) in triggers)
            {
                if (trigger is null || to is null) continue;
                machine.AddTransition(fromKey, trigger.ToString(), to.ToString());
            }
            return machine;
        }

        // ★追加：非モード（Enum）アニメトリガー付き
        /// <summary>
        /// 非モード版：from に対して複数 (trigger -> to, anim) を一括追加
        /// </summary>
        public static IStateMachine AddTransmissions<TTrigger, TState>(
            this IStateMachine machine,
            TState from,
            params (TTrigger trigger, TState to, string animationTrigger)[] triggers)
            where TTrigger : Enum
            where TState : Enum
        {
            if (machine is null) return machine;
            var fromKey = from.ToString();
            foreach (var (trigger, to, anim) in triggers)
            {
                if (trigger is null || to is null) continue;
                machine.AddTransition(fromKey, trigger.ToString(), to.ToString(), anim ?? string.Empty);
            }
            return machine;
        }

        // =========================
        // 非モード（string）: 既存（そのまま）
        // =========================
        /// <summary>文字列版（Enumじゃない運用にも対応）</summary>
        public static IStateMachine AddTransmissions(
            this IStateMachine machine,
            string from,
            params (string trigger, string to)[] triggers)
        {
            if (machine is null || string.IsNullOrEmpty(from)) return machine;
            foreach (var (trigger, to) in triggers)
            {
                if (string.IsNullOrEmpty(trigger) || string.IsNullOrEmpty(to)) continue;
                machine.AddTransition(from, trigger, to);
            }
            return machine;
        }

        // ★追加：非モード（string）アニメトリガー付き
        /// <summary>文字列版：アニメトリガー付き一括登録</summary>
        public static IStateMachine AddTransmissions(
            this IStateMachine machine,
            string from,
            params (string trigger, string to, string animationTrigger)[] triggers)
        {
            if (machine is null || string.IsNullOrEmpty(from)) return machine;
            foreach (var (trigger, to, anim) in triggers)
            {
                if (string.IsNullOrEmpty(trigger) || string.IsNullOrEmpty(to)) continue;
                machine.AddTransition(from, trigger, to, anim ?? string.Empty);
            }
            return machine;
        }

        // =========================
        // モード依存（Enum）: 既存名を維持
        // =========================
        /// <summary>
        /// ★モード依存版：同じ from & triggers でも mode によって行き先を分岐
        /// 定義がある場合は非モード表より優先されます。
        /// </summary>
        public static IStateMachine AddTransitionsForLayer<TMode, TTrigger, TStateFrom, TStateTo>(
            this IStateMachine machine,
            TMode mode,
            TStateFrom from,
            params (TTrigger trigger, TStateTo to)[] triggers)
            where TMode : Enum
            where TTrigger : Enum
            where TStateFrom : Enum
            where TStateTo : Enum
        {
            if (machine is null) return machine;
            var modeKey = mode.ToString();
            var fromKey = from.ToString();
            foreach (var (trigger, to) in triggers)
            {
                if (trigger is null || to is null) continue;
                machine.AddTransitionForLayer(modeKey, fromKey, trigger.ToString(), to.ToString());
            }
            return machine;
        }

        // ★追加：モード依存（Enum）アニメトリガー付き
        public static IStateMachine AddTransitionsForLayer<TMode, TTrigger, TStateFrom, TStateTo>(
            this IStateMachine machine,
            TMode mode,
            TStateFrom from,
            params (TTrigger trigger, TStateTo to, string animationTrigger)[] triggers)
            where TMode : Enum
            where TTrigger : Enum
            where TStateFrom : Enum
            where TStateTo : Enum
        {
            if (machine is null) return machine;
            var modeKey = mode.ToString();
            var fromKey = from.ToString();
            foreach (var (trigger, to, anim) in triggers)
            {
                if (trigger is null || to is null) continue;
                machine.AddTransitionForLayer(modeKey, fromKey, trigger.ToString(), to.ToString(), anim ?? string.Empty);
            }
            return machine;
        }

        // =========================
        // モード依存（string）: 既存（そのまま）
        // ※ 名前ゆらぎ対策で既存名を残す
        // =========================
        /// <summary>文字列版モード依存</summary>
        public static IStateMachine AddTransmissionsForLayer(
            this IStateMachine machine,
            string mode,
            string from,
            params (string trigger, string to)[] triggers)
        {
            if (machine is null || string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(from)) return machine;
            foreach (var (trigger, to) in triggers)
            {
                if (string.IsNullOrEmpty(trigger) || string.IsNullOrEmpty(to)) continue;
                machine.AddTransitionForLayer(mode, from, trigger, to);
            }
            return machine;
        }

        // ★追加：モード依存（string）アニメトリガー付き
        public static IStateMachine AddTransmissionsForLayer(
            this IStateMachine machine,
            string mode,
            string from,
            params (string trigger, string to, string animationTrigger)[] triggers)
        {
            if (machine is null || string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(from)) return machine;
            foreach (var (trigger, to, anim) in triggers)
            {
                if (string.IsNullOrEmpty(trigger) || string.IsNullOrEmpty(to)) continue;
                machine.AddTransitionForLayer(mode, from, trigger, to, anim ?? string.Empty);
            }
            return machine;
        }

        // =========================
        // おまけ：全件に共通アニメトリガーを貼る簡易版
        // =========================
        /// <summary>
        /// 非モード（Enum）：全件に同じ animationTrigger を適用
        /// </summary>
        public static IStateMachine AddTransmissionsWithAnim<TTrigger, TState>(
            this IStateMachine machine,
            TState from,
            string commonAnimationTrigger,
            params (TTrigger trigger, TState to)[] triggers)
            where TTrigger : Enum
            where TState : Enum
        {
            if (machine is null) return machine;
            var anim = commonAnimationTrigger ?? string.Empty;
            var fromKey = from.ToString();
            foreach (var (trigger, to) in triggers)
            {
                if (trigger is null || to is null) continue;
                machine.AddTransition(fromKey, trigger.ToString(), to.ToString(), anim);
            }
            return machine;
        }

        /// <summary>
        /// モード依存（Enum）：全件に同じ animationTrigger を適用
        /// </summary>
        public static IStateMachine AddTransitionsForLayerWithAnim<TMode, TTrigger, TStateFrom, TStateTo>(
            this IStateMachine machine,
            TMode mode,
            TStateFrom from,
            string commonAnimationTrigger,
            params (TTrigger trigger, TStateTo to)[] triggers)
            where TMode : Enum
            where TTrigger : Enum
            where TStateFrom : Enum
            where TStateTo : Enum
        {
            if (machine is null) return machine;
            var anim = commonAnimationTrigger ?? string.Empty;
            var modeKey = mode.ToString();
            var fromKey = from.ToString();
            foreach (var (trigger, to) in triggers)
            {
                if (trigger is null || to is null) continue;
                machine.AddTransitionForLayer(modeKey, fromKey, trigger.ToString(), to.ToString(), anim);
            }
            return machine;
        }
    }
}
