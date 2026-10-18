using System;
using System.Collections.Generic;
using System.Text;
using DG.DOTweenEditor;
using DG.Tweening;
using rmf_claude.DOTweenUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIAnimLab.EditorTools
{
    /// <summary>
    /// A quick regression sweep over test and showcase scenes, with no play mode: opens each scene
    /// additively, resolves every UI Animation Player, plays every animation to its end with DOTween's
    /// manual update, and reports every warning, error and exception logged on the way. The scene is
    /// closed without saving, so nothing it touches is kept.
    ///
    /// Steps that are broken on purpose (the warning tiles) show up in the report; anything else is news.
    /// </summary>
    public static class SceneSmokeCheck
    {
        [MenuItem("Tools/UI Anim Lab/Smoke Check All Scenes", priority = 20)]
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
            var report = new StringBuilder();
            var logs = new List<string>();
            Application.LogCallback listen = (message, stack, type) =>
            {
                if (type != LogType.Log) logs.Add(type + ": " + message.Split('\n')[0]);
            };

            var existing = SceneManager.GetSceneByPath(path);
            if (existing.IsValid() && existing.isLoaded) return path + ": skipped, it is open.";

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            int players = 0, animations = 0;
            Application.logMessageReceived += listen;

            try
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    // Inactive players never start up in play mode, so they are left out.
                    foreach (var player in root.GetComponentsInChildren<UIAnimationPlayer>())
                    {
                        players++;
                        logs.Add("@ " + Where(player));
                        int before = logs.Count;

                        player.EditorPrepareForPreview();
                        var names = new List<string>();
                        player.EditorCollectAnimationNames(names);

                        foreach (var name in names)
                        {
                            animations++;
                            PlayThrough(player, name, logs);
                        }

                        player.StopAll();
                        player.EditorRestoreBaselines();
                        player.EditorReleaseMaterials();

                        if (logs.Count == before) logs.RemoveAt(logs.Count - 1);
                    }
                }
            }
            finally
            {
                Application.logMessageReceived -= listen;
                EditorSceneManager.CloseScene(scene, true);
            }

            report.Append(path).Append(": ").Append(players).Append(" players, ").Append(animations).Append(" animations");
            int problems = 0;
            foreach (var l in logs) if (!l.StartsWith("@ ")) problems++;
            report.Append(", ").Append(problems).AppendLine(" log entries");
            foreach (var l in logs) report.Append("    ").AppendLine(l);
            return report.ToString();
        }

        private static void PlayThrough(UIAnimationPlayer player, string name, List<string> logs)
        {
            try
            {
                UnityEngine.Random.InitState(1);
                var sequence = player.Play(name);
                if (sequence == null)
                {
                    logs.Add("Play(\"" + name + "\") returned null");
                    return;
                }

                DOTweenEditorPreview.PrepareTweenForPreview(sequence, true, true, false);

                // Endless loops have no end: a few seconds of them is enough.
                float total = sequence.Loops() < 0 ? 3f : Mathf.Min(30f, sequence.Duration(true));
                float t = 0f;
                while (t < total + 0.05f)
                {
                    DOTween.ManualUpdate(0.05f, 0.05f);
                    t += 0.05f;
                }

                player.StopAll();
            }
            catch (Exception e)
            {
                logs.Add("Exception playing '" + name + "': " + e.GetType().Name + ": " + e.Message);
            }
        }

        private static string Where(Component c)
        {
            var t = c.transform;
            string path = t.name;
            while (t.parent != null && t.parent.parent != null)
            {
                t = t.parent;
                if (t.GetComponent<Station>() != null)
                {
                    path = t.name + " > " + path;
                    break;
                }
            }

            return path;
        }
    }
}
