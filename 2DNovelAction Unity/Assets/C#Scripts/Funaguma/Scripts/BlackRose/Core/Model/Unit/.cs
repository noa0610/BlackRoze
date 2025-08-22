using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MosquiteRemover : MonoBehaviour
{
    // どんな状況でも軽い（１次配列のため）が、
    // 非アクティブなオブジェクトは削除できない
    public void RemoveMosquiteWithTag()
    {
        var objects = GameObject.FindGameObjectsWithTag("Mosquite");
        foreach (var obj in objects)
        {
            Destroy(obj);
        }
    }

    // オブジェクトの総数が多い場合など重くなる可能性はあるが、
    // 非アクティブなオブジェクトも削除できる
    public void RemoveMosquiteWithRecursion()
    {
        var objects = SceneManager.GetActiveScene().GetRootGameObjects();
        Action<GameObject> recursion = null;
        recursion = obj => 
        { 
            if (obj.transform.childCount > 0)
            {
                foreach (Transform child in obj.transform)
                {
                    recursion(child.gameObject);
                }
            }
            else if (obj.CompareTag("Mosquite"))
            {
                Destroy(obj);
            }
        };
        foreach (var obj in objects)
        {
            recursion(obj);
        }
    }
}