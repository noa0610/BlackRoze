using System.Collections;
using System.Collections.Generic;
using BlackRose.Core.Models.Units;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BlackRose.Core.Models.States
{
    public class ShootCross : ShootStateBase
    {
        [SerializeField] private float _distanceflomCenter = 0;
        public ShootCross(BulletData data, LayerMask targetLayer) : base(data, targetLayer) { }
        public ShootCross() : base() { }


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

        public ShootCross SetFiring(GameObject point, Vector2? direction = null, float distanceflomCenter = 0)
        {
            _muzzle = point;
            _direction = direction ?? new Vector2(0, 1);
            _distanceflomCenter = distanceflomCenter;
            return this;
        }

        public ShootCross SetFiring(GameObject point, float angle, float distanceflomCenter = 0)
        {
            _muzzle = point;
            var radian = angle * Mathf.Deg2Rad;
            _direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
            _distanceflomCenter = distanceflomCenter;
            return this;
        }

        private void CrossShot(UnitBase unit)
        {
            var b = _data.prefab;
            var center = _muzzle.transform.position;

            // 基準方向(引数方向)
            Vector2 dir = _direction;
            if (dir.sqrMagnitude <= 0.0001f) dir = Vector2.right;
            dir = dir.normalized;

            // 直交ベクトル（引数90度回転）
            var perp = new Vector2(-dir.y, dir.x);

            // 4方向ベクトル
            var shootDirs = new Vector2[] { dir, -dir, perp, -perp };

            foreach (var d in shootDirs)
            {
                Vector3 spawnPos = center + (Vector3)(d * _distanceflomCenter);
                var instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
                float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                instantiatedBullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                InitBullet(instantiatedBullet, d);
            }
        }


        protected override UniTask Shoot(UnitBase unit)
        {
            CrossShot(unit);
            return base.Shoot(unit);
        }

        public void DirectShoot(UnitBase unit)
        {
            CrossShot(unit);
        }
    }
}
