using Unity.Collections;
using UnityEngine;

namespace BlackRose
{
    // 概要:
    // Bullet クラスは、ゲーム内の弾（Projectile）オブジェクトの挙動を制御するコンポーネントです。
    // MonoBehaviour を継承し、IStopableObject を実装することで、一時停止対応を視野に入れた設計となっています。
    // 弾のステータス情報は BulletObject 経由で管理され、移動・衝突・破壊の処理が行われます。

    // 主な機能:
    // - SetBulletStatus: 弾のステータス（進行方向や速度など）を外部から設定
    // - Update: 毎フレーム、ステータスに基づき直線的に移動（方向は2Dで、X軸基準）
    // - OnCollisionEnter2D: 他の2Dオブジェクトと衝突した際に自動で自身を破壊
    // - 一時停止インターフェース（GamePlay_Pose / GamePlay_Continue / Dispose）は未実装

    // 備考:
    // - 現状、移動ロジックは非常にシンプルで、方向ベクトル × Time.deltaTime による等速直線運動
    // - BulletObject の内容により弾の性質を変更可能（拡張時の柔軟性あり）
    // - 今後、一時停止時の Rigidbody 無効化やエフェクト制御などを実装する余地あり

    public class Bullet : MonoBehaviour, IStopableObject
    {
        [SerializeField, ReadOnly]
        private BulletObject _bulletObject;
        private LayerMask _targetLayer;
        public Transform Transform => transform;
        public BulletObject BulletObject => _bulletObject;

        // 弾のステータス設定（生成時に呼ばれる想定）
        public void SetBulletStatus(BulletObject bullet, LayerMask targetLayer)
        {
            _bulletObject = bullet;
            _targetLayer = targetLayer;
        }

        // 毎フレームの更新処理（弾の移動）
        protected virtual void Update()
        {
            transform.position = transform.position + (Vector3)_bulletObject.currentstatus.direction * _bulletObject.currentstatus.speed * Time.deltaTime;
        }

        // 2D衝突検知（敵や壁に当たったら発動）
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & _targetLayer) == 0)
                return;

            Debug.Log("Hit"); // ログ出力（当たった！）
            if (collision.transform.TryGetComponent<UnitBase>(out var target))
            {
                UnitManager.instance.AddDamage(target, null, _bulletObject.bulletData.originalstatus.damage);
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