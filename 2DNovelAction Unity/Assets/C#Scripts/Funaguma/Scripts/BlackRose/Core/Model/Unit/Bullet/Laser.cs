using HighElixir.Timers;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public sealed class Laser : Bullet
    {
        private Dictionary<GameObject, TimerTicket> _disableHitCheck = new();

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            // ヒット検出タイミングを上書き
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            //Debug.Log("Hitted");
            HitCheck(collision);
        }

        protected override void Hitted_Target(Collider2D collision)
        {// Hitted_Target
            if (!_disableHitCheck.ContainsKey(collision.gameObject))
            {
                _disableHitCheck[collision.gameObject] = GlobalTimer.FixedUpdate.CountDownRegister(0.5f, $"HC_[{collision.gameObject.name}]", initZero: true);
            }
            if (GlobalTimer.FixedUpdate.IsFinished(_disableHitCheck[collision.gameObject]))
            {
                GlobalTimer.FixedUpdate.Start(_disableHitCheck[collision.gameObject]);
                base.Hitted_Target(collision);
            }
        }

        private void OnDisable()
        {
            if (_disableHitCheck == null) return;
            foreach (var timer in _disableHitCheck.Values)
                GlobalTimer.FixedUpdate.UnRegister(timer);
            _disableHitCheck.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDisable();
        }
    }
}