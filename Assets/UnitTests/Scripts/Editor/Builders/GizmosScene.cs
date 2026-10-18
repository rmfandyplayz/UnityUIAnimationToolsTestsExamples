using DG.Tweening;
using rmf_claude.DOTweenUI;
using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UIAnimLab.EditorTools.S;

namespace UIAnimLab.UnitTests.Builders
{
    /// <summary>10 Scene Gizmos and Path Editor: the Scene view drawings, their draggable ends, and Edit Path in Scene.</summary>
    public static class GizmosScene
    {
        private const string GizmoSet = UT.Sets + "Gizmo Set.asset";

        [MenuItem(UT.Menu + "10 Scene Gizmos and Path Editor")]
        public static void Build()
        {
            var set = Anim.Asset(GizmoSet, Anim.New("Shared Slide", Pos(null).ToBase(V3(160f, 0f)).Dur(0.5f)));

            var b = UT.Begin("10 Scene Gizmos and Path Editor", "10  Scene Gizmos and Path Editor",
                "What the Scene view draws for a selected player: movement lines, resizing outlines, line styles per mode, Baseline ghosts, the selected step, the Scene Gizmos settings, " +
                "the draggable start and end handles, and Edit Path in Scene. Work in the Scene view with a tile selected; expand animations in the Inspector to draw them (the default Show is Expanded Animations).");

            Chained(b);
            LineStyles(b);
            DragPosition(b);
            SizeHandles(b);
            ScaleHandles(b);
            OffsetHandles(b);
            Outlines(b);
            ShowSetting(b);
            SelectedStep(b);
            PathEditor(b);
            PathAfterDip(b);
            Overlap(b);
            LookOnly(b, set);

            b.Save();
        }

        private const StationNeeds Scene = StationNeeds.SceneView | StationNeeds.Inspector;

        private static void Chained(LabBoard b)
        {
            var t = b.Test("Z1", "Chained moves");
            var dot = UI.Icon(t.Stage, "Dot", LabAssets.Circle, new Vector2(-150f, -70f), 30f, Pal.Yellow);
            t.Player(Anim.New("Tour",
                Pos(UT.Path("Dot")).ToCur(V3(300f, 0f)).Dur(0.4f),
                Pos(UT.Path("Dot")).ToCur(V3(0f, 140f)).Dur(0.4f),
                Pos(UT.Path("Dot")).ToCur(V3(-300f, 0f)).Dur(0.4f),
                Pos(UT.Path("Dot")).ToRest().Dur(0.4f)));

            t.Summary("Four moves in a row, none with a From. Each is drawn from where the one before leaves the dot.")
             .Watch("this tile's player, Dot", t.Go, dot.gameObject)
             .Needs(Scene | StationNeeds.Preview)
             .HowTo("Select the tile and expand Tour. Look at the Scene view. Play Tour.")
             .Expect("A closed loop: three dotted lines (Current) with arrows, each starting where the last ended, and a dashed fourth line (Baseline) back to the start. The drawing holds still while the preview moves the dot.");
        }

        private static void LineStyles(LabBoard b)
        {
            var t = b.Test("Z2", "Line styles and Baseline ghosts");
            var abs = UI.Icon(t.Stage, "Absolute", LabAssets.Circle, new Vector2(-200f, 70f), 26f, Pal.Blue);
            var bas = UI.Icon(t.Stage, "Baseline", LabAssets.Circle, new Vector2(-200f, 0f), 26f, Pal.Green);
            var cur = UI.Icon(t.Stage, "Current", LabAssets.Circle, new Vector2(-200f, -70f), 26f, Pal.Orange);
            var box = UI.Square(t.Stage, "Box", new Vector2(150f, -40f), new Vector2(60f, 40f), Pal.Purple);

            t.Player(
                Anim.New("Absolute", Pos(UT.Path("Absolute")).To(V3(60f, 70f)).Dur(0.4f)),
                Anim.New("Baseline", Pos(UT.Path("Baseline")).To(V3(-100f, 30f)).Dur(0.3f), Pos(UT.Path("Baseline")).ToBase(V3(220f, 0f)).Dur(0.4f)),
                Anim.New("Current", Pos(UT.Path("Current")).ToCur(V3(260f, 0f)).Dur(0.4f)),
                Anim.New("Box From To", Size(UT.Path("Box")).FromBase(V3(-30f, -20f)).To(V3(140f, 90f)).Dur(0.4f)));

            t.Summary("A line's style shows the mode of the end it leads to: solid Absolute, dashed Baseline, dotted Current.")
             .Watch("this tile's player", t.Go)
             .Needs(Scene)
             .HowTo("Expand all four animations and look at the Scene view.")
             .Expect("Absolute: solid. Current: dotted. Baseline: step 1 dashed, with a faint cross at the Baseline dot's rest and a faint dashed line from there to the end. Box From To: first outline dashed (From Baseline) with a faint dashed ghost of the box at rest, last outline solid (To Absolute), the outlines between solid.");
        }

