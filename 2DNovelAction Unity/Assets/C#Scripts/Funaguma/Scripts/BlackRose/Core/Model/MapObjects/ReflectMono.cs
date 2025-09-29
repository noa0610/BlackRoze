using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.Objects
{
    public class ReflectMono : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 弾に当たったら反射させる
            if (collision.TryGetComponent<Bullet>(out var bullet))
            {
                // 反射処理
                bullet.Reflect();
            }
        }
    }
}
