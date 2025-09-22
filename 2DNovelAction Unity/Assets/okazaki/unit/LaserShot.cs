using BlackRose.Core.Models.Units;
using System;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class LaserShot : StateWithAnime
    {
        [SerializeField] protected Rigidbody2D _rb;
        [SerializeField] private float _attackCooldown = 5f; // 攻撃クールダウン時間
        [SerializeField] private GameObject bulletPrefab; // 弾のPrefab

        private Transform[] firePoints; // 発射ポイント
        private float cooldownTimer = 0f; // 現在のクールダウン経過時間
        private int currentFirePointIndex = 0; // 今撃つポイントの番号

        private int shotCount = 0; // 発射した回数
        private int maxShots = 6;  // 最大発射数

        public LaserShot(Rigidbody2D rb, Transform[] firePoints, GameObject bulletPrefab)
        {
            _rb = rb;
            this.firePoints = firePoints;
            this.bulletPrefab = bulletPrefab;
        }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            Debug.Log("Enter of Attack");
            cooldownTimer = _attackCooldown; // 最初の発射をすぐにできるように
            currentFirePointIndex = 0;
            shotCount = 0; // 発射回数リセット
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            if (shotCount >= maxShots)
            {
                // 6発撃ち終わったらトリガー発火
                parent.StateMachine.ChangeState("Attack1end");
                return;
            }

            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                ShootFromPoint(currentFirePointIndex);
                currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
                cooldownTimer = _attackCooldown;
                shotCount++;
            }
        }

        private void ShootFromPoint(int index)
        {
            if (firePoints == null || firePoints.Length == 0) return;

            Transform firePoint = firePoints[index];
            GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // 弾に物理挙動を与える（Rigidbody2D必須）
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = firePoint.right * 10f; // 発射速度
            }

            Debug.Log($"Shot from firePoint {index + 1}");
        }
    }
}