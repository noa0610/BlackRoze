
namespace BlackRose.Core.Models.Units
{
    public interface IAIState
    {
        void Register();

        // === Input Action ===
        string OnShoot();
        string OnMove();
        string OnJump();
        string OnDash();
        string OnSkill();
    }
}