using DG.Tweening;
using rmf_claude.DOTweenUI;
using TMPro;
using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UIAnimLab.EditorTools.S;

namespace UIAnimLab.UnitTests.Builders
{
    /// <summary>04 Playback and Interrupts: play-mode behaviour the edit-mode preview never reaches.</summary>
    public static class PlaybackScene
    {
        [MenuItem(UT.Menu + "04 Playback and Interrupts")]
        public static void Build()
        {
            var b = UT.Begin("04 Playback and Interrupts", "04  Playback and Interrupts",
                "Play mode only. Interrupt Others (STOP / COMPLETE, REPORT / SILENT), stopping, replaying, punch spam, chaining with On Complete, Kill On Disable and unscaled time. " +
                "Enter play mode, then click the buttons on each tile in the Game view.");

            Interrupts(b);
            HoverPair(b);
            Layered(b);
            Replay(b);
            PunchSpam(b);
            Chain(b);
            ChainInterrupted(b);
            KillOnDisable(b);
            UnscaledTime(b);
            CompleteRunsSetActive(b);

            b.Save();
        }

        private static TextMeshProUGUI Status(Tile t, float y)
        {
            return UI.Label(t.Stage, "Status", "", new Vector2(0f, y), new Vector2(570f, 40f), 15f, Pal.White, TextAlignmentOptions.TopLeft);
        }

        private static TextMeshProUGUI ExpectLine(Tile t, float y)
        {
            return UI.Label(t.Stage, "Expect Line", "", new Vector2(0f, y), new Vector2(570f, 40f), 15f, Pal.ExpectText, TextAlignmentOptions.TopLeft);
        }

        private static Button Button(Tile t, string text, Vector2 at, float width = 130f)
        {
            return UI.Button(t.Stage, "Button " + text, text, at, new Vector2(width, 34f), 14f);
        }

        private static void Interrupts(LabBoard b)
        {
            var t = b.Test("P1", "Interrupt Others and Stop(complete)", 1284f, 330f);

            UI.Square(t.Stage, "Track", new Vector2(0f, 110f), new Vector2(1000f, 4f), Pal.Ghost);
            var runner = UT.Box(t, "Runner", new Vector2(-480f, 110f), Pal.Blue, 50f);
            var done = UI.Label(t.Stage, "Done", "DONE", new Vector2(540f, 110f), new Vector2(110f, 50f), 28f, Pal.Green, TextAlignmentOptions.Center, FontStyles.Bold);
            done.gameObject.SetActive(false);
            var status = Status(t, -95f);
            var expect = ExpectLine(t, -125f);

            var fixture = t.Go.AddComponent<InterruptOthersTest>();
            var player = t.Player();
            fixture.Player = player;
            fixture.Runner = runner.rectTransform;
            fixture.Done = done.gameObject;
            fixture.Status = status;
            fixture.Expect = expect;

            var travel = Anim.New("Travel",
                    Pos(UT.Path("Runner")).FromBase(Vector3.zero).ToBase(V3(960f, 0f)).Dur(2.5f).Ease(Ease.Linear),
                    Active(UT.Path("Done"), true),
                    Sound(LabAssets.Jingle))
                .Note("Crosses in 2.5 s, then shows DONE and plays a jingle. On Complete counts in InterruptOthersTest.")
                .OnDone(fixture.CountOnComplete);
            var endless = Anim.New("Travel Endless",
                    Pos(UT.Path("Runner")).FromBase(Vector3.zero).ToBase(V3(960f, 0f)).Dur(1.5f).Ease(Ease.InOutSine))
                .Loop(-1, LoopType.Yoyo).Note("Loops forever, so there is no end to complete to.");
            var reset = Anim.New("Reset",
                Pos(UT.Path("Runner")).ToRest().Dur(0.2f),
                Active(UT.Path("Done"), false)).InterruptStop();

            Anim.Set(player,
                travel, endless,
                Anim.New("Interrupt STOP", PunchScale(UT.Path("Runner"), V3(0.4f, 0.4f, 0f), 0.4f)).InterruptStop().Note("Punches the runner; Interrupt Others reads STOP."),
                Anim.New("Interrupt COMPLETE REPORT", PunchScale(UT.Path("Runner"), V3(0.4f, 0.4f, 0f), 0.4f)).InterruptComplete().Note("Punches the runner; Interrupt Others reads COMPLETE, REPORT."),
                Anim.New("Interrupt COMPLETE SILENT", PunchScale(UT.Path("Runner"), V3(0.4f, 0.4f, 0f), 0.4f)).InterruptComplete(true).Note("Punches the runner; Interrupt Others reads COMPLETE, SILENT."),
                reset);

            float y = -30f, x = -560f, w = 136f, gap = 10f;
            var buttons = new (string, UnityEngine.Events.UnityAction)[]
            {
                ("Play Travel", fixture.PlayTravel), ("Play Endless", fixture.PlayEndless), ("Stop", fixture.Stop),
                ("Stop(complete)", fixture.StopComplete), ("StopAll(complete)", fixture.StopAllComplete),
                ("Interrupt STOP", fixture.InterruptStop), ("COMPLETE REPORT", fixture.InterruptReport), ("COMPLETE SILENT", fixture.InterruptSilent),
            };
            foreach (var (label, action) in buttons)
            {
                UI.OnClick(Button(t, label, new Vector2(x + w * 0.5f, y), w), action);
                x += w + gap;
            }
            UI.PlayButton(t.Stage, "Reset", new Vector2(x + 50f, y), new Vector2(90f, 34f), player, "Reset");

            t.Summary("Play Travel (or Endless), then press another button while it runs. The white line says what happened, the yellow line what should have.")
             .Watch("Runner, DONE, the two lines", runner, done, status, expect)
             .Needs(StationNeeds.PlayMode)
             .HowTo("Enter play mode. For each of the six end buttons: press Play Travel, then that button half way across; read the two lines; press Reset. Repeat with Play Endless.")
             .Expect("Every time, the white line matches the yellow one. COMPLETE jumps to the end and shows DONE without the jingle; STOP stops where it is; SILENT never counts On Complete; an endless loop is never completed.");
        }

