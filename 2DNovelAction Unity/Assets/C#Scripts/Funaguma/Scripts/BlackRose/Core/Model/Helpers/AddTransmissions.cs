using BlackRose.Core.Models.States;
using System;
using System.Collections.Generic;

namespace BlackRose.Core.Models.Helper
{
    public static class StateMachineHelper
    {
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
            // IStateMachine に AddTransition がある前提（なければ TransmissionGroup[...] = ... に置換）
            foreach (var (trigger, to) in triggers)
            {
                machine.AddTransition(from.ToString(), trigger.ToString(), to.ToString());
            }
            return machine;
        }

        /// <summary>
        /// 文字列版（Enumじゃない運用にも対応）
        /// </summary>
        public static IStateMachine AddTransmissions(
            this IStateMachine machine,
            string from,
            params (string trigger, string to)[] triggers)
        {
            foreach (var (trigger, to) in triggers)
            {
                machine.AddTransition(from, trigger, to);
            }
            return machine;
        }

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
            foreach (var (trigger, to) in triggers)
            {
                machine.AddTransitionForLayer(
                    mode.ToString(), from.ToString(), trigger.ToString(), to.ToString());
            }
            return machine;
        }

        /// <summary>
        /// 文字列版モード依存
        /// </summary>
        public static IStateMachine AddTransmissionsForLayer(
            this IStateMachine machine,
            string mode,
            string from,
            params (string trigger, string to)[] triggers)
        {
            foreach (var (trigger, to) in triggers)
            {
                machine.AddTransitionForLayer(mode, from, trigger, to);
            }
            return machine;
        }
    }
}
