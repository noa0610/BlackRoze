using System;

namespace BlackRose.Datas.Definitions
{

    // 弾の状態を表す構造体
    [Serializable]
    public struct BulletStatus
    {
        public float hp;          // 弾の耐久値（敵の弾が壊れるとか？）
        public float time;        // 存在時間（寿命）
        public float damage;      // ダメージ量
        public float speed;       // 弾速
    }
}