using UnityEngine;

namespace BlackRose
{
    [CreateAssetMenu(menuName = "BlackRose/Effect")]
    public class EffectData : ScriptableObject
    {
        [SerializeField] public string Name { get; private set; }
        [SerializeField] public string Description { get; private set; }
        [SerializeField] public Sprite Icon { get; private set; }
        [SerializeField] public float RemainingDuration { get; private set; }
    }
}