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
        [SerializeField] public UnityEvent onShootComplete;


        // === Constractor ===
        public ShootStateBase(BulletData data, LayerMask targetLayer) 
        {
            _data = data;
            _targetLayer = targetLayer; // レイヤーをセット
        }
        public ShootStateBase() { }
        // === Public ===
        public void SetBullet(BulletData bullet)
        {
            _data = bullet;
        }

        protected virtual void Shoot(UnitBase parent)
        {
            onShootComplete?.Invoke();
        }
    }
}