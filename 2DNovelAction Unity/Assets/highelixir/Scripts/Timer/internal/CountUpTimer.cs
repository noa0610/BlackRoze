using System;

namespace HighElixir.Timer.Internal
{
    internal sealed class CountUpTimer : ITimer
    {
        public float InitialTime { get; set; }
        public float Current { get; set; }
        public CountType CountType { get; private set; }
        public bool IsRunning { get; private set; }
        public float NormalizedElapsed => 0f;
        public event Action OnFinished; // null 許容


        public CountUpTimer(CountType type)
        {
            InitialTime = 0f;
            CountType = type;
        }

        public void Reset()
        {
            IsRunning = false;
            Current = InitialTime;
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Update(float dt)
        {
            if (CountType == CountType.Tick) dt = 1f;
            if (InitialTime <= 0f) return;
            if (dt <= 0f) return; // 負やゼロを無視

            var next = Current + dt;
            if (next > 0f)
            {
                Current = next;
                return;
            }
            Current = 0f;
        }
    }
}