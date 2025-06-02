using UnityEngine;
using System;

namespace BlackRose
{
    public abstract class ShootStateBase : IState
    {
        // 弾丸にセットするレイヤー
        protected LayerMask _targetLayer;
        protected BulletObject _bulletObject;         // 発射する弾のデータ
        public event Action onShootComplete;
        public abstract bool Enter(IState previousState, IUnit parent);
        public abstract bool Exit(IState nextState, IUnit parent);
        public abstract bool Stay(IUnit parent);
        public void ActionInvoke()
        {
            onShootComplete?.Invoke(); // 発射完了イベントを呼び出す
        }
        public void SetBullet(BulletObject bullet)
        {
            _bulletObject = bullet;
        }
        public ShootStateBase(BulletObject bulletObject, LayerMask targetLayer)
        {
            _bulletObject = bulletObject;
            _targetLayer = targetLayer; // レイヤーをセット
        }
    }
}