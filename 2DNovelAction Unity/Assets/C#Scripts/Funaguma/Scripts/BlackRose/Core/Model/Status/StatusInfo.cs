using System;
using System.Text;

namespace BlackRose.Core.Models
{
    public class StatusInfo
    {
        private float _defaultAmount;
        private float _currentAmount;

        private float _minAmount = float.MinValue;
        private float _maxAmount = float.MaxValue;

        private bool _enableDynamicParams; // _temporaryChanged,_temporaryRatioを使用して値を計算するかどうか
        private float _temporaryChanged;
        private float _temporaryRatio = 1f;
        private bool _dirty = true; // 値が変更されたかどうか

        // 変更前 => 変更後
        private Action<float, float> _onAmountChanged;

        public event Action<float, float> OnAmountChanged { add => _onAmountChanged += value; remove => _onAmountChanged -= value; }
        public bool EnableDynamicParams => _enableDynamicParams;
        public float CurrentAmount
        {
            get
            {
                if (_enableDynamicParams && _dirty)
                {
                    Recalculate();
                }
                return _currentAmount;
            }
            set
            {
                if (!_enableDynamicParams)
                {
                    var before = _currentAmount;
                    _currentAmount = Math.Clamp(value, _minAmount, _maxAmount);
                    if (before != _currentAmount)
                        _onAmountChanged?.Invoke(before, _currentAmount);
                }
                else
                    throw new Exception("EnableDynamicParamsがtrueなため値をセットできません");
            }
        }
        public float DefaultAmount => _defaultAmount;
        public float TemporaryChanged
        {
            get => _temporaryChanged;
            set
            {
                if (!_enableDynamicParams) return;
                if (_temporaryChanged == value) return;
                _temporaryChanged = value;
                _dirty = true; // 値が変更されたので、ChangedMaxを再計算する必要がある
            }
        }
        public float TemporaryRatio
        {
            get => _temporaryRatio;
            set
            {
                if (!_enableDynamicParams) return;
                if (value < 0f) value = 0f;
                if (_temporaryRatio == value) return;
                _temporaryRatio = value;
                _dirty = true; // 値が変更されたので、ChangedMaxを再計算する必要がある
            }
        }

        public StatusInfo(float defaultAmount, bool isDynamic = true)
        {
            _currentAmount = defaultAmount;
            _defaultAmount = defaultAmount;
            _enableDynamicParams = isDynamic;
            _dirty = _enableDynamicParams;
            Recalculate();
        }
        public float GetClamped(float max)
        {
            return Math.Clamp(_currentAmount, 0f, max);
        }
        public void ClearDynamics()
        {
            if (!_enableDynamicParams) return;
            _temporaryRatio = 1f;
            _temporaryChanged = 0f;
            _dirty = true;
        }
        public void Recalculate()
        {
            _dirty = false;
            var before = _currentAmount;
            float newVal = (_defaultAmount + _temporaryChanged) * Math.Max(0, _temporaryRatio); // デフォルト値に一時的な変更を加え、倍率を掛ける
            newVal = Math.Clamp(newVal, _minAmount, _maxAmount);
            _currentAmount = newVal;
            if (before != _currentAmount)
                _onAmountChanged?.Invoke(before, _currentAmount);
        }

        public void SetMin(float min)
        {
            _minAmount = min;
            if (_currentAmount < _minAmount)
            {
                var before = _currentAmount;
                _currentAmount = _minAmount;
                _onAmountChanged?.Invoke(before, _currentAmount);
            }
        }
        public void SetMax(float max)
        {
            _maxAmount = max;
            if (_currentAmount > _maxAmount)
            {
                var before = _currentAmount;
                _currentAmount = _maxAmount;
                _onAmountChanged?.Invoke(before, _currentAmount);
            }
        }
        public void SetDefault(float defaultAmount)
        {
            _defaultAmount = defaultAmount;
            _dirty = true; // デフォルト値が変更されたので、ChangedMaxを再計算する必要がある
        }
        public override string ToString()
        {
            var sb = new StringBuilder($"default:{_defaultAmount}, current:{CurrentAmount}");
            if (_enableDynamicParams)
            {
                sb.Append($",changed:{_temporaryChanged}, ratio:{_temporaryRatio}");
            }
            return sb.ToString();
        }
    }
}