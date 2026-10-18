using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using rmf_claude.DOTweenUI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIAnimLab.EditorTools
{
    /// <summary>
    /// Authoring animations from code, as plain data. Everything here writes exactly the fields a person
    /// would set in the Inspector, so a built animation is indistinguishable from a hand-made one.
    ///
    ///     Anim.New("Show",
    ///         S.Alpha("Stage/Panel").From(0f).To(1f),
    ///         S.Scale("Stage/Panel").With().From(V3(0.8f)).ToBase(Vector3.zero).Ease(Ease.OutBack))
    /// </summary>
    public static class Anim
    {
        private static readonly FieldInfo animationsField = typeof(UIAnimationPlayer).GetField("Animations", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo sharedField = typeof(UIAnimationPlayer).GetField("Shared", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo useUnscaledField = typeof(UIAnimationPlayer).GetField("UseUnscaledTime", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo killOnDisableField = typeof(UIAnimationPlayer).GetField("KillOnDisable", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo sharedOnCompleteField = typeof(UIAnimationPlayer).GetField("SharedOnComplete", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>A new animation with what `+` on an empty list gives: Interrupt Others on, COMPLETE, REPORT.</summary>
        public static UIAnimation New(string name, params UIAnimationStep[] steps)
        {
            var animation = new UIAnimation
            {
                Name = name,
                Steps = new List<UIAnimationStep>(steps),
                InterruptOthers = true,
                CompleteInterrupted = true,
                CompleteSilently = false,
                ApplyFromValuesImmediately = true,
                OnComplete = new UnityEvent(),
            };
            return animation;
        }

        public static void Set(UIAnimationPlayer player, params UIAnimation[] animations)
        {
            animationsField.SetValue(player, new List<UIAnimation>(animations));
            EditorUtility.SetDirty(player);
        }

        public static void Add(UIAnimationPlayer player, params UIAnimation[] animations)
        {
            var list = (List<UIAnimation>)animationsField.GetValue(player);
            list.AddRange(animations);
            EditorUtility.SetDirty(player);
        }

        public static List<UIAnimation> Of(UIAnimationPlayer player)
        {
            return (List<UIAnimation>)animationsField.GetValue(player);
        }

        public static void SetShared(UIAnimationPlayer player, UIAnimationAsset asset)
        {
            sharedField.SetValue(player, asset);
            EditorUtility.SetDirty(player);
        }

        public static void Options(UIAnimationPlayer player, bool unscaledTime, bool killOnDisable)
        {
            useUnscaledField.SetValue(player, unscaledTime);
            killOnDisableField.SetValue(player, killOnDisable);
            EditorUtility.SetDirty(player);
        }

        /// <summary>A player's own On Complete for a shared animation, as Add On Complete On This Player makes it.</summary>
        public static UnityEvent SharedOnComplete(UIAnimationPlayer player, UIAnimation shared)
        {
            var list = (List<UIAnimationSharedOnComplete>)sharedOnCompleteField.GetValue(player);
            var entry = new UIAnimationSharedOnComplete { AnimationId = shared.Id, AnimationName = shared.Name, OnComplete = new UnityEvent() };
            list.Add(entry);
            EditorUtility.SetDirty(player);
            return entry.OnComplete;
        }

        /// <summary>Creates an Animation Set, or rewrites the one already at path (keeping its GUID).</summary>
        public static UIAnimationAsset Asset(string path, params UIAnimation[] animations)
        {
            LabAssets.Folder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
            var asset = AssetDatabase.LoadAssetAtPath<UIAnimationAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<UIAnimationAsset>();
                asset.Animations = new List<UIAnimation>(animations);
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                asset.Animations = new List<UIAnimation>(animations);
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssetIfDirty(asset);
            return asset;
        }

        // ------------------------------------------------------------ animation options

        public static UIAnimation Note(this UIAnimation a, string notes) { a.Notes = notes; return a; }
        public static UIAnimation Loop(this UIAnimation a, int loops, LoopType type = LoopType.Restart) { a.Loops = loops; a.LoopType = type; return a; }
        public static UIAnimation Fps(this UIAnimation a, float fps) { a.PlayAtCustomFPS = true; a.FPS = fps; return a; }
        public static UIAnimation SnapAll(this UIAnimation a) { a.Snapping = true; a.SnapPerStep = false; return a; }
        public static UIAnimation SnapPerStep(this UIAnimation a) { a.Snapping = true; a.SnapPerStep = true; return a; }
        public static UIAnimation NoFromImmediately(this UIAnimation a) { a.ApplyFromValuesImmediately = false; return a; }
        public static UIAnimation NoInterrupt(this UIAnimation a) { a.InterruptOthers = false; return a; }
        public static UIAnimation InterruptStop(this UIAnimation a) { a.InterruptOthers = true; a.CompleteInterrupted = false; return a; }
        public static UIAnimation InterruptComplete(this UIAnimation a, bool silent = false) { a.InterruptOthers = true; a.CompleteInterrupted = true; a.CompleteSilently = silent; return a; }
        public static UIAnimation Id(this UIAnimation a, string id) { a.Id = id; return a; }

        /// <summary>The old default, as data authored before the COMPLETE/STOP buttons existed reads.</summary>
        public static UIAnimation Legacy(this UIAnimation a) { a.CompleteInterrupted = false; a.CompleteSilently = false; return a; }

        public static UIAnimation Then(this UIAnimation a, UIAnimationPlayer player, string next)
        {
            UnityEventTools.AddStringPersistentListener(a.OnComplete, player.PlayAnimation, next);
            return a;
        }

        public static UIAnimation OnDone(this UIAnimation a, UnityAction action)
        {
            UnityEventTools.AddVoidPersistentListener(a.OnComplete, action);
            return a;
        }

        public static UIAnimation OnDone(this UIAnimation a, UnityAction<string> action, string argument)
        {
            UnityEventTools.AddStringPersistentListener(a.OnComplete, action, argument);
            return a;
        }

        public static UIAnimation OnDone(this UIAnimation a, UnityAction<bool> action, bool argument)
        {
            UnityEventTools.AddBoolPersistentListener(a.OnComplete, action, argument);
            return a;
        }
    }

    /// <summary>Step factories, one per type. The path is the step's Target Path; null leaves the step on the player's own object.</summary>
    public static class S
    {
        public static Vector3 V3(float all) { return new Vector3(all, all, all); }
        public static Vector3 V3(float x, float y, float z = 0f) { return new Vector3(x, y, z); }

        public static UIAnimationStep Of(UIAnimationStepType type, string path = null)
        {
            var step = new UIAnimationStep { Type = type, TargetPath = path ?? string.Empty };
            step.Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            step.Waypoints = new List<Vector3>();
            if (type == UIAnimationStepType.Scale) step.ToVector = Vector3.one;
            return step;
        }

        public static UIAnimationStep Pos(string path = null) { return Of(UIAnimationStepType.AnchoredPosition, path); }
        public static UIAnimationStep LocalPos(string path = null) { return Of(UIAnimationStepType.LocalPosition, path); }
        public static UIAnimationStep Rot(string path = null) { return Of(UIAnimationStepType.Rotation, path); }
        public static UIAnimationStep Scale(string path = null) { return Of(UIAnimationStepType.Scale, path); }
        public static UIAnimationStep Size(string path = null) { return Of(UIAnimationStepType.SizeDelta, path); }
        public static UIAnimationStep OffMin(string path = null) { return Of(UIAnimationStepType.OffsetMin, path); }
        public static UIAnimationStep OffMax(string path = null) { return Of(UIAnimationStepType.OffsetMax, path); }
        public static UIAnimationStep Alpha(string path = null) { return Of(UIAnimationStepType.CanvasGroupAlpha, path); }
        public static UIAnimationStep GAlpha(string path = null) { return Of(UIAnimationStepType.GraphicAlpha, path); }
        public static UIAnimationStep Tint(string path = null) { return Of(UIAnimationStepType.GraphicColor, path); }

        public static UIAnimationStep MatFloat(string path, string property)
        {
            var s = Of(UIAnimationStepType.MaterialFloat, path);
            s.ShaderProperty = property;
            return s;
        }

        public static UIAnimationStep MatColor(string path, string property)
        {
            var s = Of(UIAnimationStepType.MaterialColor, path);
            s.ShaderProperty = property;
            return s;
        }

        public static UIAnimationStep Active(string path, bool value)
        {
            var s = Of(UIAnimationStepType.SetActive, path);
            s.ActiveValue = value;
            return s;
        }

        public static UIAnimationStep Sound(AudioClip clip, float volume = 1f, float pitch = 1f, float variation = 0f, string path = null)
        {
            var s = Of(UIAnimationStepType.PlaySound, path);
            s.Clip = clip;
            s.Volume = volume;
            s.Pitch = pitch;
            s.PitchVariation = variation;
            return s;
        }

        public static UIAnimationStep PunchPos(string path, Vector3 punch, float duration = 0.4f, int vibrato = 10, float elasticity = 1f)
        {
            return Impulse(UIAnimationStepType.PunchAnchoredPosition, path, punch, duration, vibrato, elasticity);
        }

        public static UIAnimationStep PunchRot(string path, Vector3 punch, float duration = 0.4f, int vibrato = 10, float elasticity = 1f)
        {
            return Impulse(UIAnimationStepType.PunchRotation, path, punch, duration, vibrato, elasticity);
        }

        public static UIAnimationStep PunchScale(string path, Vector3 punch, float duration = 0.25f, int vibrato = 10, float elasticity = 1f)
        {
            return Impulse(UIAnimationStepType.PunchScale, path, punch, duration, vibrato, elasticity);
        }

        public static UIAnimationStep ShakePos(string path, float strength, float duration = 0.4f, int vibrato = 10, float randomness = 90f)
        {
            var s = Impulse(UIAnimationStepType.ShakeAnchoredPosition, path, Vector3.zero, duration, vibrato, 1f);
            s.ToFloat = strength;
            s.Randomness = randomness;
            return s;
        }

        public static UIAnimationStep ShakeRot(string path, Vector3 strength, float duration = 0.4f, int vibrato = 10, float randomness = 90f)
        {
            var s = Impulse(UIAnimationStepType.ShakeRotation, path, strength, duration, vibrato, 1f);
            s.Randomness = randomness;
            return s;
        }

        public static UIAnimationStep ShakeScale(string path, Vector3 strength, float duration = 0.5f, int vibrato = 10, float randomness = 90f)
        {
            var s = Impulse(UIAnimationStepType.ShakeScale, path, strength, duration, vibrato, 1f);
            s.Randomness = randomness;
            return s;
        }

        private static UIAnimationStep Impulse(UIAnimationStepType type, string path, Vector3 value, float duration, int vibrato, float elasticity)
        {
            var s = Of(type, path);
            s.ToVector = value;
            s.Duration = duration;
            s.Vibrato = vibrato;
            s.Elasticity = elasticity;
            return s;
        }

        /// <summary>A Custom Property step on componentType's member, as the Property dropdown sets it.</summary>
        public static UIAnimationStep Prop(string path, System.Type componentType, string member, UIAnimationPropertyKind kind)
        {
            var s = Of(UIAnimationStepType.CustomProperty, path);
            s.PropertyComponent = componentType.FullName;
            s.PropertyMember = member;
            s.PropertyKind = kind;
            if (kind == UIAnimationPropertyKind.Color) s.ToColor = Color.white;
            return s;
        }

        // ------------------------------------------------------------ fluent setters

        public static UIAnimationStep With(this UIAnimationStep s) { s.Start = UIAnimationStartMode.WithPrevious; return s; }
        public static UIAnimationStep Dur(this UIAnimationStep s, float seconds) { s.Duration = seconds; return s; }
        public static UIAnimationStep Delay(this UIAnimationStep s, float seconds) { s.Delay = seconds; return s; }
        public static UIAnimationStep Ease(this UIAnimationStep s, Ease ease) { s.EaseType = ease; s.UseCustomCurve = false; return s; }
        public static UIAnimationStep Curve(this UIAnimationStep s, AnimationCurve curve) { s.Curve = curve; s.UseCustomCurve = true; return s; }
        public static UIAnimationStep Snap(this UIAnimationStep s, bool on = true) { s.Snapping = on; return s; }
        public static UIAnimationStep Format(this UIAnimationStep s, string format) { s.NumberFormat = format; return s; }

        public static UIAnimationStep Path(this UIAnimationStep s, UIAnimationPathShape shape, params Vector3[] points)
        {
            s.UseCustomPath = true;
            s.PathShape = shape;
            s.Waypoints = new List<Vector3>(points);
            return s;
        }

        /// <summary>Fills the step's target slot (whichever one its type uses) with o.</summary>
        public static UIAnimationStep Target(this UIAnimationStep s, Object o)
        {
            var go = o as GameObject;
            var c = o as Component;
            if (go == null && c != null) go = c.gameObject;

            switch (UIAnimationStep.TargetKindOf(s.Type))
            {
                case UIAnimationTargetKind.Rect: s.RectTarget = go.GetComponent<RectTransform>(); break;
                case UIAnimationTargetKind.CanvasGroup: s.CanvasGroupTarget = go.GetComponent<CanvasGroup>(); break;
                case UIAnimationTargetKind.Graphic: s.GraphicTarget = go.GetComponent<Graphic>(); break;
                case UIAnimationTargetKind.Material: s.MaterialTarget = go.GetComponent<UIMaterialInstance>(); break;
                case UIAnimationTargetKind.GameObject: s.ActiveTarget = go; break;
                case UIAnimationTargetKind.Audio: s.AudioSourceTarget = go.GetComponent<AudioSource>(); break;
                case UIAnimationTargetKind.Property: s.PropertyTarget = go; break;
            }

            return s;
        }

        // From: turns the FROM button on. Absolute unless the Base variant is used.
        public static UIAnimationStep From(this UIAnimationStep s, float v) { s.UseFrom = true; s.FromMode = UIAnimationEndpointMode.Absolute; s.FromFloat = v; return s; }
        public static UIAnimationStep From(this UIAnimationStep s, Vector3 v) { s.UseFrom = true; s.FromMode = UIAnimationEndpointMode.Absolute; s.FromVector = v; return s; }
        public static UIAnimationStep From(this UIAnimationStep s, Color v) { s.UseFrom = true; s.FromMode = UIAnimationEndpointMode.Absolute; s.FromColor = v; return s; }
        public static UIAnimationStep From(this UIAnimationStep s, string v) { s.UseFrom = true; s.FromMode = UIAnimationEndpointMode.Absolute; s.FromText = v; return s; }
        public static UIAnimationStep FromBase(this UIAnimationStep s, float v) { From(s, v); s.FromMode = UIAnimationEndpointMode.Baseline; return s; }
        public static UIAnimationStep FromBase(this UIAnimationStep s, Vector3 v) { From(s, v); s.FromMode = UIAnimationEndpointMode.Baseline; return s; }
        public static UIAnimationStep FromBase(this UIAnimationStep s, Color v) { From(s, v); s.FromMode = UIAnimationEndpointMode.Baseline; return s; }
        public static UIAnimationStep FromBase(this UIAnimationStep s, string v) { From(s, v); s.FromMode = UIAnimationEndpointMode.Baseline; return s; }

        public static UIAnimationStep To(this UIAnimationStep s, float v) { s.ToMode = UIAnimationEndpointMode.Absolute; s.ToFloat = v; return s; }
        public static UIAnimationStep To(this UIAnimationStep s, Vector3 v) { s.ToMode = UIAnimationEndpointMode.Absolute; s.ToVector = v; return s; }
        public static UIAnimationStep To(this UIAnimationStep s, Color v) { s.ToMode = UIAnimationEndpointMode.Absolute; s.ToColor = v; return s; }
        public static UIAnimationStep To(this UIAnimationStep s, string v) { s.ToMode = UIAnimationEndpointMode.Absolute; s.ToText = v; return s; }
        public static UIAnimationStep ToBase(this UIAnimationStep s, float v) { To(s, v); s.ToMode = UIAnimationEndpointMode.Baseline; return s; }
        public static UIAnimationStep ToBase(this UIAnimationStep s, Vector3 v) { To(s, v); s.ToMode = UIAnimationEndpointMode.Baseline; return s; }
        public static UIAnimationStep ToBase(this UIAnimationStep s, Color v) { To(s, v); s.ToMode = UIAnimationEndpointMode.Baseline; return s; }
        public static UIAnimationStep ToBase(this UIAnimationStep s, string v) { To(s, v); s.ToMode = UIAnimationEndpointMode.Baseline; return s; }
        public static UIAnimationStep ToCur(this UIAnimationStep s, float v) { To(s, v); s.ToMode = UIAnimationEndpointMode.Current; return s; }
        public static UIAnimationStep ToCur(this UIAnimationStep s, Vector3 v) { To(s, v); s.ToMode = UIAnimationEndpointMode.Current; return s; }
        public static UIAnimationStep ToCur(this UIAnimationStep s, Color v) { To(s, v); s.ToMode = UIAnimationEndpointMode.Current; return s; }

        /// <summary>A Baseline To of zero: land on the resting value.</summary>
        public static UIAnimationStep ToRest(this UIAnimationStep s)
        {
            s.ToMode = UIAnimationEndpointMode.Baseline;
            s.ToVector = Vector3.zero;
            s.ToFloat = 0f;
            s.ToColor = new Color(0f, 0f, 0f, 0f);
            s.ToText = string.Empty;
            return s;
        }
    }
}
