using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.Objects
{
    [Serializable]
    public class ReflectMono : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 弾に当たったら反射させる
            if (
                collision.TryGetComponent<Bullet>(out var bullet) && 
                ((int)bullet.TargetLayer & LayerMask.GetMask("Player")) != 0)
            {
                // 反射処理
                bullet.TargetLayer = 1 << LayerMask.NameToLayer("Enemy");
                bullet.Reflect();
            }
        }
    }
}
