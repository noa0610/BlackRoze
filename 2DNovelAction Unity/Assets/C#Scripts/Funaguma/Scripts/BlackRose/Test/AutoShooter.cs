using BlackRose.Core.Models.Units;
using BlackRose.Datas.Definitions;
using HighElixir.Timers;
using UnityEngine;

namespace BlackRose.Test
{
    public class AutoShooter : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private BulletData _data;
        [SerializeField] private LayerMask _target;
        [SerializeField, Min(0.1f)] private float _interval;
        private TimerTicket _ticket;
        private Timer GT => GlobalTimer.Update;
        public void Shoot(bool isButton = false)
        {
            Debug.Log("[AutoShoot!]");
            if (isButton) GT.Start(_ticket, isLazy: true);
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }
            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, transform.position, Quaternion.identity);
            // ステータスをセット（速度、方向、ダメージなど）
            instantiatedBullet.SetBulletStatus(_data, _target);
            instantiatedBullet.SetDirection(transform.rotation.eulerAngles);
        }

        private void Awake()
        {
            _ticket = GT.PulseRegister(_interval,"[TestUnit] ShootInterval", () => Shoot());
            GT.Start(_ticket);
        }
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                GT.ChangeDuration(_ticket, _interval);
            }
        }
#else
        private void Awake()
        {
            Destroy(gameObject);
        }
#endif
    }
}