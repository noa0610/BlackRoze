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
        private readonly Dictionary<Type, Color> _typeColorMap = new();

        [MenuItem("HighElixir/Timer")]
        public static void ShowWindow()
        {
            GetWindow(typeof(TimerWatcher));
        }

        // 1行描画は HorizontalScope を使って確実に Begin/End を合わせる
        private void Print(string one, string two, string three, float four, string five, string six, bool seven, bool isUp, Color color = default)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // 行の矩形を先に確保（幅は伸ばす）
                var rowRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight + 2, GUILayout.ExpandWidth(true));

                // 背景を塗る（必要なら）
                if (color != default)
                    EditorGUI.DrawRect(rowRect, color);

                // 同じ行の中にコントロールをレイアウト
                // ラベル等は上で確保した rowRect と被らないよう、同じ高さで置く
                // ここから実際のUI
                var labelRect = new Rect(rowRect.x, rowRect.y, 120, rowRect.height);
                EditorGUI.LabelField(labelRect, one);

                labelRect.x += 120; labelRect.width = 100;
                EditorGUI.LabelField(labelRect, two);

                labelRect.x += 100; labelRect.width = 120;
                EditorGUI.LabelField(labelRect, three);

                // プログレスバー
                var pbRect = new Rect(labelRect.x + 120, rowRect.y, 200, rowRect.height);
                var text = $"{five:0.00}/{six:0.00}";
                var value = four;
                if (isUp)
                {
                    value = 1f;
                    text = $"{five:0.00}";
                }
                EditorGUI.ProgressBar(pbRect, Mathf.Clamp01(value), text);

                // 再生中フラグ
                var playRect = new Rect(pbRect.x + pbRect.width + 6, rowRect.y, 30, rowRect.height);
                EditorGUI.LabelField(playRect, seven ? "▶" : "■");
            }
        }

        private void OnGUI()
        {
            using (var sv = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = sv.scrollPosition;

                if (!Application.isPlaying)
                {
                    GUILayout.Label("Enter Play Mode to view timers.", EditorStyles.boldLabel);
                    return; // ← ScrollView は scope が閉じてくれる
                }

                // ソートUI
                using (new EditorGUILayout.HorizontalScope())
                {
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
                }

                // ヘッダー
                Print("ParentType", "ID", "Timer", 1f, "Current", "", true, true);

                // データ集計
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
                        timers.Sort((a, b) => _sortAscending ? a.Parent.Name.CompareTo(b.Parent.Name) : b.Parent.Name.CompareTo(a.Parent.Name));
                        break;
                    case SortMode.Id:
                        timers.Sort((a, b) => _sortAscending ? a.Snapshot.Id.CompareTo(b.Snapshot.Id) : b.Snapshot.Id.CompareTo(a.Snapshot.Id));
                        break;
                    case SortMode.TimerClass:
                        timers.Sort((a, b) => _sortAscending ? a.Snapshot.TimerClass.CompareTo(b.Snapshot.TimerClass) : b.Snapshot.TimerClass.CompareTo(a.Snapshot.TimerClass));
                        break;
                    case SortMode.Current:
                        timers.Sort((a, b) => _sortAscending ? a.Snapshot.Current.CompareTo(b.Snapshot.Current) : b.Snapshot.Current.CompareTo(a.Snapshot.Current));
                        break;
                    case SortMode.Initialize:
                        timers.Sort((a, b) => _sortAscending ? a.Snapshot.Initialize.CompareTo(b.Snapshot.Initialize) : b.Snapshot.Initialize.CompareTo(a.Snapshot.Initialize));
                        break;
                    case SortMode.IsRunning:
                        timers.Sort((a, b) => _sortAscending ? a.Snapshot.IsRunning.CompareTo(b.Snapshot.IsRunning) : b.Snapshot.IsRunning.CompareTo(a.Snapshot.IsRunning));
                        break;
                }

                // 表示
                foreach (var timer in timers)
                {
                    // ParentType ごとに色を変える（初登場なら生成）
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
                    else if (_typeColorMap.TryGetValue(timer.Parent, out var col))
                    {
                        _currentColor = col;
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
                        _currentColor
                    );
                }

                Print("LastCommandCount:", count.ToString(), "", 1f, "", "", true, true, _currentColor);
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
