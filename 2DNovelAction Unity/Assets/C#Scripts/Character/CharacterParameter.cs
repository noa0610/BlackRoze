using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAbility", menuName = "ScriptableObjects/CharacterAbility")]
public class CharacterParameter : ScriptableObject
{
    public string CharacterName;            // キャラクター名
    public string CharacterDescription;     // キャラクターの説明
    public int MaxHP = 1;                   // ヒットポイント
    public int Attack = 1;                  // 攻撃力
    public int Defense = 0;                 // 防御力
    public float MoveSpeedMultiplef = 1.0f; // 移動速度倍率
    public float DamageMultiplef = 1.0f;   // 被弾ダメージ倍率
}
