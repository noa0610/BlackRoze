using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Melee,
    Projectile
}


[CreateAssetMenu(menuName = "Attack/AttackData")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public AttackType type;
    public float damage;
    public float knockback;
    public GameObject hitboxPrefab;         // 近接用のヒットボックス
    public GameObject projectilePrefab;     // 飛び道具
    public Vector2 offset;                  // 発生位置オフセット
    public float speed;                     // 飛び道具の速度など
}