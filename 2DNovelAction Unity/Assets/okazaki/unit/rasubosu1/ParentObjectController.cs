using UnityEngine;

public class ParentObjectController : MonoBehaviour
{
    [SerializeField] public string parentName = "ParentObject"; // 親オブジェクトの名前を指定
    private Transform parentTransform;
    private Vector3 initialLocalPosition;

    void Start()
    {
        // 親オブジェクトを探す
        GameObject parent = GameObject.Find(parentName);

        if (parent != null)
        {
            parentTransform = parent.transform;
            transform.SetParent(parentTransform);
            initialLocalPosition = transform.localPosition;

            Debug.Log($"{gameObject.name} を {parent.name} の子にしました。");
        }
        else
        {
            Debug.LogWarning($"親オブジェクト {parentName} が見つかりませんでした。");
        }
    }

    void Update()
    {
        if (parentTransform == null) return;

        // 親のX軸だけを追従
        Vector3 newPosition = transform.position;
        newPosition.x = parentTransform.position.x;
        transform.position = newPosition;
    }
}
