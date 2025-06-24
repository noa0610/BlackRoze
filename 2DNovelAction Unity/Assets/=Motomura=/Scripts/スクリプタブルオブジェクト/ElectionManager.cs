using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ＝＝＝＝ランダムで選ばれたデータを表示するスクリプト＝＝＝＝

public class ElectionManager : MonoBehaviour
{
    [Header("ScriptableObjectの参照")]
    public VariationData variationData;

    [SerializeField] private TextMeshProUGUI _Name;
    [SerializeField] private TextMeshProUGUI _Info;
    [SerializeField] private Image _CharacterPortrait;
    void Start()
    {
        DrawRandomData();
    }

    void DrawRandomData()
    {
        int randomIndex = Random.Range(0, variationData.dataList.Count); //要素分ランダムでデータを選択
        LoadData selectedData = variationData.dataList[randomIndex];// 選択されたデータを取得
        Debug.Log("選択されたデータ: " + randomIndex + " - " + selectedData.NameText);
        _Name.text = selectedData.NameText;// 選択されたデータの名前を表示
        _Info.text = selectedData.infoText;// 選択されたデータの情報を表示
        _CharacterPortrait.sprite = selectedData.Character_image;// 選択されたデータのキャラクター画像を表示
    }
}

