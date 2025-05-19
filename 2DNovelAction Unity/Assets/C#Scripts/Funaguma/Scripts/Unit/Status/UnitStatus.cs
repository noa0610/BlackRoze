using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    // unitStatus構造体からスクリプタブルオブジェクトに移行作業中
    [CreateAssetMenu(menuName = "BlackRose/UnitStatus")]
    public class UnitStatusData : ScriptableObject
    {
        public int id;
        public string unitName;
        public string description;
        public float maxHp;
        public float speed;         // 移動速度
        public float speedInAir;
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