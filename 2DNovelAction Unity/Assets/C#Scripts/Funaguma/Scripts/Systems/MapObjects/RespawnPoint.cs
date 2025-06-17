using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core
{
    public class RespawnPoint : MonoBehaviour
    {
        public static RespawnPoint current;
        public static List<RespawnPoint> points = new();
        [SerializeField] private bool _enable = true;
        [SerializeField] private string _tag = "Player";
        [SerializeField] private bool _isStart = false; // スタート地点かどうか
        public void SetEnable(bool enable)
        {
            _enable = enable;
        }

#if UNITY_EDITOR
        public void SetFlag()
        {
            _isStart = false ;
        }
#endif
        // === Unity Lifecycle ===
        private void Start()
        {
            if (_isStart) current = this;
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_enable && collision.gameObject.tag == _tag)
            {
                current = this;
                SetEnable(false);
                Debug.Log("SetSpawnPoint");
            }
        }

        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy) return;
            if (!points.Contains(this))
                points.Add(this);
            if (_isStart)
            {
                foreach(var point in points)
                {
                    if (point.Equals(this))
                        continue;
                    point.SetFlag();
                }
                current = this;
            }
        }
        private void OnDestroy()
        {
            points.Remove(this);
        }
    }
}