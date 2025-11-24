using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class VisualReciever : MonoBehaviour
    {
        [SerializeField] protected UnitBase _target;
        [SerializeField] private VisualInfo _normalShoot;

        public virtual void PlayNormalShootSE()
        {
            //AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>(_normalShoot.SEName), transform.position, _normalShoot.Volume);
        }

        public void Play(VisualInfo info)
        {
            _target.PlaySE(info.SEName, info.Volume);
        }
    }

    [Serializable]
    public struct VisualInfo
    {
        public string SEName;
        public float Volume;
    }
}