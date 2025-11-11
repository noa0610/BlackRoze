using BlackRose.Core.Models.SearchSystems;
using BlackRose.Datas.Definitions;
using HighElixir.Unity.Pools;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class SearchAndFire : MonoBehaviour
    {
        [Header("ターゲット検索")]
        [SerializeField] private SearchAssistanceMono _searchAssistanceMono;
        [SerializeField] private string _profile = "LockShoot";
        private readonly List<UnitBase> _targets = new();

        [Header("Missile")]
        [SerializeField] private TrackingMissile _prefab;
        [SerializeField] private Transform _container;
        private ObjectPool<TrackingMissile> _missilePool;

        [Header("Target Marker")]
        [SerializeField] private SpriteRenderer _markerPrefab;
        [SerializeField] private Transform _markerContainer;
        [SerializeField] private Vector3 _markerOffset = new(0f, 1.5f, 0f);
        private ObjectPool<SpriteRenderer> _markerPool;
        private readonly Dictionary<UnitBase, SpriteRenderer> _markerPair = new();

        private void Awake()
        {
            if (_searchAssistanceMono == null)
                _searchAssistanceMono = GetComponent<SearchAssistanceMono>();

            // ミサイルプール
            _missilePool = new ObjectPool<TrackingMissile>(_prefab, 10, _container);

            // マーカープール
            _markerPool = new ObjectPool<SpriteRenderer>(_markerPrefab, 10, _markerContainer);
        }

        /// <summary>
        /// 検索プロファイルに従ってターゲット一覧を更新し、マーカーを同期する
        /// </summary>
        public void Search()
        {
            if (_searchAssistanceMono == null) return;

            var allUnits = UnitManager.instance.GetUnitList();
            _targets.Clear();
            if (_searchAssistanceMono.Execute(_profile, allUnits, out var result))
                _targets.AddRange(result);

            SyncMarkers();
        }

        /// <summary>
        /// ターゲット選択解除（マーカーも全解除）
        /// </summary>
        public void Cancel()
        {
            _targets.Clear();
            ReleaseAllMarkers();
        }

        /// <summary>
        /// 登録済みのターゲットへ一斉発射
        /// </summary>
        public bool Shoot(Vector2 spawnPos, BulletData data, LayerMask targetLayer)
        {
            if (_targets.Count == 0) return false;

            foreach (var t in _targets)
            {
                if (t == null) continue;

                var m = _missilePool.Get();
                var tr = m.transform;
                tr.SetParent(_container, worldPositionStays: true);
                tr.position = spawnPos;

                // 弾の基本セット
                m.SetBulletStatus(data, targetLayer);
                m.SetParent(null); // 必要なら発射元 UnitBase をセット
                var dir = ((Vector2)t.Transform.position - spawnPos).normalized;
                m.SetDirection(dir);
                m.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

                // 破棄時にプールへ返す
                m.OnDestoryHandle -= OnMissileDestroyed; // 重複防止
                m.OnDestoryHandle += OnMissileDestroyed;

                // ターゲットを外から渡す（自力探索なし）
                m.SetTarget(t);

                // 発射
                m.gameObject.SetActive(true);
                m.Invoke();
            }
            Cancel();
            return true;
        }

        private void OnMissileDestroyed(Bullet m)
        {
            if (m == null) return;
            // プール返却前にイベントを外す（多重登録防止）
            m.OnDestoryHandle -= OnMissileDestroyed;
            _missilePool.Release(m as TrackingMissile);
        }

        /// <summary>
        /// ターゲットとマーカーの対応を同期
        /// </summary>
        private void SyncMarkers()
        {
            // 1) なくなったターゲットのマーカーを返却
            var toRelease = new List<UnitBase>();
            foreach (var kv in _markerPair)
            {
                if (kv.Key == null || !_targets.Contains(kv.Key))
                    toRelease.Add(kv.Key);
            }
            foreach (var dead in toRelease)
            {
                if (_markerPair.TryGetValue(dead, out var sr) && sr != null)
                {
                    sr.gameObject.SetActive(false);
                    _markerPool.Release(sr);
                }
                _markerPair.Remove(dead);
            }

            // 2) 新規ターゲットにマーカーを割り当て
            foreach (var t in _targets)
            {
                if (t == null || _markerPair.ContainsKey(t)) continue;

                var sr = _markerPool.Get();
                sr.transform.SetParent(_markerContainer, worldPositionStays: true);
                sr.gameObject.SetActive(true);
                _markerPair.Add(t, sr);
            }

            // 3) 位置更新（即時）
            UpdateMarkers();
        }

        private void UpdateMarkers()
        {
            foreach (var kv in _markerPair)
            {
                var t = kv.Key;
                var sr = kv.Value;
                if (t == null || sr == null) continue;

                var targetPos = t.Transform != null ? t.Transform.position : t.transform.position;
                sr.transform.position = targetPos + _markerOffset;
            }
        }

        private void ReleaseAllMarkers()
        {
            foreach (var sr in _markerPair.Values)
            {
                if (sr == null) continue;
                sr.gameObject.SetActive(false);
                _markerPool.Release(sr);
            }
            _markerPair.Clear();
        }

        private void Update()
        {
            if (_markerPair.Count > 0)
                UpdateMarkers();
        }
    }
}
