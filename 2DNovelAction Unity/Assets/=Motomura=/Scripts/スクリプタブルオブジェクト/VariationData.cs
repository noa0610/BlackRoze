using System.Collections.Generic;
using UnityEngine;

// ＝＝＝＝ロードで表示するキャラクターデータ格納機＝＝＝＝

[CreateAssetMenu(fileName = "LoadVariation", menuName = "ScriptableObject/LoadVariationData")]
public class VariationData : ScriptableObject
{
    public List<LoadData> dataList = new List<LoadData>();
}

[System.Serializable]
public class LoadData 
{
    public Sprite Character_image;
    public string NameText;
    [SerializeField, MultilineAttribute(3)]
    public string infoText;
}