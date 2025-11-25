using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using DG.Tweening;

namespace BlackRose.Core.Models.Units.State
{
    [Serializable]
    public class LaserState<T> : ShootBase<T>
        where T : UnitBase
    {
        [SerializeField] private float _maxScale = 3f;
        [SerializeField] private float _duration = 2f;
        protected override async UniTask Shoot()
        {
            await base.Shoot();
            // レーザー生成
            var l = GameObject.Instantiate(_data.prefab, Cont.Muzzle.transform.position + (Vector3)(Cont.ShootDir * _createPos), GetQuaternion());
            Vector2 dir = new Vector2(Mathf.Cos(GetAngle() * Mathf.Deg2Rad), Mathf.Sin(GetAngle() * Mathf.Deg2Rad));
            l.SetDirection(dir);
            l.SetBulletStatus(_data, Cont.AttackLayer);
            l.Invoke();
            // スプライトを伸ばす
            await l.gameObject.transform.DOScaleX(_maxScale, _duration).AsyncWaitForCompletion().AsUniTask();
            await UniTask.WhenAll(
                l.gameObject.transform.DOScaleX(0, _duration * 2).AsyncWaitForCompletion().AsUniTask(),
                l.gameObject.transform.DOScaleY(0, _duration * 0.4f).AsyncWaitForCompletion().AsUniTask());

            l?.NotifyDestoy();
        }
        private Quaternion GetQuaternion()
        {
            return Quaternion.Euler(0, 0, GetAngle());
        }

        private float GetAngle()
        {
            var dir = Cont.ShootDir;
            float angle = 0f;

            if (dir.y > 0)
                angle = 30f;      // 斜め上
            else if (dir.y < 0)
                angle = -30f;     // 斜め下

            if (dir.x < 0)
                angle = 180f - angle; // 左向き調整
            return angle;
        }
    }
}