#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
namespace HighElixir.Editor
{

    public sealed class TimeHoldersWatcherWindow : EditorWindow
    {
        [SerializeField] private Object _picked;       // 任意の Object (GO/Component/SO)
        private ITimerUser _singleTarget;              // 単体ターゲット
        private readonly List<ITimerUser> _sceneUsers = new(); // シーン内 ITimerUser
        private int _mask = 0;                         // 複数監視用 MaskField
        private Vector2 _scroll;
        private string _filter = "";
        private bool _sortByRemaining = false;

        // 軽量化：0.25秒ごと再描画
        private double _nextUpdate;

        // 色分けしきい値（必要に応じて調整可）
        private static readonly float DangerSeconds = 2f;
        private static readonly float WarnSeconds = 5f;

        [MenuItem("Window/HighElixir/TimeHolders Watcher")]
        public static void Open()
        {
            GetWindow<TimeHoldersWatcherWindow>("TimeHolders").Show();
        }

        // 外部から一発指定
        public void SetTarget(ITimerUser user)
        {
            _singleTarget = user;
            Repaint();
        }
        public void SetTargetFromObject(Object obj)
        {
            _picked = obj;
            _singleTarget = ResolveITimerUser(obj);
            Repaint();
        }

        private void OnEnable()
        {
            EditorApplication.update += TickUpdate;
            RefreshSceneUsers();
            if (_picked) _singleTarget = ResolveITimerUser(_picked);
        }
        private void OnDisable()
        {
            EditorApplication.update -= TickUpdate;
        }
        private void TickUpdate()
        {
            if (EditorApplication.timeSinceStartup >= _nextUpdate)
            {
                _nextUpdate = EditorApplication.timeSinceStartup + 0.25; // 0.25s
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawToolbar();

            // 1) 単体ターゲットがいれば最優先で表示
            if (_singleTarget != null && _singleTarget.Timers != null)
            {
                EditorGUILayout.LabelField("Single Target", EditorStyles.boldLabel);
                DrawTimersSection(_singleTarget, indent: 0);
                EditorGUILayout.Space(6);
            }

            // 2) MaskField で選ばれた複数ターゲットを並列表示
            var actives = ActiveUsersFromMask();
            if (actives.Count > 0)
            {
                EditorGUILayout.LabelField("Multi Targets", EditorStyles.boldLabel);
                foreach (var u in actives)
                    DrawTimersSection(u, indent: 0);
            }

            if (_singleTarget == null && actives.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "上の ObjectField に ITimerUser をドラッグ＆ドロップするか、" +
                    "Scan Scene → Targets で監視対象を選んでね。",
                    MessageType.Info);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // 任意のオブジェクトを受け取る
                var next = EditorGUILayout.ObjectField(_picked, typeof(Object), true, GUILayout.Width(280));
                if (next != _picked)
                {
                    _picked = next;
                    _singleTarget = ResolveITimerUser(_picked);
                }

                if (GUILayout.Button("Scan Scene", EditorStyles.toolbarButton, GUILayout.Width(90)))
                {
                    RefreshSceneUsers();
                }

                GUILayout.Space(8);

                // MaskField：複数監視
                using (new EditorGUI.DisabledScope(_sceneUsers.Count == 0))
                {
                    var labels = _sceneUsers.Select(LabelOfUser).ToArray();
                    _mask = EditorGUILayout.MaskField(_mask, labels, GUILayout.MinWidth(140));
                }

                GUILayout.FlexibleSpace();

                _filter = GUILayout.TextField(_filter, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.Width(180));
                _sortByRemaining = GUILayout.Toggle(_sortByRemaining, "Sort by Remaining", EditorStyles.toolbarButton);
            }
        }

        private List<ITimerUser> ActiveUsersFromMask()
        {
            var result = new List<ITimerUser>();
            for (int i = 0; i < _sceneUsers.Count; i++)
                if ((_mask & (1 << i)) != 0) result.Add(_sceneUsers[i]);
            return result;
        }

