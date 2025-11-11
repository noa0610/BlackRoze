using HighElixir.Timers;
using UnityEditor;
using UnityEngine.UIElements;

namespace HighElixir.Editors.Timers
{
    [CustomEditor(typeof(GlobalTimerDriver))]
    public class TimerDriverEditor : Editor
    {
        private SerializedProperty _updateEnable;
        private SerializedProperty _fixedUpdateEnable;

        private void OnEnable()
        {
            var so = serializedObject;
            _updateEnable = so.FindProperty("_updateEnable");
            _fixedUpdateEnable = so.FindProperty("_fixedUpdateEnable");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new Label("Update"));
            var button_u = new Button(() =>
            {
                _updateEnable.boolValue = !_updateEnable.boolValue;
            });
            button_u.text = _updateEnable.boolValue ? " 停止" : "再生";
            root.Add(button_u);
            root.Add(new Label("FixedUpdate"));
            var button_f = new Button(() =>
            {
                _fixedUpdateEnable.boolValue = !_fixedUpdateEnable.boolValue;
            });
            button_f.text = _updateEnable.boolValue ? " 停止" : "再生";
            root.Add(button_f);
            return root;
        }
    }
}