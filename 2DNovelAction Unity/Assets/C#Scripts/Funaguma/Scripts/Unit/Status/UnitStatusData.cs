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
        public UnitTags tags;
        public List<OptionStatus> options;
    }

    [Serializable]
    public struct OptionStatus
    {
        public Status status;
        public float amount;
    }
}
// unicode