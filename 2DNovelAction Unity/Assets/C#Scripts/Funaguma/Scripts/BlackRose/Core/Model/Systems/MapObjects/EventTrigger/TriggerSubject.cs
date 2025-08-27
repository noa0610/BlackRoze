using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    /// <summary>
    /// EnterとExitのタイミングで通知が送られる
    /// </summary>
    public class TriggerSubject : MonoBehaviour
    {
        // true => Enter, false => Exit
        public Action<bool, Collider2D> OnTrigger { get; set; }

        private void OnTriggerEnter2D(Collider2D collider2D)
        {
            OnTrigger?.Invoke(true, collider2D);
        }


        private void OnTriggerExit2D(Collider2D collider2D)
        {
            OnTrigger?.Invoke(false, collider2D);
        }
    }
}