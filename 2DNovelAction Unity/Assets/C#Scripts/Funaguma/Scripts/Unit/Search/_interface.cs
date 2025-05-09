// 概要:
// この名前空間（Test）では、ユニットの絞り込み・検索機構を構築するためのインターフェース群を定義しています。
// 柔軟なフィルター構成や、検索処理の拡張性を目的とした設計です。

using System.Collections.Generic;

namespace BlackRose
{
    // ISearch:
    // フィルターを追加・削除し、ユニットのリストに対して検索処理を実行するためのインターフェース。
    // 実装側は複数のフィルター（IFilterComponent）を管理し、段階的にユニットリストを処理します。
    public interface ISearch
    {
        // フィルターの追加。キーと優先度を指定可能（同キーなら上書き想定）。
        void AddComp(string key, IFilterComponent comp, int priority = 0);

        // フィルターの削除。指定キーのコンポーネントを除去。
        void RemoveComp(string id);

        // 検索処理を実行。フィルター順にユニットを処理し、結果を返却。
        List<IUnit> Execute(List<IUnit> pool);
    }

    // IFilterComponent:
    // 任意のユニットリストに対して、特定の条件でフィルタリングを行うコンポーネント用のインターフェース。
    // 実装により、攻撃力やHP、タグなどに基づく条件処理が可能。
    // 命名規則：FilterBy○○
    public interface IFilterComponent
    {
        List<IUnit> Execute(List<IUnit> pool);
    }
}
// unicode