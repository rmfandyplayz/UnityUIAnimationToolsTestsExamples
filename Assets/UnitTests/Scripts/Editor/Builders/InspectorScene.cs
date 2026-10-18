using DG.Tweening;
using rmf_claude.DOTweenUI;
using TMPro;
using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UIAnimLab.EditorTools.S;

namespace UIAnimLab.UnitTests.Builders
{
    /// <summary>07 Inspector and Authoring: the drawer, menus, copy / paste, mirroring, the type check, the pop-out window.</summary>
    public static class InspectorScene
    {
        [MenuItem(UT.Menu + "07 Inspector and Authoring")]
        public static void Build()
        {
            var b = UT.Begin("07 Inspector and Authoring", "07  Inspector and Authoring",
                "Everything about editing animations in the Inspector: step headers, the grouped Type and Ease menus, the one-button rows, what + gives you, copy and paste, mirroring, the warnings a broken step shows, and the pop-out window. " +
                "Most of these are about what you see in the Inspector, so the stage is often just there to play on.");

            Headers(b);
            Menus(b);
            Buttons(b);
            PlusDefaults(b);
            CopyPaste(b);
            Mirror(b);
            MirrorGuesses(b);
            Warnings(b);
            PopOut(b);
            MultiEdit(b);

            b.Save();
        }

        private static void Headers(LabBoard b)
        {
            var t = b.Test("I1", "Step headers");
            var deep = UI.Rect(t.Stage, "Menu", new Vector2(-150f, 0f), new Vector2(10f, 10f));
            var level = UI.Rect(deep, "Panels", Vector2.zero, new Vector2(10f, 10f));
            var row = UI.Rect(level, "Buttons", Vector2.zero, new Vector2(10f, 10f));
            var hover = UI.Box(row, "HoverHighlight", Vector2.zero, new Vector2(120f, 60f), Pal.Purple);
            var self = UT.Box(t, "Self Box", new Vector2(150f, 0f), Pal.Teal, 60f);

            var player = t.PlayerOn(self.gameObject, Anim.New("Everything",
                S.Scale(null).To(V3(1.2f, 1.2f, 1f)).Dur(0.3f),
                GAlpha("../Menu/Panels/Buttons/HoverHighlight").With().To(0.4f).Dur(0.5f).Ease(Ease.InOutSine),
                Sound(LabAssets.Click),
                Active("../Menu", true).Delay(0.2f),
                Alpha(null).To(0f),
                PunchScale(null, V3(0.2f, 0.2f, 0f))));

            t.Summary("Collapsed steps read like a timeline: start, target, (type), duration, ease.")
             .Watch("Self Box (its player)", self)
             .Needs(StationNeeds.Inspector)
             .HowTo("Select Self Box and fold every step of Everything closed. Hover the long one.")
             .Expect("Headers read like 'AFTER [self] (Scale) 0.3s Out Quad'. The long path is shortened from the front ('…/HoverHighlight'), with the full path on hover. Sound, Set Active and punch steps show no ease. The Canvas Group Alpha step ends in a warning sign: Self Box has no Canvas Group.");
        }

        private static void Menus(LabBoard b)
        {
            var t = b.Test("I2", "Type and Ease menus");
            var box = UT.Box(t, "Box", Vector2.zero, Pal.Orange, 70f);
            t.Player(Anim.New("Try Me", S.Scale(UT.Path("Box")).To(V3(1.3f, 1.3f, 1f))));

            t.Summary("Both dropdowns are grouped menus, not alphabetical enum lists.")
             .Watch("this tile's player", t.Go)
             .Needs(StationNeeds.Inspector)
             .HowTo("Open Try Me's step. Open the Type dropdown, then the Ease dropdown. Pick a few, then Undo.")
             .Expect("Type is grouped (moving and sizing, punch and shake, colour and fading, shaders, other) and sorted inside each group. Ease starts with Custom Curve, then Linear and Flash, then In, Out and In Out groups. Picking a type swaps the fields shown; Undo restores everything.");
        }