        private static void HoverPair(LabBoard b)
        {
            var t = b.Test("P2", "Hover in and out: STOP vs COMPLETE");
            var stop = MakeHoverCard(t, "Stop Card", new Vector2(-140f, 10f), Pal.Teal, false);
            var complete = MakeHoverCard(t, "Complete Card", new Vector2(140f, 10f), Pal.Orange, true);
            UI.Caption(t.Stage, "STOP", new Vector2(-140f, -90f));
            UI.Caption(t.Stage, "COMPLETE", new Vector2(140f, -90f));

            t.Summary("Each card grows on hover and shrinks on exit, over 0.6 s. Their Interrupt Others differs.")
             .Watch("Stop Card, Complete Card", stop, complete)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode, move the mouse onto each card and off again quickly, before it has finished growing.")
             .Expect("Stop Card shrinks from however big it had got - smooth. Complete Card first jumps to full size, then shrinks - a visible pop. That's why hover pairs want STOP.");
        }

        private static Image MakeHoverCard(Tile t, string name, Vector2 at, Color color, bool complete)
        {
            var card = UI.Box(t.Stage, name, at, new Vector2(160f, 130f), color);
            card.raycastTarget = true;

            var hover = Anim.New("Hover", S.Scale(null).ToBase(V3(0.35f, 0.35f, 0f)).Dur(0.6f).Ease(Ease.Linear));
            var unhover = Anim.New("Unhover", S.Scale(null).ToRest().Dur(0.6f).Ease(Ease.Linear));
            if (complete) { hover.InterruptComplete(); unhover.InterruptComplete(); }
            else { hover.InterruptStop(); unhover.InterruptStop(); }
            var player = t.PlayerOn(card.gameObject, hover, unhover);

            UI.Trigger(card, EventTriggerType.PointerEnter, player, "Hover");
            UI.Trigger(card, EventTriggerType.PointerExit, player, "Unhover");
            return card;
        }

