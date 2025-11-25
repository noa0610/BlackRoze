using AIE2D;
using BlackRose.Core.Models.Units;
using System;
using UniRx;
using UnityEngine;

namespace BlackRose.Core.Models.States
{
    [Serializable]
    public class DashOnGround : HolizontalMovingStates
    {
        [SerializeField] private DynamicAfterImageEffect2DPlayer _afterImagePlayer;
        [SerializeField] private bool _onDashJump = false;

        [SerializeField] private float _accel = 60f;   // 加速度
        [SerializeField] private float _maxSpeedScale = 1f; // ステータスに掛ける上限倍率

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
            if (nextIState is Jump && parent is GroundedUnit grounded)
            {
                _onDashJump = true;

                grounded.OnAirToGround.Take(1).Subscribe(_ =>
                {
                    _onDashJump = false;
                    if (_afterImagePlayer != null)
                        _afterImagePlayer.SetActive(false);
                });
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