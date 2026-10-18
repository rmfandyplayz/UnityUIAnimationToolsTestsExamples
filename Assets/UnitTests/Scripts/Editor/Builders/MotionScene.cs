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
    /// <summary>03 Easing, Paths and Stylised Motion: eases, custom curves, movement paths, Play At Custom FPS, Snapping.</summary>
    public static class MotionScene
    {
        [MenuItem(UT.Menu + "03 Easing, Paths and Stylised Motion")]
        public static void Build()
        {
            var b = UT.Begin("03 Easing, Paths and Stylised Motion", "03  Easing, Paths and Stylised Motion",
                "How a step moves between its ends: ease presets and custom curves, movement paths (Curved and Linear, in each mode, on position, size and scale), Play At Custom FPS and Snapping.");

            EaseRace(b);
            CustomCurve(b);
            CurvedPath(b);
            LinearPath(b);
            CurrentPath(b);
            SizePath(b);
            PathNoPoints(b);
            SteadySpeed(b);
            CustomFps(b);
            FpsDetails(b);
            SnapAll(b);
            SnapPerStep(b);

            b.Save();
        }

        private static void EaseRace(LabBoard b)
        {
            var t = b.Test("M1", "Ease presets", 620f, 320f);
            Ease[] eases = { Ease.Linear, Ease.OutQuad, Ease.InQuad, Ease.InOutQuad, Ease.OutBack, Ease.OutBounce, Ease.OutElastic };
            var steps = new UIAnimationStep[eases.Length];
            var boxes = new Component[eases.Length];
            for (int i = 0; i < eases.Length; i++)
            {
                float y = 130f - i * 40f;
                string name = eases[i].ToString();
                UI.Label(t.Stage, "Label " + name, name, new Vector2(-235f, y), new Vector2(110f, 30f), 15f, Pal.BodyText, TextAlignmentOptions.Right);
                UI.Square(t.Stage, "Lane " + name, new Vector2(40f, y), new Vector2(420f, 2f), Pal.Ghost);
                boxes[i] = UT.Box(t, name, new Vector2(-150f, y), Pal.Teal, 26f);
                steps[i] = Pos(UT.Path(name)).ToBase(V3(360f, 0f)).Dur(1.2f).Ease(eases[i]);
                if (i > 0) steps[i].With();
            }

            UT.Mark(t, "Finish", new Vector2(210f, 10f), new Vector2(4f, 290f));

            var back = new UIAnimationStep[eases.Length];
            for (int i = 0; i < eases.Length; i++)
            {
                back[i] = Pos(UT.Path(eases[i].ToString())).ToRest().Dur(0.3f);
                if (i > 0) back[i].With();
            }

            t.Player(Anim.New("Race", steps), Anim.New("Back", back));

            t.Summary("The same move with seven ease presets, all 1.2 s long.")
             .Watch("one dot per ease", boxes)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Race</b>. Then open a step's Ease dropdown.")
             .Expect("All seven start and finish together on the line. Linear moves steadily, OutQuad starts fast, InQuad starts slow, InOutQuad is slow-fast-slow, OutBack overshoots then settles, OutBounce bounces, OutElastic springs. The dropdown lists Custom Curve first, then Linear and Flash, then one group per direction (In, Out, In Out).");
        }

        private static void CustomCurve(LabBoard b)
        {
            var t = b.Test("M2", "Custom curve");
            var box = UT.Box(t, "Box", new Vector2(-180f, 0f), Pal.Orange, 60f);
            UT.Mark(t, "Start", new Vector2(-180f, 0f), new Vector2(60f, 60f));
            UT.Mark(t, "End", new Vector2(180f, 0f), new Vector2(60f, 60f));

            // Dips back before moving, overshoots, settles.
            var curve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, -1.5f),
                new Keyframe(0.25f, -0.2f),
                new Keyframe(0.75f, 1.15f),
                new Keyframe(1f, 1f, 0f, 0f));

            t.Player(Anim.New("Wind Up",
                Pos(UT.Path("Box")).FromBase(Vector3.zero).ToBase(V3(360f, 0f)).Dur(1f).Ease(Ease.OutQuad).Curve(curve)));

            t.Summary("A hand-drawn curve: 0 is From, 1 is To, and going past them overshoots.")
             .Watch("Box", box)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Wind Up</b>. Then pick <b>Out Quad</b> from the Ease dropdown, play, and pick <b>Custom Curve</b> again.")
             .Expect("With the curve: Box backs up a little to the left, rushes right past the end outline and settles on it. Out Quad plays a plain slide. Switching back to Custom Curve brings the same curve back - nothing was lost either way.");
        }

        private static void CurvedPath(LabBoard b)
        {
            var t = b.Test("M3", "Curved path");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-230f, -90f), 36f, Pal.Yellow);
            UT.Mark(t, "Point 1", new Vector2(-100f, 90f), new Vector2(20f, 20f));
            UT.Mark(t, "Point 2", new Vector2(80f, -60f), new Vector2(20f, 20f));
            UT.Mark(t, "To", new Vector2(230f, 80f), new Vector2(36f, 36f));

            t.Player(
                Anim.New("Arc", Pos(UT.Path("Ball")).To(V3(230f, 80f)).Dur(1.4f).Ease(Ease.InOutSine)
                    .Path(UIAnimationPathShape.Curved, V3(-100f, 90f), V3(80f, -60f))),
                Anim.New("Home", Pos(UT.Path("Ball")).ToRest().Dur(0.4f)));

            t.Summary("Custom Path: Curved travels a smooth curve through every point on the way to To.")
             .Watch("Ball", ball)
             .Needs(StationNeeds.Preview | StationNeeds.SceneView)
             .HowTo("Play <b>Arc</b>, then <b>Home</b>. With the tile selected, look at the Scene view.")
             .Expect("Ball swoops through both small outlines on a smooth curve and lands on the big one. The Scene view draws the same curve with arrows, ring at the start, dot at the end.");
        }

        private static void LinearPath(LabBoard b)
        {
            var t = b.Test("M4", "Linear path");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-230f, -60f), 36f, Pal.Green);
            Vector3[] points = { V3(-150f, 80f), V3(-60f, -80f), V3(30f, 80f), V3(120f, -80f) };
            foreach (var p in points) UT.Mark(t, "Corner " + p.x, p, new Vector2(20f, 20f));
            UT.Mark(t, "To", new Vector2(230f, 60f), new Vector2(36f, 36f));

            t.Player(
                Anim.New("Zigzag", Pos(UT.Path("Ball")).To(V3(230f, 60f)).Dur(1.6f).Ease(Ease.Linear).Path(UIAnimationPathShape.Linear, points)),
                Anim.New("Home", Pos(UT.Path("Ball")).ToRest().Dur(0.4f)));

            t.Summary("Custom Path: Linear goes in straight lines between points, with a sharp corner at each.")
             .Watch("Ball", ball)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Zigzag</b>, then <b>Home</b>.")
             .Expect("Ball zigzags in straight lines through all four corners - sharp turns, no rounding - and lands on the To outline.");
        }

        private static void CurrentPath(LabBoard b)
        {
            var t = b.Test("M5", "A path in Current mode");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-220f, -40f), 36f, Pal.Pink);

            var hop = Anim.New("Hop", Pos(UT.Path("Ball")).ToCur(V3(140f, 0f)).Dur(0.6f).Ease(Ease.Linear)
                .Path(UIAnimationPathShape.Curved, V3(70f, 90f)));
            var hop2 = Anim.New("Hop Again", Pos(UT.Path("Ball")).ToCur(V3(140f, 0f)).Dur(0.6f).Ease(Ease.Linear)
                .Path(UIAnimationPathShape.Curved, V3(70f, 90f)));
            var player = t.Player(hop, hop2, Anim.New("Home", Pos(UT.Path("Ball")).ToRest().Dur(0.4f)));

            UI.PlayButton(t.Stage, "Hop", new Vector2(-60f, -100f), new Vector2(110f, 32f), player, "Hop");
            UI.PlayButton(t.Stage, "Home", new Vector2(70f, -100f), new Vector2(110f, 32f), player, "Home");

            t.Summary("With To on Current, the points are offsets from wherever the step starts, so the same hop works from anywhere.")
             .Watch("Ball", ball)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode)
             .HowTo("Preview <b>Hop</b>, then <b>Hop Again</b> (which continues from where Hop ended). In play mode click the Hop button three times, waiting for each.")
             .Expect("Each hop is the same arch, 90 up and 140 along, starting from wherever the ball is: the ball hops along the stage. Home brings it back.");
        }

        private static void SizePath(LabBoard b)
        {
            var t = b.Test("M6", "Paths on Size Delta and Scale");
            var bar = UI.Square(t.Stage, "Sized", new Vector2(-140f, 0f), new Vector2(40f, 40f), Pal.Teal);
            var scaled = UT.Box(t, "Scaled", new Vector2(150f, 0f), Pal.Purple, 50f);
            UI.Caption(t.Stage, "Size Delta: wide, then tall", new Vector2(-140f, -100f), 220f);
            UI.Caption(t.Stage, "Scale: wide, then tall", new Vector2(150f, -100f), 220f);

            t.Player(
                Anim.New("Wide Then Tall",
                    Size(UT.Path("Sized")).To(V3(160f, 160f)).Dur(1.2f).Ease(Ease.Linear).Path(UIAnimationPathShape.Linear, V3(160f, 40f)),
                    S.Scale(UT.Path("Scaled")).With().To(V3(3f, 3f, 1f)).Dur(1.2f).Ease(Ease.Linear).Path(UIAnimationPathShape.Linear, V3(3f, 1f, 1f))),
                Anim.New("Back",
                    Size(UT.Path("Sized")).ToRest().Dur(0.3f),
                    S.Scale(UT.Path("Scaled")).With().ToRest().Dur(0.3f)));

            t.Summary("A path can drive a size or a scale too: through a point, the step changes one axis, then the other.")
             .Watch("Sized, Scaled", bar, scaled)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Wide Then Tall</b>, then <b>Back</b>. Open a step: its Edit Path in Scene button is greyed (only position steps can be drawn).")
             .Expect("Both grow wide first, then tall, at a steady pace, ending square. The Edit Path in Scene button is greyed out with a tooltip saying why.");
        }

        private static void PathNoPoints(LabBoard b)
        {
            var t = b.Test("M7", "A path with no points");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-200f, 0f), 36f, Pal.Orange);
            UT.Mark(t, "To", new Vector2(200f, 0f), new Vector2(36f, 36f));

            var step = Pos(UT.Path("Ball")).To(V3(200f, 0f)).Dur(0.8f).Path(UIAnimationPathShape.Curved);
            t.Player(Anim.New("Straight", step), Anim.New("Home", Pos(UT.Path("Ball")).ToRest().Dur(0.3f)));

            t.Summary("A shape picked with no points still moves in a straight line, and Disabled keeps the points.")
             .Watch("Ball", ball)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Straight</b>. Then add a point with Add Point, set Custom Path to <b>Disabled</b>, and back to Curved. Undo afterwards.")
             .Expect("Straight moves in a plain line. Switching to Disabled hides the point; switching back to Curved shows it again with the same value.");
        }

        private static void SteadySpeed(LabBoard b)
        {
            var t = b.Test("M8", "Steady speed along a path");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-230f, 0f), 30f, Pal.Blue);
            Vector3[] points = { V3(-200f, 0f), V3(-170f, 0f), V3(150f, 0f) };
            foreach (var p in points) UT.Mark(t, "Point " + p.x, p, new Vector2(12f, 12f));
            UT.Mark(t, "To", new Vector2(230f, 0f), new Vector2(30f, 30f));

            t.Player(
                Anim.New("Glide", Pos(UT.Path("Ball")).To(V3(230f, 0f)).Dur(2f).Ease(Ease.Linear).Path(UIAnimationPathShape.Linear, points)),
                Anim.New("Home", Pos(UT.Path("Ball")).ToRest().Dur(0.3f)));

            t.Summary("Short and long stretches between points are covered at the same pace.")
             .Watch("Ball", ball)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Glide</b>. The points are bunched at the left and spread at the right.")
             .Expect("Ball moves at one even speed the whole way. It doesn't crawl through the bunched points or race across the long gap.");
        }

        private static void CustomFps(LabBoard b)
        {
            var t = b.Test("M9", "Play At Custom FPS");
            var smooth = UT.Box(t, "Smooth", new Vector2(-200f, 50f), Pal.Teal, 44f);
            var stepped = UT.Box(t, "Stepped", new Vector2(-200f, -40f), Pal.Orange, 44f);
            UI.Caption(t.Stage, "smooth", new Vector2(-200f, 85f));
            UI.Caption(t.Stage, "8 FPS", new Vector2(-200f, -5f));
            UT.Mark(t, "Smooth end", new Vector2(200f, 50f), new Vector2(44f, 44f));
            UT.Mark(t, "Stepped end", new Vector2(200f, -40f), new Vector2(44f, 44f));

            t.Player(
                Anim.New("Smooth", Pos(UT.Path("Smooth")).ToBase(V3(400f, 0f)).Dur(1.3f).Ease(Ease.InOutSine), Rot(UT.Path("Smooth")).With().ToCur(V3(0f, 0f, -180f)).Dur(1.3f)),
                Anim.New("Stepped", Pos(UT.Path("Stepped")).ToBase(V3(400f, 0f)).Dur(1.3f).Ease(Ease.InOutSine), Rot(UT.Path("Stepped")).With().ToCur(V3(0f, 0f, -180f)).Dur(1.3f)).Fps(8f),
                Anim.New("Back", Pos(UT.Path("Smooth")).ToRest().Dur(0.2f), Pos(UT.Path("Stepped")).With().ToRest().Dur(0.2f),
                    Rot(UT.Path("Smooth")).With().ToRest().Dur(0.2f), Rot(UT.Path("Stepped")).With().ToRest().Dur(0.2f)));

            t.Summary("The same move and turn, smooth and at 8 FPS. Duration 1.3 s is not a whole number of 8 FPS frames.")
             .Watch("Smooth, Stepped", smooth, stepped)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Smooth</b>, then <b>Stepped</b>, then <b>Back</b>. Look at Stepped's Play At Custom FPS row.")
             .Expect("Stepped jumps in distinct steps, its move and turn ticking together, and still lands exactly on its outline, upside down, like Smooth. The row shows the tick box with 8 in a box beside it.");
        }

        private static void FpsDetails(LabBoard b)
        {
            var t = b.Test("M10", "Custom FPS: staggers, punches, Set Active");
            var a = UT.Box(t, "A", new Vector2(-200f, 70f), Pal.Blue, 36f);
            var bb = UT.Box(t, "B", new Vector2(-200f, 20f), Pal.Blue, 36f);
            var punch = UT.Box(t, "Punch", new Vector2(150f, 40f), Pal.Pink, 60f);
            var lamp = UI.Icon(t.Stage, "Lamp", LabAssets.Star, new Vector2(0f, -70f), 60f, Pal.Yellow);
            lamp.gameObject.SetActive(false);
            UT.Mark(t, "Lamp spot", new Vector2(0f, -70f), new Vector2(60f, 60f));
            UI.Caption(t.Stage, "on at 0.33 s", new Vector2(0f, -110f));

            t.Player(Anim.New("Mixed",
                Pos(UT.Path("A")).ToBase(V3(250f, 0f)).Dur(1f).Ease(Ease.Linear),
                Pos(UT.Path("B")).With().ToBase(V3(250f, 0f)).Dur(1f).Delay(0.07f).Ease(Ease.Linear),
                PunchScale(UT.Path("Punch"), V3(0.4f, 0.4f, 0f), 0.8f).With(),
                Active(UT.Path("Lamp"), true).With().Delay(0.33f)).Fps(4f));

            t.Summary("At 4 FPS: two moves staggered by 0.07 s, a punch, and a Set Active at 0.33 s.")
             .Watch("A, B, Punch, Lamp", a, bb, punch, lamp)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Mixed</b>. Stop and Restore afterwards.")
             .Expect("A and B jump forward on the same ticks (one shared frame grid), not alternately. The punch wobbles smoothly - punch and shake are never stepped. The star appears at its own moment, between ticks, not on the nearest one.");
        }

        private static void SnapAll(LabBoard b)
        {
            var t = b.Test("M11", "Snapping: All Steps");
            var smooth = UI.Icon(t.Stage, "Smooth Hero", LabAssets.Pixel, new Vector2(-30f, 40f), 48f, Pal.Teal);
            var snapped = UI.Icon(t.Stage, "Snapped Hero", LabAssets.Pixel, new Vector2(-30f, -50f), 48f, Pal.Orange);
            UI.Caption(t.Stage, "Snapping off", new Vector2(-180f, 40f), 120f);
            UI.Caption(t.Stage, "ALL STEPS", new Vector2(-180f, -50f), 120f);
            var readout = UI.Label(t.Stage, "Readout", "", new Vector2(170f, -50f), new Vector2(200f, 30f), 16f, Pal.White);
            var live = readout.gameObject.AddComponent<LiveReadout>();
            live.Text = readout;
            live.Target = snapped.transform;
            live.Show = LiveReadout.Value.AnchoredPosition;
            live.Prefix = "x, y: ";

            t.Player(
                Anim.New("Creep Smooth", Pos(UT.Path("Smooth Hero")).ToBase(V3(30.5f, 0f)).Dur(3f).Ease(Ease.Linear)),
                Anim.New("Creep Snapped", Pos(UT.Path("Snapped Hero")).ToBase(V3(30.5f, 0f)).Dur(3f).Ease(Ease.Linear)).SnapAll(),
                Anim.New("Back", Pos(UT.Path("Smooth Hero")).ToRest().Dur(0.2f), Pos(UT.Path("Snapped Hero")).With().ToRest().Dur(0.2f)));

            t.Summary("A slow 30.5-unit creep, smooth and snapped. Snapping rounds positions to whole units every frame.")
             .Watch("Smooth Hero, Snapped Hero; the readout", smooth, snapped, readout)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Zoom the Scene view right in on the two heroes. Play <b>Creep Smooth</b>, then <b>Creep Snapped</b>, watching the readout. Click the Snapping button on an animation a few times.")
             .Expect("Smooth glides; Snapped steps a whole unit at a time, and the readout never shows a fraction, even at the end (the move ends half a unit past a whole number). The button cycles DISABLED, ALL STEPS, PER STEP.");
        }

        private static void SnapPerStep(LabBoard b)
        {
            var t = b.Test("M12", "Snapping: Per Step");
            var a = UI.Icon(t.Stage, "Per Step On", LabAssets.Pixel, new Vector2(-30f, 60f), 40f, Pal.Teal);
            var bb = UI.Icon(t.Stage, "Per Step Off", LabAssets.Pixel, new Vector2(-30f, 0f), 40f, Pal.Blue);
            var c = UI.Icon(t.Stage, "Own Box Ticked", LabAssets.Pixel, new Vector2(-30f, -60f), 40f, Pal.Pink);
            UI.Caption(t.Stage, "box ticked", new Vector2(-180f, 60f), 140f);
            UI.Caption(t.Stage, "box unticked", new Vector2(-180f, 0f), 140f);
            UI.Caption(t.Stage, "DISABLED, box ticked", new Vector2(-180f, -60f), 160f);

            t.Player(
                Anim.New("Per Step",
                    Pos(UT.Path("Per Step On")).ToBase(V3(30.5f, 0f)).Dur(3f).Ease(Ease.Linear).Snap(),
                    Pos(UT.Path("Per Step Off")).With().ToBase(V3(30.5f, 0f)).Dur(3f).Ease(Ease.Linear)).SnapPerStep(),
                Anim.New("Disabled But Ticked",
                    Pos(UT.Path("Own Box Ticked")).ToBase(V3(30.5f, 0f)).Dur(3f).Ease(Ease.Linear).Snap()));

            t.Summary("PER STEP lets each step choose; a step whose own box is ticked always snaps.")
             .Watch("Per Step On, Per Step Off, Own Box Ticked", a, bb, c)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Zoom in on the heroes. Play <b>Per Step</b>, then <b>Disabled But Ticked</b>. Open both animations' steps.")
             .Expect("In Per Step, the top hero steps a pixel at a time while the middle one glides. Disabled But Ticked steps too, though its Snapping button reads DISABLED. Every step of Per Step shows a Snapping box; Disabled But Ticked's step still shows its ticked box.");
        }
    }
}
