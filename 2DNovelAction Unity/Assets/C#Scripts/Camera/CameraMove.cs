using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // --- Inspector Variables ---　インスペクターから設定する変数

    [Header("Player GameObject")]
    [SerializeField] private float SmoothSpeed = 0.125f; // カメラの追従速度を滑らかにするための係数
    [SerializeField] private Vector3 OFFSET = new Vector3(0,0,-10); // カメラとプレイヤーの相対的な位置を設定するオフセット

    [Header("Stage ComeraOut")]
    [SerializeField] private float MinX; // ステージの左端
    [SerializeField] private float MaxX; // ステージの右端
    [SerializeField] private float MinY; // ステージの下端
    [SerializeField] private float MaxY; // ステージの上端


    // --- Private Variables ---　このスクリプトでのみ変更を加える
    private GameObject PLAYER; // プレイヤーのGameObjectを取得
    private Vector3 desiredPosition; // プレイヤーの位置

    void Start()
    {
        if (PLAYER == null)
        {
            // シーン内のインスタンス化されたプレイヤーを取得
            PLAYER = GameObject.FindGameObjectWithTag("Player");
            
            if (PLAYER == null)
            {
                Debug.LogError("シーン内に 'Player' タグが付いたオブジェクトが見つかりません。");
            }
        }
        
        if (PLAYER != null)
        {
            transform.position = PLAYER.transform.position + OFFSET;
        }
    }

    void LateUpdate()
    {
        if (PLAYER != null)
        {
            // プレイヤーの位置にオフセットを適用し、ターゲット位置を設定
            desiredPosition = PLAYER.transform.position + OFFSET;
        }

        // ステージの境界内にカメラの位置を制限する
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, MinX, MaxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, MinY, MaxY);

        // 現在のカメラ位置からターゲット位置への移動を滑らかにする
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);
        transform.position = smoothedPosition;
    }
}