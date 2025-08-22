using BlackRose.Core.Models.Units;
using BlackRose.Datas.Definitions;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class ShootForward : ShootStateBase
    {
        [SerializeField] private float _createPos = 0.35f;
        public ShootForward(BulletData data, LayerMask targetLayer) : base(data, targetLayer) { }
        public ShootForward() { }
        public override void Enter(IState preview, UnitBase parent)
        {
            _ = Shoot(parent);
        }

        public override bool AllowChange(IState nextState, UnitBase parent)
        {
            if (base.AllowChange(nextState, parent)) return true;
            return false;
        }

        protected async override UniTask Shoot(UnitBase unit)
        {
            var b = _data.prefab;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
            }
            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = unit.Transform.position + new Vector3(unit.Direction.x * _createPos, 0, 0);
            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
            // ステータスをセット（速度、方向、ダメージなど）
            instantiatedBullet.SetBulletStatus(_data, _targetLayer);
            await base.Shoot(unit);
        }
    }
}
//unicode