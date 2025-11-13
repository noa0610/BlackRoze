// using UnityEngine;

// public class Spawner : MonoBehaviour
// {
//     public GameObject prefabToSpawn;

//     public void SpawnObject()
//     {
//         // Prefabを生成
//         GameObject obj = Instantiate(prefabToSpawn);

//         // ParentObjectControllerで登録された親があれば、それを親にする
//         if (ParentObjectController.parentTransform != null)
//         {
//             obj.transform.SetParent(ParentObjectController.parentTransform);
//         }

//         Debug.Log($"{obj.name} を {ParentObjectController.parentTransform?.name} の子にしました。");
//     }
// }
