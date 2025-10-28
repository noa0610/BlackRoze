using AIE2D;
using HighElixir.Timers;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(
        typeof(Rigidbody2D),
        typeof(UnityEngine.InputSystem.PlayerInput),
        typeof(DynamicAfterImageEffect2DPlayer)
        )]
    public partial class AIController : GroundedUnit
    {
        [Header("Option Settings")]
        [SerializeField] private bool _canChargeCount = false;

        private Vector2 _shootDirection = Vector2.right;
        // ===== モード関連 =====
        [Header("Mode")]
        [SerializeField] private NormalMode _normalMode;
        [SerializeField] private LightMode _lightMode;
        [SerializeField] private HeavyMode _heavyMode;
        private TimerTicket _blockFlip;

        public bool ShouldBeBlockFlip => Timer.TryGetCurrentTime(_blockFlip, out var f) && f > 0 && CurrentMode is HeavyMode;
        public bool CanJump => !Timer.IsFinished(_coyoteTicket);

        // === UnityLifeCycle ===
        protected override void BeforeAwake()
        {
            TimerRegist();
            ModeRegist();
        }
    }
}