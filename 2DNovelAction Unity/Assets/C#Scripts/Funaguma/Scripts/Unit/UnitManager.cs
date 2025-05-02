using HighElixir.Utilities;
using System.Collections.Generic;

namespace Test
{
    public class UnitManager : SingletonBehavior<UnitManager>
    {
        private List<IUnit> _unitList = new List<IUnit>();
        public void AddUnit(IUnit unit)
        {
            _unitList.Add(unit);
        }
        public void RemoveUnit(IUnit unit)
        {
            _unitList.Remove(unit);
        }
        public void Clear()
        {
            _unitList.Clear();
        }

        public List<IUnit> GetUnitList()
        {
            return _unitList;
        }
    }
}