using BlackRose.Core.Models.Units;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [SerializeField]
    public class CustomMove : StateWithAnime
    {
        [SerializeReference, SubclassSelector]
        private List<IMoveAssist> _assists = new List<IMoveAssist>();

        public void AddComp(IMoveAssist comp)
        {
            _assists.Add(comp);
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            float t = Time.deltaTime;
            foreach (var assist in _assists)
                assist.Go(Time.deltaTime);
        }
    }
}