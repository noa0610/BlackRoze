using BlackRose.Core.Models.Units;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class MultiShoot : ShootStateBase
    {
        [Tooltip("同時シュートする際の最大角度")]
        [SerializeField, Min(0)] private float _range;
        [Tooltip("同時に発射する弾数")]
        [SerializeField, Min(1)] private int _shootCount;

        protected override async UniTask Shoot(UnitBase unit)
        {
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }
            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = _muzzle.transform.position + new Vector3(unit.Direction.x * _createPos, 0, 0);

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
                InitBullet(bullet, rot * _direction);
            }


            await base.Shoot(unit);
        }
    }
}