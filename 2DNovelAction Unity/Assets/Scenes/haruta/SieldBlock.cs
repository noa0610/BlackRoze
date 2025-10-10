using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public class SieldBlock : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // タグがplayerBulletなら判定
            if (collision.gameObject.CompareTag("playerBullet"))
            {
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();

                if (bullet != null && bullet.bulletData != null)
                {
                    // ダメージ値を参照
                    float damage = bullet.bulletData.originalstatus.damage;

                    // 3以下なら防ぐ（弾を破壊して終了）
                    if (damage <= 3f)
                    {
                        Debug.Log("ダメージが3以下のため、シールドが防御しました");
                        Destroy(collision.gameObject);
                        return;
                    }

                    // 3を超える場合はシールドが破壊されるなどの処理を追加
                    Debug.Log("シールド貫通！ ダメージ：" + damage);
                }
            }
        }
    }
}

