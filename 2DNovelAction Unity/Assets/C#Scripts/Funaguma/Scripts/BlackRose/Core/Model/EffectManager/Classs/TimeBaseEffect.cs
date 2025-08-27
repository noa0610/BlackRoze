using System.Collections.Generic;

namespace BlackRose
{
    public class TimeBaseEffect : StatusEffectBase
    {
        private IReadOnlyList<Effect> _effects;
        public override IEnumerator<Effect> GetAvaibleEffect()
        {
            foreach (Effect effect in _effects)
            {
                yield return effect;
            }
        }

        public override void Converter(StatusEffectData data)
        {
            Name = data.Name;
            Description = data.Description;
            RemainingDuration = data.RemainingDuration;
            _effects = data.Effects;
        }
    }
}