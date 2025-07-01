using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public class FilterByRange : IFilterComponent
    {
        [SerializeField]
        private Transform _center;
        [SerializeField]
        private float _range;

        public FilterByRange(Transform center, float range)
        {
            _center = center;
            _range = range;
        }
        public FilterByRange() { }
        public List<IUnit> Execute(List<IUnit> pool)
        {
            return pool.Where(u => Vector3.Distance(u.Transform.position, _center.position) <= _range).ToList();
        }
    }
}