        private static void Buttons(LabBoard b)
        {
            var t = b.Test("I3", "One-button rows and tooltips");
            var box = UT.Box(t, "Box", Vector2.zero, Pal.Blue, 70f);
            t.Player(Anim.New("Rows",
                Pos(UT.Path("Box")).ToBase(V3(100f, 0f)),
                Pos(UT.Path("Box")).With().ToRest()));

            t.Summary("Several rows are one control over several fields. Each cycles in place and has a tooltip.")
             .Watch("this tile's player", t.Go)
             .Needs(StationNeeds.Inspector)
             .HowTo("On Rows, click each in turn and hover it: Start (AFTER / WITH PREVIOUS), the FROM / TO button, a mode dropdown, Custom Path, Snapping, Play At Custom FPS, Interrupt Others' two buttons. Try it at 125% UI scaling too.")
             .Expect("Start toggles AFTER / WITH PREVIOUS. Snapping cycles DISABLED, ALL STEPS, PER STEP. Play At Custom FPS shows its FPS box on the same row while ticked. Interrupt Others shows STOP/COMPLETE and REPORT/SILENT only while ticked, and REPORT/SILENT only matters on COMPLETE. No label is clipped. The mode dropdown's tooltip lists only the modes it offers.");
        }

        private static void PlusDefaults(LabBoard b)
        {
            var t = b.Test("I4", "What + gives you", 620f, 140f);
            var fresh = UI.Rect(t.Stage, "Fresh Player", Vector2.zero, new Vector2(10f, 10f));
            var player = fresh.gameObject.AddComponent<UIAnimationPlayer>();
            Anim.Set(player);
            UI.Label(t.Stage, "Note", "Fresh Player has no animations yet.", Vector2.zero, new Vector2(500f, 40f), 18f, Pal.BodyText);

            t.Summary("Unity fills a new list entry with zeros. The tool patches what's still zero, and never what you set.")
             .Watch("Fresh Player", fresh.gameObject)
             .Needs(StationNeeds.Inspector)
             .HowTo("Select Fresh Player. Press + on Animations. Then + on its Steps twice. Change the second step's Duration to 0, click elsewhere. Set Loops to 0. Undo everything afterwards.")
             .Expect("The new animation is named New Animation, Loops 1, Apply From Values Immediately ticked, Interrupt Others ticked with COMPLETE and REPORT. The first step has Duration 0.25 and Out Quad; the second is a copy of the first. Duration 0 becomes 0.25 again; Loops 0 becomes 1.");
        }

        private static void CopyPaste(LabBoard b)
        {
            var t = b.Test("I5", "Copy and paste", 620f, 280f);
            var source = UT.Box(t, "Source", new Vector2(-150f, 30f), Pal.Teal, 70f);
            var target = UT.Box(t, "Target", new Vector2(150f, 30f), Pal.Orange, 70f);
            var pinned = UI.Icon(t.Stage, "Pinned", LabAssets.Star, new Vector2(0f, -70f), 50f, Pal.Yellow);

            t.PlayerOn(source.gameObject,
                Anim.New("Show",
                    S.Scale(null).From(Vector3.zero).ToRest().Dur(0.4f).Ease(Ease.OutBack),
                    Sound(LabAssets.Pop).With(),
                    Rot(null).ToCur(V3(0f, 0f, -360f)).Target(pinned).With().Dur(0.6f)),
                Anim.New("Wiggle", ShakeRot(null, V3(0f, 0f, 20f))));
            t.PlayerOn(target.gameObject,
                Anim.New("Show", PunchScale(null, V3(0.3f, 0.3f, 0f))),
                Anim.New("Fade",
                    GAlpha(null).To(0.3f),
                    GAlpha(null).With().ToRest().Delay(0.3f)));

            t.Summary("Right-click headers to copy and paste animations and steps, between players too. Source's Show holds a sound clip and a step dragged onto Pinned.")
             .Watch("Source, Target", source, target, pinned)
             .Needs(StationNeeds.Inspector | StationNeeds.Preview)
             .HowTo("Right-click Source's Show → Copy Animation. On Target, right-click the Animations list → Paste Animation, then right-click Fade → Paste Animation Above. Copy one of Source's steps and Paste Step Below the first of Fade's steps. Play Target's pasted Show. Undo afterwards.")
             .Expect("Pasted animations are named 'Show 2' and 'Show 3' (names stay unique). The pasted Show still has its Pop clip and still spins Pinned. A step pasted under a WITH step joins its group (reads WITH). The menu has one divider above the tool's entries, not two.");
        }

