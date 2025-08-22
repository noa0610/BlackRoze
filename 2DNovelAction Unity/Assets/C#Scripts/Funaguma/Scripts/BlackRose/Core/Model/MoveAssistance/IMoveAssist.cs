using System;
using UnityEngine;

namespace BlackRose
{
    public interface IMoveAssist
    {
        Action ArrivedCallback { get; set; }
        void Go(float time);
        void SetTarget(Transform target);
        void SetTarget(Vector2 target);
    }
}