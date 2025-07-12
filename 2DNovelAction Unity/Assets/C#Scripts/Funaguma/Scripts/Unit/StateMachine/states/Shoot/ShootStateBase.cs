using UnityEngine;
using System;
using UnityEngine.Events;

namespace BlackRose
{
    [Serializable]
    public class ShootStateBase : StateWithAnime
    {
        // 弾丸にセットするレイヤー
        [SerializeField] protected LayerMask _targetLayer;
        [SerializeField] protected BulletData _data;         // 発射する弾のデータ
        [SerializeField] protected string _animeTrigger;
        [SerializeField] public UnityEvent onShootComplete;


        // === Constractor ===
        public ShootStateBase(BulletData data, LayerMask targetLayer, string animeTrigger) 
        {
            _data = data;
            _targetLayer = targetLayer; // レイヤーをセット
            _animeTrigger = animeTrigger;
        }

        public ShootStateBase() { }
        // === Public ===
        public void SetBullet(BulletData bullet)
        {
            _data = bullet;
        }


        /// <param name="shootInterval">射撃間の待機tick</param>
        public ShootStateBase SetShootInterval(int shootInterval)
        {
            return this;
        }
    }
}