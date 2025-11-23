using System.Collections;
using UnityEngine;

public class udenoidou : MonoBehaviour
{
    [Header("インスタンスしたいオブジェクト（ヒエラルキー上の参照でもOK）")]
    public GameObject spawnObject;

    [Header("生成間隔 (秒)")]
    public float interval = 0.4f;

    [Header("生成したオブジェクトの寿命 (秒)")]
    public float lifeTime = 2f; // 2秒後に削除

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (spawnObject != null)
            {
                GameObject obj = Instantiate(spawnObject, transform.position, transform.rotation);

                // ⬇ 生成したオブジェクトを lifeTime 秒後に削除
                Destroy(obj, lifeTime);
            }
            else
            {
                Debug.LogWarning("spawnObjectが設定されていません。");
            }

            yield return new WaitForSeconds(interval);
        }
    }
}
