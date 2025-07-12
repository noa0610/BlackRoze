using System;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public class ShootForward : ShootStateBase
    {
        [SerializeField] private float _createPos = 0.35f;
        [SerializeField] private int _fireCount = 3;
        [SerializeField] private float _allowShootCancel = 0.4f;
        private int _count = 0;
        public ShootForward(BulletData data, LayerMask targetLayer) : base(data, targetLayer) { }
        public ShootForward() { }
        public override void Enter(IState preview, UnitBase parent)
        {
            _count++;
            Shoot(parent);
        }

        public override void Exit(IState next, UnitBase parent)
        {
            if (!(next is ShootForward)) _count = 0;
        }
        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (nextState is Stun) return true;
            if (nextState is ShootForward && _count <= _fireCount && GetNormalized(parent) >= _allowShootCancel) return true;
            return base.AllowChange(nextState, parent);
        }

        protected override void Shoot(UnitBase unit)
        {
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }
            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = unit.Transform.position + new Vector3(unit.Direction.x * _createPos, 0, 0);
            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
            // ステータスをセット（速度、方向、ダメージなど）
            instantiatedBullet.SetBulletStatus(_data, _targetLayer);
            base.Shoot(unit);
        }

        public ShootForward SetMaxCount(int maxCount)
        {
            _fireCount = maxCount;
            return this;
        }
    }
}
//unicode