        private static void DragPosition(LabBoard b)
        {
            var t = b.Test("Z3", "Dragging a move's ends");
            var a = UI.Icon(t.Stage, "Ball A", LabAssets.Circle, new Vector2(-180f, 60f), 30f, Pal.Blue);
            var c = UI.Icon(t.Stage, "Ball B", LabAssets.Circle, new Vector2(-180f, -60f), 30f, Pal.Pink);

            t.Player(
                Anim.New("With From", Pos(UT.Path("Ball A")).FromBase(V3(40f, 0f)).ToBase(V3(300f, 0f)).Dur(0.5f)),
                Anim.New("Current Snapped", Pos(UT.Path("Ball B")).ToCur(V3(300f, 0f)).Dur(0.5f)).SnapAll());

            t.Summary("A move's To dot and (with a From) its From ring can be dragged; the value keeps its mode.")
             .Watch("this tile's player", t.Go)
             .Needs(Scene | StationNeeds.Preview)
             .HowTo("Expand both. Hover the dot and ring of With From (a white ring and a name like 'With From, step 1: To' appear). Drag both, then Play. Drag Current Snapped's dot and let go. Ctrl+Z.")
             .Expect("Dropped ends land exactly where let go when played. With From's values stay Baseline; Current Snapped's stays Current and is rounded to whole numbers only on release. Each drag is one Undo step. Current Snapped has no ring to drag (no From).");
        }

        private static void SizeHandles(LabBoard b)
        {
            var t = b.Test("Z4", "Size Delta handles");
            var abs = UI.Square(t.Stage, "Abs", new Vector2(-180f, 0f), new Vector2(60f, 60f), Pal.Teal);
            var bas = UI.Square(t.Stage, "Base", new Vector2(0f, 0f), new Vector2(60f, 60f), Pal.Green);
            var snap = UI.Square(t.Stage, "Snap", new Vector2(180f, 0f), new Vector2(60f, 60f), Pal.Orange);

            t.Player(
                Anim.New("Absolute", Size(UT.Path("Abs")).To(V3(100f, 120f)).Dur(0.4f)),
                Anim.New("Baseline With From", Size(UT.Path("Base")).FromBase(V3(-30f, -30f)).ToBase(V3(40f, 20f)).Dur(0.4f)),
                Anim.New("Current Snapped", Size(UT.Path("Snap")).ToCur(V3(30f, 50f)).Dur(0.4f)).SnapAll());

            t.Summary("The last outline of a resizing step has corner and edge dots that change To; with a From, the first has rings that change From.")
             .Watch("this tile's player", t.Go)
             .Needs(Scene | StationNeeds.Preview)
             .HowTo("Expand all three. Drag corners and edges of each, with and without Shift. Play each.")
             .Expect("Each box grows around its pivot (centre), so dragging the right edge moves the left one too. Shift on a corner keeps the box's shape; on an edge the other side grows with it. Played back, each lands on the dropped outline. Modes are kept; Current Snapped rounds on release.");
        }

