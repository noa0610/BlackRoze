using BlackRose.Datas.Definitions;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// コライダーはTriggerにすること
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [SerializeField, Tooltip("常に衝突可能なレイヤー")] private LayerMask _canHitLayer;
        protected LayerMask _targetLayer;
        protected BulletStatus _status;
        protected Vector2 _direction;
        protected UnitBase _parent;
        public Transform Transform => transform;
        public UnitBase Parent => _parent;

        // 弾のステータス設定（生成時に呼ばれる想定）
        public void SetBulletStatus(BulletData bullet, LayerMask targetLayer)
        {
            _status = bullet.originalstatus; // 初期ステータスを設定
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
        // 毎フレームの更新処理（弾の移動）
        protected virtual void Update()
        {
            transform.position = transform.position + (Vector3)_direction * _status.speed * Time.deltaTime;
        }

        // 2D衝突検知（敵や壁に当たったら発動）
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & _canHitLayer) == 1)
            {
                Hitted_Another(collision);
            }
            if (((1 << collision.gameObject.layer) & _targetLayer) == 0)
                return;
            Hitted_Target(collision);
        }

        // 目的じゃないオブジェクトにヒットした場合に呼ばれる
        protected virtual void Hitted_Another(Collider2D collision)
        {
            Destroy(gameObject);
        }
        // 目的のオブジェクトにヒットした場合に呼ばれる
        protected virtual void Hitted_Target(Collider2D collision)
        {
            if (collision.transform.TryGetComponent<UnitBase>(out var target))
            {
                Debug.Log("Hit to Target. Name : " + target.UnitStatusData.unitName); // ログ出力（当たった！）
                if (target.IsInvincible) return;
                UnitManager.instance.AddDamage(target, null, _status.damage);
            }
            Destroy(gameObject); // 弾を破壊（寿命）
        }
    }
}