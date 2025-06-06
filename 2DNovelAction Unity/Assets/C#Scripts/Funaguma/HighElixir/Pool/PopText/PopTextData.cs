using TMPro;
using UnityEngine;

namespace HighElixir.Pool
{
    public abstract class PopTextData : ScriptableObject, IPopTextData
    {
        public virtual TMP_FontAsset FontAsset { get; }

        /// <summary>
        /// 範囲外のidxの場合はデフォルト値を返す.
        /// </summary>
        public abstract (string text, Color color) GetInfo(int id);
    }
}