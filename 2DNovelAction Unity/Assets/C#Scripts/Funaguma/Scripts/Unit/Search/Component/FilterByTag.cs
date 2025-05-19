using System.Collections.Generic;
using System.Linq;

namespace BlackRose
{
    public class FilterByTag : IFilterComponent
    {
        private UnitTags _tag;

        public FilterByTag(UnitTags tag)
        {
            _tag = tag;
        }

        public List<IUnit> Execute(List<IUnit> pool)
        {
            return pool.Where(u => u.UnitStatus.tags.HasFlag(_tag)).ToList();
        }
    }
}