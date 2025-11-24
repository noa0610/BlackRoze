using UnityEngine;
using UnityEngine.UI;

public class ModeChange : MonoBehaviour
{
    [SerializeField] public GFFInputAction _InputAction;//InputActionのインスタンス
    [SerializeField] private Image targetImage;
    [SerializeField]private Sprite NORMAL;
    [SerializeField]private Sprite RIGHT;
    [SerializeField]private Sprite HEAVY;

    void Start()
    {
        targetImage.sprite = NORMAL;
        _InputAction = new GFFInputAction();//InputActionのインスタンスを生成
        _InputAction.Enable();//InputActionを有効化
    }

    void Update()
    {
        if (_InputAction.GFF.ChangeMode.triggered)//[W] key
        {
            switch (targetImage.sprite.name)
            {
                case "NORMAL":
                    targetImage.sprite = RIGHT;            //|ライトモード  
                    break;
                case "RIGHT":
                    targetImage.sprite = HEAVY;            //|ヘビーモード
                    break;
                case "HEAVY":
                    targetImage.sprite = NORMAL;           //|ノーマルモード
                    break;
            }
        }
    }
}
