using BlackRose.Test;
using UnityEditor;
using UnityEngine;

namespace BlackRose.Editors
{
    [CustomEditor(typeof(AutoShooter))]
    public class AutoShooterEditor : Editor
    {
        private AutoShooter _shooter;
        private float _length = 2.5f;
        private bool _rayEnabled = false;
        private void OnEnable()
        {
            _shooter = (AutoShooter)target;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("シュート"))
            {
                if (EditorApplication.isPlaying)
                    _shooter.Shoot(true);
            }

            var q = _shooter.transform.rotation;

        }
    }
}