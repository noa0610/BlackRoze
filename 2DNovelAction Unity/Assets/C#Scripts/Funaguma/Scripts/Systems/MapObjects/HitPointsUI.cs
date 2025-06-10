using TMPro;
using UnityEngine;

namespace BlackRose
{
    [DefaultExecutionOrder(1)]
    public class HitPointsUI : MonoBehaviour
    {
        public UnitBase unit;
        private TMP_Text _text;
        private StatusAmount _statusAmount;

        
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _statusAmount = unit.StatusManager.GetStatusAmount(Status.HP);
        }

        private void FixedUpdate()
        {
            if (_statusAmount == null) Debug.LogError("StatusAmountがnull");
            if (!_text) Debug.LogError("textがnull");
            if (_text && _statusAmount != null)
            {
                var t = $"<color=red>{_statusAmount.currentAmount}</color> / <color=blue>{_statusAmount.ChangedMax}</color>";
                _text.SetText(t);
            }
        }
    }
}