        private static void Layered(LabBoard b)
        {
            var t = b.Test("P3", "Interrupt Others unticked");
            var icon = UI.Icon(t.Stage, "Icon", LabAssets.Star, new Vector2(0f, 30f), 90f, Pal.Yellow);
            var player = t.Player(
                Anim.New("Pulse", S.Scale(UT.Path("Icon")).FromBase(Vector3.zero).ToBase(V3(0.2f, 0.2f, 0f)).Dur(0.5f).Ease(Ease.InOutSine)).Loop(-1, LoopType.Yoyo).NoInterrupt(),
                Anim.New("Spin", Rot(UT.Path("Icon")).ToCur(V3(0f, 0f, -360f)).Dur(1f).Ease(Ease.InOutCubic)).NoInterrupt(),
                Anim.New("Spin And Stop Others", Rot(UT.Path("Icon")).ToCur(V3(0f, 0f, -360f)).Dur(1f).Ease(Ease.InOutCubic)).InterruptStop());

            UI.PlayButton(t.Stage, "Pulse", new Vector2(-195f, -80f), new Vector2(120f, 34f), player, "Pulse");
            UI.PlayButton(t.Stage, "Spin", new Vector2(-65f, -80f), new Vector2(120f, 34f), player, "Spin");
            UI.PlayButton(t.Stage, "Spin + stop others", new Vector2(110f, -80f), new Vector2(210f, 34f), player, "Spin And Stop Others");
            var stopButton = Button(t, "Stop Pulse", new Vector2(0f, -120f), 160f);
            UnityEventTools.AddStringPersistentListener(stopButton.onClick, player.StopAnimation, "Pulse");

            t.Summary("A looping pulse that plays on top of other animations, because its Interrupt Others is unticked - and theirs decides whether it survives them.")
             .Watch("Icon", icon)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode: Pulse, then Spin. Then Spin + stop others. Then Pulse again and Stop Pulse.")
             .Expect("Spin (unticked) turns the star while it keeps pulsing. Spin + stop others (ticked, STOP) stops the pulse where it stands, then spins. Stop Pulse stops it through StopAnimation.");
        }

        private static void Replay(LabBoard b)
        {
            var t = b.Test("P4", "Replaying a running animation");
            var bar = UI.Square(t.Stage, "Bar", new Vector2(-220f, 20f), new Vector2(20f, 40f), Pal.Purple);
            bar.rectTransform.pivot = new Vector2(0f, 0.5f);
            UT.Mark(t, "Full", new Vector2(0f, 20f), new Vector2(440f, 40f));
            var player = t.Player(
                Anim.New("Grow", Size(UT.Path("Bar")).To(V3(440f, 40f)).Dur(3f).Ease(Ease.Linear)).InterruptComplete(),
                Anim.New("Empty", Size(UT.Path("Bar")).ToRest().Dur(0.2f)));
            UI.PlayButton(t.Stage, "Grow", new Vector2(-70f, -70f), new Vector2(120f, 34f), player, "Grow");
            UI.PlayButton(t.Stage, "Empty", new Vector2(70f, -70f), new Vector2(120f, 34f), player, "Empty");

            t.Summary("Grow (3 s, Interrupt Others on COMPLETE) has no From. Pressing it again while it runs restarts it from where it is - its COMPLETE is about the others.")
             .Watch("Bar", bar)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode, press Grow, then Grow again half way.")
             .Expect("The second press carries on growing from where the bar had got to, slower now (the same 3 s for less distance). It never jumps to full first.");
        }

