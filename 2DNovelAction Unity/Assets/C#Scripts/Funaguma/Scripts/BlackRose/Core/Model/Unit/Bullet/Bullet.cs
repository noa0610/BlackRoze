using BlackRose.Datas.Definitions;
using HighElixir.Timers;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// 弾丸の基本クラス（Trigger Collider 必須）
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [SerializeField, Tooltip("常に衝突可能なレイヤー")]
        private LayerMask _canHitLayer;

        protected LayerMask _targetLayer;
        protected BulletStatus _status;
        protected Vector2 _direction;
        protected UnitBase _parent;
        protected TimerTicket _ticket;
        public Transform Transform => transform;
        public UnitBase Parent => _parent;
        public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }

        // 生成時にステータスをセット
        public void SetBulletStatus(BulletData bullet, LayerMask targetLayer)
        {
            _status = bullet.originalstatus;
            _targetLayer = targetLayer;
        }

        public void SetDirection(Vector2 dir)
        {
            _direction = dir.normalized;
        }

        public void SetParent(UnitBase parent)
        {
            _parent = parent;
        }

        public void Reflect()
        {
            _direction = -_direction;
        }

        protected virtual void Move(float deltaTime)
        {
            Debug.Log($"Move. delta:{deltaTime}");
            transform.position += (Vector3)_direction * _status.speed * deltaTime;
        }

        public virtual void Invoke()
        {
            var gt = GlobalTimer.FixedUpdate;
            _ticket = gt.CountDownRegister(_status.time, $"[{name}] duration", () => Destroy(gameObject));
            gt.GetReactiveProperty(_ticket).Subscribe(dt => Move(dt));
            gt.Start(_ticket);
        }
        protected virtual void OnDestroy()
        {
            GlobalTimer.FixedUpdate.Unregister(_ticket);
        }
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            int layerBit = 1 << collision.gameObject.layer;

            // 衝突可能レイヤー
            if ((_canHitLayer.value & layerBit) != 0)
            {
                Hitted_Another(collision);
                return;
            }

            // ターゲットレイヤー
            if ((_targetLayer.value & layerBit) != 0)
            {
                Hitted_Target(collision);
                return;
            }
        }

        protected virtual void Hitted_Another(Collider2D collision)
        {
            // 「貫通可能」なレイヤーはスルー
            if (LayerMask.NameToLayer("Throughable") == collision.gameObject.layer)
                return;

            Destroy(gameObject);
        }

        protected virtual void Hitted_Target(Collider2D collision)
        {
            if (collision.transform.TryGetComponent<UnitBase>(out var target))
            {
                Debug.Log($"Hit Target: {target.UnitStatusData.unitName}");

                if (target.IsInvincible)
                    return;

                UnitManager.instance.AddDamage(target, _parent, _status.damage);
            }

            Destroy(gameObject);
        }
    }
}
