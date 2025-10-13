using BlackRose.Datas.Definitions;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField, Tooltip("常に衝突可能なレイヤー")]
        private LayerMask _canHitLayer;

        protected LayerMask _targetLayer;
        protected BulletStatus _status;
        protected Vector2 _direction;
        protected UnitBase _parent;

        public Transform Transform => transform;
        public UnitBase Parent => _parent;
        public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }
        public float Damage => _status.damage;

        public void SetBulletStatus(BulletData bullet, LayerMask targetLayer)
        {
            _status = bullet.originalstatus;
            _targetLayer = targetLayer;
        }

        public void SetDirection(Vector2 dir) => _direction = dir.normalized;
        public void SetParent(UnitBase parent) => _parent = parent;
        public void Reflect(Vector2 normal = default)
        {
            if (normal != default)
                _direction = Vector2.Reflect(_direction, normal);
            else
                _direction = -_direction;
        }
        public virtual void DestroyThis()
        {
            Destroy(gameObject);
        }

        protected virtual void FixedUpdate()
        {
            transform.position += (Vector3)_direction * _status.speed * Time.fixedDeltaTime;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            int layer = collision.gameObject.layer;

            // Target と CanHit に同時に同じレイヤーが指定されている場合、
            // 常にTargetを優先する
            if (Has(layer, _targetLayer))
            {
                Hitted_Target(collision);
            }
            else if (Has(layer, _canHitLayer))
            {
                Hitted_Another(collision);
            }
        }

        protected bool Has(int layer, LayerMask mask)
        {
            return (mask & (1 << layer)) != 0;
        }

        protected virtual void Hitted_Another(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Throughable"))
                return;

            DestroyThis();
        }

        protected virtual void Hitted_Target(Collider2D collision)
        {
            if (collision.TryGetComponent<UnitBase>(out var target))
            {
                if (target.IsInvincible)
                    return;

                UnitManager.instance.AddDamage(target, _parent, _status.damage);
            }

            DestroyThis();
        }
    }
}
