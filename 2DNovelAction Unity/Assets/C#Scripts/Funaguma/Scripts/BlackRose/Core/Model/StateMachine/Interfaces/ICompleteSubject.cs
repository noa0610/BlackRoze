using System;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// 動作が完了したことを通知する機能を持つステート
    /// </summary>
    public interface ICompleteEmitter
    {
        event Action OnCompleted;
    }
}