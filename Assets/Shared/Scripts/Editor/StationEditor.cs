using UnityEditor;
using UnityEngine;

namespace UIAnimLab.EditorTools
{
    /// <summary>
    /// Shows a station's directions as a readable card at the top of its Inspector, with buttons that
    /// select the objects to watch. The raw text fields are folded away under Edit; changing them
    /// re-lays the tile out.
    /// </summary>
    [CustomEditor(typeof(Station))]
    internal class StationEditor : Editor
    {
        private static GUIStyle titleStyle;
        private static GUIStyle bodyStyle;
        private static GUIStyle headStyle;
        private static GUIStyle expectStyle;
        private static GUIStyle badgeStyle;

        private const string EditKey = "UIAnimLab.Station.Edit";

        private static void Styles()
        {
            if (titleStyle != null) return;

            titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15, wordWrap = true, richText = true };
            bodyStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, fontSize = 12 };
            headStyle = new GUIStyle(EditorStyles.miniBoldLabel) { fontSize = 10 };
            headStyle.normal.textColor = new Color(0.5f, 0.75f, 1f);
            expectStyle = new GUIStyle(bodyStyle);
            expectStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(1f, 0.85f, 0.4f) : new Color(0.55f, 0.4f, 0f);
            badgeStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
            badgeStyle.normal.textColor = new Color(1f, 0.6f, 0.35f);
        }

        public override void OnInspectorGUI()
        {
            Styles();
            var station = (Station)target;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    string title = station.Style == StationStyle.Showcase
                        ? station.WikiPage + "  ·  " + station.Title
                        : (string.IsNullOrEmpty(station.Code) ? "" : station.Code + "   ") + station.Title;
                    GUILayout.Label(title, titleStyle);
                }

                string badges = Station.BadgesOf(station.Needs);
                if (badges.Length > 0) GUILayout.Label(badges, badgeStyle);

                Section(null, station.Summary, bodyStyle);

                if (!string.IsNullOrEmpty(station.Watch) || HasWatchObjects(station))
                {
                    Section("WATCH", station.Watch, bodyStyle);
                    WatchButtons(station);
                }

                Section(station.Style == StationStyle.Showcase ? "HOW TO SET IT UP" : "HOW TO TEST", station.HowTo, bodyStyle);
                Section(station.Style == StationStyle.Showcase ? "RESULT" : "EXPECT", station.Expect, expectStyle);
                Section("CAPTURE", station.Capture, expectStyle);
                GUILayout.Space(2f);
            }

            bool edit = SessionState.GetBool(EditKey, false);
            bool now = EditorGUILayout.Foldout(edit, "Edit station text", true);
            if (now != edit) SessionState.SetBool(EditKey, now);
            if (!now) return;

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                foreach (var t in targets)
                {
                    var s = (Station)t;
                    s.Refresh();
                    EditorUtility.SetDirty(s);
                }
            }

            if (GUILayout.Button("Re-lay out tile"))
            {
                foreach (var t in targets) ((Station)t).Refresh();
            }
        }

        private static bool HasWatchObjects(Station station)
        {
            if (station.WatchObjects == null) return false;
            foreach (var o in station.WatchObjects) if (o != null) return true;
            return false;
        }

        private static void Section(string head, string text, GUIStyle style)
        {
            if (string.IsNullOrEmpty(text)) return;
            GUILayout.Space(4f);
            if (head != null) GUILayout.Label(head, headStyle);
            GUILayout.Label(text, style);
        }

        private static void WatchButtons(Station station)
        {
            if (!HasWatchObjects(station)) return;

            float width = EditorGUIUtility.currentViewWidth - 40f;
            float used = 0f;
            EditorGUILayout.BeginHorizontal();
            foreach (var o in station.WatchObjects)
            {
                if (o == null) continue;

                var content = new GUIContent("Select " + o.name, "Selects " + o.name + " and pings it in the Hierarchy.");
                float w = EditorStyles.miniButton.CalcSize(content).x + 6f;
                if (used + w > width && used > 0f)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    used = 0f;
                }

                if (GUILayout.Button(content, EditorStyles.miniButton, GUILayout.Width(w)))
                {
                    Selection.activeGameObject = o;
                    EditorGUIUtility.PingObject(o);
                }

                used += w;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}
