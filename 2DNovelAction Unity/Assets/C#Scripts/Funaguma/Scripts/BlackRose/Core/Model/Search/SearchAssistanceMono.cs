using BlackRose.Core.Models.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose.Core.Models.SearchSystems
{
    /// <summary>
    /// ユニットの検索に使う
    /// 候補になるユニットリストは<see cref="UnitManager.GetUnitList">UnitManager.GetUnitList()</see>等で取得する
    /// <summary>
    /// <br />使い方の例
    /// <code>
    /// if (searchAssistance.Execute("myProfile", unitList, out var resultUnits))
    /// {
    ///     var target = resultUnits.<see cref="BlackRose.Core.Models.Helper.UnitHelper.GetUnitNearest(List{UnitBase}, Vector3)">GetUnitNearest</see>(transform.position);
    ///     任意のメソッド.SetTarget(target);
    /// }
    /// </code>
    /// </summary>
    /// </summary>
    public class SearchAssistanceMono : MonoBehaviour, ISearch
    {
        [Serializable]
        public class SearchProfile
        {
            public string profileName;
            public List<SearchCompInfo> comps = new List<SearchCompInfo>();

            public bool AddComp(SearchCompInfo compInfo)
            {
                if (comps.FirstOrDefault(c => c.id == compInfo.id) != null) return false; // 既に存在する場合は追加しない
                comps.Add(compInfo);
                return true;
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
                    profileName = key,
                    comps = new List<SearchCompInfo> { new SearchCompInfo { id = key, Comp = info.Comp, Priority = info.Priority } }
                };
                _profiles.Add(profile1);
            }
        }

        public void RemoveComp(string profileName, string id)
        {
            if (TryGetProfile(profileName, out var profile))
            {
                var compToRemove = profile.comps.FirstOrDefault(c => c.id == id);
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
                Debug.LogWarning($"No profile found for key: {profileName}");
            }
        }

        public bool Execute(string key, List<UnitBase> pools, out List<UnitBase> res)
        {
            res = new List<UnitBase>();
            if (string.IsNullOrEmpty(key))
                key = _profiles[0].profileName; // デフォルトのプロファイルキーを使用
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
            profile = _profiles.FirstOrDefault(c => c.profileName == key);
            return profile != null;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var profile in _profiles)
            {
                if (profile.comps == null || profile.comps.Count == 0)
                {
                    Debug.LogWarning($"Profile '{profile.profileName}' has no components defined.", this);
                }
                else
                    profile.comps = profile.comps.OrderBy(x => x.Priority).ToList();

            }
        }
#endif
    }
}