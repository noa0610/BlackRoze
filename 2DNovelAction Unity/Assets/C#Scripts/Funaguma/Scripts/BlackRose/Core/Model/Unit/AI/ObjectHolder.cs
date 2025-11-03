using AIE2D;
using BlackRose.Core.Models.Objects;
using BlackRose.Datas.Definitions;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{    public partial class AIController
    {
        // 各モードで使用するオブジェクト群
        [Header("Objects")]
        [SerializeField] private GameObject _muzzle;
        [SerializeField] private GameObject _PreWarp;
        [SerializeField] private LayerMask _attackTarget;
        [SerializeField] private ReflectMono _reflectMono;
        [SerializeField] private Rigidbody2D _2d;
        [SerializeField] private BulletData _lazer;

        // GetComponent
        private DynamicAfterImageEffect2DPlayer _dPlayer;
        private AutoFlipHelper _flippingUnit;

        public DynamicAfterImageEffect2DPlayer DynamicAfterImageEffect2D { get { return _dPlayer; } }
        public LayerMask AttackTarget => _attackTarget;
        public GameObject Muzzle => _muzzle;
        public GameObject PreWarp => _PreWarp;
        public ReflectMono ReflectMono => _reflectMono;
        public Rigidbody2D Rigidbody2D => _2d;
        public AutoFlipHelper AutoFlipper => _flippingUnit;
    }
}