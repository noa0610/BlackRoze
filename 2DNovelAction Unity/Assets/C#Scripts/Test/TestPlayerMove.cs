using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class TestPlayerMove : MonoBehaviour
{
    // インスペクターから設定する変数
    [SerializeField] private float MOVE_SPEED = 5f; // 移動速度
    [SerializeField] private float JUMP_FORCE = 7.5f; // ジャンプ力

    // 内部処理する変数
    private Rigidbody2D _rigidbody2D; // リジッドボディ
    private GroundChack _groundChack; // 地面接触判定クラス
    private float currentDirection = 0; // 現在の向き（1:右、-1:左、0:停止）

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>(); // リジッドボディ取得
        _groundChack = FindObjectOfType<GroundChack>(); // 地面接触判定クラスを探して取得
    }

    void Update()
    {

        // 移動入力
        if (Input.GetKey(KeyCode.D)) // 右
        {
            Move(Vector3.right);
        }
        else if (Input.GetKey(KeyCode.A)) // 左
        {
            Move(Vector3.left);
        }
        else // 入力無し
        {
            Move(Vector2.zero);
        }

        // ジャンプ入力
        if (Input.GetKeyDown(KeyCode.Space) && _groundChack.IsGround)
        {
            Jump();
        }
    }

    // 移動処理メソッド
    private void Move(Vector3 direction)
    {
        if (direction.x != 0) // 入力がある場合のみ処理
        {
            Turn(direction);
        }

        _rigidbody2D.velocity = new Vector2(direction.x * MOVE_SPEED, _rigidbody2D.velocity.y);
    }
    // 向きを変更するメソッド
    private void Turn(Vector3 direction)
    {
        float newDirection = Mathf.Sign(direction.x); // 移動する方向を取得
        if (newDirection != 0 && newDirection != currentDirection) // 向きが変わった場合のみ処理
        {
            currentDirection = newDirection;
            float scaleX = Mathf.Abs(transform.localScale.x); // 元のスケールの絶対値を取得
            transform.localScale = new Vector3(newDirection * scaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    // ジャンプ処理メソッド
    private void Jump()
    {
        _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, JUMP_FORCE);
    }
}
