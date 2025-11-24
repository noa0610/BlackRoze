using BlackRose.Core.Models.Objects;
using UnityEngine;

namespace BlackRose.Core.Models.Units.Helpers
{
    public sealed class RespawnBehaviour : MonoBehaviour
    {
        public void Receive()
        {
            PlayerSpawnner.ShowRespawnMenu();
        }
    }
}