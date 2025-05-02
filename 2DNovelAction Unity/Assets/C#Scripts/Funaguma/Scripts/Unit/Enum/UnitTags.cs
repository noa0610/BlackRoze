using System;

namespace Test
{
    [Flags]
    public enum UnitTags
    {
        None = 0,
        Player = 1 << 0,
        Enemy = 1 << 1,
    }
}