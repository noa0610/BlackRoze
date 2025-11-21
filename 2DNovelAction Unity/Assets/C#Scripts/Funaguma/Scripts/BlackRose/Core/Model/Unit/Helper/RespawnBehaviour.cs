using BlackRose.Core.Models.Objects;
using UnityEngine;

namespace BlackRose.Core.Models.Units.Helpers
{
    public sealed class RespawnBehaviour : MonoBehaviour
    {
        [SerializeField] private UnitBase unitBase;
        public void Receive()
        {
            PlayerSpawnner.instance.Respawn(unitBase);
        }
    }
}