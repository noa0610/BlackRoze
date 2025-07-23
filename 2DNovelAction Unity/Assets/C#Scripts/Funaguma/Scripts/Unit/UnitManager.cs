using HighElixir.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    public class UnitManager : SingletonBehavior<UnitManager>
    {
        private List<UnitBase> _unitList = new List<UnitBase>();
        public void AddUnit(UnitBase unit)
        {
            _unitList.Add(unit);
        }
        public void RemoveUnit(UnitBase unit)
        {
            _unitList.Remove(unit);
        }
        public void Clear()
        {
            _unitList.Clear();
        }

        public List<UnitBase> GetUnitList()
        {
            return _unitList;
        }

        public void AddDamage(UnitBase target, UnitBase from, float damage)
        {
            target.TakeDamage(damage);
        }

        public void Pause(bool pause, bool isTimeStop = true)
        {
            UnitBase._isPlaying = !pause;
            if (isTimeStop)
                Time.timeScale = pause ? 0f : 1f;
            foreach (var unit in _unitList)
            {
                if (unit is IPausable pausable)
                {
                    if (pause)
                        pausable.Pause();
                    else
                        pausable.Play();
                }
            }
        }
    }
}