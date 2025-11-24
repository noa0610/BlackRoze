using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    public class SEReciver : VisualReciever
    {
        [Header("SEs (ActionRobot)")]
        [SerializeField] private VisualInfo _idleSE;
        [SerializeField] private VisualInfo _landingSE;
        [SerializeField] private VisualInfo _moveSE;
        [SerializeField] private VisualInfo _fallSE;
        [SerializeField] private VisualInfo _jumpSE;
        [SerializeField] private VisualInfo _shootIntervalSE;
        [SerializeField] private VisualInfo _stunSE;
        [SerializeField] private VisualInfo _deadSE;

        // Play methods for each StateKey
        public void PlayIdleSE() => Play(_idleSE);
        public void PlayLandingSE() => Play(_landingSE);
        public void PlayMoveSE() => Play(_moveSE);
        public void PlayFallSE() => Play(_fallSE);
        public void PlayJumpSE() => Play(_jumpSE);

        public void PlayShootIntervalSE() => Play(_shootIntervalSE);
        public void PlayStunSE() => Play(_stunSE);
        public void PlayDeadSE() => Play(_deadSE);

        private void Play(VisualInfo info)
        {
            _target.PlaySE(info.SEName, info.Volume);
        }
    }
}