using UnityEngine;
using AIE2D;
using System; // AfterImageEffect2DPlayerBaseを使用するための名前空間

namespace BlackRose
{
	[Serializable]
	public class DashOnGround : StateComp
	{
		[SerializeField] private StatusAmount _dashSpeed;
		[SerializeField] private Rigidbody2D _rigidbody2D;
		[SerializeField] private DynamicAfterImageEffect2DPlayer _afterImagePlayer;
		[SerializeField] private bool _onDashJump = false;

		public DashOnGround(Rigidbody2D rigidbody2D, GroundedUnit parent, StatusAmount dashSpeed)
		{
			if (_afterImagePlayer == null)
				_afterImagePlayer = parent.gameObject.GetComponent<DynamicAfterImageEffect2DPlayer>();
			_afterImagePlayer.SetActive(false); // 初期状態ではAfterImageを非表示にする
			_dashSpeed = dashSpeed;
			_rigidbody2D = rigidbody2D;
		}
		public DashOnGround() { }
		public override void Enter(IState previousIState, UnitBase parent)
		{
			_afterImagePlayer.SetActive(true);
		}
		public override void Stay(UnitBase parent)
		{
			if (parent is not GroundedUnit grounded) return;
			if (!grounded.IsGrounded) return; // 地面にいない場合はDashを行わない
											  // Dash中の処理
			Vector2 dashDirection = parent.Direction * _dashSpeed.ChangedMax;
			_rigidbody2D.velocity = dashDirection + _rigidbody2D.velocity * new Vector2(0, 1);
		}
		public override void Exit(IState nextIState, UnitBase parent)
		{
			if (nextIState is Jump jumpIState && parent is GroundedUnit grounded)
			{
				_onDashJump = true; // DashからJumpに移行する場合はフラグを立てる
                Action onLanding = null;
                onLanding = () =>
                {
                    _onDashJump = false;
                    _afterImagePlayer.SetActive(false);
                    grounded.OnAirToGround -= onLanding; // ちゃんと同じ参照を解除する
                };
                grounded.OnAirToGround += onLanding;

                // 着地時のコールバックを登録
            }
            if (!_onDashJump)
				_afterImagePlayer.SetActive(false); // Dash終了時にAfterImageの再生を停止
		}
	}
}