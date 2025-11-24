using Cysharp.Threading.Tasks;
using HighElixir.Unity.Pools;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class ParticleHelper : MonoBehaviour
    {
        [Header("Effect")]
        [SerializeField] private ParticleSystem _chargeEffect; // チャージエフェクト
        [SerializeField] private Transform _container;
        [SerializeField] private ObjectPool<ParticleSystem> _chargeEffectPool;

        public void PlayAndSetPos(Vector3 position)
        {
            var effect = _chargeEffectPool.Get();
            effect.transform.position = position;
            effect.Play();
            ReleaseAfterPlay(effect).Forget();
        }
        private async UniTask ReleaseAfterPlay(ParticleSystem effect)
        {
            await System.Threading.Tasks.Task.Delay((int)(effect.main.duration * 1000));
            _chargeEffectPool.Release(effect);
        }
        private void Awake()
        {
            _chargeEffectPool = new ObjectPool<ParticleSystem>(_chargeEffect,5, _container);
        }
    }
}