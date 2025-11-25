using BlackRose.Core.Models.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose.Core.Models.SearchSystems
{
    /// <summary>
    /// ユニットの検索に使う。
    /// 候補になるユニットリストは <see cref="UnitManager.GetUnitList"/> 等で取得する。
    /// <example>
    /// 使い方:
    /// <code>
    /// if (searchAssistance.Execute("myProfile", unitList, out var resultUnits))
    /// {
    ///     var target = BlackRose.Core.Models.Helper.UnitHelper
    ///                   .GetUnitNearest(resultUnits, transform.position);
    ///     任意のメソッド.SetTarget(target);
    /// }
    /// </code>
    /// </example>
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
        private Dictionary<string, SearchProfile> _profileMap;

        public void AddComp(string key, SearchCompInfo info)
        {
            if (TryGetProfile(key, out var profile))
            {
                if (profile.AddComp(info))
                {
                    // ここでのみ確実にソート
                    profile.comps.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                }
            }
            else
            {
                var profile1 = new SearchProfile
                {
                    profileName = key,
                    comps = new List<SearchCompInfo>
            {
                // ★ 修正: id = info.id
                new SearchCompInfo { id = info.id, Comp = info.Comp, Priority = info.Priority }
            }
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

        public bool Execute(string key, List<UnitBase> candidates, out List<UnitBase> res)
        {
            res = null;
            if (candidates == null || candidates.Count == 0)
            {
                Debug.LogWarning("Candidates list is null or empty.");
                return false;
            }
            // デフォルトキー決定を安全化
            if (string.IsNullOrEmpty(key))
            {
                if (_profiles.Count == 0)
                {
                    Debug.LogWarning("No profiles available.");
                    return false;
                }
                key = _profiles[0].profileName;
            }

            if (!TryGetProfile(key, out var profile) || profile.comps == null || profile.comps.Count == 0)
            {
                Debug.LogWarning($"No profile found for key: {key} or no components defined.");
                return false;
            }

            // 実行時に OrderBy せず、既にソートされている前提で for ループ
            var pool = candidates;
            for (int i = 0; i < profile.comps.Count; i++)
            {
                var comp = profile.comps[i];
                pool = comp.Comp.Execute(pool);
                if (pool == null || pool.Count == 0) return false;
            }

            res = pool;
            return true;
        }

        public bool TryGetProfile(string key, out SearchProfile profile)
        {
            profile = _profiles.FirstOrDefault(c => c.profileName == key);
            return profile != null;
        }
        private void RebuildMap()
        {
            _profileMap = new Dictionary<string, SearchProfile>(_profiles.Count);
            foreach (var p in _profiles)
            {
                if (string.IsNullOrEmpty(p.profileName)) continue;
                // 後勝ち/先勝ちは設計に合わせて
                _profileMap[p.profileName] = p;

                // ついでに comps を整列
                if (p.comps != null && p.comps.Count > 1)
                    p.comps.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Null/空警告 & 並び替え
            foreach (var profile in _profiles)
            {
                if (profile == null) continue;

                if (profile.comps == null || profile.comps.Count == 0)
                {
                    Debug.LogWarning($"Profile '{profile.profileName}' has no components defined.", this);
                    continue;
                }
            }
            // マップ再構築
            RebuildMap();
        }
#endif

    }
}