using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose
{
    public class SearchAssistanceMono : MonoBehaviour, ISearch
    {
        [SerializeField] private List<SearchCompInfo> _infos = new List<SearchCompInfo>();

        public void AddComp(string key, SearchCompInfo info)
        {
            var existing = _infos.FirstOrDefault(c => c.Key == key);
            if (existing != null)
            {
                existing.Comp = info.Comp;
                existing.Priority = info.Priority;
            }
            else
            {
                _infos.Add(new SearchCompInfo { Key = key, Comp = info.Comp, Priority = info.Priority });
            }
        }

        public void RemoveComp(string key, SearchCompInfo info)
        {
            _infos.RemoveAll(c => c.Key == key);
        }

        public bool Execute(string key, List<UnitBase> pools, out List<UnitBase> res)
        {
            res = new List<UnitBase>();
            foreach (var compInfo in _infos.OrderBy(c => c.Priority))
            {
                pools = compInfo.Comp.Execute(pools);
                if (pools.Count <= 0) break;
            }
            if (pools.Count <= 0) return false;
            res = pools;
            return true;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            _infos.OrderBy(x => x.Priority);
        }
#endif
    }
}