        private static void PunchSpam(LabBoard b)
        {
            var t = b.Test("P5", "Punch and shake spam");
            var box = UT.Box(t, "Box", new Vector2(-120f, 30f), Pal.Pink, 80f);
            var readout = UI.Label(t.Stage, "Readout", "", new Vector2(140f, 30f), new Vector2(260f, 60f), 18f, Pal.White, TextAlignmentOptions.Left);
            var live = readout.gameObject.AddComponent<LiveReadout>();
            live.Text = readout;
            live.Target = box.transform;
            live.Show = LiveReadout.Value.Scale;
            live.Prefix = "scale: ";

            var player = t.Player(
                Anim.New("Squish", PunchScale(UT.Path("Box"), V3(-0.3f, -0.3f, 0f), 0.4f)),
                Anim.New("Jelly", ShakeScale(UT.Path("Box"), V3(0.4f, 0.4f, 0f), 0.6f)));
            var actions = t.Go.AddComponent<TestActions>();
            actions.Player = player;

            UI.PlayButton(t.Stage, "Squish", new Vector2(-220f, -80f), new Vector2(100f, 34f), player, "Squish");
            UI.PlayButton(t.Stage, "Jelly", new Vector2(-110f, -80f), new Vector2(100f, 34f), player, "Jelly");
            UI.OnClick(Button(t, "Spam Squish x20", new Vector2(40f, -80f), 180f), actions.Spam, "Squish");
            UI.OnClick(Button(t, "Spam Jelly x20", new Vector2(220f, -80f), 160f), actions.Spam, "Jelly");

            t.Summary("A punch or shake cut short is put back where it started, so spamming one never drifts.")
             .Watch("Box, the scale readout", box, readout)
             .Needs(StationNeeds.PlayMode | StationNeeds.Preview)
             .HowTo("In play mode, click Squish and Jelly as fast as you can, then the two Spam buttons (20 plays, 50 ms apart), alternating. Also mash Play in the preview.")
             .Expect("When everything settles the readout says exactly 1.000, 1.000, every time.");
        }

        private static void Chain(LabBoard b)
        {
            var t = b.Test("P6", "Chaining with On Complete");
            var a = UT.Box(t, "A", new Vector2(-180f, 20f), Pal.Blue, 60f);
            var bb = UT.Box(t, "B", new Vector2(0f, 20f), Pal.Green, 60f);
            var c = UT.Box(t, "C", new Vector2(180f, 20f), Pal.Orange, 60f);

            var player = t.Player();
            Anim.Set(player,
                Anim.New("One", PunchScale(UT.Path("A"), V3(0.5f, 0.5f, 0f), 0.6f)).Then(player, "Two"),
                Anim.New("Two", PunchScale(UT.Path("B"), V3(0.5f, 0.5f, 0f), 0.6f)).Then(player, "Three"),
                Anim.New("Three", PunchScale(UT.Path("C"), V3(0.5f, 0.5f, 0f), 0.6f)),
                Anim.New("Spin Forever", Rot(UT.Path("B")).ToCur(V3(0f, 0f, -90f)).Dur(0.5f)).Then(player, "Spin Forever"));

            UI.PlayButton(t.Stage, "One", new Vector2(-200f, -80f), new Vector2(100f, 34f), player, "One");
            var stop = Button(t, "Stop All", new Vector2(-90f, -80f), 100f);
            var actions = t.Go.AddComponent<TestActions>();
            actions.Player = player;
            UI.OnClick(stop, actions.StopAll);
            UI.PlayButton(t.Stage, "Spin Forever", new Vector2(60f, -80f), new Vector2(170f, 34f), player, "Spin Forever");

            t.Summary("One's On Complete plays Two, Two's plays Three. Spin Forever plays itself from its own On Complete.")
             .Watch("A, B, C", a, bb, c)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode press One. Press One again and Stop All during B's punch. Press Spin Forever, then Stop All.")
             .Expect("A, B, C punch in turn. Stopped during B: C never punches (a stopped link doesn't start the next). Spin Forever keeps turning in quarter turns until Stop All, then stops dead.");
        }

