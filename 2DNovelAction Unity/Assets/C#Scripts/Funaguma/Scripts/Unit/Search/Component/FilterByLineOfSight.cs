using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose
{
    [Serializable]
    public class FilterByLineOfSight : IFilterComponent
    {
        [SerializeField, Tooltip("検出の基準になるトランスフォーム")]
        private Transform _viewpoint;
        [SerializeField, Tooltip("検出の距離")]
        private float _maxDistance;
        [SerializeField, Tooltip("衝突を検出可能なレイヤーマスク")]
        private LayerMask _obstructionMask;
        private List<UnitBase> _unitList = new List<UnitBase>();
        private RaycastHit[] _hitBuffer = new RaycastHit[1];

        public FilterByLineOfSight(Transform viewpoint, float maxDistance, LayerMask obstructionMask)
        {
            _viewpoint = viewpoint;
            _maxDistance = maxDistance;
            _obstructionMask = obstructionMask;
            _unitList = new List<UnitBase>();
        }
        public FilterByLineOfSight() { }
        public List<UnitBase> Execute(List<UnitBase> pool)
        {
            _unitList.Clear();

            float maxDistSqr = _maxDistance * _maxDistance;
            foreach (var unit in pool)
            {
                Vector3 dir = unit.Transform.position - _viewpoint.position;
                float distSqr = dir.sqrMagnitude;

                // 距離外ならスキップ
                if (distSqr > maxDistSqr)
                    continue;

                // ノーマライズ（√は1度だけ）
                float dist = Mathf.Sqrt(distSqr);
                Vector3 dirNorm = dir / dist;

                // NonAlloc版Raycast：バッファにヒット数が返る
                int hitCount = Physics.RaycastNonAlloc(
                    _viewpoint.position,
                    dirNorm,
                    _hitBuffer,
                    _maxDistance,
                    _obstructionMask);

                // ヒット0なら見通しOK！
                if (hitCount == 0)
                    _unitList.Add(unit);
            }

            return _unitList;
        }
    }
}