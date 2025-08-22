using BlackRose.Core.Models.Units;
using System.Collections.Generic;

namespace BlackRose.Core.Models.SearchSystems
{

    // ISearch:
    // フィルターを追加・削除し、ユニットのリストに対して検索処理を実行するためのインターフェース。
    // 実装側は複数のフィルター（IFilterComponent）を管理し、段階的にユニットリストを処理します。
    public interface ISearch
    {
        // フィルターの追加。
        void AddComp(string key, SearchCompInfo info);

        // フィルターの削除。指定キーのコンポーネントを除去。
        void RemoveComp(string profileName, string id);

        // 検索処理を実行。フィルター順にユニットを処理し、結果を返却。
        bool Execute(string key, List<UnitBase> pool, out List<UnitBase> res);
    }

    // IFilterComponent:
    // 任意のユニットリストに対して、特定の条件でフィルタリングを行うコンポーネント用のインターフェース。
    // 実装により、攻撃力やHP、タグなどに基づく条件処理が可能。
    // 命名規則：FilterBy○○
    public interface IFilterComponent
    {
        List<UnitBase> Execute(List<UnitBase> pool);
    }
}
// unicode