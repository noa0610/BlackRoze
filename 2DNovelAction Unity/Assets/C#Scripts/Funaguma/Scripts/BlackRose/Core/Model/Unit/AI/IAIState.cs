
using UnityEngine.InputSystem;

namespace BlackRose.Core.Models.Units
{
    public interface IAIState
    {
        UnitStatusData StatusData { get; }
        bool CanJump { get; }
        void Register();
        void Bind(AIController parent);

        //
        void OnGrounded();

        // === Input Action ===
        void OnShoot(InputValue value);
        void OnReleaseShoot(InputValue value);
        void OnJump(InputValue value);
        void OnDash(InputValue value);
        void OnSkill(InputValue value);
    }
}