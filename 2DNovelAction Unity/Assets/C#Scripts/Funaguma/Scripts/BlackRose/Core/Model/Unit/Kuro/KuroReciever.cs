using BlackRose.Core.Models.Units;
using UnityEngine;
namespace BlackRose.Core.Model.Unit
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