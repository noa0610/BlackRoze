using HighElixir.Timers;
using HighElixir.Timers.Internal;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HighElixir.Editor
{
    public class TimerWatcher : EditorWindow
    {
        private class Wrapper
        {
            public Type Parent;
            public TimerSnapshot Snapshot;

            public Wrapper(Type type, TimerSnapshot snapshot)
            {
                Parent = type;
                Snapshot = snapshot;
            }

            public static List<Wrapper> FromSnapshots(Type parent, IEnumerable<TimerSnapshot> snapshots)
            {
                var list = new List<Wrapper>();
                foreach (var snap in snapshots)
                {
                    list.Add(new Wrapper(parent, snap));
                }
                return list;
            }
        }


        private enum SortMode
        {
            ParentType,
            Id,
            TimerClass,
            Current,
            Initialize,
            IsRunning
        }
        private SortMode _sortMode = SortMode.ParentType;
        private bool _sortAscending = true;
        private Vector2 _scroll;
        private Color _currentColor = Color.clear;
        private Type _lastParentType = null;
        private Dictionary<Type, Color> _typeColorMap = new Dictionary<Type, Color>();

        [MenuItem("HighElixir/Timer")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TimerWatcher));
        }

        private void Print(string one, string two, string three, float four, string five, string six, bool seven, bool isUp, Color color = default)
        {
            Rect rect = EditorGUILayout.BeginHorizontal();

            // 背景を塗る
            if (color != default)
                EditorGUI.DrawRect(rect, color);

            // 行の内容
            EditorGUILayout.LabelField(one, GUILayout.Width(120));
            EditorGUILayout.LabelField(two, GUILayout.Width(100));
            EditorGUILayout.LabelField(three, GUILayout.Width(120));
            var text = $"{five:0.00}/{six:0.00}";
            if (isUp)
            {
                four = 1f;
                text = $"{five:0.00}";
            }
            EditorGUI.ProgressBar(
                EditorGUILayout.GetControlRect(GUILayout.Width(200)),
                four,
                text
            );
            EditorGUILayout.LabelField(seven ? "▶" : "■", GUILayout.Width(30));

            EditorGUILayout.EndHorizontal();

        }
        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (Application.isPlaying)
            {


                // ソートモード管理
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SortMode:", GUILayout.Width(60));
                if (GUILayout.Button(_sortMode.ToString(), GUILayout.Width(120)))
                {
                    _sortMode++;
                    if (!Enum.IsDefined(typeof(SortMode), _sortMode))
                        _sortMode = SortMode.ParentType;
                }
                var ascText = _sortAscending ? "Ascending" : "Descending";
                if (GUILayout.Button(ascText, GUILayout.Width(120)))
                {
                    _sortAscending = !_sortAscending;
                }
                EditorGUILayout.EndHorizontal();

                // ヘッダー
                Print(
                    "ParentType",
                    "ID",
                    "Timer",
                    1f,
                   "Current",
                    "",
                    true,
                    true);

                // データ表示
                int count = 0;
                var rTimers = new List<IReadOnlyTimer>(Timer.AllTimers);
                List<Wrapper> timers = new();
                foreach (var timer in rTimers)
                {
                    count += timer.CommandCount;
                    timers.AddRange(Wrapper.FromSnapshots(timer.ParentType, timer.GetSnapshot()));
                }

                // ソート
                switch (_sortMode)
                {
                    case SortMode.ParentType:
                        if (_sortAscending)
                            timers.Sort((a, b) => a.Parent.Name.CompareTo(b.Parent.Name));
                        else
                            timers.Sort((a, b) => b.Parent.Name.CompareTo(a.Parent.Name));
                        break;
                    case SortMode.Id:
                        if (_sortAscending)
                            timers.Sort((a, b) => a.Snapshot.Id.CompareTo(b.Snapshot.Id));
                        else
                            timers.Sort((a, b) => b.Snapshot.Id.CompareTo(a.Snapshot.Id));
                        break;
                    case SortMode.TimerClass:
                        if (_sortAscending)
                            timers.Sort((a, b) => a.Snapshot.TimerClass.CompareTo(b.Snapshot.TimerClass));
                        else
                            timers.Sort((a, b) => b.Snapshot.TimerClass.CompareTo(a.Snapshot.TimerClass));
                        break;
                    case SortMode.Current:
                        if (_sortAscending)
                            timers.Sort((a, b) => a.Snapshot.Current.CompareTo(b.Snapshot.Current));
                        else
                            timers.Sort((a, b) => b.Snapshot.Current.CompareTo(a.Snapshot.Current));
                        break;
                    case SortMode.Initialize:
                        if (_sortAscending)
                            timers.Sort((a, b) => a.Snapshot.Initialize.CompareTo(b.Snapshot.Initialize));
                        else
                            timers.Sort((a, b) => b.Snapshot.Initialize.CompareTo(a.Snapshot.Initialize));
                        break;
                    case SortMode.IsRunning:
                        if (_sortAscending)
                            timers.Sort((a, b) => a.Snapshot.IsRunning.CompareTo(b.Snapshot.IsRunning));
                        else
                            timers.Sort((a, b) => b.Snapshot.IsRunning.CompareTo(a.Snapshot.IsRunning));
                        break;
                }

                // 表示
                foreach (var timer in timers)
                {
                    // ParentType が変わったときだけ色を切り替え
                    if (_lastParentType != timer.Parent && !_typeColorMap.ContainsKey(timer.Parent))
                    {
                        Color newColor;
                        do
                        {
                            newColor = UnityEngine.Random.ColorHSV(0f, 1f, 0.4f, 0.8f, 0.7f, 1f);
                        } while (newColor == _currentColor);

                        _currentColor = newColor;
                        _typeColorMap[timer.Parent] = newColor;
                        _lastParentType = timer.Parent;
                    }
                    else if (_typeColorMap.ContainsKey(timer.Parent))
                    {
                        _currentColor = _typeColorMap[timer.Parent];
                    }
                    bool isUp = timer.Snapshot.TimerClass.Contains("CountUp");
                    Print(
                        timer.Parent.Name,
                        timer.Snapshot.Id,
                        timer.Snapshot.TimerClass,
                        timer.Snapshot.NormalizedElapsed,
                        $"{timer.Snapshot.Current:0.00}",
                        $"{timer.Snapshot.Initialize:0.00}",
                        timer.Snapshot.IsRunning,
                        isUp,
                        _currentColor);
                }
                Print("LastCommandCount:", count.ToString(), "", 1f, "", "", true, true, _currentColor);
            }
            else
            {
                GUILayout.Label("Enter Play Mode to view timers.", EditorStyles.boldLabel);
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}

