using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class AIMuzzleSetter : MonoBehaviour
    {
        [SerializeField] private GameObject _muzzle;

        public void Set(params IAIState[] state)
        {
            foreach(var s in state)
            {
                s.SetMuzzle(_muzzle);
            }
        }
    }
}