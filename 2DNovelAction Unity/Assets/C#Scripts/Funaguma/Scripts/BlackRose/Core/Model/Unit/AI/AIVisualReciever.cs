using UnityEngine;

namespace BlackRose.Core.Models.Units
{
    /// <summary>
    /// AI専用のVisualReciever拡張。AIModeBase.SubState に対応する SE をまとめて持ちます。
    /// UnitBase.PlaySE を経由して再生されます（VisualReciever.Play を利用）。
    /// </summary>
    public class AIVisualReciever : VisualReciever
    {
        [Header("SEs (AI)")]
        [SerializeField] private VisualInfo _idleSE;
        [SerializeField] private VisualInfo _shootIntervalSE;
        [SerializeField] private VisualInfo _moveSE;
        [SerializeField] private VisualInfo _jumpSE;

        [SerializeField] private VisualInfo _shootSE;
        [SerializeField] private VisualInfo _halfSE;
        [SerializeField] private VisualInfo _fullSE;

        [SerializeField] private VisualInfo _dashSE;
        [SerializeField] private VisualInfo _fallingSE;
        [SerializeField] private VisualInfo _landingSE;
        [SerializeField] private VisualInfo _skillSE;
        [SerializeField] private VisualInfo _specialAttackSE;

        [SerializeField] private VisualInfo _other1SE;
        [SerializeField] private VisualInfo _other2SE;
        [SerializeField] private VisualInfo _other3SE;

        // Override base normal shoot to use AI shoot SE
        public override void PlayNormalShootSE()
        {
            Play(_shootSE);
        }

        // 各サブステート用の Play メソッド
        public void PlayIdleSE() => Play(_idleSE);
        public void PlayShootIntervalSE() => Play(_shootIntervalSE);
        public void PlayMoveSE() => Play(_moveSE);
        public void PlayJumpSE() => Play(_jumpSE);

        public void PlayShootSE() => Play(_shootSE);
        public void PlayHalfSE() => Play(_halfSE);
        public void PlayFullSE() => Play(_fullSE);

        public void PlayDashSE() => Play(_dashSE);
        public void PlayFallingSE() => Play(_fallingSE);
        public void PlayLandingSE() => Play(_landingSE);
        public void PlaySkillSE() => Play(_skillSE);
        public void PlaySpecialAttackSE() => Play(_specialAttackSE);

        public void PlayOther1SE() => Play(_other1SE);
        public void PlayOther2SE() => Play(_other2SE);
        public void PlayOther3SE() => Play(_other3SE);

        public void RequestGoundCheck()
        {
            if (_target is GroundedUnit groundedUnit)
            {
                if (groundedUnit.IsGrounded)
                {
                    _target.Animator.SetTrigger("toRand");
                }
            }
        }
    }
}
