using System;
using System.Collections.Generic;
using System.Linq;

namespace HighElixir
{
    /// <summary>
    /// ID 付きクールダウン/タイマー管理。
    /// </summary>
    [Serializable]
    public sealed class TimeHolders
    {
        // 内部タイマー。
        [Serializable]
        private sealed class Timer
        {
            public float Max { get; }
            public float Remaining { get; private set; }
            public event Action Finished; // null 許容
            public bool Running => Remaining > 0f;

            public Timer(float duration, bool start, Action onFinished)
            {
                if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration));
                Max = duration;
                Remaining = start ? duration : 0f;
                if (onFinished != null) Finished += onFinished;
            }

            public void Reset(bool start = true) => Remaining = start ? Max : 0f;

            public void Start() => Remaining = Max;

            public void Stop() => Remaining = 0f;

            public void Tick(float dt)
            {
                if (Remaining <= 0f) return;
                if (dt <= 0f) return; // 負やゼロを無視

                var next = Remaining - dt;
                if (next > 0f)
                {
                    Remaining = next;
                    return;
                }

                // ちょうど/下回った → 0 に丸め、完了を 1 回だけ通知
                Remaining = 0f;
                Finished?.Invoke();
            }

            public float NormalizedElapsed => Max <= 0f ? 1f : 1f - Math.Clamp(Remaining / Max, 0f, 1f);
        }

        // エディタ監視用のスナップショット。
        public readonly struct TimerSnapshot
        {
            public readonly string Id;
            public readonly float Max;
            public readonly float Remaining;
            public readonly float NormalizedElapsed;
            public readonly bool Running;

            public TimerSnapshot(string id, float max, float remaining, float normalized, bool running)
            {
                Id = id; Max = max; Remaining = remaining; NormalizedElapsed = normalized; Running = running;
            }
        }
        private readonly Dictionary<string, Timer> _timers = new(StringComparer.Ordinal);

        /// <summary>
        /// 新規登録。既に同じ id があれば false。
        /// </summary>
        public bool Register(string id, float duration, bool start = false, Action onFinished = null)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty", nameof(id));
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration));

            var timer = new Timer(duration, start, onFinished);
            return _timers.TryAdd(id, timer);
        }

        /// <summary>
        /// 登録解除。存在しなければ false。
        /// </summary>
        public bool Unregister(string id) => _timers.Remove(id);

        /// <summary>
        /// Max にリセット。start=false で 0 に戻す。
        /// </summary>
        public bool Reset(string id, bool start = true)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                t.Reset(start);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 進行開始（Max にセット）。
        /// </summary>
        public bool Start(string id)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                t.Start();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 停止（残りを 0）。
        /// </summary>
        public bool Stop(string id)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                t.Stop();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 終了済みか（登録が無ければ false）。
        /// </summary>
        public bool IsFinished(string id)
        {
            return _timers.TryGetValue(id, out var t) && t.Remaining <= 0f;
        }

        /// <summary>
        /// 残り時間を取得。
        /// </summary>
        public bool TryGetRemaining(string id, out float remaining)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                remaining = t.Remaining;
                return true;
            }
            remaining = 0f;
            return false;
        }

        /// <summary>
        /// 経過正規化 [0..1] を取得（未登録は 1 として返す）。
        /// </summary>
        public float GetNormalizedElapsed(string id)
        {
            return _timers.TryGetValue(id, out var t) ? t.NormalizedElapsed : 1f;
        }

        /// <summary>
        /// 完了時の Action を追加。存在しない場合 false。
        /// </summary>
        public bool AddAction(string id, Action action)
        {
            if (action == null) return false;
            if (_timers.TryGetValue(id, out var t))
            {
                t.Finished += action;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 完了時の Action を削除。存在しない場合 false。
        /// </summary>
        public bool RemoveAction(string id, Action action)
        {
            if (action == null) return false;
            if (_timers.TryGetValue(id, out var t))
            {
                t.Finished -= action;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 毎フレーム等で呼ぶ。内部で Keys のスナップショットを取るので、
        /// コールバック内で Unregister しても安全。
        /// </summary>
        public void Update(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            // 変更に強いようにキーのスナップショットで回す
            var ids = _timers.Keys.ToList();
            foreach (var id in ids)
            {
                if (_timers.TryGetValue(id, out var t))
                {
                    t.Tick(deltaTime);
                }
            }
        }

        public IEnumerable<TimerSnapshot> GetSnapshot()
        {
            // キーのスナップショットで安全に列挙
            var ids = _timers.Keys.ToList();
            foreach (var id in ids)
            {
                if (_timers.TryGetValue(id, out var t))
                {
                    yield return new TimerSnapshot(
                        id,
                        t.Max,
                        t.Remaining,
                        t.NormalizedElapsed,
                        t.Running
                    );
                }
            }
        }
    }
    public interface ITimerUser
    {
        TimeHolders Timers { get; }
    }
}
