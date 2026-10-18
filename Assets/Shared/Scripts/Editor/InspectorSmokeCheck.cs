using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using rmf_claude.DOTweenUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIAnimLab.EditorTools
{
    /// <summary>
    /// Draws the Inspector of every Station and UI Animation Player in the test and showcase scenes with
    /// everything unfolded, inside a throwaway window, and reports any exception or error logged. Catches a
    /// drawer that chokes on some combination of data no one has opened yet. Nothing is saved.
    /// </summary>
    public class InspectorSmokeCheck : EditorWindow
    {
        private readonly List<UnityEngine.Object> targetsToDraw = new List<UnityEngine.Object>();
        private readonly List<string> problems = new List<string>();

        [MenuItem("Tools/UI Anim Lab/Inspector Check All Scenes", priority = 21)]
        public static void CheckAllMenu()
        {
            Debug.Log(CheckAll());
        }

        public static string CheckAll()
        {
            var report = new StringBuilder();
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/UnitTests/Scenes", "Assets/Showcase/Scenes" }))
            {
                report.AppendLine(Check(AssetDatabase.GUIDToAssetPath(guid)));
            }

            return report.ToString();
        }

        public static string Check(string path)
        {
            var existing = SceneManager.GetSceneByPath(path);
            if (existing.IsValid() && existing.isLoaded) return path + ": skipped, it is open.";

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            var window = CreateInstance<InspectorSmokeCheck>();
            window.titleContent = new GUIContent("UI Anim Lab Inspector Check");
            window.position = new Rect(80f, 80f, 520f, 900f);
            window.ShowUtility();

            Application.LogCallback listen = (message, stack, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                    window.problems.Add(type + ": " + message.Split('\n')[0]);
            };
            Application.logMessageReceived += listen;

            int drawn = 0;
            try
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var station in root.GetComponentsInChildren<Station>(true)) window.targetsToDraw.Add(station);
                    foreach (var player in root.GetComponentsInChildren<UIAnimationPlayer>(true))
                    {
                        Expand(player);
                        window.targetsToDraw.Add(player);
                    }
                }

                drawn = window.targetsToDraw.Count;
                var repaint = typeof(EditorWindow).GetMethod("RepaintImmediately", BindingFlags.Instance | BindingFlags.NonPublic);
                repaint.Invoke(window, null);
                repaint.Invoke(window, null);
            }
            finally
            {
                Application.logMessageReceived -= listen;
                window.Close();
                EditorSceneManager.CloseScene(scene, true);
            }

            var report = new StringBuilder();
            report.Append(path).Append(": ").Append(drawn).Append(" inspectors, ").Append(window.problems.Count).AppendLine(" problems");
            foreach (var p in window.problems) report.Append("    ").AppendLine(p);
            return report.ToString();
        }

        // Unfolds every animation and every step, so the drawer draws every field it can.
        private static void Expand(UIAnimationPlayer player)
        {
            var so = new SerializedObject(player);
            var it = so.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.hasVisibleChildren) it.isExpanded = true;
            }
        }

        private void OnGUI()
        {
            foreach (var target in targetsToDraw)
            {
                if (target == null) continue;
                Editor editor = null;
                try
                {
                    editor = Editor.CreateEditor(target);
                    editor.OnInspectorGUI();
                }
                catch (ExitGUIException)
                {
                }
                catch (Exception e)
                {
                    problems.Add(target.name + " (" + target.GetType().Name + "): " + e.GetType().Name + ": " + e.Message);
                }
                finally
                {
                    if (editor != null) DestroyImmediate(editor);
                }
            }
        }
    }
}
