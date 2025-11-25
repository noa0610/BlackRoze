using HighElixir;
using HighElixir.Timers;
using UnityEngine;
using UniRx;
using BlackRose.Core.Models.SearchSystems;
using DG.Tweening;

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

        protected override void OnDeath()
        {
            _fms.Send(AITriggers.dead);
        }

        // === UnityLifeCycle ===
        protected override void BeforeAwake()
        {
            _flippingUnit = GetComponent<AutoFlipHelper>();
            AutoFlipper.OnFlipped.Subscribe(flip =>
            {
                _searchEffects.GetComponent<RecursionFliper>().SetFlipRecursively(_searchEffects, flip.x < 0);
                if (GetComponent<SearchAssistanceMono>().TryGetProfile("LockShoot", out var profile))
                {
                    foreach (var comp in profile.comps)
                    {
                        if (comp.Comp is FilterByLookingForward look)
                        {
                            look.EyeAngleOffset = flip;
                        }
                    }
                }
            }).AddTo(this);
            TimerRegist();
            ModeRegist();
        }

        public override void Refresh()
        {
            base.Refresh();
            Animator.SetTrigger("Reset");
            ChangeMode(AIStates.Normal);
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            _fms.Update(Time.deltaTime);
            if (Mathf.Abs(MoveDirection.x) < 0.01f)
            {
                var v = Rigidbody2D.velocity;
                v.x = Mathf.MoveTowards(v.x, 0f, _horizontalDecel * Time.deltaTime);
                Rigidbody2D.velocity = v;
                if (Mathf.Abs(Rigidbody2D.velocity.x) < 0.01f)
                    _fms.Send(AITriggers.cancelMove);
            }
            else if (IsGrounded)
            {
                _fms.Send(AITriggers.moveInput);
            }
        }
    }
}