        private static void Mirror(LabBoard b)
        {
            var t = b.Test("I6", "Duplicate as Mirrored", 620f, 300f);
            string[] names = { "One", "Two", "Three" };
            var items = new Component[3];
            var steps = new System.Collections.Generic.List<UIAnimationStep>
            {
                Active(UT.Path("Panel"), true),
                Alpha(UT.Path("Panel")).From(0f).To(1f).Dur(0.3f),
            };
            var panel = UI.Box(t.Stage, "Panel", new Vector2(0f, 10f), new Vector2(440f, 200f), Pal.FramePanel);
            UI.Group(panel);
            for (int i = 0; i < 3; i++)
            {
                items[i] = UI.Box(panel.transform, names[i], new Vector2(0f, 60f - i * 60f), new Vector2(360f, 44f), Pal.Blue);
                UI.Label(items[i].transform, "L", names[i], Vector2.zero, new Vector2(360f, 44f), 18f, Pal.White);
                var move = Pos("Stage/Panel/" + names[i]).FromBase(V3(-300f, 0f)).ToRest().Dur(0.35f).Ease(Ease.OutCubic).Delay(i * 0.1f);
                if (i > 0) move.With();
                steps.Add(move);
            }

            t.Player(Anim.New("Open", steps.ToArray()).Loop(1));

            t.Summary("Open: switch the panel on, fade it in, then slide three rows in, staggered 0.1 s, all with Out Cubic.")
             .Watch("this tile's player, Panel", t.Go, panel.gameObject)
             .Needs(StationNeeds.Inspector | StationNeeds.Preview)
             .HowTo("Right-click Open → <b>Duplicate as Mirrored</b>. Read 'Open Mirrored', then preview Open and then Open Mirrored. Undo afterwards. Then right-click one slide step → Mirror Step, and Undo.")
             .Expect("Open Mirrored runs in reverse: the rows slide out first - Three, then Two, then One (the delays are flipped: Three 0, Two 0.1, One 0.2), each from rest to -300 - then the panel fades to 0, then Set Active switches it off. From and To are swapped; every ease is still Out Cubic. No warning in the Console. Mirror Step flips just that one step.");
        }

        private static void MirrorGuesses(LabBoard b)
        {
            var t = b.Test("I7", "Mirroring has to guess", 620f, 260f);
            var box = UT.Box(t, "Box", new Vector2(-150f, 0f), Pal.Pink, 60f);
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(100f, -60f), 36f, Pal.Yellow);

            t.Player(Anim.New("Go",
                S.Scale(UT.Path("Box")).To(V3(1.5f, 1.5f, 1f)).Dur(0.4f),
                Pos(UT.Path("Ball")).FromBase(Vector3.zero).To(V3(200f, 60f)).Dur(0.8f).Path(UIAnimationPathShape.Linear, V3(100f, 80f), V3(160f, -40f))));

            t.Summary("Step 0 has no From: there's no exact start to mirror onto. Step 1's From is Baseline and its To Absolute: swapping them changes which mode its path points are read in.")
             .Watch("this tile's player", t.Go)
             .Needs(StationNeeds.Inspector | StationNeeds.Console)
             .HowTo("Right-click Go → Duplicate as Mirrored. Read the Console and the new animation. Undo afterwards.")
             .Expect("Two warnings, each naming its step. Step 0 of the mirror is switched to FROM 1.5 with To Baseline 0 (a guess: back to rest). The path's points are reversed but now read in Baseline mode, and the second warning says so.");
        }

