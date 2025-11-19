using System;
using System.Collections.Generic;
using UnityEngine;
namespace BlackRose
{
    public static class SpriteEffectHolders
    {
        public enum SpriteEffects
        {
            Blinking = 1,
        }
        private static Dictionary<SpriteEffects, Action<GameObject, Value>> _dict = new()
        {
            {SpriteEffects.Blinking, Blinking }
        };



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
        public static Action<GameObject, Value> GetEffect(SpriteEffects effect)
        {
            return _dict[effect];
        }

        private static void Blinking(GameObject root, Value data)
        {
            var t = data.time;
            var sprites = root.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
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
}