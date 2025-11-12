using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
/// <summary>
/// Rayを飛ばしてワープする
/// </summary>
public class RayWarp : MonoBehaviour
{
    // インスペクターから設定する変数
    [Header("Rayの長さ")]
    [SerializeField] private float RAY_LANGE = 10f;

    [Header("Rayを飛ばすオブジェクト")]
    [SerializeField] private GameObject RAY_FIRING;

    [Header("壁のレイヤー")]
    [SerializeField] private LayerMask LAYER_MASK;

    // 内部処理する変数

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            Vector2 origin = RAY_FIRING.transform.position;
            Vector2 direction = Vector2.right; // 右方向にRayを飛ばす

            RaycastHit2D hit2D = Physics2D.Raycast(origin, direction, RAY_LANGE, LAYER_MASK); // Rayを飛ばす


            if (hit2D.collider != null) // Rayが衝突したとき
            {
                Vector2 hitPoint = hit2D.point; // 命中点を取得
                Debug.Log($"Hit Point: {hitPoint}, Hit Object: {hit2D.collider.name}"); // Rayが命中したらオブジェクト名を出力

                transform.position = hitPoint; // 命中点にワープ
            }
            else // 衝突しなかった場合
            {
                Vector2 endPoint = origin + direction * RAY_LANGE; // 終点位置を計算
                Debug.Log($"No hit. End Point: {endPoint}");

                transform.position = endPoint; // 終点位置にワープ
            }

            // デバッグ用にRayを可視化(命中で赤、なければ緑)
            Debug.DrawRay(origin, direction * RAY_LANGE, hit2D.collider != null ? Color.red : Color.red);
        }
    }
}
