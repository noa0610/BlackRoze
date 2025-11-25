using BlackRose.Core.Models.Systems;
using BlackRose.Core.Models.Units;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class ModeChange : MonoBehaviour, IPlayerFollower
{
    [SerializeField] public PlayerControls _InputAction;
    [SerializeField] private Image _targetImage;
    [SerializeField] private Sprite NORMAL;
    [SerializeField] private Sprite RIGHT;
    [SerializeField] private Sprite HEAVY;
    [SerializeField] private Sprite ActionRobot;
    [SerializeField] private bool IsAR;
    [SerializeField] private bool IsStory;
    private AIController _ai;
    private void Start()
    {
        if (IsAR)
        {
            _targetImage.sprite = ActionRobot;
        }
        else
        {
            _targetImage.sprite = NORMAL;
        }
    }

    public void Story()
    {
        IsStory = !IsStory;
    }

    public void SetTarget(UnitBase target)
    {
        if (target is AIController ai)
        {
            _ai = ai;
            this.UpdateAsObservable().Subscribe(_ => {
                if (IsAR || IsStory) return;
                if (_ai.CurrentMode is NormalMode)
                {
                    _targetImage.sprite = NORMAL;
                }
                else if (_ai.CurrentMode is LightMode)
                {
                    _targetImage.sprite = RIGHT;
                }
                else if (_ai.CurrentMode is HeavyMode)
                {
                    _targetImage.sprite = HEAVY;
                }
            }).AddTo(this);
        }
    }
}
