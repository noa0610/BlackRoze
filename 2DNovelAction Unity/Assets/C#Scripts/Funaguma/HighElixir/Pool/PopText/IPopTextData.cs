using TMPro;
using UnityEngine;

namespace HighElixir.Pool
{
    public interface IPopTextData
    {
        TMP_FontAsset FontAsset { get; }

        /// <summary>
        /// 範囲外のidxの場合はデフォルト値を返す.
        /// </summary>
        (string text, Color color) GetInfo(int id);
    }
}