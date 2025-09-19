using BlackRose.Core.Models.Units;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Objects
{
    /// <summary>
    /// <see cref="DamageFloorMaker">DamageFloorMaker</see>をシーンに配置し、このスクリプトをアタッチしたぷれふぁぶをセットする。<br/>
    /// その後、<see cref="DamageFloorMaker.Create(Vector2, float, float)">Create</see>を呼び出してください
    /// </summary>
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public class DamageFloor : MonoBehaviour
    {
        [Header("Floor Settings")]
        [SerializeField] private float _step = 0.2f;         // サンプリング間隔
        [SerializeField] private LayerMask _groundLayer;     // 地形のレイヤー
        [SerializeField] private float _width = 0.2f;
        [SerializeField] private Color _color = Color.red;
        [SerializeField] private float _damage = 1;

        private LineRenderer _line;
        private EdgeCollider2D _collider2D;


        /// <summary>
        /// 起点を指定して床を生成
        /// </summary>
        public void Generate(Vector2 start, float maxLength)
        {
            Queue<(Vector2 pos, bool isLeft)> research = new();
            void Push(Vector2 current, bool isLeft)
            {
                if (isLeft)
                {
                    var left = current + Vector2.left * _step;
                    if (Vector2.Distance(start, left) < maxLength)
                        research.Enqueue((left, true));

                }
                else
                {
                    var right = current + Vector2.right * _step;
                    if (Vector2.Distance(start, right) < maxLength)
                        research.Enqueue((right, false));
                }

            }
            var points = new List<Vector3>();
            research.Enqueue((start, true));
            research.Enqueue((start + Vector2.right * _step, false));

            float traveled = 0f;
            points.Add(start);
            while (traveled < maxLength)
            {
                var next = research.Dequeue();
                //地形に沿わせる：下方向にRaycastを打って「地表」を探す
                RaycastHit2D hit = Physics2D.Raycast(next.pos + Vector2.up * 2f, Vector2.down, 5f, _groundLayer);
                if (hit.collider != null)
                {
                    points.Add(hit.point);
                    Push(hit.point, next.isLeft);
                }
                else
                {
                    // 地面がなくなったら終了
                    break;
                }
                traveled += _step;
            }
            _line.positionCount = points.Count;
            points.Sort((a, b) => a.x.CompareTo(b.x));
            _line.SetPositions(points.ToArray());
            _collider2D.SetPoints(points.ConvertAll(p => (Vector2)(p - transform.position)));
        }

        private void OnTriggerEnter2D(Collider2D collision)
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
            //_line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = _color;
            _line.endColor = _color;
            _line.useWorldSpace = true;

            _collider2D = GetComponent<EdgeCollider2D>();
        }
    }
}
