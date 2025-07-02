using System;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public class ShootForward : ShootStateBase
    {
        [SerializeField] private float _createPos = 0.35f;
        public ShootForward(BulletData data, LayerMask targetLayer, string animeTrigger) : base(data, targetLayer, animeTrigger) { }
        public ShootForward() { }
        public override void Enter(IState previousIState, IUnit parent)
        {
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }

            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = parent.Transform.position + new Vector3(parent.Direction.x * _createPos, 0, 0);

            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);


            // ステータスをセット（速度、方向、ダメージなど）
            instantiatedBullet.SetBulletStatus(_data, _targetLayer);
        }

        public override bool AllowChange(IState nextState, IUnit parent)
        {
            if (nextState is Stun) return true;
            return base.AllowChange(nextState, parent);
        }
    }
}
//unicode