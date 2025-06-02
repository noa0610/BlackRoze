using System;
using UnityEngine;

namespace BlackRose
{

    public class ShootForward : ShootStateBase
    {
        public BulletObject BulletObject => _bulletObject;

        public override bool Enter(IState previousState, IUnit parent)
        {
            return true;
        }

        public override bool Exit(IState nextState, IUnit parent)
        {
            // 攻撃終了時（今は特に処理なし）
            return true;
        }

        public override bool Stay(IUnit parent)
        {
            var b = _bulletObject.bulletData.bullet;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
                return false; // 弾のプレハブが設定されてなかったらエラー扱い
            }

            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = parent.Transform.position + new Vector3(parent.Direction.x * 1.5f, 0, 0);

            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);


            // ステータスをセット（速度、方向、ダメージなど）
            instantiatedBullet.SetBulletStatus(_bulletObject, _targetLayer);
            ActionInvoke();
            return true;
        }

        public ShootForward(BulletObject bulletObject, LayerMask targetLayer) : base(bulletObject, targetLayer) { }
    }
}
//unicode