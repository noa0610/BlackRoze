using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Test
{
    public class FilterByRange : IFilterComponent
    {
        private Vector3 _center;
        private float _range;

        public FilterByRange(Vector3 center, float range)
        {
            _center = center;
            _range = range;
        }

        public List<IUnit> Execute(List<IUnit> pool)
        {
            return pool.Where(u => Vector3.Distance(u.Transform.position, _center) <= _range).ToList();
        }
    }
}