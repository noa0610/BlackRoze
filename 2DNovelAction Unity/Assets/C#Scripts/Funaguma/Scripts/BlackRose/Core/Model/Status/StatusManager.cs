using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models
{
    public class StatusManager
    {
        private readonly Dictionary<Status, StatusInfo> _statusAmounts = new();

        public StatusManager Initialize(UnitStatusData data)
        {
            _statusAmounts.Clear();

            // 基礎ステータスを一括登録
            var hp = AddStatus(Status.HP, data.maxHp, false, true);
            var mHp = AddStatus(Status.MaxHP, data.maxHp, false, true);
            mHp.OnAmountChanged += (_, after) =>
            {
                hp.SetMax(mHp.CurrentAmount);
            };
            AddStatus(Status.DamageRatio, data.damageTakeScale, isHideIfDefault: true);
            AddStatus(Status.JumpPower, data.jumpPower);
            AddStatus(Status.Speed, data.speed);
            AddStatus(Status.SpeedInAir, data.speedInAir);
            AddStatus(Status.Power, data.power, isHideIfDefault: true);
            AddStatus(Status.DashSpeed, data.dashSpeed);
            AddStatus(Status.DamageRatio, 1f, isHideIfDefault: true);
            return this;
        }

        public StatusInfo AddStatus(Status status, float amount, bool isDynamic = true, bool isHideIfDefault = false)
        {
            var info = new StatusInfo(amount, isDynamic);
            info.SetMin(0);
            _statusAmounts[status] = info;
            return info;
        }
        public bool RemoveStatus(Status status)
        {
            return _statusAmounts.Remove(status);
        }
        public IEnumerable<StatusInfo> GetAllInfos()
        {
            foreach (var m in _statusAmounts.Values)
                yield return m;
        }

        public bool IsRegistered(Status status) => _statusAmounts.ContainsKey(status);

        public bool TryGetStatus(Status status, out StatusInfo model)
        {
            return _statusAmounts.TryGetValue(status, out model);
        }
        public bool TryGetCurrentValue(Status status, out float current)
        {
            current = float.MinValue;
            if (_statusAmounts.TryGetValue(status, out var model))
            {
                current = model.CurrentAmount;
                return true;
            }
            return false;
        }
        public StatusInfo GetStatus(Status status)
        {
            return _statusAmounts[status];
        }
        // 注：エラーを返さない
        public float ReadValue(Status status)
        {
            if (TryGetStatus(status, out var info))
            {
                return info.CurrentAmount;
            }
            return float.NaN;
        }
        public bool TryReadValue(Status status, out float value)
        {
            value = ReadValue(status);
            return !float.IsNaN(value);
        }
        public void TakeHeal(float value)
        {
            _statusAmounts[Status.HP].CurrentAmount += value;
        }

        /// <returns>死亡したかどうか</returns>
        public bool TakeDamage(float value)
        {
            var s = _statusAmounts[Status.HP];
            s.CurrentAmount -= value;
            Debug.Log($"TakeDamage: {value}, HP: {s.CurrentAmount}/{_statusAmounts[Status.MaxHP].CurrentAmount}");
            return s.CurrentAmount <= 0;
        }
    }
}
