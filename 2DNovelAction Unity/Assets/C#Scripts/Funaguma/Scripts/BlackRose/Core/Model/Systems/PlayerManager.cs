using System.Linq;
using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.Models.Systems
{
    [DefaultExecutionOrder(-1)]
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private UnitBase _playerUnit;

        private void Awake()
        {
            var playerFollower = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerFollower>();
            foreach (var follower in playerFollower)
            {
                follower.SetTarget(_playerUnit);
            }
        }
    }

    public interface IPlayerFollower
    {
        void SetTarget(UnitBase target);
    }
}