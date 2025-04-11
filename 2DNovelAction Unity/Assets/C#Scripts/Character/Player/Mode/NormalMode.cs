using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalMode : IPlayerMode
{
    public void EnterMode(Player player)
    {
        // Debug.Log("Enter Normal Mode");
        // player.Anime.Animator.SetTrigger("Normal");
    }

    public void ExecuteMode(Player player, InputInfo inputInfo)
    {
        // Debug.Log("Execut Normal Mode");
        // 必要であれば毎フレーム処理を追加
    }

    public void ExitMode(Player player)
    {
        // Debug.Log("Exit Normal Mode");
    }
}
