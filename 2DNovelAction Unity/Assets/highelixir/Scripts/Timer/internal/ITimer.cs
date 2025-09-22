using System;

namespace HighElixir.Timer.Internal
{
    internal interface ITimer
    {
        /// <summary>
        /// Reset時に戻る時間
        /// </summary>
        float InitialTime { get; set; }
        float Current { get; set; }
        bool IsRunning { get; }
        CountType CountType { get; }
        float NormalizedElapsed { get; }

        event Action OnFinished;
        void Start();
        void Stop();
        void Reset();
        void Update(float dt);
    }
}