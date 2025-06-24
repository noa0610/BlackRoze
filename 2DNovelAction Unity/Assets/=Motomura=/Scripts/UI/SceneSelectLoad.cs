using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;


//＝＝＝＝ボタンを押した際ロード画面を挟んだのち引数によって行先が変えれるスクリプト＝＝＝＝


public class SceneSelectLoad : MonoBehaviour
{
    [SerializeField] private RectTransform TargetObject; // ターゲットオブジェクト
    [SerializeField] private Vector3 ENDPosition; // ターゲットオブジェクトの最終的な位置
    [SerializeField] private float AnimationTime = 1f; // アニメーション時間
    [SerializeField] private string LoadEffectName = "Load_GFF"; // ロードシーン名



public void OnButtonClick(string  _AfterLoadingSceneName)//引数の中はロード後のシーン名を指定
    {
        if (TargetObject == null)//nullの場合はアニメーションなしでロードシーンへ
        {
            Debug.Log("ターゲットオブジェクトがNullのためアニメーションなしで実行します。");
            Load();
        }
        else//アタッチされてる場合スライドアニメーションを実行したのちロードシーンへ
        {
            Debug.Log("アニメーションが終わり次第シーン遷移します。");
            TargetObject.DOAnchorPos(ENDPosition, AnimationTime).OnComplete(() =>//現在は座標指定式スライドアニメーション
            {
                Load();
            });
        }
        PercentManager.NextSceneName = _AfterLoadingSceneName; // 次のシーン名を指定
    }

    private void Load()
    {
        Debug.Log("シーン遷移します。");
        SceneManager.LoadScene(LoadEffectName); // ロードシーンへ遷移
    }
}
