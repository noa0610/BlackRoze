using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private readonly float GROUND_ACCEL = 20.0f;        // 地上での加速度
    private readonly float MAX_GROUND_SPEED = 5.0f;     // 地上での最大速度

    private readonly float AIR_ACCEL = 10.0f;           // 空中での加速度
    private readonly float MAX_AIR_SPEED = 4.0f;        // 空中での最大速度

    private readonly float GROUND_ATTACK_ACCEL = 5.0f;  // スライド時の減速加速度

    private readonly float JUMP_POWER = 5.0f;           // ジャンプ力

    private Rigidbody2D _rigidbody2D;
    private Vector3 _horizontalVelocity;                // 水平方向の速度
    private Vector3 _verticalVelocity;                  // 垂直方向の速度
    private SpriteRenderer _spriteRenderer;
    private Vector3 _facingDirection = Vector3.right;   // 現在向いている方向


    /// <summary>
    /// 水平方向の速度の大きさを取得
    /// </summary>
    public float horizontalMagnitude { get { return _horizontalVelocity.magnitude; } }


    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 水平方向の速度を計算する（移動ベクトル、加速度、最大速度を指定）
    /// </summary>
    /// <param name="moveVec"></param>
    /// <param name="accel"></param>
    /// <param name="maxSpeed"></param>
    private void CalcHorizontalVelocity(Vector3 moveVec, float accel, float maxSpeed)
    {
        _horizontalVelocity = Vector3.MoveTowards(
                                _horizontalVelocity,
                                moveVec * maxSpeed,
                                accel * Time.deltaTime);
    }

    /// <summary>
    /// 水平方向の速度を減速させる（加速度を指定）
    /// </summary>
    /// <param name="accel"></param>
    private void CalcHorizontalVelocity(float accel)
    {
        _horizontalVelocity = Vector3.MoveTowards(
                                _horizontalVelocity,
                                Vector3.zero,
                                accel * Time.deltaTime);
    }

    /// <summary>
    /// プレイヤーの向きを変更する
    /// </summary>
    /// <param name="moveVec"></param>
    public void Direction(Vector3 moveVec)
    {
        if (moveVec.x != 0f)
        {
            bool isLeft = moveVec.x < 0;
            _spriteRenderer.flipX = isLeft;
            _facingDirection = isLeft ? Vector3.left : Vector3.right;
        }
    }

    // 現在の向きを取得
    private Vector3 GetDirection()
    {
        return _facingDirection;
    }

    /// <summary>
    /// 地上移動を行う
    /// </summary>
    /// <param name="moveVec"></param>
    public void GroundMove(Vector3 moveVec)
    {
        CalcHorizontalVelocity(moveVec, GROUND_ACCEL, MAX_GROUND_SPEED);
    }

    /// <summary>
    /// 地上移動を行う（加速倍率指定可能）
    /// </summary>
    /// <param name="moveVec"></param>
    /// <param name="addAccelMultiple"></param>
    public void GroundMove(Vector3 moveVec, float addAccelMultiple)
    {
        CalcHorizontalVelocity(moveVec, GROUND_ACCEL * addAccelMultiple, MAX_GROUND_SPEED * addAccelMultiple);
    }

    /// <summary>
    /// スライド移動（攻撃モーション時の移動）
    /// </summary>
    public void GroundMove()
    {
        CalcHorizontalVelocity(GROUND_ATTACK_ACCEL);
    }

    /// <summary>
    /// 空中移動を行う
    /// </summary>
    /// <param name="moveVec"></param>
    public void AirMove(Vector3 moveVec)
    {
        CalcHorizontalVelocity(moveVec, AIR_ACCEL, MAX_AIR_SPEED);
    }

    /// <summary>
    /// ジャンプ処理（Y軸方向の速度をセット）
    /// </summary>
    public void Jump()
    {
        _verticalVelocity.y = JUMP_POWER;
    }

    /// <summary>
    /// 空中にいるかを判定する
    /// </summary>
    /// <returns></returns>
    public bool AirJudge()
    {
        var collider = GetComponent<Collider2D>(); // プレイヤーのコライダー取得
        float rayStartHeight = collider.bounds.min.y; // コライダーの底＋少し浮かせる
        var rayStart = new Vector3(transform.position.x, rayStartHeight, transform.position.z);
        var ray = new Ray(rayStart, Vector3.down);
        var layerMask = LayerMask.GetMask("Ground");
        

        // プレイヤーの下方向にSphereCastを行い、地面との接触を判定
        if (!Physics2D.Raycast(rayStart, Vector2.down * 0.1f, 0.1f, layerMask))
        {
            Debug.DrawRay(rayStart, Vector2.down * 0.1f ,Color.green, 0.1f);
            return true; // 地面に接触していない（空中）
        }
        Debug.DrawRay(rayStart, Vector2.down * 0.1f ,Color.red, 0.1f);
        return false; // 地面に接触している
    }

    /// <summary>
    /// 現在のRigidbodyの速度を取得して分解
    /// </summary>
    public void GetVelocity()
    {
        var velocity = _rigidbody2D.velocity;
        _horizontalVelocity = new Vector3(velocity.x, 0.0f, 0.0f);
        _verticalVelocity = new Vector3(0.0f, velocity.y, 0.0f);
    }

    /// <summary>
    /// 計算した速度をRigidbodyに適用
    /// </summary>
    public void SetVelocity()
    {
        _rigidbody2D.velocity = _horizontalVelocity + _verticalVelocity;
    }
}