        private void DrawTimersSection(ITimerUser user, int indent)
        {
            if (user == null || user.Timers == null) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(HeaderOfUser(user), EditorStyles.miniBoldLabel);

                var snaps = user.Timers.GetSnapshot();

                if (!string.IsNullOrEmpty(_filter))
                    snaps = snaps.Where(s => s.Id.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) >= 0);

                snaps = _sortByRemaining
                    ? snaps.OrderByDescending(s => s.Remaining)
                    : snaps.OrderBy(s => s.Id);

                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(80));
                foreach (var s in snaps)
                    DrawRow(user, s);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawRow(ITimerUser owner, TimeHolders.TimerSnapshot s)
        {
            // 背景色：終了(緑) / 危険(赤) / 注意(黄) / 既定
            var oldBg = GUI.backgroundColor;
            if (s.Remaining <= 0f) GUI.backgroundColor = new Color(0.65f, 1f, 0.65f);
            else if (s.Remaining < DangerSeconds) GUI.backgroundColor = new Color(1f, 0.65f, 0.65f);
            else if (s.Remaining < WarnSeconds) GUI.backgroundColor = new Color(1f, 0.95f, 0.7f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(s.Id, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(s.Running ? "▶ Running" : "■ Stopped", GUILayout.Width(90));
                }

                EditorGUILayout.LabelField($"{s.Remaining:F2}s / {s.Max:F2}s   (elapsed: {(s.NormalizedElapsed * 100f):F1}%)");

                // プログレス（経過率）
                var rect = GUILayoutUtility.GetRect(18, 18, "TextField");
                EditorGUI.ProgressBar(rect, s.NormalizedElapsed, "");

                // 操作ボタン
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Start", GUILayout.Width(60))) owner.Timers.Start(s.Id);
                    if (GUILayout.Button("Stop", GUILayout.Width(60))) owner.Timers.Stop(s.Id);
                    if (GUILayout.Button("Reset", GUILayout.Width(60))) owner.Timers.Reset(s.Id, start: true);
                    GUILayout.FlexibleSpace();
                }
            }

            GUI.backgroundColor = oldBg; // 戻す
        }

        // --- Helpers ---

        private ITimerUser ResolveITimerUser(Object obj)
        {
            if (!obj) return null;

            // 1) そのまま
            if (obj is ITimerUser iu) return iu;

            // 2) GameObject → コンポーネントから拾う
            if (obj is GameObject go)
                return go.GetComponents<MonoBehaviour>().FirstOrDefault(c => c is ITimerUser) as ITimerUser;

            // 3) Component 自身 or 同一GO の別コンポーネント
            if (obj is Component comp)
            {
                if (comp is ITimerUser ic) return ic;
                return comp.GetComponents<MonoBehaviour>().FirstOrDefault(c => c is ITimerUser) as ITimerUser;
            }

            // 4) ScriptableObject も OK（SO に ITimerUser 実装していれば拾える）
            return obj as ITimerUser;
        }

        private void RefreshSceneUsers()
        {
            _sceneUsers.Clear();
            foreach (var mb in GameObject.FindObjectsOfType<MonoBehaviour>(true))
                if (mb is ITimerUser iu) _sceneUsers.Add(iu);

            _sceneUsers.Sort((a, b) => string.Compare(LabelOfUser(a), LabelOfUser(b), System.StringComparison.Ordinal));
            if (_sceneUsers.Count == 0) _mask = 0;
        }

        private static string LabelOfUser(ITimerUser u)
        {
            if (u is Component c)
                return $"[Scene] {GetHierarchyPath(c.transform)} : {u.GetType().Name}";
            return u.GetType().Name;
        }

        private static string HeaderOfUser(ITimerUser u)
        {
            if (u is Component c)
                return $"{c.gameObject.name} ({u.GetType().Name})";
            return u.GetType().Name;
        }

        private static string GetHierarchyPath(Transform t)
        {
            var stack = new Stack<string>();
            while (t != null) { stack.Push(t.name); t = t.parent; }
            return string.Join("/", stack);
        }
    }
}
#endif