using System;
using UnityEngine;

namespace BlackRose
{

    public class ShootForward : ShootStateBase
    {
        public BulletObject BulletObject => _bulletObject;

        public override bool Enter(IState previousState, IUnit parent)
        {
            parent.Animator?.SetTrigger(_animeTrigger);
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
            return true;
        }

        public override bool Exit(IState nextState, IUnit parent)
        {
            parent.Animator?.SetTrigger(_animeTrigger);
            // 攻撃終了時（今は特に処理なし）
            return true;
        }

        public override bool Stay(IUnit parent)
        {
            if (parent.Animator == null || parent.Animator?.GetCurrentAnimatorStateInfo(0).IsName(_animeTrigger) == false)
            {
                ActionInvoke();
            }
            return true;
        }

        public ShootForward(BulletObject bulletObject, LayerMask targetLayer, string animeTrigger) : base(bulletObject, targetLayer, animeTrigger) { }
    }
}
//unicode