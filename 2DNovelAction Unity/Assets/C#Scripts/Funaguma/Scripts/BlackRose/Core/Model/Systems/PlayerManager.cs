using System.Linq;
using BlackRose.Core.Models.Objects;
using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.Systems
{
    [DefaultExecutionOrder(-10)]
    public class PlayerManager : MonoBehaviour
    {
        [Tooltip("Awakeのタイミングで、IPlayerFollowerを実装したすべてのコンポーネントにアタッチします")]
        [SerializeField] private UnitBase _playerUnit;

        private void Awake()
        {
            var playerFollower = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerFollower>();
            foreach (var follower in playerFollower)
            {
                follower.SetTarget(_playerUnit);
            }
            PlayerSpawnner.instance.SetTarget(_playerUnit);
        }
    }

    public interface IPlayerFollower
    {
        void SetTarget(UnitBase target);
    }
}