using TMPro;
using UnityEngine;

namespace Minigames.UI
{
    public interface IPopTextData
    {
        public TMP_FontAsset FontAsset { get; }

        /// <summary>
        /// 範囲外のidxの場合はデフォルト値を返す.
        /// </summary>
        public (string text, Color color) GetInfo(int id);
    }
}