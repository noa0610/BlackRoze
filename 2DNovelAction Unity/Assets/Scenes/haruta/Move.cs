using UnityEngine;

public class RedKoopaLike : MonoBehaviour
{
    public float speed = 2f;              // 移動速度
    public Transform groundCheck;         // 足元チェック用の位置
    public Transform wallCheck;           // 壁チェック用の位置
    public LayerMask groundLayer;         // 地面レイヤー

    private Rigidbody2D rb;
    private bool facingRight = true;      // 右向きならtrue
    private float raycooldown = 0f;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // 前方に常に移動
        rb.velocity = new Vector2((facingRight ? 1 : -1) * speed, rb.velocity.y);

        // 足元チェック
        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.2f, groundLayer);

        // 前方に壁チェック
        bool isWallAhead = Physics2D.Raycast(wallCheck.position, facingRight ? Vector2.right : Vector2.left, 0.2f, groundLayer);

        // 地面が途切れている or 壁にぶつかる → 折り返し
        if (raycooldown <= 0f && !isGroundAhead)
    {
        Flip();
        raycooldown = 0.2f; // 0.2秒は反転禁止
    }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // 見た目も反転
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        // エディタでレイ確認用
        Gizmos.color = Color.red;
        if (groundCheck != null)
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 0.2f);
        if (wallCheck != null)
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (facingRight ? Vector3.right : Vector3.left) * 0.2f);
    }
}
