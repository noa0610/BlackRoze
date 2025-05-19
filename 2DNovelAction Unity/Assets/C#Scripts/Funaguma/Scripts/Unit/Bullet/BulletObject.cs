using System;
using UnityEngine;

namespace BlackRose
{
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