using UnityEngine;


namespace BlackRose
{
    public partial class Enemy_Girder : UnitBase
    {
        [SerializeField] private float _AttackInterval = 2f;
        [SerializeField] private float _AfterTransitionInterval = 1f;
    }
}