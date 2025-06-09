using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;

public class PercentManager : MonoBehaviour
{
    [Range(0, 100)] private int _percent = 0;
    [SerializeField] private TextMeshProUGUI _ActiveText;
    [SerializeField] private Image _LOADbarImage;
    [SerializeField] private Image _FadeImage; // フェード用のImageコンポーネント
    [SerializeField] public static string NextSceneName; // 次のシーン名を指定
    [SerializeField] private float _fadeDuration = 1f; // フェード時間
    [SerializeField] private float StockTime = 2f; // フェードイン時間
    public string A ;

    private float timer = 0f;
    private bool IsIE = false; 
    private float updateInterval = 0.05f; // パーセントを増やす間隔（秒）

    void Start()
    {//初期化
        Debug.Log(NextSceneName);
        _percent = 0;
        _LOADbarImage.fillAmount = 0f;
        _ActiveText.text = "0%";
         _FadeImage.DOFade(0f, 0f); // 初期状態は透明
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (_percent < 100 && timer >= updateInterval)
        {
            _percent++;
            timer = 0f;
        }
        else if (_percent == 100)
        {            
            _FadeImage.DOFade(1f, _fadeDuration).OnComplete(() =>
            {
                if(!IsIE)
                {
                    Debug.Log("Loading Complete!");
                    StartCoroutine(IE());
                }
            });
        }
        // fillAmountとTextを更新
            _LOADbarImage.fillAmount = _percent / 100f;
            _ActiveText.text = _percent.ToString() + "%";        
    }
    IEnumerator IE()
    {
        IsIE = true;
        Debug.Log("IE");
        yield return new WaitForSeconds(StockTime);
        DOTween.KillAll();
        SceneManager.LoadScene(NextSceneName);
        // NextSceneName = null; // 次のシーン名をリセット
        Debug.Log("complete!!");
        
    }
}