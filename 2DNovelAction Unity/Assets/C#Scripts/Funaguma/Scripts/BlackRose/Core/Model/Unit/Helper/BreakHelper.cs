using Cysharp.Threading.Tasks;
using HighElixir;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace BlackRose.Core.Models.Units.Helpers
{
    // 親オブジェクトにアタッチして使用する
    public class BreakHelper : MonoBehaviour
    {
        [Tooltip("処理によって無視するオブジェクトのタグ")]
        [SerializeField] private string _throughTag;
        [Tooltip("ThroughTagを反転するかどうか")]
        [SerializeField] private bool _reverse = false;
        [SerializeField] private float _duration = 3f;
        [SerializeField] private float _minPow = 1f;
        [SerializeField] private float _maxPow = 1f;
        [SerializeField] private bool _addSelf = true;
        public async UniTask InvokeBreak()
        {
            var targets = GetObjects();
            if (_addSelf) targets.Add(gameObject);
            if (_minPow > _maxPow)
            {
                var tmp = _minPow;
                _minPow = _maxPow;
                _maxPow = tmp;
            }
            foreach (var target in targets)
            {
                if (!target.TryGetComponent(out Rigidbody2D rb2)) rb2 = target.AddComponent<Rigidbody2D>();
                rb2.isKinematic = true;
                Vector2 forcePower = new Vector2(RandomExtensions.Rand(_minPow, _maxPow), RandomExtensions.Rand(_minPow, _maxPow));
                float torquePower = RandomExtensions.Rand(_minPow, _maxPow);

                // パーツをふっとばす！
                rb2.isKinematic = false;
                rb2.AddForce(forcePower, ForceMode2D.Impulse);
                rb2.AddTorque(torquePower, ForceMode2D.Impulse);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_duration));
            foreach (var target in targets)
            {
                try
                {
                    if (target.TryGetComponent<SpriteRenderer>(out var sp))
                    {
                        sp.DOFade(0, 2.2f).OnComplete(() => Destroy(target));
                    }
                    else
                        Destroy(target);
                }
                catch { } // 握りつぶし
            }
        }

        private List<GameObject> GetObjects()
        {
            Queue<Transform> queue = new();
            queue.Enqueue(transform);
            List<GameObject> objs = new List<GameObject>();
            while (true)
            {
                if (!queue.TryDequeue(out var tr)) break;
                for (int i = 0; i < tr.childCount; i++)
                {
                    var child = tr.GetChild(i);
                    var flg = !child.gameObject.CompareTag(_throughTag);
                    if (_reverse) flg = !flg;
                    if (flg)
                    {
                        objs.Add(child.gameObject);
                        if (child.childCount > 0)
                            queue.Enqueue(child);
                    }
                }
            }
            return objs;
        }
    }
}