using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose
{
    // 概要:
    // SearchAssistance クラスは、複数のフィルター処理（IFilterComponent）を組み合わせて順次実行し、
    // ユニット（UnitBase）のリストに対して段階的な絞り込み（検索）を行う補助ツールです。
    // 各フィルターにはキー（string）と優先度（priority）を設定でき、
    // 優先度の昇順に処理が実行されます。
    // 
    // 主な機能:
    // - AddComp: 新しいフィルターコンポーネントの追加（同じキーのフィルターは上書き）
    // - RemoveComp: 指定キーのフィルターを削除
    // - Execute: 現在登録されているフィルターを順に適用し、結果をログ出力しながら返却
    // 
    // 用途:
    // 例として、複数の検索条件を持つユニット選定システム（AIやターゲット選別等）などに応用可能です。
    // 
    // 注意:
    // - IFilterComponentとUnitBaseの具体的な実装に依存します。
    // - フィルターの結果が空になった時点で処理を打ち切ります。
    // - UnityEngine.Debug.Logを用いて処理ログを出力しています。

    public class SearchAssistance : ISearch
    {
        private List<SearchCompInfo> _comps = new();

        public void AddComp(string key, SearchCompInfo info)
        {
            var existing = _comps.FirstOrDefault(c => c.Key == key);
            if (existing != null)
            {
                existing.Comp = info.Comp;
                existing.Priority = info.Priority;
            }
            else
            {
                _comps.Add(new SearchCompInfo { Key = key, Comp = info.Comp, Priority = info.Priority });
            }
        }

        public void RemoveComp(string key, SearchCompInfo info)
        {
            _comps.RemoveAll(c => c.Key == key);
        }

        public bool Execute(string key, List<UnitBase> pool, out List<UnitBase> res)
        {
            res = new List<UnitBase>();
            foreach (var compInfo in _comps.OrderBy(c => c.Priority))
            {
                pool = compInfo.Comp.Execute(pool);
                if (pool.Count <= 0)
                {
                    return false;
                }
            }
            res = new(pool);
            return true;
        }
    }
}
// unicode