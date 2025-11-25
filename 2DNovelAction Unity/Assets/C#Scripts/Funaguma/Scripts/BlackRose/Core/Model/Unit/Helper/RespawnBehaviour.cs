using BlackRose.Core.Models.Objects;
using UnityEngine;

namespace BlackRose.Core.Models.Units.Helpers
{
    public sealed class RespawnBehaviour : MonoBehaviour
    {
        [SerializeField] private string _loadSceneName = "GameScene";
        [SerializeField] private SceneSelectLoad _loader;
        public void Receive()
        {
            _loader.OnButtonClick(_loadSceneName);
        }
    }
}