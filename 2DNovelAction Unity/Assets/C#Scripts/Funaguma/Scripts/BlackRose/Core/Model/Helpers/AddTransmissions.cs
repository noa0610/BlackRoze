using BlackRose.Core.Models.States;
using System;

namespace BlackRose.Core.Models.Helper
{
    public static class StateMachineHelper
    {
        public static IStateMachine AddTransmissions<Trigger, State>(this IStateMachine machine, State from, (Trigger trigger, State to)[] triggers) 
            where Trigger : Enum
            where State : Enum
        {
            foreach (var trg in triggers)
                machine.TransmissionGroup.Add(
                    (from.ToString(), trg.trigger.ToString()), trg.to.ToString());
            return machine;
        }
    }
}