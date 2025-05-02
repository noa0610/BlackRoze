using System;
using UnityEngine;

namespace Test
{
    // ユニットの状態を表す構造体（SerializableなのでUnityのInspectorでも見える！）
    [Serializable]
    public struct UnitStatus
    {
        public int id;
        public string name;
        public string description;
        public float hp;         // 体力
        public Vector2 direction; // 向き（2Dベクトル）
        public float speed;      // 移動速度
        public float speedInAir;
        public float jumpPower;
        public UnitTags tags;
    }

    // 弾の状態を表す構造体
    [Serializable]
    public struct BulletStatus
    {
        public float hp;          // 弾の耐久値（敵の弾が壊れるとか？）
        public float time;        // 存在時間（寿命）
        public float damage;      // ダメージ量
        public float speed;       // 弾速
        public Vector2 direction; // 飛ぶ方向
    }
}