using UnityEngine;
using AIE2D;
using System;
using BlackRose.Core.Models.Units;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class DashOnGround : HolizontalMovingStates
    {
        [SerializeField] private DynamicAfterImageEffect2DPlayer _afterImagePlayer;
        [SerializeField] private bool _onDashJump = false;

        [SerializeField] private float _accel = 60f;   // 加速度
        [SerializeField] private float _maxSpeedScale = 1f; // ステータスに掛ける上限倍率

        // Exit→着地までの購読を保持しておく（破棄時に保険で解除）
        private Action _onLandingHandler;

        [Obsolete]
        public DashOnGround(Rigidbody2D rigidbody2D, GroundedUnit parent)
            : base()
        {
            if (_afterImagePlayer == null)
                _afterImagePlayer = parent.gameObject.GetComponent<DynamicAfterImageEffect2DPlayer>();

            if (_afterImagePlayer != null)
                _afterImagePlayer.SetActive(false);
        }
        public DashOnGround(GroundedUnit parent)
            : base()
        {
            if (_afterImagePlayer == null)
                _afterImagePlayer = parent.gameObject.GetComponent<DynamicAfterImageEffect2DPlayer>();

            if (_afterImagePlayer != null)
                _afterImagePlayer.SetActive(false);
        }
        public DashOnGround() { }

        public override void Enter(IState previousIState, UnitBase parent)
        {
            base.Enter(previousIState, parent);
            // Null保険
            if (_afterImagePlayer != null)
                _afterImagePlayer.SetActive(true);
        }

        public override void Stay(UnitBase parent, float deltaTime)
        {
            base.Stay(parent, deltaTime);
            if (parent is not GroundedUnit grounded) return;
            if (!grounded.IsGrounded) return;
            if (Rigidbody2D == null) return;

            // 速度・向き取得（キャッシュ）
            var dashStatus = parent.statusManager.GetStatus(Status.DashSpeed);
            float maxSpeed = dashStatus.CurrentAmount * _maxSpeedScale;

            // 目標速度（向き × 上限）
            float dirX = Mathf.Sign(GetDirection(grounded).x); // -1 or 1 を期待
            float targetVx = dirX * maxSpeed;

            // MoveTowardsでスムーズに目標へ近づける
            float newVx = Mathf.MoveTowards(Rigidbody2D.velocity.x, targetVx, _accel * deltaTime);

            Rigidbody2D.velocity = new Vector2(newVx, Rigidbody2D.velocity.y);
        }

        public override void Exit(IState nextIState, UnitBase parent)
        {
            // 既存の購読が残っていたら解除（保険）
            if (_onLandingHandler != null && parent is GroundedUnit g0)
            {
                g0.OnAirToGround -= _onLandingHandler;
                _onLandingHandler = null;
            }

            if (nextIState is Jump && parent is GroundedUnit grounded)
            {
                _onDashJump = true;

                _onLandingHandler = () =>
                {
                    _onDashJump = false;
                    if (_afterImagePlayer != null)
                        _afterImagePlayer.SetActive(false);

                    grounded.OnAirToGround -= _onLandingHandler;
                    _onLandingHandler = null;
                };

                grounded.OnAirToGround += _onLandingHandler;
            }
            else
            {
                // ダッシュ→ジャンプ以外の遷移なら即OFF
                if (!_onDashJump && _afterImagePlayer != null)
                    _afterImagePlayer.SetActive(false);
            }
        }
    }

}