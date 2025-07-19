using UnityEngine;


namespace BlackRose
{
    public partial class Enemy_Hopper : UnitBase
    {
        [SerializeField] private float _IdleInterval; // 弾データ
        [SerializeField] private float _ApproachInterval = 1.0f; // 接近時間
        [SerializeField] private float _AttackInterval = 1.0f; // 攻撃時間
    }
}