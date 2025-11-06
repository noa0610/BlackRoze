using AIE2D;
using BlackRose.Core.Models.Objects;
using BlackRose.Datas.Definitions;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{    public partial class AIController
    {
        // 各モードで使用するオブジェクト群
        [Header("Objects")]
        [SerializeField] private GameObject _preWarp;
        [SerializeField] private LayerMask _attackTarget;
        [SerializeField] private ReflectMono _reflectMono;

        // GetComponent
        private DynamicAfterImageEffect2DPlayer _dPlayer;
        private AutoFlipHelper _flippingUnit;

        public DynamicAfterImageEffect2DPlayer DynamicAfterImageEffect2D { get { return _dPlayer; } }
        public LayerMask AttackTarget => _attackTarget;
        public GameObject PreWarp => _preWarp;
        public ReflectMono ReflectMono => _reflectMono;
        public AutoFlipHelper AutoFlipper => _flippingUnit;
    }
}