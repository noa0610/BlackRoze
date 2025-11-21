using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Datas.Definitions
{
    [CreateAssetMenu(menuName = "BlackRose/BulletData")]
    public class BulletData : ScriptableObject
    {
        public string bulletName;
        public Bullet prefab;
        public BulletStatus originalstatus;
    }
}