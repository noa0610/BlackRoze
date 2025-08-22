using System;
using UnityEditor;
using UnityEngine;

namespace BlackRose.Core
{
    [CustomEditor(typeof(CameraBounds))]
    public class CameraBoundsEditor : Editor
    {
        private SerializedProperty boundsProp;

        private void OnEnable()
        {
            // _bounds Rect プロパティをキャッシュ
            boundsProp = serializedObject.FindProperty("_bounds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(boundsProp, new GUIContent("Camera Bounds"));
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();

            var boundsComp = (CameraBounds)target;
            Rect bounds = boundsProp.rectValue;

            // 四隅のワールド座標（Z はコンポーネントの Z 座標を使用）
            float z = boundsComp.transform.position.z;
            Vector3 leftUp = new Vector3(bounds.xMin, bounds.yMax, z);
            Vector3 rightUp = new Vector3(bounds.xMax, bounds.yMax, z);
            Vector3 rightDown = new Vector3(bounds.xMax, bounds.yMin, z);
            Vector3 leftDown = new Vector3(bounds.xMin, bounds.yMin, z);

            float size = HandleUtility.GetHandleSize(boundsComp.transform.position) * 0.1f;
            Handles.color = Color.yellow;

            // 各コーナー移動ハンドル
            leftUp = DrawCornerHandle(leftUp, size, newX => bounds.xMin = newX, newY => bounds.yMax = newY);
            rightUp = DrawCornerHandle(rightUp, size, newX => bounds.xMax = newX, newY => bounds.yMax = newY);
            leftDown = DrawCornerHandle(leftDown, size, newX => bounds.xMin = newX, newY => bounds.yMin = newY);
            rightDown = DrawCornerHandle(rightDown, size, newX => bounds.xMax = newX, newY => bounds.yMin = newY);

            // 四辺を描画
            Handles.DrawLine(leftUp, rightUp);
            Handles.DrawLine(rightUp, rightDown);
            Handles.DrawLine(rightDown, leftDown);
            Handles.DrawLine(leftDown, leftUp);

            // 変更をプロパティに反映
            boundsProp.rectValue = bounds;
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// XY 平面で移動可能なハンドルを描画し、X/Y の変更をコールバックで受け取る
        /// </summary>
        private Vector3 DrawCornerHandle(Vector3 pos, float size, Action<float> onChangedX, Action<float> onChangedY)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.FreeMoveHandle(pos, size, Vector3.zero, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move CameraBounds Corner");
                onChangedX(newPos.x);
                onChangedY(newPos.y);
                EditorUtility.SetDirty(target);
            }
            return newPos;
        }
    }
}
// unicode