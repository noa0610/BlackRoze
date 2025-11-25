using UnityEngine;

/* 背景ループ用スクリプト
 *
 * ターゲット指定されたオブジェクトの移動に合わせて背景をループさせます。
 * 基本はメインカメラの移動に合わせて移動します。
 */
public class BackGroundLoop : MonoBehaviour
{
    private const float k_maxLength = 1f;         // 背景のループ処理の範囲を1で設定
    private const string k_propName = "_MainTex"; // シェーダープロパティ名を定義（MainTexのオフセットを変更するため）

    [SerializeField] private Vector2 m_offsetSpeed = Vector2.one; // オフセットの移動速度を設定する変数。Inspectorで設定可能
    [SerializeField] private GameObject Target;                   // Transformを参照するための変数

    private Material m_material;            // 背景に適用するマテリアル
    private Vector3 previousPlayerPosition; // 前フレームのターゲットの位置を保持するための変数

    private void Start()
    {
        if (GetComponent<SpriteRenderer>() is SpriteRenderer sr) // スプライトレンダラーからマテリアルを取得
        {
            m_material = sr.material;
        }

        if (Target == null)
        {
            Target = GameObject.FindWithTag("MainCamera");
        }
        previousPlayerPosition = Target.transform.position; // カメラの初期位置を設定
    }

    private void Update()
    {
        if (m_material && Target)
        {
            // ターゲットの移動量を計算
            Vector3 playerDelta = Target.transform.position - previousPlayerPosition;
            previousPlayerPosition = Target.transform.position;

            // ターゲットの移動に基づいて背景のオフセットを計算
            float x = Mathf.Repeat(m_material.GetTextureOffset(k_propName).x + (playerDelta.x * m_offsetSpeed.x), k_maxLength);
            float y = Mathf.Repeat(m_material.GetTextureOffset(k_propName).y + (playerDelta.y * m_offsetSpeed.y), k_maxLength);
            var offset = new Vector2(x, y);

            // 計算したオフセットをマテリアルに適用
            m_material.SetTextureOffset(k_propName, offset);
        }
    }

    private void OnDestroy()
    {
        // ゲームをやめた後にマテリアルのOffsetを戻しておく
        if (m_material)
        {
            m_material.SetTextureOffset(k_propName, Vector2.zero);
        }
    }
}