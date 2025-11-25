using BlackRose.Core.Models.Units;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class ShootMultiplePositions : ShootStateBase
    {
        [SerializeField] private GameObject[] _muzzles;

        public ShootMultiplePositions(BulletData data, LayerMask targetLayer) : base(data, targetLayer) { }
        public ShootMultiplePositions() : base() { }


        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            _ = Shoot(parent);
        }

        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (base.AllowChange(nextState, parent)) return true;
            return false;
        }


        public ShootMultiplePositions SetFiring(GameObject[] positions, Vector2 direction)
        {
            _muzzles = positions;
            _direction = direction;
            return this;
        }

        protected override UniTask Shoot(UnitBase unit)
        {
            var b = _data.prefab;
            foreach (GameObject muzzle in _muzzles)
            {
                // 弾の生成位置
                Vector3 spawnPos = muzzle.transform.position;
                // 弾を生成
                Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
                // ステータスをセット
                InitBullet(instantiatedBullet, _direction);
            }

            return base.Shoot(unit);
        }
    }
}
