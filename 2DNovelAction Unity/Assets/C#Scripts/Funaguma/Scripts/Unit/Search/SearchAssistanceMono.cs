using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose
{
    public class SearchAssistanceMono : MonoBehaviour
    {
        [Serializable]
        public class SearchCompInfo
        {
            public string Key;
            [SerializeReference, SubclassSelector]
            public IFilterComponent Comp;
            public int Priority; // プライオリティが低いほど、検索において優先される
        }
        [SerializeField] private List<SearchCompInfo> _comps = new();

        public void AddComp(string key, IFilterComponent comp, int priority = 0)
        {
            var existing = _comps.FirstOrDefault(c => c.Key == key);
            if (existing != null)
            {
                existing.Comp = comp;
                existing.Priority = priority;
            }
            else
            {
                _comps.Add(new SearchCompInfo { Key = key, Comp = comp, Priority = priority });
            }
        }

        public void RemoveComp(string key)
        {
            _comps.RemoveAll(c => c.Key == key);
        }

        public List<IUnit> Execute(List<IUnit> pools)
        {
            foreach (var compInfo in _comps.OrderBy(c => c.Priority))
            {
                pools = compInfo.Comp.Execute(pools);
                if (pools.Count <= 0) break;
            }
            return pools;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            _comps.OrderBy(x => x.Priority);
        }
#endif
    }
}