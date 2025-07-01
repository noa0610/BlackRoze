using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BlackRose
{
    [Serializable]
    public class FilterByTag : IFilterComponent
    {
        [SerializeField, Tooltip("検出したいタグ")]
        private UnitTags _tag;

        public FilterByTag(UnitTags tag)
        {
            _tag = tag;
        }
        public FilterByTag() { }
        public List<IUnit> Execute(List<IUnit> pool)
        {
            return pool.Where(u => u.UnitStatusData.tags.HasFlag(_tag)).ToList();
        }
    }
}