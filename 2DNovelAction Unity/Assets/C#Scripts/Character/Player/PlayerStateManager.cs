using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager
{
    public IPlayerState CurrentState { get; private set; }

    /// <summary>
    /// 現在のステートから次のステートへの切り替えを行う
    /// </summary>
    /// <param name="player"></param>
    /// <param name="fromState"></param>
    /// <param name="nextState"></param>
    public void Transition(Player player, IPlayerState fromState, IPlayerState nextState)
    {
        if (CurrentState != fromState) return;

        CurrentState.Exit(player);  // 現在のステートが終了するときの処理を行う
        CurrentState = nextState;   // 次のステートに移行する
        CurrentState.Enter(player); // 次のステートが開始するときの処理を行う
    }

    /// <summary>
    /// GroundMoveState状態に初期化するメソッド
    /// </summary>
    /// <param name="player"></param>
    public void Init(Player player)
    {
        CurrentState = new GroundMoveState();
        CurrentState.Enter(player);
        player.Anime.Animator.ResetTrigger("Move");
    }
}
