using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose
{
    public class FilterByXDistance : IFilterComponent
    {
        private IUnit _parent;
        private float _detectionDistance;
        public FilterByXDistance(IUnit parent, float detectionDistance)
        {
            _parent = parent;
            _detectionDistance = detectionDistance;
        }
        public List<IUnit> Execute(List<IUnit> units)
        {
            float myX = _parent.Transform.position.x;
            List<IUnit> result = new List<IUnit>();
            foreach (var unit in units)
            {
                if (unit == _parent)
                    continue;

                float targetX = unit.Transform.position.x;
                float distance = Mathf.Abs(targetX - myX);
                bool flag = distance <= _detectionDistance;

                Debug.Log(Debugs.StringProssecing.GetFilterSummary(this, unit, flag, $"\nDitection:{_detectionDistance}\nDistance:{distance}"));
                if (flag)
                {
                    result.Add(unit);
                }
            }
            return result;
        }
    }
}