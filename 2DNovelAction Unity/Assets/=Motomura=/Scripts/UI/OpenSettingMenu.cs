using UnityEngine;
using DG.Tweening;


//＝＝＝＝音量設定やキー設定のメニュー表示・非表示を行うスクリプト＝＝＝＝


public class OpenSettingMenu : MonoBehaviour
{
    //宣言
    [SerializeField] private GameObject _SettingMenuObject;//メニュー本体
    [SerializeField] private float _AnimationTime = 0.5f;//何秒アニメーションするか

    void Start()//初期化
    {
        _SettingMenuObject.SetActive(false);
        _SettingMenuObject.transform.localScale = new Vector3(1f, 0f, 1f);
    }

    public void OnButton_OpenSettingMenu()//表示機能
    {
        OpenMenu.isKey = false;//他のメニュー（音量設定など）を開いている際後ろセレクトメニューを消せないようにするためのフラグ
        _SettingMenuObject.SetActive(true);
        _SettingMenuObject.transform.DOScale(new Vector3(1f, 1f, 1f), _AnimationTime);//中央から拡大するようなアニメーション
    }

    public void OnButton_CloseSettingMenu()//非表示機能
    {
        _SettingMenuObject.SetActive(false);
        _SettingMenuObject.transform.localScale = new Vector3(1f, 0f, 0f);
        OpenMenu.isKey = true;
    }
}
