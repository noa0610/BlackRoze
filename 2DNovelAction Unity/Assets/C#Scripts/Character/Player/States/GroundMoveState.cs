using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMoveState : IPlayerState
{
    private bool isGround;
    public void Enter(Player player)
    {
        Debug.Log("Enter GroundMoveState");
        player.Anime.Animator.SetTrigger("Move"); // 移動のアニメーションに切り替える
    }

    public void Execute(Player player ,InputInfo inputInfo)
    {
        player.Move.GetVelocity();

        player.Move.GroundMove(inputInfo.Move);
        player.Move.Direction(inputInfo.Move);
        isGround = !player.Move.AirJudge();
        
        if(inputInfo.Jump && isGround)
        {
            Debug.Log("Jump");
            player.Move.Jump();
        }
        

        player.Move.SetVelocity();

        if(inputInfo.Attack)
        {
            // ステートをAttackStateに切り替える
            player.StateTransition(this, new AttackState());
        }
    }

    public void Exit(Player player)
    {
        
    }
}
