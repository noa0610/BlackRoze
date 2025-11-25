using BlackRose.Core.Models.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlackRose.Core.Models.SearchSystems
{
    [Serializable]
    public class FilterByLookingForward : IFilterComponent
    {
        [Header("視野設定")]
        [SerializeField] private Transform _eye;
        [SerializeField] private float _distance = 10f;
        [SerializeField] private float _verticalView = 60f;
        [SerializeField] private float _horizontalView = 90f;
        [SerializeField] private LayerMask _layerMask;

        [Header("視点設定")]
        [SerializeField] private Vector3 _eyeOffset = Vector3.zero;
        [SerializeField] private Vector3 _eyeAngleOffset = Vector3.zero;

        [Header("オプション")]
        [SerializeField] private bool _reversing = false;
        [SerializeField] private float _deadZone = 0f;
        [SerializeField] private bool _is2D = false;

        public float Distance { get => _distance; set => _distance = value; }
        public float VerticalView { get => _verticalView; set => _verticalView = value; }
        public float HorizontalView { get => _horizontalView; set => _horizontalView = value; }
        public LayerMask LayerMask { get => _layerMask; set => _layerMask = value; }
        public Vector3 EyeOffset { get => _eyeOffset; set => _eyeOffset = value; }
        public Vector3 EyeAngleOffset { get => _eyeAngleOffset; set => _eyeAngleOffset = value; }
        public bool Reversing { get => _reversing; set => _reversing = value; }
        public float DeadZone { get => _deadZone; set => _deadZone = value; }
        public bool Is2D { get => _is2D; set => _is2D = value; }

        public FilterByLookingForward()
        {
        }

        public FilterByLookingForward(Transform eye, float distance, float verticalView, float horizontalView, LayerMask layerMask, bool is2D = false)
        {
            _eye = eye;
            _distance = distance;
            _verticalView = verticalView;
            _horizontalView = horizontalView;
            _is2D = is2D;
            _layerMask = layerMask;
        }

        public void Dispose() { }

        public void Initialize() { }

        public List<UnitBase> Execute(List<UnitBase> pool)
        {
            var eyePos3 = _eye.position + _eyeOffset;
            // Apply angle offset to the eye rotation so EyeAngleOffset affects forward direction
            var baseRotation = _eye.rotation * Quaternion.Euler(_eyeAngleOffset);
            var forward3D = baseRotation * Vector3.forward;
            var copy = new List<UnitBase>(pool);
            if (_is2D)
            {
                // 2Dモード：計算の基準を Z 回転（eulerAngles.z）にして確実に2D前方を取得
                var eyePos2 = (Vector2)eyePos3;
                float rotZ = _eye.eulerAngles.z + _eyeAngleOffset.z;
                var rad = rotZ * Mathf.Deg2Rad;
                var forward2D = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                // Get colliders in radius, but final visibility uses angle + raycast
                var cols = Physics2D.OverlapCircleAll(eyePos2, _distance, _layerMask);
                FilterTargets2D(eyePos2, forward2D, cols, ref copy);
            }
            else
            {
                // 3Dモード
                var hits = Physics.SphereCastAll(eyePos3, 0.1f, forward3D, _distance, _layerMask);
                FilterTargets3D(eyePos3, forward3D, hits, ref copy);
            }
            return copy;
        }

        private void FilterTargets2D(Vector2 eyePos, Vector2 forward, Collider2D[] cols, ref List<UnitBase> targets)
        {
            // We no longer rely solely on overlap results; use them only as potential obstacles list.
            var potentialObstacles = cols.Select(h => h.gameObject).ToList();

            targets.RemoveAll(target =>
            {
                if (target == null) return true;
                var targetPos = (Vector2)target.transform.position;
                Vector2 dir = (targetPos - eyePos).normalized;

                // DeadZoneチェック（先に）
                if (_deadZone > 0 && Vector2.Distance(eyePos, targetPos) <= _deadZone)
                    return !_reversing;

                // 視野角チェック
                float angle = Vector2.Angle(forward, dir);
                if (angle > _horizontalView * 0.5f)
                    return !_reversing;

                // Raycastで実際に見えてるかチェック（目からターゲットまでの最短ヒットがターゲット自身であるか）
                float dist = Vector2.Distance(eyePos, targetPos);
                var hit = Physics2D.Raycast(eyePos, dir, dist, _layerMask);
                bool visible = false;
                if (hit.collider != null)
                {
                    var hitRoot = hit.collider.transform.root.gameObject;
                    visible = (hit.collider.gameObject == target.gameObject) || hit.collider.transform.IsChildOf(target.transform) || hitRoot == target.gameObject || hitRoot.transform.IsChildOf(target.transform);
                }

                return _reversing ? visible : !visible;
            });
        }

        private void FilterTargets3D(Vector3 eyePos, Vector3 forward, RaycastHit[] hits, ref List<UnitBase> targets)
        {
            var visibleObjects = hits.Select(h => h.collider.gameObject).ToList();

            targets.RemoveAll(target =>
            {
                if (target == null) return true;
                Vector3 dir = (target.transform.position - eyePos).normalized;

                // 水平方向・垂直方向の視野角チェック
                float horizontalAngle = Vector3.Angle(Vector3.ProjectOnPlane(forward, Vector3.up), Vector3.ProjectOnPlane(dir, Vector3.up));
                float verticalAngle = Vector3.Angle(forward, dir);

                if (horizontalAngle > _horizontalView * 0.5f || verticalAngle > _verticalView * 0.5f)
                    return !_reversing;

                // DeadZoneチェック
                if (_deadZone > 0 && Vector3.Distance(eyePos, target.transform.position) <= _deadZone)
                    return !_reversing;

                // Raycastで実際に見えてるかチェック
                bool visible = visibleObjects.Contains(target.gameObject);
                return _reversing ? visible : !visible;
            });
        }

    }

    internal static class VectorExtensions
    {
        public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.x, v.y);
    }
}
