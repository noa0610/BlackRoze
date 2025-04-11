using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController
{
    private int currentHealthPoint; // ヒットポイント
    private int maxHealthPoint; // 最大ヒットポイント
    private int defense; // 防御力
    private float damageMultiplef; // 被弾ダメージ倍率
    private bool isInvincible; // 無敵フラグ
    private bool isDead; // 死亡フラグ

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="maxHP"></param>
    /// <param name="damageMultiplef"></param>
    public void Init(int maxHP, float damageMultiplef = 1.0f, int Defense = 0)
    {
        this.maxHealthPoint = maxHP; // 最大ヒットポイントを設定
        this.currentHealthPoint = maxHP; // ヒットポイントを最大値で初期化
        this.defense = Defense; // 防御力を設定
        this.damageMultiplef = damageMultiplef; // 被弾ダメージ倍率を設定
    }

    /// <summary>
    /// ダメージを受けたときの処理
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        currentHealthPoint -= (int)(damage * damageMultiplef - defense); // ヒットポイントを減少させる
        if (currentHealthPoint < 0) currentHealthPoint = 0; // ヒットポイントが0未満にならないようにする
        if (currentHealthPoint == 0)
        {
            // ヒットポイントが0になったときの処理
            DeadHealth();
        }
    }
    
    /// <summary>
    /// 死亡時の処理
    /// </summary>
    public void DeadHealth()
    {
        isDead = true; // 死亡フラグを立てる
    }
}
