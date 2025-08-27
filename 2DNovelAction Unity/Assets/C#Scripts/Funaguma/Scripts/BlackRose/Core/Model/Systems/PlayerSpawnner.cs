using BlackRose.Core.Models.Systems;
using BlackRose.Core.Models.Units;
using HighElixir;
using UnityEngine;

namespace BlackRose.Core.Models.Objects
{
    public class PlayerSpawnner : SingletonBehavior<PlayerSpawnner>, IPlayerFollower
    {
        private RespawnPoint _currentRespawnPoint;
        [SerializeField] private UnitBase player;
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

        public void SetRespawnPoint(RespawnPoint respawnPoint)
        {
            _currentRespawnPoint = respawnPoint;
        }

        protected override void Awake()
        {
            base.Awake();
            Spawn(player);
            player.gameObject.SetActive(true);
        }

        public void SetTarget(UnitBase target)
        {
            player = target;
        }
    }
}
