using UnityEngine;
using UnityEngine.UI;

public class ModeChange : MonoBehaviour
{
    [SerializeField] public PlayerControls _InputAction;
    [SerializeField] private Image _targetImage;
    [SerializeField] private Sprite NORMAL;
    [SerializeField] private Sprite RIGHT;
    [SerializeField] private Sprite HEAVY;
    [SerializeField] private Sprite ActionRobot;
    [SerializeField] private bool IsAR;
    [SerializeField] private bool IsStory;

    void Start()
    {
        _InputAction = new PlayerControls();//InputActionのインスタンスを生成
        _InputAction.Enable();
        if(IsAR)
        {
            _targetImage.sprite = ActionRobot;
        }
        else
        {
            _targetImage.sprite = NORMAL;
        }

    }

    void Update()
    {
        if(IsAR || IsStory) return;
        
        if (_InputAction.Player.ModeChange1.triggered) // C
        {
            switch (_targetImage.sprite.name)//HEAVY
            {
                case "NORMAL":
                    _targetImage.sprite = HEAVY;
                    break;
                case "RIGHT":
                    _targetImage.sprite = HEAVY;
                    break;
                case "HEAVY":
                    _targetImage.sprite = NORMAL;
                    break;
            }
        }
        if (_InputAction.Player.ModeChange2.triggered)// V
        {
            switch (_targetImage.sprite.name)
            {
                case "NORMAL":
                    _targetImage.sprite = RIGHT;
                    break;
                case "RIGHT":
                    _targetImage.sprite = NORMAL;
                    break;
                case "HEAVY":
                    _targetImage.sprite = RIGHT;
                    break;
            }
        }
    }
    public void Story()
    {
        IsStory =! IsStory;
    }
}
