using UnityEngine;

namespace BlackRose.Core.Models.Units.Helpers
{
    public class SignalReciever : MonoBehaviour
    {
        [SerializeField] private TrailRenderer _target;
        public void TurnOff()
        {
            _target.emitting = false;
        }
    }
}