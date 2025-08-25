using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    /// <summary>
    /// 指定した対象にセットしたオブジェクトを向ける。
    /// </summary>
    public class Idle_Looking : Idle
    {
        private GameObject _rotationTarget;
        private GameObject _target; // 回転したい相手
        private float _angularVel;         // 角速度の一時値（SmoothDamp用）
        public float turnSmoothTime = 0.08f;

        public Vector2 Direction { get; private set; } = new(1, 0);
        /// <param name="target">回転させたいもの</param>
        public Idle_Looking(GameObject target)
        {
            _rotationTarget = target;
        }

        public void SetTarget(GameObject target)
        {
            _target = target;
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            if (_rotationTarget == null || _target == null) return;

            var toTarget = _target.transform.position - _rotationTarget.transform.position;
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

            float current = _rotationTarget.transform.eulerAngles.z;
            float smoothed = Mathf.SmoothDampAngle(current, targetAngle, ref _angularVel, turnSmoothTime);

            _rotationTarget.transform.rotation = Quaternion.Euler(0f, 0f, smoothed);
            Direction = new Vector2(toTarget.x, toTarget.y).normalized;
        }

        // 指定した角度以内にターゲットがいるか
        public bool IsLookingTarget(float ditectionAngle)
        {
            if (_rotationTarget == null || _target == null) return false;
            var toTarget = _target.transform.position - _rotationTarget.transform.position;
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            float current = _rotationTarget.transform.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(current, targetAngle);
            return Mathf.Abs(angleDiff) <= ditectionAngle;
        }
    }
}