        private static void Warnings(LabBoard b)
        {
            var t = b.Test("I8", "The type check", 1284f, 260f);
            var plain = UT.Box(t, "Plain", new Vector2(-400f, 20f), Pal.Grey, 60f);
            UI.Caption(t.Stage, "Plain: an Image, nothing else", new Vector2(-400f, -30f), 220f);
            var glow = UI.Icon(t.Stage, "Glow", LabAssets.Heart, new Vector2(-150f, 20f), 60f, Pal.Pink);
            glow.material = LabAssets.GlowMaterial;
            glow.gameObject.AddComponent<UIMaterialInstance>();
            UI.Caption(t.Stage, "Glow: Lab Glow + UI Material Instance", new Vector2(-150f, -30f), 260f);
            var score = UI.Label(t.Stage, "Score", "120", new Vector2(100f, 20f), new Vector2(120f, 50f), 30f, Pal.Yellow);
            UI.Caption(t.Stage, "Score: a TMP text", new Vector2(100f, -30f), 200f);

            t.Player(Anim.New("Broken",
                Alpha(UT.Path("Plain")).To(0f),
                Pos("Stage/Plian").ToBase(V3(20f, 0f)),
                MatFloat(UT.Path("Glow"), "_Nope").To(1f),
                MatFloat(UT.Path("Plain"), "_Glow").To(1f),
                Sound(null),
                Prop(UT.Path("Score"), typeof(TextMeshProUGUI), "", UIAnimationPropertyKind.Float),
                Prop(UT.Path("Plain"), typeof(TestGauge), "Health", UIAnimationPropertyKind.Float).To(50f),
                Prop(UT.Path("Score"), typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.Float).To(5f),
                Sound(LabAssets.Click, 1f, 1f, 0f, "Stage/Speakerr"),
                PunchScale(UT.Path("Plain"), V3(0.2f, 0.2f, 0f))));

            t.Summary("Every step of Broken but the last is wrong in a different way. Each shows a warning sign, a fading header and a reason; the last step still plays.")
             .Watch("this tile's player", t.Go)
             .Needs(StationNeeds.Inspector | StationNeeds.Preview | StationNeeds.PlayMode | StationNeeds.Console)
             .HowTo("Open Broken's steps and read each warning. Narrow the Inspector: the warnings wrap. Preview Broken. Enter play mode and read the Console.")
             .Expect("In order: 'Plain' has no Canvas Group; Target Path \"Stage/Plian\" matches nothing; Shader has no property '_Nope'; 'Plain' has no UI Material Instance; No Clip; No Property picked; 'Plain' has no TestGauge; TextMeshProUGUI.text is a string, not a float. Fields below each warning are greyed; the fields that fix it (Type, slot, path, Clip, Shader Property) aren't. The sound with a bad path is warned about but not greyed - it still plays, on the shared source. Preview: only Plain's punch moves. Play mode: the Console warns about the broken steps once, at start-up, and never again.");
        }

        private static void PopOut(LabBoard b)
        {
            var t = b.Test("I9", "The pop-out window");
            var box = UT.Box(t, "Box", new Vector2(-120f, 0f), Pal.Purple, 60f);
            t.Player(
                Anim.New("First", Pos(UT.Path("Box")).ToBase(V3(240f, 0f)).Dur(0.6f)),
                Anim.New("Second", Rot(UT.Path("Box")).ToCur(V3(0f, 0f, -180f)).Dur(0.6f)),
                Anim.New("Third", S.Scale(UT.Path("Box")).To(V3(1.4f, 1.4f, 1f)).Dur(0.4f)));

            t.Summary("Pop Out opens one animation in a window of its own that stays whatever you select.")
             .Watch("this tile's player, Box", t.Go, box.gameObject)
             .Needs(StationNeeds.Inspector | StationNeeds.PopOut | StationNeeds.Preview)
             .HowTo("Press Pop Out on Second. Select something else, then this tile again. Rename Second in the window to Turn. Drag First below Turn in the Inspector. Delete Turn from the Inspector, then Undo. Close the window.")
             .Expect("The Inspector shows Second as one line with a Focus button. The window keeps showing it whatever is selected. Renamed or moved, it still finds it. Deleted, it says it's gone; after Undo it finds it again. Play in the window keeps playing while you select other things; closing the window stops and restores it.");
        }

        private static void MultiEdit(LabBoard b)
        {
            var t = b.Test("I10", "Editing two players at once");
            var a = UT.Box(t, "Left", new Vector2(-100f, 0f), Pal.Teal, 60f);
            var c = UT.Box(t, "Right", new Vector2(100f, 0f), Pal.Orange, 60f);
            t.PlayerOn(a.gameObject, Anim.New("Pop", PunchScale(null, V3(0.3f, 0.3f, 0f))));
            t.PlayerOn(c.gameObject, Anim.New("Pop", PunchScale(null, V3(0.3f, 0.3f, 0f))));

            t.Summary("Two players with the same layout, selected together.")
             .Watch("Left, Right", a, c)
             .Needs(StationNeeds.Inspector | StationNeeds.SceneView)
             .HowTo("Select Left and Right together (Ctrl+click). Change Pop's punch Duration. Look for Pop Out. Look at the Scene view.")
             .Expect("The change goes into both players. There is no Pop Out button while two are selected. No errors appear in the Console while the Scene view repaints.");
        }
    }
}