        private static void ChainInterrupted(LabBoard b)
        {
            var t = b.Test("P7", "Interrupting a chain with COMPLETE");
            var a = UT.Box(t, "A", new Vector2(-120f, 30f), Pal.Blue, 60f);
            var bb = UT.Box(t, "B", new Vector2(120f, 30f), Pal.Green, 60f);
            var counter = UI.Label(t.Stage, "Counter", "", new Vector2(0f, -40f), new Vector2(400f, 30f), 16f, Pal.White);
            var count = t.Go.AddComponent<EventCounter>();
            count.Text = counter;
            count.Label = "First's On Complete ran";

            var player = t.Player();
            Anim.Set(player,
                Anim.New("First", Pos(UT.Path("A")).ToBase(V3(0f, 60f)).Dur(2f)).OnDone(count.Count).Then(player, "Second"),
                Anim.New("Second", Pos(UT.Path("B")).ToBase(V3(0f, 60f)).Dur(1f)),
                Anim.New("Report", PunchScale(UT.Path("A"), V3(0.3f, 0.3f, 0f))).InterruptComplete(),
                Anim.New("Silent", PunchScale(UT.Path("A"), V3(0.3f, 0.3f, 0f))).InterruptComplete(true),
                Anim.New("Reset", Pos(UT.Path("A")).ToRest().Dur(0.2f), Pos(UT.Path("B")).With().ToRest().Dur(0.2f)).InterruptStop());

            UI.PlayButton(t.Stage, "First", new Vector2(-210f, -95f), new Vector2(90f, 34f), player, "First");
            UI.PlayButton(t.Stage, "REPORT", new Vector2(-105f, -95f), new Vector2(100f, 34f), player, "Report");
            UI.PlayButton(t.Stage, "SILENT", new Vector2(5f, -95f), new Vector2(100f, 34f), player, "Silent");
            UI.PlayButton(t.Stage, "Reset", new Vector2(110f, -95f), new Vector2(90f, 34f), player, "Reset");

            t.Summary("First (2 s) chains into Second. Report and Silent interrupt it with COMPLETE.")
             .Watch("A, B, the counter", a, bb, counter)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode: First, then REPORT half way. Reset. First, then SILENT half way.")
             .Expect("REPORT: A jumps up and punches, the counter goes up by one (First's On Complete ran), and Second is started then stopped at once - B doesn't move. SILENT: A jumps up and punches, the counter doesn't change, B doesn't move.");
        }

        private static void KillOnDisable(LabBoard b)
        {
            var t = b.Test("P8", "Kill On Disable");
            var killed = MakeSpinner(t, "Killed", new Vector2(-140f, 30f), Pal.Teal, true);
            var kept = MakeSpinner(t, "Kept", new Vector2(140f, 30f), Pal.Orange, false);
            UI.Caption(t.Stage, "Kill On Disable on", new Vector2(-140f, -40f));
            UI.Caption(t.Stage, "Kill On Disable off", new Vector2(140f, -40f));

            var toggleKilled = Button(t, "Toggle Killed", new Vector2(-140f, -90f), 160f);
            var toggleKept = Button(t, "Toggle Kept", new Vector2(140f, -90f), 160f);
            var a1 = toggleKilled.gameObject.AddComponent<TestActions>();
            a1.Toggled = killed.gameObject;
            var a2 = toggleKept.gameObject.AddComponent<TestActions>();
            a2.Toggled = kept.gameObject;
            UI.OnClick(toggleKilled, a1.ToggleObject);
            UI.OnClick(toggleKept, a2.ToggleObject);

            t.Summary("Each square spins slowly (8 s per turn). Turning it off with Kill On Disable on stops its animation; off lets it carry on unseen.")
             .Watch("Killed, Kept", killed, kept)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode, wait a moment, toggle both off, wait 2 seconds, toggle both on.")
             .Expect("Killed comes back standing still where it was (its spin ended; it plays again only when told). Kept comes back further round, still spinning - it carried on while hidden.");
        }

        private static Image MakeSpinner(Tile t, string name, Vector2 at, Color color, bool kill)
        {
            var box = UT.Box(t, name, at, color, 70f);
            var player = t.PlayerOn(box.gameObject, Anim.New("Spin", Rot(null).ToCur(V3(0f, 0f, -360f)).Dur(8f).Ease(Ease.Linear)).Loop(-1, LoopType.Incremental));
            Anim.Options(player, true, kill);
            var auto = box.gameObject.AddComponent<PlayOnEnable>();
            auto.Animation = "Spin";
            auto.OnlyOnce = true;
            return box;
        }

