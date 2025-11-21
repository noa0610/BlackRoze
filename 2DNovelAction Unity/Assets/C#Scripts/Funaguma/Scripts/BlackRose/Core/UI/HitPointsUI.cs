using BlackRose.Core.Models;
using BlackRose.Core.Models.Units;
using TMPro;
using UnityEngine;

namespace BlackRose.Core.UI
{
    [DefaultExecutionOrder(1)]
    public class HitPointsUI : MonoBehaviour
    {
        public UnitBase unit;
        private TMP_Text _text;


        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void FixedUpdate()
        {
            if (!_text) Debug.LogError("textがnull");
            if (!unit) return;
            if (unit.StatusManager == null) return;
            var c = unit.StatusManager.ReadValue(Status.HP);
            var m = unit.StatusManager.ReadValue(Status.MaxHP);
            var t = $"<color=red>{c}</color> / <color=blue>{m}</color>";
            _text.SetText(t);
        }
    }
}