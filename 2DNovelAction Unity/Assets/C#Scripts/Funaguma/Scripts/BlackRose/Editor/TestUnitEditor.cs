using BlackRose.Core.Models.Units;
using UnityEditor;
using UnityEngine;

namespace BlackRose.Editors
{
    [CustomEditor(typeof(TestUnit))]
    public class TestUnitEditor : Editor
    {
        private TestUnit _testUnit;
        private SerializedProperty _basePos;
        private void OnEnable()
        {
            _testUnit = (TestUnit)target;
            _basePos = serializedObject.FindProperty("_basePos");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("ステート実行"))
            {
                if (EditorApplication.isPlaying)
                    _testUnit.Invoke();
            }
            if (GUILayout.Button("リセット"))
            {
                if (EditorApplication.isPlaying)
                    _testUnit.StateReset();
            }
            if (GUILayout.Button("現在の位置を初期値にする"))
            {

                _basePos.vector2Value = _testUnit.transform.position;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}