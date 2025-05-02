using UnityEngine;
namespace Test
{
    public class Enemy_MoveOnLine : UnitBase
    {
        [SerializeField] private float _sightDistance;
        [SerializeField] private LayerMask _layerMask;
        private SearchAssistance _assistance;
        protected override void RegisterStats()
        {
            _assistance = new SearchAssistance();
            _assistance.AddComp("sight", new FilterByLineOfSight(Transform, _sightDistance, _layerMask));
            _assistance.AddComp("tag", new FilterByTag(UnitTags.Player));
        }

        protected override string StateDecision()
        {
            throw new System.NotImplementedException();
        }
    }
}