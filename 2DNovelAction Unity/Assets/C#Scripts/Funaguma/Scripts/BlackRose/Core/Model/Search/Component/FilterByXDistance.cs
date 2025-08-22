using BlackRose.Core.Models.Units;
using BlackRose.Editors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.SearchSystems
{
    [Serializable]
    public class FilterByXDistance : IFilterComponent
    {
        [SerializeReference, SubclassSelector]
        private UnitBase _parent;
        [SerializeField]
        private float _detectionDistance;
        public FilterByXDistance(UnitBase parent, float detectionDistance)
        {
            _parent = parent;
            _detectionDistance = detectionDistance;
        }
        public FilterByXDistance() { }
        public List<UnitBase> Execute(List<UnitBase> units)
        {
            float myX = _parent.transform.position.x;
            List<UnitBase> result = new List<UnitBase>();
            foreach (var unit in units)
            {
                if (unit == _parent)
                    continue;

                float targetX = unit.transform.position.x;
                float distance = Mathf.Abs(targetX - myX);
                bool flag = distance <= _detectionDistance;
#if UNITY_EDITOR
                Debug.Log(StringProssecing.GetFilterSummary(this, unit, flag, $"\nDitection:{_detectionDistance}\nDistance:{distance}"));
#endif
                if (flag)
                {
                    result.Add(unit);
                }
            }
            return result;
        }
    }
}