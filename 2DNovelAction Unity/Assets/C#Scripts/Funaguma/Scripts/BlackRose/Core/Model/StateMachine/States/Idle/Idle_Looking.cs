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
                // 1) ターゲットまでの方向ベクトル（2D）
                Vector3 toTarget = _target.transform.position - _rotationTarget.transform.position;
                // 2) Atan2でY/Xの角度をラジアン→度数に変換
                float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
                // 3) Z軸まわりにグイッと回転
                _rotationTarget.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                // 4) Direction も更新（例：正規化したベクトルを再計算）
                Direction = QuaternionToVector2_ViaEuler(_rotationTarget.transform.rotation).normalized;
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