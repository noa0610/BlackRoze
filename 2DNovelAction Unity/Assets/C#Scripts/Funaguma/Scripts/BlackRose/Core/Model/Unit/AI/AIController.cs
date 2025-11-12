using HighElixir;
using HighElixir.Timers;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    [RequireComponent(
        typeof(Rigidbody2D),
        typeof(UnityEngine.InputSystem.PlayerInput)
        )]
    public partial class AIController : GroundedUnit
    {
        [Header("Option Settings")]
        [SerializeField] private bool _canChargeCount = false;
        [SerializeField] private float _horizontalDecel = 20f;
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
            _flippingUnit = GetComponent<AutoFlipHelper>();
            TimerRegist();
            ModeRegist();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            _fms.Update(Time.deltaTime);
            if (Mathf.Abs(MoveDirection.x) < 0.01f && Rigidbody2D != null)
            {
                var v = Rigidbody2D.velocity;
                v.x = Mathf.MoveTowards(v.x, 0f, _horizontalDecel * Time.deltaTime);
                Rigidbody2D.velocity = v;
                if (Mathf.Abs(Rigidbody2D.velocity.x) < 0.01f)
                    _fms.Send(AITriggers.cancelMove);
            }
        }
    }
}