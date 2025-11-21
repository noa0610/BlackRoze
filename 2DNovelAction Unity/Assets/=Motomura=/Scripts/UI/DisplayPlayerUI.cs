using BlackRose.Core.Models.Units;
using BlackRose.Core.Models.Systems;
using BlackRose.Core.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPlayerUI : MonoBehaviour, IPlayerFollower
{
    [SerializeField]
    private TextMeshProUGUI _HPCount;

    [SerializeField]
    private Image _HPImage;

    private UnitBase _player;

    public void SetTarget(UnitBase target)
    {
        _player = target;
        Debug.Log("SET");
    }

    void Update()
    {
        // if (_statusManager  == null) return;
        var status = _player.StatusManager;
        var _hp = status.ReadValue(Status.HP);
        var _maxhp = status.ReadValue(Status.MaxHP);

        _HPCount.text = $"{_hp}/{_maxhp}";
        _HPImage.fillAmount = _hp / _maxhp;
        Debug.Log($"HPステータス{_hp}/{_maxhp}");
    }
}
