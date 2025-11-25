using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//＝＝＝＝メニュー全般を制御しています。＝＝＝＝
[System.Serializable]
public class ObjectProperty
{
    [SerializeField] public RectTransform Button;//メニュー本体
    [SerializeField] public Vector2 Start_Position;
    [SerializeField] public Vector2 End_Position;
    [SerializeField] public float AnimationTime = 1.0f; //表示にかかる時間
}


public class Menu : MonoBehaviour
{
    //全般項目
    [Header("全般項目")]
    private bool _Menu = false; //メニューを開いてるか
    private bool _IsKey = true; //Escまたはゲームパットスタートでメニューを開くか
    private string _TitleSceneName = "Title_GFF";//タイトルシーンの名前
    [SerializeField] private Ease _OutEase;//メニュー本体
    [SerializeField] private Ease _InEase;//メニュー本体のアニメーションイージング


    //メインメニュー項目
    [Header("メインメニュー項目")]
    [SerializeField] public GFFInputAction _InputAction;//InputActionのインスタンス
    [SerializeField]
    public List<ObjectProperty> _Objects;
    [SerializeField] private Button MenuButton;//メニューボタン

    //子メニュー項目
    [Header("子メニュー項目")]
    [SerializeField] private GameObject _Setting_MenuObject;//設定メニュー本体
    [SerializeField] private GameObject _Log_MenuObject;//ログメニュー本体
    [SerializeField] private float _AnimationTime = 0.5f; //子メニューの表示にかかる時間
    private Vector3 _StartPos = new Vector3(0, 1080, 0);
    private Vector3 _EndPos = new Vector3(0, 0, 0);


    private void Start()//初期化
    {
        foreach (var obj in _Objects)
        {
            obj.Button.DOAnchorPos(obj.Start_Position, 0f);
        }
        _Setting_MenuObject.SetActive(false);
        try
        {
            _Log_MenuObject.transform.DOLocalMove(_StartPos, 0f);
        }
        catch
        {
            // タイトルにはログメニューが存在しないので、エラーを無視
        }

        _Setting_MenuObject.transform.localScale = new Vector3(1f, 0f, 1f);

        _InputAction = new GFFInputAction();//InputActionのインスタンスを生成
        _InputAction.Enable();//InputActionを有効化
    }

    private void Update()
    {
        if (_IsKey == true)//Escまたはゲームパットスタートでメニューを開くか
        {
            if (_InputAction.GFF.Mene.triggered)
            {
                Judgement(); //メニューの選択を監視
            }
        }

    }

    public void Judgement()
    {
        Debug.Log("メニューの選択を監視中");
        if (SceneManager.GetActiveScene().name == _TitleSceneName)//タイトル画面にいる時はメニューを開けない
        {
            Debug.Log("タイトル画面なのでメニューを開けません");
            return;
        }
        if (_Menu == false)//Escまたはゲームパットスタートが押された際
        {
            Debug.Log("メニューを開く");
            OpenMainMenu();
        }
        else if (_Menu == true)//Escまたはゲームパットスタートが押された際
        {
            Debug.Log("メニューを閉じる");
            CloseMainMenu();
        }
    }

    public void OpenMainMenu()//メインメニュー
    {
        MenuButton.interactable = false; //Button連打対策
        foreach (var obj in _Objects)
        {
            obj.Button.DOAnchorPos(obj.End_Position, obj.AnimationTime).SetEase(_OutEase).OnComplete(() =>
            {
                MenuButton.interactable = true; //Button連打対策解除
                _Menu = true; //メニューを開いている状態にする
                Debug.Log("メニューを開きました");
            });
        }
    }

    public void CloseMainMenu()//メインメニューの非表示
    {
        MenuButton.interactable = false; //Button連打対策
        foreach (var obj in _Objects)
        {
            obj.Button.DOAnchorPos(obj.Start_Position, obj.AnimationTime).SetEase(_InEase).OnComplete(() =>
            {
                MenuButton.interactable = true; //Button連打対策解除
                _Menu = false; //メニューを閉じている状態にする
                Debug.Log("メニューを閉じました");
            });
        }
    }

    public void OpenChildMenu(string MeneName)//設定・操作方法メニュー
    {
        switch (MeneName)
        {
            case "Setting":
                _IsKey = false; //メニューを開くキーを無効化
                _Setting_MenuObject.SetActive(true);
                _Setting_MenuObject.transform.DOScale(new Vector3(1f, 1f, 1f), _AnimationTime);
                break;
            case "Log":
                _IsKey = false; //メニューを開くキーを無効化
                _Log_MenuObject.transform.DOLocalMove(_EndPos, 0f);
                break;
        }
    }
    public void CloseChildMenu(string MeneName)//設定・操作方法メニューの非表示
    {
        switch (MeneName)
        {
            case "Setting":
                _IsKey = true; //メニューを開くキーを無効化
                _Setting_MenuObject.SetActive(false);
                _Setting_MenuObject.transform.localScale = new Vector3(1f, 0f, 1f);
                break;
            case "Log":
                _IsKey = true; //メニューを開くキーを無効化
                _Log_MenuObject.transform.DOLocalMove(_StartPos, 0f);
                break;
        }
    }

}
