using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    public class KuroReciever : VisualReciever
    {
        [SerializeField] private VisualInfo _landed;
        [SerializeField] private VisualInfo _onMove;
        public void PlayLandedSE()
        {
            _target.PlaySE(_landed.SEName, _landed.Volume);
        }
        public void PlayMove()
        {
            _target.PlaySE(_onMove.SEName, _onMove.Volume);
        }
    }
}