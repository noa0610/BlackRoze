using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class Warp : StateComp
    {
        [SerializeField]
        private Vector2 _pos;

        public event Action warped;
        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            parent.transform.position = _pos;
            warped?.Invoke();
        }

        public void SetPos(Vector2 pos)
        {
            _pos = pos;
        }


    }
}