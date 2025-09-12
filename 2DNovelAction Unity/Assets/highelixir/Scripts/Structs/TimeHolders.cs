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
        public enum CountType
        {
            Time, // 時間でカウント
            Tick, // 更新回数でカウント
        }
        // 内部タイマー。
        [Serializable]
        private sealed class Timer
        {
            public bool IsEnable { get; private set; }
            public float Max { get; private set; }
            public float Remaining { get; private set; }
            public CountType Type { get; }
            public event Action Finished; // null 許容
            public bool Running => Remaining > 0f;

            // オプション Maxが無視される
            public bool IsCountUp { get; set; }

            public Timer(float duration, CountType type, bool start, Action onFinished, bool isCountup = false)
            {
                if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration));
                Max = IsCountUp ? duration : float.MaxValue;
                Type = type;
                IsCountUp = isCountup;
                if (onFinished != null) Finished += onFinished;
                Reset();
                if (start)
                    Start();
            }

            public void Reset()
            {
                IsEnable = false;
                Remaining = IsCountUp ? 0f : Max;
            }

            public void Start()
            {
                IsEnable = true;
            }

            public void Stop()
            {
                IsEnable = false;
            }

            public void Tick(float dt)
            {
                if (Type == CountType.Tick) dt = 1f;
                if (Remaining <= 0f) return;
                if (dt <= 0f) return; // 負やゼロを無視

                var next = Remaining + (IsCountUp ? dt : -dt);
                if (next > 0f)
                {
                    Remaining = next;
                    return;
                }

                // ちょうど/下回った → 0 に丸め、完了を 1 回だけ通知
                Remaining = 0f;
                if (!IsCountUp)
                    Finished?.Invoke();
            }

            public void UpdateMaxTime(float newMax)
            {
                if (newMax < 0f) throw new ArgumentOutOfRangeException(nameof(newMax));
                Max = newMax;
                if (Remaining > Max) Remaining = Max;
            }
            public void UpdataRemaining(float newRemaining)
            {
                if (newRemaining < 0f) throw new ArgumentOutOfRangeException(nameof(newRemaining));
                if (newRemaining > Max) newRemaining = Max;
                Remaining = newRemaining;
            }
            public float NormalizedElapsed => !IsCountUp ? (Max <= 0f ? 1f : 1f - Math.Clamp(Remaining / Max, 0f, 1f)) : 1f;
        }

        // エディタ監視用のスナップショット。
        public readonly struct TimerSnapshot
        {
            public readonly string Id;
            public readonly float Max;
            public readonly float Remaining;
            public readonly float NormalizedElapsed;
            public readonly bool IsCountUp;
            public readonly bool Running;

            public TimerSnapshot(string id, float max, float remaining, float normalized, bool running, bool isCountup)
            {
                Id = id; Max = max; Remaining = remaining; NormalizedElapsed = normalized; Running = running; IsCountUp = isCountup;
            }
        }
        private readonly Dictionary<string, Timer> _timers = new(StringComparer.Ordinal);

        /// <summary>
        /// 新規登録。既に同じ id があれば false。
        /// </summary>
        public bool Register(string id, float duration, CountType countType = CountType.Time, bool start = false, Action onFinished = null, bool isCountup = false)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty", nameof(id));
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration));

            var timer = new Timer(duration, countType, start, onFinished, isCountup);
            return _timers.TryAdd(id, timer);
        }

        public bool Contains(string id) => _timers.ContainsKey(id);

        public void ChangeDuration(string id, float newDuration)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty", nameof(id));
            if (newDuration < 0f) throw new ArgumentOutOfRangeException(nameof(newDuration));
            if (_timers.TryGetValue(id, out var t))
            {
                t.UpdateMaxTime(newDuration);
                if (t.Remaining > newDuration)
                    t.UpdataRemaining(newDuration);
            }
        }
        /// <summary>
        /// 登録解除。存在しなければ false。
        /// </summary>
        public bool Unregister(string id) => _timers.Remove(id);

        /// <summary>
        /// Max にリセット。IsCountupがtrueの場合、0fに
        /// </summary>
        public bool Reset(string id)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                t.Reset();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 進行開始。
        /// </summary>
        public bool Start(string id, bool reset = true)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                if (reset) t.Reset();
                t.Start();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 停止。
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
            return _timers.TryGetValue(id, out var t) && !t.IsCountUp && t.Remaining <= 0f;
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
                        t.Running,
                        t.IsCountUp
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
