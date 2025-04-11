using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーのモードのインターフェース
/// </summary>
public interface IPlayerMode
{
    /// <summary>
    /// そのモードになった瞬間の処理
    /// </summary>
    /// <param name="player"></param>
    void EnterMode(Player player);

    /// <summary>
    /// そのモード中、毎フレーム行われる処理
    /// </summary>
    /// <param name="player"></param>
    void ExecuteMode(Player player, InputInfo inputInfo);

    /// <summary>
    /// 次のモードに切り替わる瞬間の処理
    /// </summary>
    /// <param name="player"></param>
    void ExitMode(Player player);
}
