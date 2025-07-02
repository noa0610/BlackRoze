using UnityEngine;
using System;

namespace BlackRose
{
    public abstract class ShootStateBase : IState
    {
        // 弾丸にセットするレイヤー
        protected LayerMask _targetLayer;
        protected BulletData _data;         // 発射する弾のデータ
        protected string _animeTrigger;
        public event Action onShootComplete;

        // === Public ===
        public abstract bool Enter(IState previousState, IUnit parent);
        public abstract bool Exit(IState nextState, IUnit parent);
        public abstract bool Stay(IUnit parent);
        public void ActionInvoke()
        {
            onShootComplete?.Invoke(); // 発射完了イベントを呼び出す
        }
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
    }
}