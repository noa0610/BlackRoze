using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    [CreateAssetMenu(menuName = "BlackRose/UnitStatus")]
    public class UnitStatusData : ScriptableObject
    {
        public int id;
        public string unitName;
        public string description;
        public float maxHp;
        public float speed;         // 移動速度
        public float speedInAir;    // 空中での移動速度
        public float dashSpeed;      // ダッシュ速度
        public float power;         // 技の威力がスケール
        public float jumpPower;
        public float damageTakeScale = 1f; // ダメージをスケールするための値（1.0fならスケールなし）
        public UnitTags tags;
    }


}
// unicode