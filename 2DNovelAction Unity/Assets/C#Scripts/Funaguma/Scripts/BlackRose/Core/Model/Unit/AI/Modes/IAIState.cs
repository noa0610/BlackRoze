
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlackRose.Core.Models.Units
{
    public interface IAIState
    {
        UnitStatusData StatusData { get; }
        void Register();
        void Bind(AIController parent);
        //
        void OnGrounded();

        // === Input Action ===
        void OnShoot(InputValue value);
        void OnReleaseShoot(float chargeTime);
        void OnJump(InputValue value);
        void CanceldJump(InputValue value);
        void OnSkill(InputValue value);

        // Unity Lifecycle
        void Update(float deltaTime);
        void FixedUpdate(float deltaTime);

    }
}