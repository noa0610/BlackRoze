using Unity.Collections;
using UnityEngine;

namespace BlackRose
{
    public class Bullet : MonoBehaviour, IStopableObject
    {
        [SerializeField,Tooltip("常に衝突可能なレイヤー")] private LayerMask _canHitLayer;
        private LayerMask _targetLayer;
        private BulletStatus _status;
        public Transform Transform => transform;

        // 弾のステータス設定（生成時に呼ばれる想定）
        public void SetBulletStatus(BulletData bullet, LayerMask targetLayer)
        {
            _status = bullet.originalstatus; // 初期ステータスを設定
            _targetLayer = targetLayer;
        }

        // 毎フレームの更新処理（弾の移動）
        protected virtual void Update()
        {
            transform.position = transform.position + (Vector3)_status.direction * _status.speed * Time.deltaTime;
        }

        // 2D衝突検知（敵や壁に当たったら発動）
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & _canHitLayer) == 1)
            {
                Destroy(gameObject);
            }
            if (((1 << collision.gameObject.layer) & _targetLayer) == 0)
                return;

            Debug.Log("Hit"); // ログ出力（当たった！）
            if (collision.transform.TryGetComponent<UnitBase>(out var target))
            {
                UnitManager.instance.AddDamage(target, null, _status.damage);
            }
            Destroy(gameObject); // 弾を破壊（寿命）
        }

        // 以下、一時停止／再開など（未実装）

        public void Dispose()
        {
            // 弾の終了処理（今は空）
        }

        public void GamePlay_Continue()
        {
            // 一時停止解除時に呼ばれる予定（今は空）
        }

        public void GamePlay_Pose()
        {
            // ゲーム一時停止時に呼ばれる予定（今は空）
        }
    }
}
//unicode