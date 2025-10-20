using UnityEngine;
using System;

namespace BlackRose.Core.Models.Units
{
    public class SieldBlock : MonoBehaviour
    {
        [Header("攻撃状態")]
        [SerializeField] private bool isAttackEnabled = false; // 攻撃ON/OFF

        public void SetAttack(bool enabled)
        {
            isAttackEnabled = enabled;
            Debug.Log($"[SieldBlock] 攻撃判定が {(enabled ? "有効" : "無効")} になりました");
        }

        private Enemy_Gurter gurter;

        private void Start()
{
    gurter = GetComponentInParent<Enemy_Gurter>();
    if (gurter != null)
    {
        gurter.OnStunStart += DisableShield;
        gurter.OnStunEnd += EnableShield;
        gurter.OnAttackStart += EnableAttack;
        gurter.OnAttackEnd += DisableAttack;
    }
}

        private void OnDestroy()
        {
            if (gurter != null)
            {
                gurter.OnStunStart -= DisableShield;
                gurter.OnStunEnd -= EnableShield;
                gurter.OnAttackStart -= EnableAttack;
                gurter.OnAttackEnd -= DisableAttack;
            }
        }
        private void EnableAttack()
        {
            SetAttack(true);
        }
        private void DisableAttack()
        {
            SetAttack(false);
        }
        // 

        private void DisableShield()
        {
            Debug.Log("シールド防御無効化：スタン中");
            GetComponent<Collider2D>().enabled = false;
        }

        private void EnableShield()
        {
            Debug.Log("シールド防御再有効化：スタン終了");
            GetComponent<Collider2D>().enabled = true;
        }
        public event Action<float> OnPenetrate; // ダメージ値を渡すイベント

        [Header("防御状態")]
        [SerializeField] private bool isDefenseEnabled = true; // 防御ON/OFF

        /// <summary>
        /// シールド防御の有効・無効を切り替える
        /// </summary>
        public void SetDefense(bool enabled)
        {
            isDefenseEnabled = enabled;
            Debug.Log($"[SieldBlock] 防御が {(enabled ? "有効" : "無効")} になりました");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isAttackEnabled && collision.CompareTag("Player"))
            {
                Debug.Log("シールドタックルでプレイヤーにダメージ！");
                return;
            }

            // 防御が無効なら何もせず弾を素通りさせる
            if (!isDefenseEnabled)
                return;

            if (collision.gameObject.CompareTag("Playerbullet"))
            {
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                if (bullet != null)
                {
                    float damage = bullet.Damage;

                    if (damage <= 3f)
                    {
                        Debug.Log("ダメージが3以下のため、シールドが防御しました");
                    }
                    else
                    {
                        Debug.Log($"シールド貫通！ ダメージ：{damage}");
                        OnPenetrate?.Invoke(damage); // イベント発火
                    }
                }
                else
                {
                    Debug.LogError("Bulletコンポーネントが見つかりません");
                }
            }
        }
    }
}