        private static void ScaleHandles(LabBoard b)
        {
            var t = b.Test("Z5", "Scale handles");
            var pop = UT.Box(t, "Pop", new Vector2(-170f, 0f), Pal.Pink, 60f);
            var tilted = UT.Box(t, "Tilted", new Vector2(10f, 0f), Pal.Blue, 60f);
            tilted.rectTransform.localEulerAngles = new Vector3(0f, 0f, 25f);
            var deep = UT.Box(t, "Deep", new Vector2(180f, 0f), Pal.Purple, 60f);
            deep.rectTransform.localScale = new Vector3(1f, 1f, 3f);

            t.Player(
                Anim.New("Pop In", S.Scale(UT.Path("Pop")).From(Vector3.zero).ToRest().Dur(0.4f).Ease(Ease.OutBack)),
                Anim.New("Tilted Grow", S.Scale(UT.Path("Tilted")).To(V3(1.6f, 1.2f, 1f)).Dur(0.4f)),
                Anim.New("Z Kept", S.Scale(UT.Path("Deep")).To(V3(1.5f, 1.5f, 3f)).Dur(0.4f)));

            t.Summary("Scale outlines have handles too: a pop-in from scale 0, a tilted box, and one with scale Z 3.")
             .Watch("this tile's player", t.Go)
             .Needs(Scene | StationNeeds.Preview)
             .HowTo("Expand all three. Drag Pop In's From rings out of the point they start as. Drag Tilted Grow's edges. Drag Z Kept's corner and read its To.")
             .Expect("Pop In's From grows outwards in whichever direction you pull (never a negative scale). The tilted outline's handles follow its tilt. Z Kept's Z stays 3 whatever you drag.");
        }

        private static void OffsetHandles(LabBoard b)
        {
            var t = b.Test("Z6", "Offset Min / Max handles, bottom-left pivot");
            var frame = UI.Square(t.Stage, "Frame", Vector2.zero, new Vector2(460f, 200f), Pal.Ghost);
            var panel = UI.Square(frame.transform, "Panel", Vector2.zero, Vector2.zero, Pal.Teal);
            panel.rectTransform.anchorMin = Vector2.zero;
            panel.rectTransform.anchorMax = Vector2.one;
            panel.rectTransform.offsetMin = new Vector2(40f, 30f);
            panel.rectTransform.offsetMax = new Vector2(-40f, -30f);
            var corner = UI.Square(t.Stage, "Corner Pivot", new Vector2(150f, -60f), new Vector2(60f, 40f), Pal.Orange);
            corner.rectTransform.pivot = Vector2.zero;

            t.Player(
                Anim.New("Min", OffMin("Stage/Frame/Panel").To(V3(80f, 50f)).Dur(0.4f)),
                Anim.New("Max", OffMax("Stage/Frame/Panel").To(V3(-120f, -20f)).Dur(0.4f)),
                Anim.New("Pivot Grow", Size(UT.Path("Corner Pivot")).To(V3(120f, 80f)).Dur(0.4f)));

            t.Summary("Offset Min only moves the bottom-left corner, Offset Max only the top-right. A size step on a bottom-left pivot grows up and right.")
             .Watch("this tile's player", t.Go)
             .Needs(Scene | StationNeeds.Preview)
             .HowTo("Expand all three and look at the handles. Drag them. Play each.")
             .Expect("Min has handles only on the bottom-left corner and the left and bottom edges; Max only top-right. Pivot Grow has no handles on its left and bottom edges (they sit on the pivot and can't change anything). Everything lands where dropped.");
        }

        private static void Outlines(LabBoard b)
        {
            var t = b.Test("Z7", "Resizing outlines and Outlines count");
            var box = UI.Square(t.Stage, "Box", Vector2.zero, new Vector2(60f, 40f), Pal.Green);
            t.Player(
                Anim.New("Grow", Size(UT.Path("Box")).To(V3(300f, 160f)).Dur(0.6f).Ease(Ease.OutBack)),
                Anim.New("Shrink", Size(UT.Path("Box")).From(V3(300f, 160f)).ToRest().Dur(0.6f)));

            t.Summary("A resizing step is drawn as outlines at several moments, fading in from first to last.")
             .Watch("this tile's player", t.Go)
             .Needs(Scene)
             .HowTo("Expand Grow. Open Scene Gizmos (under the preview buttons) and move Outlines from 2 to 12.")
             .Expect("Grow's outlines fade in towards the end, and OutBack's overshoot shows as an outline bigger than the last. The number of outlines follows the setting. With an Out ease the last moments bunch up near the end, and no outline is drawn within a few points of an end.");
        }

