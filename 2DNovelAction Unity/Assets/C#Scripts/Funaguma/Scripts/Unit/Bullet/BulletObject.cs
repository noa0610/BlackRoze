using System;

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

        private void Awake()
        {
            if (bulletData == null)
            {
                throw new NullReferenceException("BulletData is not set.");
            }
            currentstatus = bulletData.originalstatus; // Initialize current status with original status
        }
    }
}
//unicode