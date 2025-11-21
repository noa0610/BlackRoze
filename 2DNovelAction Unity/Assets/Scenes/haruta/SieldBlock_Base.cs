using UnityEngine;
using System;
using BlackRose.Core.Models.EffectManager;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// シールド（基底）
    /// - 攻撃/防御切り替え、弾の貫通イベント発火はそのまま保持
    /// - プレイヤーへのダメージ/スタン付与は UnitManager / StatusEffect を使って行う
    /// </summary>
    public class SieldBlock_Base : MonoBehaviour
    {
        [Header("攻撃状態")]
        [SerializeField] private bool isAttackEnabled = false; // 攻撃ON/OFF

        [Header("攻撃コライダー（任意）")]
        [SerializeField] protected Collider2D attackCollider; // 攻撃判定用のコライダー（子に分けている場合に設定）

        public void SetAttack(bool enabled)
        {
            isAttackEnabled = enabled;
            // attackCollider が指定されていればそれを切り替え、なければ自身の Collider2D を切り替える
            if (attackCollider != null)
            {
                attackCollider.enabled = enabled;
            }
            else
            {
                var col = GetComponent<Collider2D>();
                if (col != null)
                    col.enabled = enabled;
            }
        }

        protected Enemy_Gurter gurter;

        protected virtual void Awake()
        {
            gurter = GetComponentInParent<Enemy_Gurter>();
            if (gurter != null)
            {
                gurter.OnStunStart += DisableShield;
                gurter.OnStunEnd += EnableShield;
                // OnAttackStart/End events may exist; subscribe if available
                gurter.OnAttackStart += EnableAttack;
                gurter.OnAttackEnd += DisableAttack;
            }

            // attackCollider が未指定なら、子オブジェクトに名前に "Attack" を含む Collider2D を探す
            if (attackCollider == null)
            {
                foreach (Transform t in transform)
                {
                    if (t.name.ToLower().Contains("attack"))
                    {
                        var c = t.GetComponent<Collider2D>();
                        if (c != null)
                        {
                            attackCollider = c;
                            break;
                        }
                    }
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (gurter != null)
            {
                gurter.OnStunStart -= DisableShield;
                gurter.OnStunEnd -= EnableShield;
                gurter.OnAttackStart -= EnableAttack;
                gurter.OnAttackEnd -= DisableAttack;
            }
        }

        protected void EnableAttack() => SetAttack(true);
        protected void DisableAttack() => SetAttack(false);

        protected void DisableShield()
        {
            Debug.Log("シールド防御無効化：スタン中");
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        protected void EnableShield()
        {
            Debug.Log("シールド防御再有効化：スタン終了");
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }

        public event Action<float> OnPenetrate; // ダメージ値を渡すイベント

        [Header("防御状態")]
        [SerializeField] protected bool isDefenseEnabled = true; // 防御ON/OFF

        /// <summary>
        /// シールド防御の有効・無効を切り替える
        /// </summary>
        public void SetDefense(bool enabled)
        {
            isDefenseEnabled = enabled;
        }

    [Header("タックル設定（Inspectorで調整可）")]
    [SerializeField] protected float damageScale = 1f; // ガーダーの power に対する倍率
    [SerializeField] protected float tackleStunDuration = 0.5f; // タックルで与えるスタン時間（秒）

    [Header("ノックバック設定（Inspectorで調整可）")]
    [SerializeField] protected Vector2 knockbackDirection = new Vector2(1f, 0.6f); // ローカル方向（Xは正方向 -> 被弾側へ反転可）
    [SerializeField] protected float knockbackForce = 8f; // インパルスの強さ

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            // --- シールドタックルでプレイヤーに接触 ---
            if (isAttackEnabled && collision.CompareTag("Player"))
            {
                Debug.Log("シールドタックルでプレイヤーに接触（SieldBlock_Base）");

                // プレイヤーの UnitBase を参照
                var targetUnit = collision.gameObject.GetComponent<UnitBase>() ?? collision.gameObject.GetComponentInParent<UnitBase>();

                if (targetUnit == null)
                {
                    Debug.LogError("Hit target has no UnitBase component");
                    return;
                }

                if (gurter == null)
                {
                    Debug.LogError("Gurter (owner) not found for SieldBlock_Base");
                    return;
                }

                // ガーダーの公開プロパティ（ScriptableObject の設定優先）から攻撃力を取得
                float basePower = 1f;
                try
                {
                    basePower = gurter != null ? gurter.MeleeAttackPower : 1f;
                }
                catch { basePower = 1f; }

                int damage = Mathf.Max(1, Mathf.CeilToInt(basePower * damageScale));

                // UnitManager を通してダメージを与える（安全なルート）
                UnitManager.instance.AddDamage(targetUnit, gurter, damage);

                // ノックバック適用（対象の Rigidbody2D にインパルス）
                try
                {
                    var rb = targetUnit.Rigidbody2D;
                    if (rb != null && knockbackForce != 0f)
                    {
                        // 押す方向は、ターゲットがガーダーのどちら側にいるかで反転
                        float side = targetUnit.transform.position.x >= gurter.transform.position.x ? 1f : -1f;
                        Vector2 kbDir = new Vector2(knockbackDirection.x * side, knockbackDirection.y).normalized;
                        rb.velocity = Vector2.zero;
                        rb.AddForce(kbDir * knockbackForce, ForceMode2D.Impulse);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"ノックバック適用に失敗: {ex.Message}");
                }

                // スタン付与（StatusEffectManager を経由）
                if (tackleStunDuration > 0f && StatusEffectHolder.instance != null && StatusEffectHolder.instance.Stun != null)
                {
                    // スタンのエフェクトファクトリを取得して追加
                    var stunEffect = StatusEffectHolder.instance.Stun.EffectFactory();
                    targetUnit.StatusEffectManager.AddEffect(stunEffect);
                }

                return;
            }

            // ここで、もし攻撃が有効になっているがガーダーが ShieldTackle 状態でないなら無効化する（安全策）
            if (isAttackEnabled && gurter != null)
            {
                // StateMachine のキーワード名は Enemy_Gurter の定義に合わせる
                var currentKey = gurter.StateMachine.CurrentState.key;
                if (currentKey != "ShieldTackle")
                {
                    DisableAttack();
                }
            }

            // 防御が無効なら何もせず弾を素通りさせる
            if (!isDefenseEnabled) return;

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
