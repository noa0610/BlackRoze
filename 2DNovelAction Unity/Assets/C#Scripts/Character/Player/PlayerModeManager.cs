using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModeManager
{
    public IPlayerMode CurrentMode {get; private set;}

    /// <summary>
    /// 現在のモードから次のモードへの切り替えを行う
    /// </summary>
    /// <param name="player"></param>
    /// <param name="fromState"></param>
    /// <param name="nextState"></param>
    public void TransitionMode(Player player, IPlayerMode fromMode, IPlayerMode nextMode)
    {
        if(CurrentMode != fromMode) return;

        CurrentMode.ExitMode(player);  // 現在のモードが終了するときの処理を行う
        CurrentMode = nextMode;        // 次のモードに移行する
        CurrentMode.EnterMode(player); // 次のモードが開始するときの処理を行う
    }

    /// <summary>
    /// NormalMode状態に初期化するメソッド
    /// </summary>
    /// <param name="player"></param>
    public void InitMode(Player player)
    {
        CurrentMode = new NormalMode();
        CurrentMode.EnterMode(player);
        player.Anime.Animator.ResetTrigger("Normal");
    }
}
