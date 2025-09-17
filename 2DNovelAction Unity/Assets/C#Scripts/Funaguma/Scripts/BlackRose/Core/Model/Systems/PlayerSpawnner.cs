using BlackRose.Core.Models.Units;

namespace BlackRose.Core.Models.Objects
{
    public static class PlayerSpawnner
    {
        private static RespawnPoint _currentRespawnPoint;
        private static UnitBase player;
        public static void SetRespawnPoint(RespawnPoint respawnPoint)
        {
            _currentRespawnPoint = respawnPoint;
        }
        public static void Spawn(UnitBase Player, RespawnPoint customRespawnPoint = null)
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


        public static void InitialSpawn()
        {
            Spawn(player);
            player.gameObject.SetActive(true);
        }

        public static void SetTarget(UnitBase target)
        {
            player = target;
        }
    }
}
