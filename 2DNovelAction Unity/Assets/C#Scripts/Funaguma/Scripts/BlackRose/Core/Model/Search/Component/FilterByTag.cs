using BlackRose.Core.Models.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BlackRose.Core.Models.SearchSystems
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
        public List<UnitBase> Execute(List<UnitBase> pool)
        {
            return pool.Where(u => u.UnitStatusData.tags.HasFlag(_tag)).ToList();
        }
    }
}