using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class ParticleHelper : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _normal;
        [SerializeField] private ParticleSystem _heavy;
        [SerializeField] private ParticleSystem _light;
        [SerializeField] private float _effectDuration = 1f;
        public void PlayAndSetPos(Vector3 position, AIController.AIStates aIStates)
        {
            var effect = aIStates switch
            {
                AIController.AIStates.Normal => _normal,
                AIController.AIStates.Heavy => _heavy,
                AIController.AIStates.Light => _light,
                _ => null,
            };
            effect.transform.position = position;
            ReleaseAfterPlay(effect).Forget();
        }
        private async UniTask ReleaseAfterPlay(ParticleSystem effect)
        {
            effect.gameObject.SetActive(true);
            effect.Play();
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(_effectDuration));
            effect.gameObject.SetActive(false);
        }
    }
}