using UnityEngine;
using System;

namespace BlackRose
{
    [Serializable]
    public class ShootStateBase : StateWithAnime
    {
        // 弾丸にセットするレイヤー
        [SerializeField] protected LayerMask _targetLayer;
        [SerializeField] protected BulletData _data;         // 発射する弾のデータ
        [SerializeField] protected string _animeTrigger;
        [SerializeField] public event Action onShootComplete;

        // === Public ===
        public void SetBullet(BulletData bullet)
        {
            _data = bullet;
        }

        // === Constractor ===
        public ShootStateBase(BulletData data, LayerMask targetLayer, string animeTrigger) 
        {
            _data = data;
            _targetLayer = targetLayer; // レイヤーをセット
            _animeTrigger = animeTrigger;
        }

        public ShootStateBase() { }
    }
}