using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose
{
    public class SearchAssistanceMono : MonoBehaviour, ISearch
    {
        [Serializable]
        public class SearchProfile
        {
            public string key;
            public List<SearchCompInfo> comps = new List<SearchCompInfo>();

            public void AddComp(SearchCompInfo compInfo)
            {
                var existingComp = comps.FirstOrDefault(c => c.Key == compInfo.Key);
                if (existingComp != null)
                {
                    existingComp.Comp = compInfo.Comp;
                    existingComp.Priority = compInfo.Priority;
                }
                else
                {
                    comps.Add(compInfo);
                }
            }
        }
        [SerializeField] private List<SearchProfile> _profiles = new();

        public void AddComp(string key, SearchCompInfo info)
        {
            if (TryGetProfile(key, out var profile))
            {
                profile.AddComp(info);
                profile.comps = profile.comps.OrderBy(x => x.Priority).ToList();
            }
            else
            {
                var profile1 = new SearchProfile
                {
                    key = key,
                    comps = new List<SearchCompInfo> { new SearchCompInfo { Key = key, Comp = info.Comp, Priority = info.Priority } }
                };
                _profiles.Add(profile1);
            }
        }

        public void RemoveComp(string key, SearchCompInfo info)
        {
            if (TryGetProfile(key, out var profile))
            {
                var compToRemove = profile.comps.FirstOrDefault(c => c.Key == info.Key);
                if (compToRemove != null)
                {
                    profile.comps.Remove(compToRemove);
                    if (profile.comps.Count == 0)
                    {
                        _profiles.Remove(profile);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"No profile found for key: {key}");
            }
        }

        public bool Execute(string key, List<UnitBase> pools, out List<UnitBase> res)
        {
            res = new List<UnitBase>();
            if (string.IsNullOrEmpty(key))
                key = _profiles[0].key; // デフォルトのプロファイルキーを使用
            if (TryGetProfile(key, out var profile) && profile.comps.Count > 0)
            {
                foreach (var compInfo in profile.comps.OrderBy(c => c.Priority))
                {
                    pools = compInfo.Comp.Execute(pools);
                    if (pools.Count <= 0) break;
                }
                if (pools.Count <= 0) return false;
                res = pools;
                return true;
            }
            else
            {
                Debug.LogWarning($"No profile found for key: {key} or no components defined.");
                return false;
            }
        }
        private bool TryGetProfile(string key, out SearchProfile profile)
        {
            profile = _profiles.FirstOrDefault(c => c.key == key);
            return profile != null;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var profile in _profiles)
            {
                if (profile.comps == null || profile.comps.Count == 0)
                {
                    Debug.LogWarning($"Profile '{profile.key}' has no components defined.", this);
                }
                else
                    profile.comps = profile.comps.OrderBy(x => x.Priority).ToList();

            }
        }
#endif
    }
}