using BlackRose.Core.Models.Units;
using HighElixir;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlackRose.Core.Models.Objects
{
    public class PlayerSpawnner : SingletonBehavior<PlayerSpawnner>
    {
        [SerializeField] private RespawnPoint _currentRespawnPoint;
        [SerializeField] private UnitBase player;
        public void SetRespawnPoint(RespawnPoint respawnPoint)
        {
            _currentRespawnPoint = respawnPoint;
        }
        public void Spawn(UnitBase Player, RespawnPoint customRespawnPoint = null)
        {
            if (customRespawnPoint == null)
            {
                Player.transform.position = _currentRespawnPoint.transform.position;
            }
            else
            {
                Player.transform.position = customRespawnPoint.transform.position;
            }
        }

        // ステータスをリフレッシュする
        public void Respawn(UnitBase Player, RespawnPoint customRespawnPoint = null)
        {
            if (customRespawnPoint == null)
            {
                Player.transform.position = _currentRespawnPoint.transform.position;
            }
            else
            {
                Player.transform.position = customRespawnPoint.transform.position;
            }
            Player.Refresh();
        }


        public void InitialSpawn()
        {
            Spawn(player);
            player.gameObject.SetActive(true);
        }

        public void SetTarget(UnitBase target)
        {
            player = target;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // In editor, keep _currentRespawnPoint synchronized to the RespawnPoint marked as start in the same scene
            var all = FindObjectsOfType<RespawnPoint>(true);
            foreach (var p in all)
            {
                if (!p.gameObject.scene.IsValid()) continue;
                if (!p.gameObject.activeInHierarchy) continue;
                if (p.IsStart)
                {
                    if (_currentRespawnPoint != p)
                    {
                        _currentRespawnPoint = p;
                        EditorUtility.SetDirty(this);
                    }
                    return;
                }
            }
        }
#endif
    }
}
