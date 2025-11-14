using BlackRose.Core.Models.States;
using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Test
{
    public class TestEnemy : UnitBase
    {
        protected override void RegisterStats()
        {
            StateMachine.AddState("idle", new Idle());
        }

        protected override void OnDeath()
        {
            Debug.Log($"死亡:{name}");
            statusManager.TakeHeal(statusManager.ReadValue(Core.Models.Status.MaxHP));
        }
    }
}