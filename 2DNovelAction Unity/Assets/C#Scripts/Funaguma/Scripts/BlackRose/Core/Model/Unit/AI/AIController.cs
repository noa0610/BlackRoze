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
        private Vector2 _shootDirection = Vector2.right;

        public Vector2 ShootDir => _shootDirection;
        public bool ShouldBeBlockFlip => Timer.TryGetCurrentTime(_blockFlip, out var f) && f > 0 && CurrentMode is HeavyMode;
        public bool CanJump => !Timer.IsFinished(_coyoteTicket);

        // ===== モード関連 =====
        [Header("Mode")]
        [SerializeField] private NormalMode _normalMode;
        [SerializeField] private LightMode _lightMode;
        [SerializeField] private HeavyMode _heavyMode;
        private TimerTicket _blockFlip;

        // 各モードで使用するオブジェクト群
        [Header("Objects")]
        [SerializeField] private GameObject _muzzle;
        [SerializeField] private LayerMask _attackTarget;
        [SerializeField] private ReflectMono _reflectMono;
        [SerializeField] private Rigidbody2D _2d;
        private DynamicAfterImageEffect2DPlayer _dPlayer;

        public LayerMask AttackTarget => _attackTarget;
        public GameObject Muzzle => _muzzle;
        public ReflectMono ReflectMono => _reflectMono;
        public Rigidbody2D Rigidbody2D => _2d;

        // === UnityLifeCycle ===
        protected override void BeforeAwake()
        {
            _dPlayer = GetComponent<DynamicAfterImageEffect2DPlayer>();
            _2d = GetComponent<Rigidbody2D>();
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