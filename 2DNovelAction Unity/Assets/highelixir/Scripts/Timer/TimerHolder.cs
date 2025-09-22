using HighElixir.Timer.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HighElixir.Timer
{
    /// <summary>
    /// ID 付きクールダウン/タイマー管理。
    /// </summary>
    [Serializable]
    public sealed class TimerHolder
    {
        private readonly Dictionary<string, ITimer> _timers = new(StringComparer.Ordinal);

        /// <summary>
        /// 新規登録。既に同じ id があれば false。
        /// </summary>
        public bool Register(string id, float duration, CountType countType = CountType.Time, Action onFinished = null)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty", nameof(id));
            if (duration < 0f) throw new ArgumentOutOfRangeException(nameof(duration));

            var timer = new CountDownTimer(duration, countType, onFinished);
            return _timers.TryAdd(id, timer);
        }

        // カウントアップ式のタイマー
        public bool Register(string id, CountType countType = CountType.Time)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty", nameof(id));

            var timer = new CountUpTimer(countType);
            return _timers.TryAdd(id, timer);
        }
        public bool Contains(string id) => _timers.ContainsKey(id);

        public void ChangeDuration(string id, float newDuration)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty", nameof(id));
            if (newDuration < 0f) throw new ArgumentOutOfRangeException(nameof(newDuration));
            if (_timers.TryGetValue(id, out var t))
            {
                if (t is CountUpTimer) return;

                t.InitialTime = newDuration;
                if (t.Current > newDuration)
                    t.Current = newDuration;
            }
        }
        /// <summary>
        /// 登録解除。存在しなければ false。
        /// </summary>
        public bool Unregister(string id) => _timers.Remove(id);

        /// <summary>
        /// 初期値へリセット。
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

        public bool Stop(string id, out float current)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                t.Stop();
                current = t.Current;
                return true;
            }
            current = 0f;
            return false;
        }

        public void StopAll()
        {
            foreach (var t in _timers.Values)
            {
                t.Stop();
            }
        }
        /// <summary>
        /// 終了済みか（登録が無ければ false）。
        /// </summary>
        public bool IsFinished(string id)
        {
            return _timers.TryGetValue(id, out var t) && t is CountDownTimer && t.Current <= 0f;
        }

        /// <summary>
        /// 現在の時間を取得。
        /// </summary>
        public bool TryGetRemaining(string id, out float remaining)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                remaining = t.Current;
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
                t.OnFinished += action;
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
                t.OnFinished -= action;
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
                    t.Update(deltaTime);
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
                        t.InitialTime,
                        t.Current,
                        t.NormalizedElapsed,
                        t.IsRunning,
                        t is CountDownTimer
                    );
                }
            }
        }
    }
}
