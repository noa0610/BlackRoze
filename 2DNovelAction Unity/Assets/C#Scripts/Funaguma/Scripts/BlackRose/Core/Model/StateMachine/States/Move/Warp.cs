using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    public class Warp : StateComp
    {
        private Vector2 _pos;

        public event Action warped;
        public override void Enter(IState previousIState, UnitBase parent)
        {
            parent.transform.position = _pos;
            warped?.Invoke();
        }

        public void SetPos(Vector2 pos)
        {
            _pos = pos;
        }


    }
}