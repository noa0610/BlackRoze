using System.Collections.Generic;

namespace BlackRose
{
    public interface IAIState
    {
        List<IState> Register();

        // === Input Action ===
        string OnShoot();
        string OnMove();
        string OnJump();
        string OnDash();
        string OnSkill();
    }
}