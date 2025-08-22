using System;

namespace BlackRose.Core.Models
{
    public class TemporaryDifference : IDisposable
    {
        private StatusManager _manager;
        private Status _status;
        private float _delta, _ratio;
        private TemporaryDifference() { }
        internal static TemporaryDifference Create(StatusManager target, Status status, float delta = 0, float ratio = 0)
        {
            var res = new TemporaryDifference();
            res.SetTarget(target);
            res.Add(status, delta, ratio);
            return res;
        }
        public void Dispose()
        {
            Add(_status, -_delta, -_ratio);
        }

        public void SetTarget(StatusManager manager) { _manager = manager; }
        public void Add(Status status, float delta = 0, float ratio = 0)
        {
                _status = status;
            if (_manager.TryGetStatus(status, out var info))
            {
                info.TemporaryChanged += delta;
                info.TemporaryRatio += ratio;
                _ratio = ratio;
                _delta = delta;
            }
        }
    }
}