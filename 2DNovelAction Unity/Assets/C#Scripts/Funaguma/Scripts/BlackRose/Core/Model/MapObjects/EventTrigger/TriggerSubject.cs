using System;
using UnityEngine;
using UnityEngine.Events;

namespace BlackRose
{
    /// <summary>
    /// EnterとExitのタイミングで通知が送られる
    /// </summary>
    public class TriggerSubject : MonoBehaviour
    {
        [SerializeField] private UnityEvent<bool, Collider2D> _notifyHit = new();
        [SerializeField] private LayerMask _targetLayer;
        // true => Enter, false => Exit
        public Action<bool, Collider2D> OnTrigger { get; set; }

        private void OnTriggerEnter2D(Collider2D collider2D)
        {
            if ((1 << _targetLayer) != 0)
            {
                OnTrigger?.Invoke(true, collider2D);
                _notifyHit.Invoke(true, collider2D);
            }
        }


        private void OnTriggerExit2D(Collider2D collider2D)
        {
            if ((1 << _targetLayer) != 0)
            {
                OnTrigger?.Invoke(false, collider2D);
                _notifyHit.Invoke(true, collider2D);
            }
        }
    }
}