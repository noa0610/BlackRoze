using UnityEngine;

namespace BlackRose.Core.Models.States
{
    public interface IRigidbodyUser
    {
        Rigidbody2D Rigidbody2D { get; }

        void SetRB2(Rigidbody2D rb);
    }
}