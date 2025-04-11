/// <summary>
/// キャラクターのステートパターンのインターフェース
/// </summary>
public interface ICharacterState
{
    /// <summary>
    /// そのステートになった瞬間の処理(入力時処理)
    /// </summary>
    /// <param name="player"></param>
    void Enter(Player player);

    /// <summary>
    /// そのステート中、毎フレーム行われる処理(実行中処理)
    /// </summary>
    /// <param name="player"></param>
    void Execute(Player player, InputInfo inputInfo);

    /// <summary>
    /// 次のステートに切り替わる瞬間の処理(出力時処理)
    /// </summary>
    /// <param name="player"></param>
    void Exit(Player player);
}