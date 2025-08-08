using System;

namespace BlackRose.Core.Models.Tickets
{
    public interface ITicket : IDisposable
    {
        event Action OnExpired;
        void Punch();
    }
}