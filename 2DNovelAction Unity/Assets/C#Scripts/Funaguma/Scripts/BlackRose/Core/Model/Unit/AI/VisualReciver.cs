using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class VisualReciever : MonoBehaviour
    {
        [SerializeField] private VisualInfo _normalShoot;

        public void PlayNormalShootSE()
        {
            //AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>(_normalShoot.SEName), transform.position, _normalShoot.Volume);
        }
    }

    [Serializable]
    public struct VisualInfo
    {
        public string SEName;
        public float Volume;
    }
}