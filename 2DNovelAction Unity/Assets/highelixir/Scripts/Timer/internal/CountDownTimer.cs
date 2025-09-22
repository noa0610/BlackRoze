using System;

namespace HighElixir.Timer.Internal
{
    internal sealed class CountDownTimer : ITimer
    {
        public float InitialTime { get; set; }
        public float Current { get; set; }
        public CountType CountType { get; private set; }
        public bool IsRunning { get; private set; }
        public float NormalizedElapsed => Current <= 0f ? 1f : 1f - Math.Clamp(Current / InitialTime, 0f, 1f);
        public event Action OnFinished; // null 許容


        public CountDownTimer(float duration, CountType type, Action onFinished)
        {
            if (duration <= 0f) duration = 1;
            InitialTime = duration;
            CountType = type;
            if (onFinished != null) OnFinished += onFinished;
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

            var next = Current - dt;
            if (next > 0f)
            {
                Current = next;
                return;
            }

            // ちょうど/下回った → 0 に丸め、完了を 1 回だけ通知
            Current = 0f;
            OnFinished?.Invoke();
        }
    }
}