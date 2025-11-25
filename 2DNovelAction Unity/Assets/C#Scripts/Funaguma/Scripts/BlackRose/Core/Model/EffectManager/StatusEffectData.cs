using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    [CreateAssetMenu(fileName = "EffectData", menuName = "BlackRose/Effect")]
    public class StatusEffectData : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private float _remainingDuration;
        [SerializeField] private List<Effect> _effects;
        public string Name => _name;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float RemainingDuration => _remainingDuration;
        public IReadOnlyList<Effect> Effects => _effects.AsReadOnly();

        public virtual StatusEffectBase EffectFactory()
        {
            var effect = new TimeBaseEffect();
            effect.Converter(this);
            return effect;
        }
    }
}