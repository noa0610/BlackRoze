using BlackRose.Datas.Definitions;
using Fungus;
using HighElixir.Timers;
using System;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// 弾丸の基本クラス（Trigger Collider 必須）
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour, IDisposable
    {
        [SerializeField, Tooltip("常に衝突可能なレイヤー")]
        private LayerMask _canHitLayer;

        protected LayerMask _targetLayer;
        protected BulletStatus _status;
        [SerializeField] protected Vector2 _direction;
        protected float _currentHP;
        protected UnitBase _parent;
        protected TimerTicket _ticket;
        public Transform Transform => transform;
        public UnitBase Parent => _parent;
        public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }

        public virtual bool CanSelfMove => true;

        public event Action<Bullet> OnDestoryHandle;

        // 生成時にステータスをセット
        public void SetBulletStatus(BulletData bullet, LayerMask targetLayer)
        {
            _status = bullet.originalstatus;
            _currentHP = _status.hp;
            _targetLayer = targetLayer;
        }

        public void SetDirection(Vector2 dir)
        {
            _direction = dir;
        }

        public void SetParent(UnitBase parent)
        {
            _parent = parent;
        }

        public void Reflect()
        {
            _direction = -_direction;
        }

        public virtual void NotifyDestoy()
        {
            if (OnDestoryHandle != null)
                OnDestoryHandle(this);
            else
                Destroy(gameObject);
        }
        protected virtual void Move(float deltaTime)
        {
            //Debug.Log($"Move. delta:{deltaTime}");
            transform.position += (Vector3)_direction * _status.speed * deltaTime;
        }

        public virtual void Invoke()
        {
            var gt = GlobalTimer.FixedUpdate;
            _ticket = gt.CountDownRegister(_status.time, $"[{name}] duration", () => NotifyDestoy());
            gt.GetReactiveProperty(_ticket).Subscribe(td => Move(-td.Delta));
            gt.Start(_ticket);
        }
        protected virtual void OnDestroy()
        {
            GlobalTimer.FixedUpdate.UnRegister(_ticket);
        }
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            HitCheck(collision);
        }

        protected virtual void HitCheck(Collider2D collision)
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

            if (Hit()) Destroy(gameObject);
        }

        protected virtual void Hitted_Target(Collider2D collision)
        {
            var go = collision.gameObject;
            if (!go.TryGetComponent<UnitBase>(out var target))
            {
                target = go.GetComponentInParent<UnitBase>();
            }
            if (target != null)
            {
                Debug.Log($"Hit Target: {target.UnitStatusData.unitName}");
                if (target.IsInvincible)
                    return;
                UnitManager.instance.AddDamage(target, _parent, _status.damage);
            }

            if (Hit()) NotifyDestoy();
        }

        protected bool Hit()
        {
            if (_currentHP != -1)
            {
                _currentHP--;
                if (_currentHP < 0)
                {
                    _currentHP = 0;
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            OnDestoryHandle = null;
        }
    }
}
