using System;
using UnityEngine;

namespace BlackRose
{
	// =======================
	// Move（移動）状態
	// =======================
	[Serializable]
	public class MoveOnGround : StateComp
	{
		[SerializeField] private Rigidbody2D _rigidbody2D;
		[SerializeField] private string _animationTrigger;
		[SerializeField] private StatusAmount _statusAmount;
		[SerializeField] private bool _inex = false; // inExitStopの代わりに使用するフラグ

		public MoveOnGround(Rigidbody2D rigidbody2D, StatusAmount status, bool inExitStop = false)
		{
			_rigidbody2D = rigidbody2D;
			_statusAmount = status;
			_inex = inExitStop; // inExitStopの代わりに使用するフラグを設定
		}
		public MoveOnGround() { }
		public override void Enter(IState previousIState, UnitBase parent)
		{
			parent.Animator.SetTrigger(_animationTrigger);
		}

		public override void Exit(IState nextIState, UnitBase parent)
		{
			if (_inex)
				_rigidbody2D.velocity = Vector2.zero; // inExitStopの代わりに使用するフラグがtrueなら速度をゼロにする
		}

		public override void Stay(UnitBase parent)
		{
			var s = _statusAmount.ChangedMax;
			_rigidbody2D.velocity = new Vector2(s * parent.Direction.x, _rigidbody2D.velocity.y);
		}
	}
}