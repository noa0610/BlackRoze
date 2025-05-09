using UnityEngine;

namespace BlackRose
{
	[CreateAssetMenu(menuName = "BlackRose/BulletData")]
	public class BulletData : ScriptableObject
	{
		public string bulletName;
		public Bullet bullet;
		public BulletStatus originalstatus;
	}
}