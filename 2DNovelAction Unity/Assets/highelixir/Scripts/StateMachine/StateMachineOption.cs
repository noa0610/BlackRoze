using HighElixir.Loggings;
using System;

namespace HighElixir.StateMachine
{
    [Flags]
    public enum RequiredLoggerLevel
    {
        None = 0,
        Info = 1 << 0,
        Warning = 1 << 1,
        Error = 1 << 2,
        Fatal = 1 << 3,
        INFOS = Info | Warning,
        ERRORS = Error | Fatal,
        ALL = INFOS | ERRORS,
    }
    public sealed class StateMachineOption<TCont, TEvt, TState>
    {
        // イベントキュー
        public QueueMode QueueMode { get; set; } = QueueMode.UntilFailures;
        public IEventQueue<TCont, TEvt, TState> Queue { get; set; }
        public TCont Cont { get; set; }
        // ステート上書きのルール
        // falseの場合、上書きしようとすると例外をスローする
        public bool EnableOverriding { get; set; } = false;

        // ロガー(null以外の場合、ログ出力が有効化される)
        public ILogger Logger { get; set; } = null;
        public RequiredLoggerLevel LogLevel { get; set; } = RequiredLoggerLevel.Error | RequiredLoggerLevel.Fatal;
        public StateMachineOption(TCont cont)
        {
            Cont = cont;
        }
    }
}