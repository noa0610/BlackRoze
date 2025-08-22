using System;
using UnityEngine;

namespace BlackRose.Datas.Definitions
{
    // ユニットの状態を表す構造体（SerializableなのでUnityのInspectorでも見える！）
    [Serializable]
    public struct UnitStatus
    {
        public int id;
        public string name;
        public string description;
        public float maxHp;
        public float hp;         // 体力
        public Vector2 direction; // 向き（2Dベクトル）
        public float speed;      // 移動速度
        public float speedInAir;
        public float jumpPower;
        public UnitTags tags;

        public UnitStatus(int id, string name, string description, float maxHp, float hp, Vector2 direction, float speed, float speedInAir, float jumpPower, UnitTags tags)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.maxHp = maxHp;
            this.hp = hp;
            this.direction = new Vector2(1, 0);
            this.speed = speed;
            this.speedInAir = speedInAir;
            this.jumpPower = jumpPower;
            this.tags = tags;
        }
    }
}