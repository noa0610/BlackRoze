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
                ShootFromPoint(currentFirePointIndex, parent);
                currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
                cooldownTimer = _attackCooldown;
                shotCount++;
            }
        }

        private void ShootFromPoint(int index, UnitBase parent)
        {
            if (firePoints == null || firePoints.Length == 0) return;

            Transform firePoint = firePoints[index];
            GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // 弾に物理挙動を与える（Rigidbody2D必須）
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                // プレイヤーのいる方向に力を加える
                UnitBase player = FindNearestPlayer(firePoint.position);
                if (player != null)
                {
                    Vector2 dir = (player.Transform.position - firePoint.position).normalized;
                    bulletRb.velocity = dir * 10f;
                }
                else
                {
                    // プレイヤーが見つからない場合は右方向
                    bulletRb.velocity = firePoint.right * 10f;
                }
            }

            Debug.Log($"Shot from firePoint {index + 1}");
        }

        // プレイヤーを探す補助メソッド
        private UnitBase FindNearestPlayer(Vector2 from)
        {
            var units = UnitManager.instance.GetUnitList();
            UnitBase nearest = null;
            float minDist = float.MaxValue;
            foreach (var unit in units)
            {
                // プレイヤーのタグで判定
                if (unit.gameObject.CompareTag("Player")) // プレイヤーのGameObjectのタグを"Player"に設定してください
                {
                    float dist = (unit.Transform.position - (Vector3)from).sqrMagnitude;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = unit;
                    }
                }
            }
            return nearest;
        }
    }
}