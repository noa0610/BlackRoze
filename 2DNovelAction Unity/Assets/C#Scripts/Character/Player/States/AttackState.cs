using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IPlayerState
{
    public void Enter(Player player)
    {
        player.Anime.Animator.SetTrigger("Attack"); // 攻撃のアニメーションに切り替える
    }

    public void Execute(Player player,InputInfo inputInfo)
    {
        player.Move.GetVelocity(); // 水平方向の速度を取得
        player.Move.GroundMove(); // 攻撃中は移動しない
        if(inputInfo.Skill)
        {
            // 攻撃アニメーションが終了したらGroundMoveStateに戻る
            player.StateTransition(this, new GroundMoveState());
        }
        player.Move.SetVelocity(); // Rigidbody2Dに速度を設定
    }

    public void Exit(Player player)
    {

    }
}
