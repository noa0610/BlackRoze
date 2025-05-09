using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose
{
    // 概要:
    // SearchAssistance クラスは、複数のフィルター処理（IFilterComponent）を組み合わせて順次実行し、
    // ユニット（IUnit）のリストに対して段階的な絞り込み（検索）を行う補助ツールです。
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
    // - IFilterComponentとIUnitの具体的な実装に依存します。
    // - フィルターの結果が空になった時点で処理を打ち切ります。
    // - UnityEngine.Debug.Logを用いて処理ログを出力しています。

    public class SearchAssistance : ISearch
    {
        private class SearchCompInfo
        {
            public string Key;
            public IFilterComponent Comp;
            public int Priority;
        }

        private List<SearchCompInfo> _comps = new();

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

                Debug.Log($"Executing filter: {compInfo.Key}\n Result : {Debugs.StringProssecing.GetUnitSummary(pools)}");
                if (pools.Count <= 0)
                    break;
            }
            return pools;
        }
    }
}
// unicode