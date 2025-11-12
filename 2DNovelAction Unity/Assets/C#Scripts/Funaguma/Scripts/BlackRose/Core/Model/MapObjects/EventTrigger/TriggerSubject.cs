using System;
using System.Collections.Generic;
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
        // true => Enter, false => Exit
        public Action<bool, Collider2D> OnTrigger { get; set; }

        private void OnTriggerEnter2D(Collider2D collider2D)
        {
            OnTrigger?.Invoke(true, collider2D);
            _notifyHit.Invoke(true, collider2D);
        }


        private void OnTriggerExit2D(Collider2D collider2D)
        {
            OnTrigger?.Invoke(false, collider2D);
            _notifyHit.Invoke(true, collider2D);
        }
    }
}