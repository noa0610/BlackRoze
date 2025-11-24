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
        [SerializeField] private ReflectMono _reflectMono;
        [SerializeField] private TrailRenderer _trailRenderer;

        private AutoFlipHelper _flippingUnit;

        public GameObject PreWarp => _preWarp;
        public ReflectMono ReflectMono => _reflectMono;
        public AutoFlipHelper AutoFlipper => _flippingUnit;
        public TrailRenderer TrailRenderer => _trailRenderer;
    }
}