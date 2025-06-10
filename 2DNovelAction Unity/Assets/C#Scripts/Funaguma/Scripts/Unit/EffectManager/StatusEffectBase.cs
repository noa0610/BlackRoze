using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    public abstract class StatusEffectBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Sprite Icon { get; set; }
        public float RemainingDuration { get; set; }
        public virtual bool IsAvaible => RemainingDuration > 0;

        // 上から順に追加時、Tickごと、取り除かれるときに呼び出されるコールバック
        public Action<StatusEffectBase> OnAplly;
        public Action<StatusEffectBase> OnTick;
        public Action<StatusEffectBase> OnRemove;
        public virtual void OnApllyEffect() => OnAplly?.Invoke(this);
        public virtual void OnTickEffect() => OnTick?.Invoke(this);
        public virtual void OnRemoveEffect() => OnRemove?.Invoke(this);
        public abstract IEnumerator<Effect> GetAvaibleEffect();

        public abstract void Converter(StatusEffectData data);
    }

    [Serializable]
    public struct Effect
    {
        public Status target;
        public float fix;
        public float scale;
    }
}