using System;
using UnityEngine;
namespace BlackRose
{
    public static class SpriteEffectHolders
    {
        public enum SpriteEffects
        {
            None = 0,
            Blinking = 1,
        }

        public struct Value // 以下の値はAction内で書き換えられることはない
        {
            public float duration;
            public float remainingDuration;
            public float speed; // 効果の速さ
            public float deltaTime; // Time.deltaTime
            public float time;  // 経過時間(Time.time)
            public bool mustRemove; // 取り除かれるかどうか
        }
        // 任意の値を渡すとActionが返される。
        // SpriteRenderer => ターゲット
        public static Action<SpriteRenderer, Value> GetEffect(SpriteEffects effect)
        {
            return effect switch
            {
                SpriteEffects.Blinking => Blinking,
                _ => null,
            };
        }

        private static void Blinking(SpriteRenderer sprite, Value data)
        {
            var t = data.time;
            Color c = sprite.color;
            float a;
            if (!data.mustRemove)
                a = Mathf.Clamp01(Mathf.Sin(t * (2 * Mathf.PI / Mathf.Max(data.remainingDuration / data.duration, 0.5f)) * data.speed));
            else
                a = 1f;
                c.a = a;
            sprite.color = c;

        }
    }
}