        private static void UnscaledTime(LabBoard b)
        {
            var t = b.Test("P9", "Use Unscaled Time");
            var unscaled = MakeLooper(t, "Unscaled", new Vector2(-140f, 40f), Pal.Teal, true);
            var scaled = MakeLooper(t, "Scaled", new Vector2(140f, 40f), Pal.Orange, false);
            UI.Caption(t.Stage, "Use Unscaled Time on", new Vector2(-140f, -20f));
            UI.Caption(t.Stage, "Use Unscaled Time off", new Vector2(140f, -20f));

            var actions = t.Go.AddComponent<TestActions>();
            UI.OnClick(Button(t, "Pause / Unpause", new Vector2(-60f, -80f), 180f), actions.TogglePause);
            var readout = UI.Label(t.Stage, "Time Scale", "", new Vector2(130f, -80f), new Vector2(180f, 34f), 16f, Pal.White);
            var live = readout.gameObject.AddComponent<LiveReadout>();
            live.Text = readout;
            live.Show = LiveReadout.Value.TimeScale;
            live.Prefix = "Time.timeScale = ";

            t.Summary("Two pulsing squares. Pause sets Time.timeScale to 0.")
             .Watch("Unscaled, Scaled", unscaled, scaled)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode press Pause / Unpause, watch, and press it again.")
             .Expect("While paused, Unscaled keeps pulsing and Scaled freezes. Unpausing lets Scaled carry on from where it froze.");
        }

        private static Image MakeLooper(Tile t, string name, Vector2 at, Color color, bool unscaled)
        {
            var box = UT.Box(t, name, at, color, 70f);
            var player = t.PlayerOn(box.gameObject, Anim.New("Pulse", S.Scale(null).ToBase(V3(0.3f, 0.3f, 0f)).Dur(0.5f).Ease(Ease.InOutSine)).Loop(-1, LoopType.Yoyo));
            Anim.Options(player, unscaled, true);
            var auto = box.gameObject.AddComponent<PlayOnEnable>();
            auto.Animation = "Pulse";
            return box;
        }

        private static void CompleteRunsSetActive(LabBoard b)
        {
            var t = b.Test("P10", "Completing runs Set Active, not sounds");
            var box = UT.Box(t, "Box", new Vector2(-200f, 40f), Pal.Blue, 50f);
            var badge = UI.Icon(t.Stage, "Badge", LabAssets.Check, new Vector2(220f, 40f), 60f, Pal.Green);
            badge.gameObject.SetActive(false);
            UT.Mark(t, "Badge spot", new Vector2(220f, 40f), new Vector2(60f, 60f));

            var player = t.Player(
                Anim.New("Deliver",
                    Pos(UT.Path("Box")).ToBase(V3(380f, 0f)).Dur(3f).Ease(Ease.Linear),
                    Sound(LabAssets.Coin),
                    Active(UT.Path("Badge"), true)),
                Anim.New("Reset", Pos(UT.Path("Box")).ToRest().Dur(0.2f), Active(UT.Path("Badge"), false)).InterruptStop());
            var stop = Button(t, "Stop(complete)", new Vector2(-30f, -60f), 170f);
            var actions = t.Go.AddComponent<TestActions>();
            actions.Player = player;
            UI.OnClick(stop, actions.StopComplete, "Deliver");
            UI.PlayButton(t.Stage, "Deliver", new Vector2(-190f, -60f), new Vector2(120f, 34f), player, "Deliver");
            UI.PlayButton(t.Stage, "Reset", new Vector2(130f, -60f), new Vector2(100f, 34f), player, "Reset");

            t.Summary("Deliver moves the box (3 s), then plays a coin sound and turns the badge on.")
             .Watch("Box, Badge", box, badge)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode: Deliver and let it finish. Reset. Deliver, then Stop(complete) half way.")
             .Expect("Finishing on its own: coin sound, then the tick. Stop(complete): the box jumps to the end and the tick appears, with no coin sound.");
        }
    }
}
