using System.Collections.Generic;
using System.Data;
using Unity.Collections;
using UnityEngine;

namespace BlackRose.Core
{
    [DefaultExecutionOrder(-3)]
    public class RespawnPoint : MonoBehaviour
    {
        [SerializeField, ReadOnly] public static List<RespawnPoint> points = new();
        [SerializeField] private bool _enable = true;
        [SerializeField] private string _tag = "Player";
        public bool isStart = false; // スタート地点かどうか
        public int count = 0; // スタート地点のカウント
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
                point.isStart = false;
            }
            PlayerSpawnner.instance.SetRespawnPoint(this);
        }
        // === Unity Lifecycle ===
        private void Awake()
        {
            if (isStart)
            {
                _enable = false; // スタート地点は初期化時に無効化
                if (PlayerSpawnner.instance != null)
                    PlayerSpawnner.instance.SetRespawnPoint(this);
                else
                {
                    Debug.LogWarning("PlayerSpawnner instance is not set. Please ensure PlayerSpawnner is initialized before using RespawnPoint.");
                }
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_enable && collision.gameObject.CompareTag(_tag))
            {
                PlayerSpawnner.instance.SetRespawnPoint(this);
                SetEnable(false);
                Debug.Log("SetSpawnPoint");
            }
        }

        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy) return;
            if (!points.Contains(this))
                points.Add(this);
            if (isStart)
            {
                SetStart();
            }
        }
        private void OnDestroy()
        {
            points.Remove(this);
        }

        private void Reset()
        {
            if (!points.Contains(this))
                points.Add(this);
            count = points.Count; // カウントを更新
            _enable = true;
            _tag = "Player";
            if (isStart) SetStart(); // スタート地点なら初期化時に設定
        }
    }
}