        private static void ShowSetting(LabBoard b)
        {
            var t = b.Test("Z8", "Scene Gizmos: Show and Colors");
            var dot = UI.Icon(t.Stage, "Dot", LabAssets.Circle, Vector2.zero, 28f, Pal.Yellow);
            t.Player(
                Anim.New("Left", Pos(UT.Path("Dot")).ToCur(V3(-200f, 0f)).Dur(0.4f)),
                Anim.New("Up", Pos(UT.Path("Dot")).ToCur(V3(0f, 100f)).Dur(0.4f)),
                Anim.New("Loop", Pos(UT.Path("Dot")).ToCur(V3(150f, 80f)).Dur(0.4f), Pos(UT.Path("Dot")).ToCur(V3(50f, -160f)).Dur(0.4f), Pos(UT.Path("Dot")).ToRest().Dur(0.4f)),
                Anim.New("Diagonal", Pos(UT.Path("Dot")).ToCur(V3(200f, -100f)).Dur(0.4f)));

            t.Summary("Four animations from one dot. The Scene Gizmos settings choose what's drawn and how it's coloured.")
             .Watch("this tile's player", t.Go)
             .Needs(Scene)
             .HowTo("Collapse all four. Open Scene Gizmos and try each Show: Disabled, All Animations, Expanded Animations, Expanded Steps, Selected Step. Expand Loop and some of its steps. Try Colors: Distinct, Shuffle Colors, Rainbow.")
             .Expect("All draws all four from rest even collapsed; Expanded Animations only the expanded ones; Expanded Steps only open steps; Disabled nothing. Distinct gives each a clearly different colour and Shuffle changes them. Rainbow colours red to purple by step, and only applies with Expanded Steps.");
        }

        private static void SelectedStep(LabBoard b)
        {
            var t = b.Test("Z9", "The selected step");
            var steps = new System.Collections.Generic.List<UIAnimationStep>();
            Color[] colors = { Pal.Blue, Pal.Green, Pal.Orange, Pal.Pink, Pal.Purple };
            for (int i = 0; i < 5; i++)
            {
                UI.Square(t.Stage, "Member " + (i + 1), new Vector2(-160f + i * 80f, 0f), new Vector2(50f, 50f), colors[i]);
                steps.Add(S.Scale(UT.Path("Member " + (i + 1))).To(V3(1.6f, 1.6f, 1f)).Dur(0.4f));
                if (i > 0) steps[i].With();
            }
            UI.Square(t.Stage, "Still", new Vector2(0f, -80f), new Vector2(40f, 40f), Pal.Grey);
            steps.Add(S.Scale(UT.Path("Still")).ToRest().Dur(0.3f));

            t.Player(Anim.New("Crowd", steps.ToArray()));

            t.Summary("Five scale steps whose outlines overlap, and a sixth that goes nowhere (Still, scaled to the size it already is).")
             .Watch("this tile's player", t.Go)
             .Needs(Scene)
             .HowTo("Expand Crowd. Click on step 3's header, then on a field of step 4, then on the drag-handle margin of step 6. Set Show to Selected Step, then back.")
             .Expect("The last-clicked step gets a blue bar down its left in the Inspector, and in the Scene view is drawn on top with a dark halo, even step 6 which goes nowhere. Selected Step draws it alone. Collapsing the animation still draws the selected step.");
        }

        private static void PathEditor(LabBoard b)
        {
            var t = b.Test("Z10", "Edit Path in Scene");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-220f, -60f), 30f, Pal.Teal);
            t.Player(Anim.New("Swoop",
                Pos(UT.Path("Ball")).To(V3(220f, 60f)).Dur(1.2f).Ease(Ease.InOutSine).Path(UIAnimationPathShape.Curved, V3(-80f, 90f), V3(80f, -80f))));

