using System;

namespace BlackRose
{
    // 状態をビットフラグで表す列挙型（複数状態を持てるように[Flags]付き）
    [Flags]
    public enum StateFlags
    {
        None = 0,            // 状態なし
        InMove = 1 << 0,     // 移動中
        InFall = 1 << 1,     // 落下中
        InShoot = 1 << 2,    // 射撃中
        InJump = 1 << 3,     // ジャンプ
        // もっとビットを増やせば複数のフラグ管理できる！
    }
}