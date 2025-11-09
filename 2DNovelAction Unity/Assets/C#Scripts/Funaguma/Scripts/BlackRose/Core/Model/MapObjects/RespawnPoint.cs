using BlackRose.Core.Models.Systems;
using BlackRose.Core.Models.Units;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace BlackRose.Core.Models.Objects
{
    [DefaultExecutionOrder(-3)]
    public class RespawnPoint : MonoBehaviour, IPlayerFollower
    {
        [SerializeField, ReadOnly] public static List<RespawnPoint> points = new();
        [SerializeField] private bool _enable = true;
        [SerializeField] private bool _isStart = false; // スタート地点かどうか
        private UnitBase _target;
#if UNITY_EDITOR
        [SerializeField] private int count = 0; // スタート地点のカウント
#endif
        public void SetEnable(bool enable)
        {
            _enable = enable;
        }

        private void SetStart()
        {
            foreach (var point in points)
            {
                if (point.Equals(this))
                    continue;
                point._isStart = false;
            }
            PlayerSpawnner.SetRespawnPoint(this);
        }
        // === Unity Lifecycle ===
        private void Awake()
        {
            if (!points.Contains(this)) points.Add(this);
            if (_isStart)
            {
                _enable = false; // スタート地点は初期化時に無効化
                SetStart();
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_enable && collision.gameObject.Equals(_target.gameObject))
            {
                PlayerSpawnner.SetRespawnPoint(this);
                SetEnable(false);
                Debug.Log("SetSpawnPoint");
            }
        }

        private void OnDestroy()
        {
            points.Remove(this);
        }
#if UNITY_EDITOR
        private void Reset()
        {
            if (!points.Contains(this)) points.Add(this);
            count = points.Count; // カウントを更新
            _enable = true;
            if (_isStart) SetStart(); // スタート地点なら初期化時に設定
        }
        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy) return;
            if (!points.Contains(this)) points.Add(this);
            if (_isStart) SetStart();
        }

        public void SetTarget(UnitBase target)
        {
            _target = target;
        }
#endif
    }
}
