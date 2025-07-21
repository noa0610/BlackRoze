using UnityEngine;

namespace BlackRose
{
    /// <summary>
    /// 指定した対象にセットしたオブジェクトを向ける。
    /// </summary>
    public class Idle_Looking : Idle
    {
        private GameObject _rotationTarget;
        private GameObject _target; // 回転したい相手

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

        public override void Stay(UnitBase parent)
        {
            if (_target != null)
            {
                _rotationTarget.transform.LookAt(_target.transform);
                Direction = QuaternionToVector2_ViaEuler(_rotationTarget.transform.rotation);
            }
        }

        public static Vector2 QuaternionToVector2_ViaEuler(Quaternion q)
        {
            // Z回転角をラジアンに変換
            float rad = q.eulerAngles.z * Mathf.Deg2Rad;
            // cos→X成分、sin→Y成分
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

    }
}