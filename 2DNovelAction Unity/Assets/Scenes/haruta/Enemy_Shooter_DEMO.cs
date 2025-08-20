using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*動き確認用の簡易版*/
public class Enemy_Shooter_DEMO : MonoBehaviour
{
    public enum States // 用意したステート
    {
        none,
        move, // 移動 
        idle, // 待機 
        shootReady, // ショットを打つための準備
        dead, // 死亡
        shoot, // ショットを打つshoot
        knockBack, // ノックバック
    }
    private enum Triggers//ステートを移行するための条件
    {
        None,
        MissingPlayer, // プレイヤーを見失った
        FoundPlayer,   // プレイヤーを発見した
        AttackRange,   // 攻撃範囲に入った
        shootCoolDown, //射撃のためのクールタイムが終わった
        shoot,         // すでに発射した
        Died,          // 死亡した（HPが０になった）
        time,// 一定時間が経過する
        Damage,//攻撃を受ける
    }
    private States _currentState = States.idle;   // 現在のステート。デフォルトは待機
    protected void Start()
    {
        ChangeState(States.idle);
    }
    private float shootReadyTimer = 0f;
    private float shootReadyDuration = 1.0f;

    private float shootTimer = 0f;
    private float shootDuration = 0.2f;

    private void FixedUpdate()
    {
        Debug.Log($"CurrentState.key = {_currentState}");
        switch (_currentState)
        {
            case States.idle:
                if (searchPlayer()) ChangeState(States.shootReady);
                break;

            case States.shootReady:
                shootReadyTimer += Time.fixedDeltaTime;
                if (shootReadyTimer >= shootReadyDuration)
                    ChangeState(States.shoot);
                break;

            case States.shoot:
                // 弾を生成する処理など
                Debug.Log("Shoot!");
                shootTimer += Time.fixedDeltaTime;
                if (shootTimer >= shootDuration)
                    ChangeState(States.idle);
                break;

            case States.move:
                //向きを取得
                //向いている方向に力を加える
            // もしこ
                // 移動処理
                break;

            case States.knockBack:
                // ノックバック処理
                  //向いている方向を取得
                break;

            case States.dead:
                Destroy(gameObject);// 死亡処理
                break;
        }
    }

    private void ChangeState(States nextState)
{
    // 現在のステートから抜ける時の処理
    if (_currentState == States.shootReady) shootReadyTimer = 0f;
    if (_currentState == States.shoot) shootTimer = 0f;

    // 新しいステートに入る時の処理
    if (nextState == States.shootReady)
    {
        Debug.Log("Shoot Ready!");
    }
    else if (nextState == States.shoot)
    {
        Debug.Log("Shoot!");
        // ここで弾生成なども可能
    }

    _currentState = nextState;
}
private bool searchPlayer(float D = 19f)
{
    // 範囲内にプレイヤーがいるか判定
    // 今回はテスト用に常に true
    return true;
}

}
