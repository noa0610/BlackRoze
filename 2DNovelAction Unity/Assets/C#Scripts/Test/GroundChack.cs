using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地面の接地判定をコライダーで取る
/// </summary>
public class GroundChack : MonoBehaviour
{
    // インスペクターから設定する変数
    [SerializeField] private float GROUND_CHECK_RANGE; // 接地判定範囲
    [SerializeField] private const string GROUND_LAYER_NAME = "Ground";

    // 内部処理する変数
    private CircleCollider2D _circleCollider2D; // コライダー
    private TestPlayerMove _playerMove; // プレイヤーの移動
    private bool isGround; // 地面接触の判定

    // プロパティ
    public bool IsGround
    {
        get {return isGround;}
    }
    void Awake()
    {
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _circleCollider2D.radius = GROUND_CHECK_RANGE; // コライダーに範囲を反映

        isGround = false;
    }

    // 地面に触れた時
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }

    // 地面から離れた時
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Ground"))
        {
            isGround = false;
        }
    }

    // 接地判定をギズモで確認する
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green; // 接地判定を赤色で表示
    //     Gizmos.DrawWireSphere(gameObject.transform.position, GROUND_CHECK_RANGE / 2);
    // }
}
