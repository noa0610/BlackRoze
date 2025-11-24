using BlackRose.Core.Models.Systems;
using BlackRose.Core.Models.Units;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace BlackRose.Core.Models.Objects
{
    [DefaultExecutionOrder(-3)]
    public class RespawnPoint : MonoBehaviour, IPlayerFollower
    {
        [SerializeField, ReadOnly] public static List<RespawnPoint> points = new();
        [SerializeField] private bool _enable = true;
        [SerializeField] private bool _isStart = false; // スタート地点かどうか
        private UnitBase _target;

        public bool IsStart => _isStart;
#if UNITY_EDITOR
        [SerializeField] private int count = 0; // スタート地点のカウント
    #endif

        public void SetEnable(bool enable)
        {
            _enable = enable;
        }

        private void SetStart()
        {
            // Clear other start flags in the same scene
            var all = FindObjectsOfType<RespawnPoint>(true);
            foreach (var point in all)
            {
                if (point == this) continue;
                if (point.gameObject.scene != this.gameObject.scene) continue;
                if (point._isStart)
                {
                    point._isStart = false;
    #if UNITY_EDITOR
                    EditorUtility.SetDirty(point);
                    EditorSceneManager.MarkSceneDirty(point.gameObject.scene);
    #endif
                }
            }

            // At runtime or editor, ensure PlayerSpawnner exists and set this as current respawn point
            var mgr = EnsurePlayerSpawner();
            if (mgr != null)
            {
                mgr.SetRespawnPoint(this);
            }
        }

        // Ensure a PlayerSpawnner exists in the scene; create one if missing
        private PlayerSpawnner EnsurePlayerSpawner()
        {
    #if UNITY_EDITOR
            // Try instance first
            var mgr = PlayerSpawnner.instance;
            if (mgr != null) return mgr;

            // Find existing in scene
            mgr = FindObjectOfType<PlayerSpawnner>(true);
            if (mgr != null) return mgr;

            // Create new GameObject with PlayerSpawnner
            var go = new GameObject("PlayerSpawnner");
            Undo.RegisterCreatedObjectUndo(go, "Create PlayerSpawnner");
            mgr = go.AddComponent<PlayerSpawnner>();
            EditorUtility.SetDirty(mgr);
            EditorSceneManager.MarkSceneDirty(go.scene);
            return mgr;
    #else
            // Runtime: if instance exists return it, otherwise create a GameObject and add component
            var mgr = PlayerSpawnner.instance;
            if (mgr != null) return mgr;
            var go = new GameObject("PlayerSpawnner");
            mgr = go.AddComponent<PlayerSpawnner>();
            return mgr;
    #endif
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
            if (!_enable) return;
            if (_target == null) return;
            if (collision.gameObject.Equals(_target.gameObject))
            {
                var mgr = EnsurePlayerSpawner();
                if (mgr != null) mgr.SetRespawnPoint(this);
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

            // When _isStart is set in the inspector, clear others in the same scene and mark scene dirty
            if (_isStart)
            {
                SetStart();
                if (Application.isPlaying) return;
                // Ensure this object is marked dirty so the flag is serialized
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        public void SetTarget(UnitBase target)
        {
            _target = target;
        }
#endif
    }
}
