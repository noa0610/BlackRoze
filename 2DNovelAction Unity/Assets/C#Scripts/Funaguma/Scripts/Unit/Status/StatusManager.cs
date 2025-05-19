using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    public class StatusManager : IStatusManager
    {
        private Dictionary<Status, IStatusManager.StatusAmount> _statusAmounts = new();
        public Dictionary<Status, IStatusManager.StatusAmount> StatusPair => _statusAmounts;
        public Action DeadCallBack { get; set; }
        public void AddChanged(Status status, float amount)
        {
            if (_statusAmounts.ContainsKey(status))
            {
                _statusAmounts[status].temporaryChanged += amount;
            }
        }

        public void AddRatio(Status status, float ratio)
        {
            if (_statusAmounts.ContainsKey(status))
            {
                _statusAmounts[status].temporaryRatio += ratio;
            }
        }

        public void AddStatus(Status status, float amount)
        {
            if (_statusAmounts.ContainsKey(status))
                _statusAmounts[status].currentAmount = amount;
            else
                _statusAmounts.TryAdd(status, new IStatusManager.StatusAmount(amount));
        }

        public IStatusManager.StatusAmount GetStatusAmount(Status status)
        {
            return _statusAmounts[status];
        }

        public void RemoveStatus(Status status)
        {
            if (status == Status.HP)return;
            if (_statusAmounts.ContainsKey(status))
                _statusAmounts.Remove(status, out var _);
        }

        public void ResetChanged(Status status)
        {
            if (_statusAmounts.ContainsKey(status))
                _statusAmounts[status].temporaryChanged = 0;
        }

        public void ResetRatio(Status status)
        {

            if (_statusAmounts.ContainsKey(status))
                _statusAmounts[status].temporaryRatio = 1f;
        }

        public void TakeDamage(float damage)
        {
            if (_statusAmounts.ContainsKey(Status.HP))
            {
                damage *= _statusAmounts.ContainsKey(Status.DamageRatio) ? _statusAmounts[Status.DamageRatio].CurrentMax : 1f;
                _statusAmounts[Status.HP].currentAmount = Math.Max(0, _statusAmounts[Status.HP].currentAmount - damage);
                if (_statusAmounts[Status.HP].currentAmount <= 0)
                    DeadCallBack?.Invoke();
            }
        }

        public void UpdateStatus(Status status, float amount)
        {
            if (_statusAmounts.ContainsKey(status))
                _statusAmounts[status].currentAmount = amount;
            else
                Debug.LogError(status.ToString() + "が存在しないよ！");
        }
    }
}
// unicode