            t.Summary("A path's middle points are edited in the Scene view, like Unity's Edit Collider.")
             .Watch("this tile's player, Ball", t.Go, ball.gameObject)
             .Needs(Scene | StationNeeds.Preview)
             .HowTo("Open Swoop's step and press <b>Edit Path in Scene</b>. Drag the numbered points, From/To, click a small + on the line, Ctrl+click a point, click empty space, press Esc. Undo the drags.")
             .Expect("While editing, the move tool is hidden and clicking empty space does nothing (nothing deselects). + adds a point there; Ctrl+click turns a point red and removes it. Every drag is one Undo step. Esc (or Done) stops editing and the move tool comes back. The line is solid (To is Absolute).");
        }

        private static void PathAfterDip(LabBoard b)
        {
            var t = b.Test("Z11", "A path drawn from where it really starts");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-200f, 40f), 30f, Pal.Orange);
            t.Player(Anim.New("Dip Then Arc",
                Pos(UT.Path("Ball")).ToCur(V3(0f, -100f)).Dur(0.4f),
                Pos(UT.Path("Ball")).ToCur(V3(400f, 60f)).Dur(1f).Path(UIAnimationPathShape.Curved, V3(200f, 140f))));

            t.Summary("Step 1 is a path with no From. It starts where step 0's dip leaves the ball, and must be drawn from there.")
             .Watch("this tile's player, Ball", t.Go, ball.gameObject)
             .Needs(Scene | StationNeeds.Preview)
             .HowTo("Press Edit Path in Scene on step 1, then Play the animation.")
             .Expect("The path starts at the bottom of the dip, not at the ball's resting place, and it stays put while the preview moves the ball.");
        }

        private static void Overlap(LabBoard b)
        {
            var t = b.Test("Z12", "Handles on top of each other");
            var drawer = UI.Square(t.Stage, "Drawer", new Vector2(-150f, 0f), new Vector2(60f, 60f), Pal.Blue);
            t.Player(
                Anim.New("Open", Pos(UT.Path("Drawer")).ToBase(V3(300f, 0f)).Dur(0.4f)),
                Anim.New("Close", Pos(UT.Path("Drawer")).FromBase(V3(300f, 0f)).ToRest().Dur(0.4f)));

            t.Summary("Open's end and Close's start sit on the same point.")
             .Watch("this tile's player", t.Go)
             .Needs(Scene)
             .HowTo("Expand both. Hover the shared point. Click Close's step in the Inspector and hover again. Drag it.")
             .Expect("The name that shows is the handle you'll drag. Once Close's step is selected it's drawn on top, and dragging moves Close's From, not Open's To.");
        }

        private static void LookOnly(LabBoard b, UIAnimationAsset set)
        {
            var t = b.Test("Z13", "Where handles can't be dragged");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-160f, 50f), 30f, Pal.Green);
            var player = t.PlayerOn(ball.gameObject);
            Anim.SetShared(player, set);

            var flatParent = UI.Rect(t.Stage, "Flattened", new Vector2(100f, -50f), new Vector2(10f, 10f));
            flatParent.localScale = new Vector3(0f, 1f, 1f);
            var flat = UI.Icon(flatParent, "Ball Inside", LabAssets.Circle, Vector2.zero, 30f, Pal.Pink);
            t.PlayerOn(flat.gameObject, Anim.New("Slide", Pos(null).ToBase(V3(100f, 0f)).Dur(0.4f)));

            t.Summary("Ball's player uses Gizmo Set (Shared Slide); Ball Inside sits under a parent scaled to 0 on X.")
             .Watch("Ball, Ball Inside", ball, flat)
             .Needs(Scene)
             .HowTo("Select Ball and expand Shared Slide under Shared Animations; try to drag its dot. Then select Gizmo Set.asset, set Preview On to Ball, and drag there. Select Ball Inside.")
             .Expect("Under Shared Animations the drawing is look-only. From the set's Inspector, with Preview On, the dot drags and changes the set. Ball Inside draws nothing draggable and throws no errors.");
        }
    }
}
