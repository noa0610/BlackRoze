using BlackRose.Core.Models.Units;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class ShootForward : ShootStateBase
    {
        public ShootForward(BulletData data, LayerMask targetLayer) : base(data, targetLayer) { }
        public ShootForward() :base() { }

        protected override async UniTask Shoot(UnitBase unit)
        {
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }
            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = _muzzle.transform.position + new Vector3(unit.Direction.x * _createPos, 0, 0);
            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
            float angle = Mathf.Atan2(unit.Direction.y, unit.Direction.x) * Mathf.Rad2Deg;
            instantiatedBullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            InitBullet(instantiatedBullet, _direction);
            await base.Shoot(unit);
        }
    }
}
//unicode