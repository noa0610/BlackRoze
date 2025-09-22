// エディタ監視用のスナップショット。
using System;

namespace HighElixir.Timer.Internal
{
    // TODO : スナップショットから復元する機能の実装
    [Serializable]
    public readonly struct TimerSnapshot
    {
        public readonly string Id;
        public readonly float Max;
        public readonly float Remaining;
        public readonly float NormalizedElapsed;
        public readonly bool Running;
        public readonly bool IsCountUp;
        public TimerSnapshot(string id, float max, float remaining, float normalized, bool running, bool isCountUp)
        {
            Id = id; Max = max; Remaining = remaining; NormalizedElapsed = normalized; Running = running; IsCountUp = isCountUp;
        }
    }
}