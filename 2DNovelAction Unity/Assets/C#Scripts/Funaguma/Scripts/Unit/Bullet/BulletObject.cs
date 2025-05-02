using System;
using UnityEngine;

namespace Test
{
    [CreateAssetMenu(menuName = "弾")]
    public class BulletData : ScriptableObject
    {
        public string bulletName;
        public Bullet bullet;
        public BulletStatus originalstatus;
    }

    [Serializable]
    public class BulletObject
    {
        public BulletData bulletData;
        public BulletStatus currentstatus;// BulletStatus is struct.


        /// <returns>Type => BulletObject</returns>
        public BulletObject Clone()
        {
            var clone = new BulletObject()
            {
                bulletData = bulletData,
                currentstatus = currentstatus
            };
            return clone;
        }
    }
}
//unicode