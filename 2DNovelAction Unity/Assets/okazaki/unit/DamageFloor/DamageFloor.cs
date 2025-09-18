using BlackRose.Core.Models.Units;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Objects
{
    [RequireComponent(typeof(LineRenderer))]
    public class DamageFloor : MonoBehaviour
    {
        [Header("Floor Settings")]
        [SerializeField] private float _maxLength = 5f;      // 最大長さ
        [SerializeField] private float _step = 0.2f;         // サンプリング間隔
        [SerializeField] private LayerMask _groundLayer;     // 地形のレイヤー
        [SerializeField] private float _width = 0.2f;
        [SerializeField] private Color _color = Color.red;
        [SerializeField] private float _damage = 1;

        private LineRenderer _line;


        /// <summary>
        /// 起点と進行方向を指定して床を生成
        /// </summary>
        public void Generate(Vector2 start)
        {
            var leftPoints = new List<Vector2>();
            var rightPoints = new List<Vector2>();

            // 起点を地面にスナップ
            RaycastHit2D startHit = Physics2D.Raycast(start + Vector2.up * 2f, Vector2.down, 5f, _groundLayer);
            if (startHit.collider == null) return;

            Vector2 center = startHit.point;
            leftPoints.Add(center);
            rightPoints.Add(center);

            // 左方向に探索
            Vector2 current = center;
            while (Vector2.Distance(center, current) < _maxLength)
            {
                Vector2 next = current + Vector2.left * _step;
                RaycastHit2D hit = Physics2D.Raycast(next + Vector2.up * 2f, Vector2.down, 5f, _groundLayer);
                if (hit.collider == null) break;
                current = hit.point;
                leftPoints.Add(current);
            }

            // 右方向に探索
            current = center;
            while (Vector2.Distance(center, current) < _maxLength)
            {
                Vector2 next = current + Vector2.right * _step;
                RaycastHit2D hit = Physics2D.Raycast(next + Vector2.up * 2f, Vector2.down, 5f, _groundLayer);
                if (hit.collider == null) break;
                current = hit.point;
                rightPoints.Add(current);
            }

            // 左を逆順に → 中心 → 右 の順番でつなぐ
            leftPoints.Reverse();
            leftPoints.AddRange(rightPoints);

            _line.positionCount = leftPoints.Count;
            _line.SetPositions(leftPoints.ConvertAll(p => (Vector3)p).ToArray());
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent<UnitBase>(out var unit))
            {
                UnitManager.instance.AddDamage(unit, new ObjectDamageWorker(), _damage);
            }
        }
        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.startWidth = _width;
            _line.endWidth = _width;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = _color;
            _line.endColor = _color;
            _line.useWorldSpace = true;
        }
    }
}
