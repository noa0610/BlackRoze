using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Test
{
    public sealed class Death : MonoBehaviour
    {
        private void Start()
        {
            var t = GetComponent<UnitBase>();
            var current = t.statusManager.ReadValue(Core.Models.Status.HP);
            UnitManager.instance.AddDamage(t, DebugDamageWorker.Instance, current);
            Destroy(this);
        }
    }
}