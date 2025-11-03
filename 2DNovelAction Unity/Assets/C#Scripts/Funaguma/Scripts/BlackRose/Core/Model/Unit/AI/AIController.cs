using AIE2D;
using BlackRose.Core.Models.Objects;
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
        [SerializeField]private Vector2 _shootDirection = Vector2.right;

        public Vector2 ShootDir => _shootDirection;
        public bool ShouldBeBlockFlip => !_flippingUnit.Enable && CurrentMode is HeavyMode;
        public bool CanJump => !Timer.IsFinished(_coyoteTicket);

        // ===== モード関連 =====
        [Header("Mode")]
        [SerializeField] private NormalMode _normalMode;
        [SerializeField] private LightMode _lightMode;
        [SerializeField] private HeavyMode _heavyMode;

        // === UnityLifeCycle ===
        protected override void BeforeAwake()
        {
            _dPlayer = GetComponent<DynamicAfterImageEffect2DPlayer>();
            _2d = GetComponent<Rigidbody2D>();
            _flippingUnit = GetComponent<AutoFlipHelper>();
            TimerRegist();
            ModeRegist();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            _fms.Update(Time.deltaTime);
        }
    }
}