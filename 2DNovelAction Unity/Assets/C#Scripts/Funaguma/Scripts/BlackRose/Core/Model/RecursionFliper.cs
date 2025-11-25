using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// 再帰的に子オブジェクトの向きを反転させるヘルパークラス
    /// </summary>
    public class RecursionFliper : MonoBehaviour
    {
        /// <summary>
        /// 指定したParticleSystemをルートとして、その子オブジェクトを幅優先探索でたどり
        /// ParticleSystemをもつTransformのXスケールを反転させます。
        /// </summary>
        /// <param name="sys">反転の開始点となるParticleSystem（nullなら何もしない）</param>
        /// <param name="flip">反転させるかどうか（true = 反転）</param>
        /// <param name="selfFlip">ルートTransform自体も反転するかどうか</param>
        public void SetFlipRecursively(ParticleSystem sys, bool flip, bool selfFlip = true)
        {
            if (sys == null) return;

            var root = sys.transform;

            var queue = new Queue<Transform>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var t = queue.Dequeue();

                // ルート自身は selfFlip フラグに従う
                if (t == root)
                {
                    if (selfFlip)
                        FlipXScale(t, flip);
                }
                else
                {
                    // ParticleSystem を持つ Transform を見つけたら反転
                    if (t.GetComponent<ParticleSystem>() != null)
                    {
                        FlipXScale(t, flip);
                    }
                }

                foreach (Transform child in t)
                {
                    queue.Enqueue(child);
                }
            }
        }

        private void FlipXScale(Transform t, bool flip)
        {
            var s = t.localScale;
            s.x = Mathf.Abs(s.x) * (flip ? -1f : 1f);
            t.localScale = s;
        }
    }
}