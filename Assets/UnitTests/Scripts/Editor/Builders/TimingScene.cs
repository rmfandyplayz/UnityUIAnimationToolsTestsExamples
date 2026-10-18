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
    /// <summary>02 Timing & Endpoints: Start, Delay, From / To and their modes, Apply From Values Immediately, Loops.</summary>
    public static class TimingScene
    {
        [MenuItem(UT.Menu + "02 Timing and Endpoints")]
        public static void Build()
        {
            var b = UT.Begin("02 Timing and Endpoints", "02  Timing and Endpoints",
                "When steps start and where they go: AFTER / WITH PREVIOUS, Delay, the FROM / TO button, the Absolute / Baseline / Current modes, Apply From Values Immediately, and Loops.");

            AfterWith(b);
            DelayClock(b);
            Cascade(b);
            ToModes(b);
            FromModes(b);
            ToCarriesOn(b);
            ApplyFromImmediately(b);
            ResetButton(b);
            Loops(b);
            Incremental(b);
            LegacyFromCurrent(b);
            CurrentToWithFrom(b);

            b.Save();
        }

        private static void AfterWith(LabBoard b)
        {
            var t = b.Test("E1", "After and With Previous");
            var a = UT.Box(t, "A", new Vector2(-200f, 70f), Pal.Blue, 50f);
            var bb = UT.Box(t, "B", new Vector2(-200f, 0f), Pal.Green, 50f);
            var c = UT.Box(t, "C", new Vector2(-200f, -70f), Pal.Orange, 50f);
            UI.Label(a.transform, "L", "A", Vector2.zero, new Vector2(50f, 50f), 22f, Pal.Stage);
            UI.Label(bb.transform, "L", "B", Vector2.zero, new Vector2(50f, 50f), 22f, Pal.Stage);
            UI.Label(c.transform, "L", "C", Vector2.zero, new Vector2(50f, 50f), 22f, Pal.Stage);

            t.Player(
                Anim.New("Go",
                    Pos(UT.Path("A")).ToBase(V3(400f, 0f)).Dur(0.6f),
                    Pos(UT.Path("B")).ToBase(V3(400f, 0f)).Dur(0.6f),
                    Pos(UT.Path("C")).With().ToBase(V3(400f, 0f)).Dur(0.6f)),
                Anim.New("Back",
                    Pos(UT.Path("A")).ToRest().Dur(0.3f),
                    Pos(UT.Path("B")).With().ToRest().Dur(0.3f),
                    Pos(UT.Path("C")).With().ToRest().Dur(0.3f)));

            t.Summary("AFTER PREVIOUS waits for everything above; WITH PREVIOUS starts with the step above.")
             .Watch("A, B, C", a, bb, c)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Go</b>, then <b>Back</b>. Fold Go's steps closed and read their headers.")
             .Expect("Go: A crosses first; only when it arrives do B and C cross, side by side. Back brings all three home together. The headers read AFTER, AFTER, WITH.");
        }

        private static void DelayClock(LabBoard b)
        {
            var t = b.Test("E2", "Delay is applied once");

            // A clock: a bar that fills linearly over 2 s, with a mark at 1 s.
            var track = UI.Square(t.Stage, "Track", new Vector2(0f, 60f), new Vector2(400f, 16f), Pal.Ghost);
            var fill = UI.Square(t.Stage, "Clock", new Vector2(-200f, 60f), new Vector2(0f, 16f), Pal.Teal);
            fill.rectTransform.pivot = new Vector2(0f, 0.5f);
            UI.Square(t.Stage, "One Second", new Vector2(0f, 60f), new Vector2(3f, 34f), Pal.Yellow);
            UI.Caption(t.Stage, "1 s", new Vector2(0f, 90f), 60f);
            var box = UT.Box(t, "Box", new Vector2(-160f, -40f), Pal.Pink, 50f);

            t.Player(
                Anim.New("Run",
                    Size(UT.Path("Clock")).To(V3(400f, 16f)).Dur(2f).Ease(Ease.Linear),
                    Pos(UT.Path("Box")).With().ToBase(V3(320f, 0f)).Dur(0.4f).Delay(1f)));

            t.Summary("A step's Delay counts once, from where Start puts it. (It was once applied twice.)")
             .Watch("Clock (fills in 2 s), Box", fill, box)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Run</b> and watch when Box sets off.")
             .Expect("Box starts moving exactly as the clock reaches the yellow 1 s mark, not at the end of the bar.");
        }

        private static void Cascade(LabBoard b)
        {
            var t = b.Test("E3", "Staggered cascade");
            string[] names = { "One", "Two", "Three", "Four" };
            var objects = new Component[4];
            for (int i = 0; i < 4; i++)
            {
                objects[i] = UI.Box(t.Stage, names[i], new Vector2(0f, 75f - i * 50f), new Vector2(300f, 38f), Pal.Purple);
                UI.Label(objects[i].transform, "L", names[i], Vector2.zero, new Vector2(300f, 38f), 18f, Pal.Stage);
            }

            var steps = new UIAnimationStep[8];
            for (int i = 0; i < 4; i++)
            {
                var move = Pos(UT.Path(names[i])).FromBase(V3(-260f, 0f)).ToRest().Dur(0.35f).Ease(Ease.OutCubic).Delay(i * 0.08f);
                if (i > 0) move.With();
                steps[i * 2] = move;
                steps[i * 2 + 1] = GAlpha(UT.Path(names[i])).With().From(0f).To(1f).Dur(0.35f).Delay(i * 0.08f);
            }

            t.Player(Anim.New("Cascade In", steps));

            t.Summary("Joined steps with small rising delays arrive one after another.")
             .Watch("One to Four", objects)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Cascade In</b>.")
             .Expect("The four bars slide in from the left and fade in, One first, each 0.08 s after the one above, and all land exactly on their resting places.");
        }

        private static void ToModes(LabBoard b)
        {
            var t = b.Test("E4", "To: Absolute, Baseline, Current", 620f, 300f);
            var abs = UT.Box(t, "Absolute", new Vector2(-220f, 80f), Pal.Blue, 44f);
            var bas = UT.Box(t, "Baseline", new Vector2(-220f, 10f), Pal.Green, 44f);
            var cur = UT.Box(t, "Current", new Vector2(-220f, -60f), Pal.Orange, 44f);
            UI.Caption(t.Stage, "Absolute (60, 80)", new Vector2(200f, 80f));
            UI.Caption(t.Stage, "Baseline (+140, 0)", new Vector2(200f, 10f));
            UI.Caption(t.Stage, "Current (+140, 0)", new Vector2(200f, -60f));
            UT.Mark(t, "Absolute end", new Vector2(60f, 80f), new Vector2(44f, 44f));
            UT.Mark(t, "Baseline end", new Vector2(-80f, 10f), new Vector2(44f, 44f));

            var player = t.Player(
                Anim.New("Nudge",
                    Pos(UT.Path("Absolute")).To(V3(60f, 80f)).Dur(0.4f),
                    Pos(UT.Path("Baseline")).With().ToBase(V3(140f, 0f)).Dur(0.4f),
                    Pos(UT.Path("Current")).With().ToCur(V3(140f, 0f)).Dur(0.4f)),
                Anim.New("Home",
                    Pos(UT.Path("Absolute")).ToRest().Dur(0.3f),
                    Pos(UT.Path("Baseline")).With().ToRest().Dur(0.3f),
                    Pos(UT.Path("Current")).With().ToRest().Dur(0.3f)));

            UI.PlayButton(t.Stage, "Nudge", new Vector2(-170f, -118f), new Vector2(120f, 34f), player, "Nudge");
            UI.PlayButton(t.Stage, "Home", new Vector2(-40f, -118f), new Vector2(120f, 34f), player, "Home");

            t.Summary("What a To value means in each mode.")
             .Watch("Absolute, Baseline, Current", abs, bas, cur)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode)
             .HowTo("Preview <b>Nudge</b>. Then in play mode click the Nudge button three times (waiting for each), then Home.")
             .Expect("Absolute and Baseline land on their outlines every time. Current moves 140 further on every click in play mode, wandering off the stage; Home brings everything back. In the preview, Nudge replays from where it started, so Current never wanders there.");
        }

        private static void FromModes(LabBoard b)
        {
            var t = b.Test("E5", "From: Absolute and Baseline");
            var abs = UT.Box(t, "Absolute", new Vector2(120f, 50f), Pal.Blue, 50f);
            var bas = UT.Box(t, "Baseline", new Vector2(120f, -50f), Pal.Green, 50f);
            UI.Caption(t.Stage, "From Absolute (-200, 50)", new Vector2(-150f, 85f), 220f);
            UI.Caption(t.Stage, "From Baseline (-300, 0)", new Vector2(-150f, -15f), 220f);

            t.Player(Anim.New("Slide In",
                Pos(UT.Path("Absolute")).From(V3(-200f, 50f)).ToRest().Dur(0.5f).Ease(Ease.OutCubic),
                Pos(UT.Path("Baseline")).With().FromBase(V3(-300f, 0f)).ToRest().Dur(0.5f).Ease(Ease.OutCubic)));

            t.Summary("A From in Baseline mode follows the element when you move it; an Absolute one doesn't.")
             .Watch("Absolute, Baseline", abs, bas)
             .Needs(StationNeeds.Preview | StationNeeds.SceneView)
             .HowTo("Play <b>Slide In</b>. Stop and Restore. Move both boxes 100 to the right in the Scene view (or add 100 to Pos X), then Play again. Undo the move afterwards.")
             .Expect("First play: both slide in from the left to rest. After moving them: Baseline still starts 300 left of where it now rests; Absolute still starts at x -200, so its slide is longer. Both end where they now rest.");
        }

        private static void ToCarriesOn(LabBoard b)
        {
            var t = b.Test("E6", "TO carries on, FROM starts over");
            var to = UT.Box(t, "To Only", new Vector2(-140f, 0f), Pal.Teal, 80f);
            var from = UT.Box(t, "From And To", new Vector2(140f, 0f), Pal.Pink, 80f);
            UI.Caption(t.Stage, "TO (no From)", new Vector2(-140f, -70f));
            UI.Caption(t.Stage, "FROM 0.5 to 1.3", new Vector2(140f, -70f));

            t.Player(
                Anim.New("Grow",
                    S.Scale(UT.Path("To Only")).To(V3(1.3f, 1.3f, 1f)).Dur(0.6f),
                    S.Scale(UT.Path("From And To")).With().From(V3(0.5f, 0.5f, 1f)).To(V3(1.3f, 1.3f, 1f)).Dur(0.6f)),
                Anim.New("Shrink",
                    S.Scale(UT.Path("To Only")).ToRest().Dur(0.6f),
                    S.Scale(UT.Path("From And To")).With().ToRest().Dur(0.6f)));

            t.Summary("A step with no From travels from wherever the value is; with a From it always jumps there first.")
             .Watch("To Only, From And To", to, from)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Grow</b>, and while it's still growing press <b>Shrink</b>. Then press Grow again once Shrink has finished.")
             .Expect("Pressing Shrink mid-grow first finishes Grow (the preview finishes a running animation before playing another), then both shrink smoothly from 1.3. The second Grow: To Only grows smoothly from 1; From And To snaps to 0.5 first.");
        }

        private static void ApplyFromImmediately(LabBoard b)
        {
            var t = b.Test("E7", "Apply From Values Immediately", 620f, 280f);
            var on = UI.Label(t.Stage, "On", "Title (on)", new Vector2(-140f, 40f), new Vector2(240f, 50f), 30f, Pal.White, TextAlignmentOptions.Center, FontStyles.Bold);
            var off = UI.Label(t.Stage, "Off", "Title (off)", new Vector2(140f, 40f), new Vector2(240f, 50f), 30f, Pal.White, TextAlignmentOptions.Center, FontStyles.Bold);
            UI.Group(on);
            UI.Group(off);

            var player = t.Player(
                Anim.New("Show On", Alpha(UT.Path("On")).From(0f).To(1f).Dur(0.4f).Delay(0.8f)),
                Anim.New("Show Off", Alpha(UT.Path("Off")).From(0f).To(1f).Dur(0.4f).Delay(0.8f)).NoFromImmediately());

            UI.PlayButton(t.Stage, "Show On", new Vector2(-140f, -50f), new Vector2(150f, 36f), player, "Show On");
            UI.PlayButton(t.Stage, "Show Off", new Vector2(140f, -50f), new Vector2(150f, 36f), player, "Show Off");

            t.Summary("Each title fades in from invisible after a 0.8 s delay. One animation has the setting on, the other off.")
             .Watch("On, Off", on, off)
             .Needs(StationNeeds.PlayMode | StationNeeds.Preview)
             .HowTo("In play mode, click <b>Show On</b>, then <b>Show Off</b>. Then preview both with the Play buttons.")
             .Expect("Play mode: Show On blinks the title out at once and fades it in 0.8 s later. Show Off leaves the title visible for 0.8 s, then snaps it invisible and fades it in - the flash this setting prevents. The preview always starts from the first frame, so both look like Show On there.");
        }

        private static void ResetButton(LabBoard b)
        {
            var t = b.Test("E8", "The Reset button");
            var withFrom = UT.Box(t, "With From", new Vector2(-120f, 0f), Pal.Blue, 70f);
            var noFrom = UT.Box(t, "No From", new Vector2(120f, 0f), Pal.Orange, 70f);
            UI.Caption(t.Stage, "From scale 0", new Vector2(-120f, -60f));
            UI.Caption(t.Stage, "no From", new Vector2(120f, -60f));

            t.Player(Anim.New("Appear",
                S.Scale(UT.Path("With From")).From(Vector3.zero).ToRest().Dur(0.4f).Ease(Ease.OutBack),
                S.Scale(UT.Path("No From")).With().To(V3(1.4f, 1.4f, 1f)).Dur(0.4f)));

            t.Summary("Reset jumps to an animation's first frame without playing: steps with a From jump to it, the rest stay put.")
             .Watch("With From, No From", withFrom, noFrom)
             .Needs(StationNeeds.Preview)
             .HowTo("Press <b>Reset</b> beside Appear. Then Play. Then Stop and Restore.")
             .Expect("Reset: With From vanishes (scale 0); No From doesn't change. Play: With From pops in, No From grows to 1.4. Stop and Restore puts both back at 1.");
        }

        private static void Loops(LabBoard b)
        {
            var t = b.Test("E9", "Loops: Restart and Yoyo");
            var restart = UT.Box(t, "Restart", new Vector2(-180f, 50f), Pal.Blue, 44f);
            var yoyo = UT.Box(t, "Yoyo", new Vector2(-180f, -40f), Pal.Green, 44f);
            UI.Caption(t.Stage, "Loops 3, Restart", new Vector2(-180f, 85f));
            UI.Caption(t.Stage, "Loops -1, Yoyo", new Vector2(-180f, -5f));

            t.Player(
                Anim.New("Three Times", Pos(UT.Path("Restart")).ToBase(V3(360f, 0f)).Dur(0.5f).Ease(Ease.InOutSine)).Loop(3).NoInterrupt(),
                Anim.New("Forever", Pos(UT.Path("Yoyo")).ToBase(V3(360f, 0f)).Dur(0.5f).Ease(Ease.InOutSine)).Loop(-1, LoopType.Yoyo).NoInterrupt());

            t.Summary("Loops repeats the whole animation; Loop Type decides how.")
             .Watch("Restart, Yoyo", restart, yoyo)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Three Times</b>. Play <b>Forever</b>, then press Stop and Restore.")
             .Expect("Three Times crosses three times, jumping back to the left between crossings, and stops on the right. Forever swings back and forth until stopped.");
        }

        private static void Incremental(LabBoard b)
        {
            var t = b.Test("E10", "Loops: Incremental");
            var box = UT.Box(t, "Climber", new Vector2(-220f, -80f), Pal.Purple, 40f);
            for (int i = 1; i <= 4; i++) UT.Mark(t, "Step " + i, new Vector2(-220f + i * 110f, -80f + i * 40f), new Vector2(40f, 40f));

            t.Player(Anim.New("Climb", Pos(UT.Path("Climber")).ToCur(V3(110f, 40f)).Dur(0.35f).Ease(Ease.OutQuad)).Loop(4, LoopType.Incremental));

            t.Summary("Incremental repeats the same change, so a +110, +40 move keeps climbing.")
             .Watch("Climber", box)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Climb</b>.")
             .Expect("Climber steps up the stairs four times, landing on each outline in turn, and stays on the top one.");
        }

        private static void LegacyFromCurrent(LabBoard b)
        {
            var t = b.Test("E11", "Old data: From set to Current");
            var box = UT.Box(t, "Box", new Vector2(-100f, 0f), Pal.Yellow, 60f);

            // Old data could hold From: Current, which never meant an offset: it makes the step ignore From
            // entirely. The dropdown no longer offers it, but shows it when stored.
            var step = Pos(UT.Path("Box")).ToBase(V3(200f, 0f)).Dur(0.5f);
            step.UseFrom = true;
            step.FromMode = UIAnimationEndpointMode.Current;
            step.FromVector = V3(-500f, 0f);
            t.Player(Anim.New("Old Step", step), Anim.New("Home", Pos(UT.Path("Box")).ToRest().Dur(0.3f)));

            t.Summary("A step saved before the From dropdown lost Current still shows it, and behaves like the button reading TO.")
             .Watch("Box", box)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Open Old Step's step. Hover its From dropdown. Play <b>Old Step</b>.")
             .Expect("The From row shows Current with a tooltip explaining it. Opening the dropdown doesn't offer Current. Play ignores the From of -500: Box slides from where it is to 200 right of rest. Nothing is rewritten.");
        }

        private static void CurrentToWithFrom(LabBoard b)
        {
            var t = b.Test("E12", "To: Current with a From");
            var box = UT.Box(t, "Box", new Vector2(-100f, 0f), Pal.Pink, 60f);

            var step = Pos(UT.Path("Box")).From(V3(-200f, 0f)).ToCur(V3(150f, 0f)).Dur(0.5f);
            t.Player(Anim.New("Stored Current", step));

            t.Summary("Current is only offered on To while From is off. A stored Current To on a step with a From is read, and rewritten, as Baseline.")
             .Watch("Box", box)
             .Needs(StationNeeds.Inspector)
             .HowTo("Open Stored Current's step and look at its To row. Play it.")
             .Expect("The To row shows Baseline (150, 0), not Current: the drawer rewrites it as it draws, the one place the Inspector rewrites stored data on sight. Play slides Box from -200 to 150 right of where it rests.");
        }
    }
}
