using System;
using UniRx;
using UnityEngine;
using Fungus;
using HighElixir.Timers;
using BlackRose.Datas.Definitions;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// 弾丸の基本クラス（Trigger Collider 必須）
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour, IDisposable
    {
        #region === Inspector ===
        [Header("Collision Layers")]
        [SerializeField, Tooltip("常に衝突可能なレイヤー")]
        private LayerMask _canHitLayer;
        [SerializeField, Tooltip("初期方向")]
        protected Vector2 _direction;
        #endregion

        #region === Fields ===
        protected LayerMask _targetLayer;
        protected BulletStatus _status;
        protected float _currentHP;
        protected UnitBase _parent;
        protected TimerTicket _ticket;
        #endregion

        #region === Properties ===
        public Transform Transform => transform;
        public UnitBase Parent => _parent;
        public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }
        public virtual bool CanSelfMove => true;
        public float Damage => _status.damage;
        #endregion

        #region === Events ===
        public event Action<Bullet> OnDestoryHandle;
        #endregion

        #region === Setup & Initialization ===
        /// <summary>
        /// 弾丸のステータスをセット（生成時に呼ばれる）
        /// </summary>
        public void SetBulletStatus(BulletData bullet, LayerMask targetLayer)
        {
            _status = bullet.originalstatus;
            _currentHP = _status.hp;
            _targetLayer = targetLayer;
        }

        public void SetDirection(Vector2 dir) { _direction = dir; OrientToDirection(dir); }
        public void SetParent(UnitBase parent) => _parent = parent;

        public void Reflect() { _direction = -_direction; OrientToDirection(_direction); }
        #endregion

        #region === Core Logic ===
        public virtual void Invoke()
        {
            var gt = GlobalTimer.FixedUpdate;
            _ticket = gt.CountDownRegister(_status.time, $"[{name}] duration", () => NotifyDestoy());
            gt.GetReactiveProperty(_ticket).Subscribe(td => Move(-td.Delta));
            gt.Start(_ticket);

            // Ensure orientation matches direction when invoked
            OrientToDirection(_direction);
        }

        protected virtual void Move(float deltaTime)
        {
            transform.position += (Vector3)_direction * _status.speed * deltaTime;
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
        #endregion

        #region === Collision Handling ===
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            HitCheck(collision);
        }

        protected virtual void HitCheck(Collider2D collision)
        {
            int layerBit = 1 << collision.gameObject.layer;

            // 常に衝突可能なレイヤー
            if ((_canHitLayer.value & layerBit) != 0)
            {
                Hitted_Another(collision);
                return;
            }

            // 対象レイヤー
            if ((_targetLayer.value & layerBit) != 0)
            {
                Hitted_Target(collision);
                return;
            }
        }

        protected virtual void Hitted_Another(Collider2D collision)
        {
            // 「Throughable」レイヤーは貫通
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
        #endregion

        #region === Destroy & Cleanup ===
        public virtual void NotifyDestoy()
        {
            if (OnDestoryHandle != null)
                OnDestoryHandle(this);
            else if (gameObject != null)
                Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            GlobalTimer.FixedUpdate.UnRegister(_ticket);
        }

        public void Dispose()
        {
            OnDestoryHandle = null;
        }
        #endregion

        // Orient sprite so that (1,0) is treated as right-facing.
        private void OrientToDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude < 1e-6f) return;
            // Set transform.right so the object's right points along dir (1,0 is right)
            transform.right = new Vector3(dir.x, dir.y, 0f);
        }
    }
}
