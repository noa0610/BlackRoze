using BlackRose.Core.Models.Units;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.EffectManagers
{
    public class StatusEffectManager
    {
        public UnitBase parent;
        public List<StatusEffectBase> effects = new List<StatusEffectBase>();

        public void Update() // UnitBaseから1Tickごとに呼ばれる
        {
            var t = Time.deltaTime;
            List<StatusEffectBase> pendingRemove = new List<StatusEffectBase>();
            foreach (var effect in effects)
            {
                effect.RemainingDuration -= t;
                effect.OnTickEffect();
                if (!effect.IsAvaible)
                    pendingRemove.Add(effect);
            }
            foreach (var effect in pendingRemove)
                RemoveEffect(effect);
        }

        public void AddEffect(StatusEffectBase effect)
        {
            effects.Add(effect);
            effect.OnApllyEffect();
            var s = parent.statusManager;
            var enumerator = effect.GetAvaibleEffect();
            while (enumerator.MoveNext())
            {
                var e = enumerator.Current;
                if (e.fix != 0) s.GetStatus(e.target).TemporaryChanged += e.fix;
                if (e.scale != 0) s.GetStatus(e.target).TemporaryRatio += e.scale;
            }
        }
        public void RemoveEffect(StatusEffectBase effect)
        {
            if (!effects.Contains(effect)) return;
            effect.OnRemoveEffect();
            var s = parent.statusManager;
            var enumerator = effect.GetAvaibleEffect();
            while (enumerator.MoveNext())
            {
                var e = enumerator.Current;
                if (e.fix != 0) s.GetStatus(e.target).TemporaryChanged -=e.fix;
                if (e.scale != 0) s.GetStatus(e.target).TemporaryRatio -= e.scale;
            }
            effects.Remove(effect);
        }
        public StatusEffectBase GetFirstEffect(string name)
        {
            foreach (var effect in effects)
            {
                if (effect.Name == name)
                    return effect;
            }
            return null;
        }
        public StatusEffectManager(UnitBase parent)
        {
            this.parent = parent;
        }
    }
}