using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


//＝＝＝＝メイン画面から一時停止メニューを開く動作＝＝＝＝



public class OpenMenu : MonoBehaviour
{
    //宣言
    [SerializeField] private GameObject _MenuObject;//メニュー本体
    [SerializeField] private Button MenuButton;//メニューボタン
    public static bool isKey = true; //ButtonとEscが押せるかどうかのフラグ
    private GFFInputAction _InputAction;//InputActionのインスタンス




    void Start()//初期化
    {
        _MenuObject.SetActive(false);
        _MenuObject.transform.localScale = Vector3.zero;
        _InputAction = new GFFInputAction();//InputActionのインスタンスを生成
        _InputAction.Enable();//InputActionを有効化
        if(MenuButton == null) return;
    }

    void Update()
    {
        if(isKey == false | SceneManager.GetActiveScene().name == "Title_GFF")//タイトル画面にいる時はESCでメニューを開けない
            return;
        if(_InputAction.GFF.Mene.triggered)//Escまたはゲームパットスタートが押された際
        {
            OnButton_OpenMenu();
            isKey = false;
            if(MenuButton == null) return;
            MenuButton.interactable = false; 
        }

    }

    public void OnButton_OpenMenu()
    {
        if(MenuButton != null) 
        {
            MenuButton.interactable = false; //Button連打対策
        }
        
            _MenuObject.SetActive(!_MenuObject.activeSelf);//メニューの表示・非表示を切り替え
            _MenuObject.transform.DOScale(new Vector3(1f,1f,1f), 0.5f).OnComplete(() =>//メニューのアニメーション開始 =>アニメーション完了後の処理
            {
                if (_MenuObject.activeSelf == false)//メニューが非表示の時
                {
                    _MenuObject.transform.localScale = Vector3.zero;//アニメーションのために初期化
                }
                isKey = true;
                if(MenuButton == null) return;
                MenuButton.interactable = true; 
            });
    }
}
