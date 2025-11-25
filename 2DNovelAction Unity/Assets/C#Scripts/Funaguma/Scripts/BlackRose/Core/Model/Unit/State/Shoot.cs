using BlackRose.Core.Models.Helper;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Units.State
{
    [Serializable]
    public class ShootForward<T> : ShootBase<T>
        where T : UnitBase
    {
        public ShootForward(BulletData data) : base(data) { }
        public ShootForward() : base() { }

        protected override async UniTask Shoot()
        {
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }
            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = Cont.Muzzle.transform.position + new Vector3(Cont.Direction.x * _createPos, 0, 0);
            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
            InitBullet(instantiatedBullet, Cont.ShootDir);
            await base.Shoot();
        }
    }
    [Serializable]
    public class ShootWithMove<T> : ShootBase<T>
        where T : UnitBase
    {
        [SerializeField] private float _duration = 0.5f;
        public ShootWithMove(BulletData data) : base(data) { }
        public ShootWithMove() : base() { }

        protected override async UniTask Shoot()
        {
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }
            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = Cont.Muzzle.transform.position + new Vector3(Cont.Direction.x * _createPos, 0, 0);
            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
            InitBullet(instantiatedBullet, Cont.ShootDir);
            var time = 0f;
            while (time < _duration)
            {
                var t = Time.deltaTime;
                if (Cont.MoveDirection.x != 0)
                {
                    MoveHelpers.CalcVerocity(Cont, Cont.Rigidbody2D, Cont.MoveDirection.x, Status.Speed, t);
                }
                time += t;
                await UniTask.DelayFrame(1);
            }
            await base.Shoot();
        }
    }

    [Serializable]
    public class MultiShoot<T> : ShootBase<T>
        where T : UnitBase
    {
        [Tooltip("同時シュートする際の最大角度")]
        [SerializeField, Min(0)] private float _range;
        [Tooltip("同時に発射する弾数")]
        [SerializeField, Min(1)] private int _shootCount;

        protected override async UniTask Shoot()
        {
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }
            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = Cont.Muzzle.transform.position + new Vector3(Cont.Direction.x * _createPos, 0, 0);

            float startAngle = -_range;
            float angleStep = (_range * 2f) / (_shootCount == 1 ? 1 : (_shootCount - 1));

            for (int i = 0; i < _shootCount; i++)
            {
                float currentAngle = startAngle + angleStep * i;

                // 回転を適用（Z軸回転）
                Quaternion rot = Quaternion.Euler(0, 0, currentAngle);

                // 弾生成
                var bullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);

                // 回転方向を適用して初期化
                InitBullet(bullet, rot * Cont.ShootDir);
            }


            await base.Shoot();
        }
    }
}
//unicode