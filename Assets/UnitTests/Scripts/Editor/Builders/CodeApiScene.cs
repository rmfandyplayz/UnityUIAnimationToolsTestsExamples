using DG.Tweening;
using rmf_claude.DOTweenUI;
using TMPro;
using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UIAnimLab.EditorTools.S;

namespace UIAnimLab.UnitTests.Builders
{
    /// <summary>05 Code API: everything a script can call, driven by buttons in play mode.</summary>
    public static class CodeApiScene
    {
        [MenuItem(UT.Menu + "05 Code API")]
        public static void Build()
        {
            var b = UT.Begin("05 Code API", "05  Code API",
                "Play mode only. Every public call on UIAnimationPlayer, from buttons: Play with onEnd and its reasons, Stop / StopAll, IsPlaying / Has, ApplyFromState, CaptureBaseline, SetTo / SetFrom / ClearOverrides, " +
                "waiting on the returned Sequence, and UIAnimationAudio.SetShared. Each tile shows what happened in white and what should have in yellow.", 3);

            EndReasons(b);
            SetToFrom(b);
            Probes(b);
            Baseline(b);
            Waiting(b);
            SharedAudio(b);
            FromUnityEvents(b);

            b.Save();
        }

        private static TextMeshProUGUI Line(Tile t, string name, float y, Color color)
        {
            return UI.Label(t.Stage, name, "", new Vector2(0f, y), new Vector2(t.Station.Width - 50f, 44f), 15f, color, TextAlignmentOptions.TopLeft);
        }

        private static void Buttons(Tile t, float y, float width, params (string label, UnityAction action)[] buttons)
        {
            float gap = 8f;
            float total = buttons.Length * width + (buttons.Length - 1) * gap;
            float x = -total * 0.5f + width * 0.5f;
            foreach (var (label, action) in buttons)
            {
                UI.OnClick(UI.Button(t.Stage, "Button " + label, label, new Vector2(x, y), new Vector2(width, 34f), 14f), action);
                x += width + gap;
            }
        }

        private static void EndReasons(LabBoard b)
        {
            var t = b.Test("C1", "Play(name, onEnd): every end reason", 900f, 300f);
            UI.Square(t.Stage, "Track", new Vector2(0f, 100f), new Vector2(700f, 4f), Pal.Ghost);

            var mover = UT.Box(t, "Mover", new Vector2(-350f, 100f), Pal.Blue, 44f);
            var player = t.PlayerOn(mover.gameObject,
                Anim.New("Slide", Pos(null).FromBase(Vector3.zero).ToBase(V3(700f, 0f)).Dur(2f).Ease(Ease.Linear)),
                Anim.New("Other", ShakeRot(null, V3(0f, 0f, 25f), 0.5f)).InterruptStop());

            // An inactive copy the fixture instantiates after destroying the live one.
            var template = Object.Instantiate(mover.gameObject, t.Stage);
            template.name = "Mover Template";
            template.SetActive(false);

            var status = Line(t, "Status", -45f, Pal.White);
            var expect = Line(t, "Expect Line", -85f, Pal.ExpectText);
            var fixture = t.Go.AddComponent<EndReasonTest>();
            fixture.Player = player;
            fixture.MoverTemplate = template;
            fixture.Status = status;
            fixture.Expect = expect;

            Buttons(t, 40f, 98f,
                ("Completed", fixture.Completed), ("Stopped", fixture.Stopped), ("Stop(complete)", fixture.StoppedComplete),
                ("Interrupted", fixture.Interrupted), ("Replayed", fixture.Replayed), ("Disabled", fixture.Disabled),
                ("Destroyed", fixture.Destroyed), ("NotFound", fixture.NotFound));
            Buttons(t, -5f, 140f, ("Show Mover", fixture.ShowMover));

            t.Summary("Each button starts Slide with an onEnd callback, then ends it a different way. onEnd must fire exactly once, with the reason.")
             .Watch("Mover, the white and yellow lines", mover, status, expect)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode, press each button in turn and compare the lines. After Disabled, press Show Mover.")
             .Expect("The white line always matches the yellow one, and each reason appears once (Replayed shows two: the first play's and the second's).");
        }

        private static void SetToFrom(LabBoard b)
        {
            var t = b.Test("C2", "SetTo, SetFrom, ClearOverrides", 900f, 300f);
            var levelBack = UI.Square(t.Stage, "Level Back", new Vector2(-150f, 100f), new Vector2(400f, 26f), Pal.Ghost);
            var fill = UI.Square(levelBack.transform, "Level Fill", Vector2.zero, Vector2.zero, Pal.Green);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0.6f, 1f);
            fill.rectTransform.sizeDelta = Vector2.zero;
            var levelLabel = UI.Label(t.Stage, "Level Label", "Level 60", new Vector2(150f, 100f), new Vector2(150f, 30f), 18f, Pal.White);
            var coinsLabel = UI.Label(t.Stage, "Coins Label", "12 coins", new Vector2(320f, 100f), new Vector2(150f, 30f), 18f, Pal.Yellow);
            var message = UI.Label(t.Stage, "Message", "Authored in the Inspector.", new Vector2(0f, 60f), new Vector2(800f, 34f), 20f, Pal.White);

            var status = Line(t, "Status", -40f, Pal.White);
            var expect = Line(t, "Expect Line", -85f, Pal.ExpectText);

            var fixture = t.Go.AddComponent<SetToFromTest>();
            fixture.LevelFill = fill.rectTransform;
            fixture.LevelLabel = levelLabel;
            fixture.CoinsLabel = coinsLabel;
            fixture.Status = status;
            fixture.Expect = expect;

            var player = t.Player(
                Anim.New("Level", Prop(null, typeof(SetToFromTest), "Level", UIAnimationPropertyKind.Float).To(100f).Dur(0.8f).Ease(Ease.OutCubic))
                    .Note("To is 100 here; the Level buttons set it from code."),
                Anim.New("Coins", Prop(null, typeof(SetToFromTest), "Coins", UIAnimationPropertyKind.Int).From(0f).To(50f).Dur(1.5f).Ease(Ease.Linear))
                    .Note("From 0 to 50 here; +7 coins sets both from code."),
                Anim.New("Message", Prop(UT.Path("Message"), typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.Text).From("").To("Authored in the Inspector.").Dur(1f).Ease(Ease.Linear))
                    .Note("Types the text here; New message sets it from code."));
            fixture.Player = player;

            // Each animation stands alone: playing one must not stop the others.
            foreach (var a in Anim.Of(player)) a.InterruptOthers = false;

            Buttons(t, 10f, 130f,
                ("Level -25", fixture.LevelDown), ("Level +25", fixture.LevelUp), ("+7 coins", fixture.AddCoins),
                ("New message", fixture.NextMessage), ("Clear", fixture.Clear), ("Refused calls", fixture.Refused));

            t.Summary("Values set from code before playing, without touching the Inspector's. The tile's own C# properties Level and Coins are animated.")
             .Watch("the level bar, the coins, the message", levelBack, coinsLabel, message)
             .Needs(StationNeeds.PlayMode | StationNeeds.Console | StationNeeds.Inspector)
             .HowTo("In play mode press each button, a few times, reading the lines. Select the tile while doing it: the Inspector values never change.")
             .Expect("The white line matches the yellow one. +7 coins pressed mid-count still lands 7 higher per click. Refused calls returns False three times and logs three warnings.");
        }

        private static void Probes(LabBoard b)
        {
            var t = b.Test("C3", "IsPlaying, IsAnyPlaying, Has, onComplete", 900f, 250f);
            var dot = UI.Icon(t.Stage, "Dot", LabAssets.Circle, new Vector2(-330f, 60f), 50f, Pal.Pink);
            var probe = UI.Label(t.Stage, "Probe", "", new Vector2(100f, 70f), new Vector2(620f, 60f), 16f, Pal.WatchText, TextAlignmentOptions.TopLeft);
            var status = Line(t, "Status", -50f, Pal.White);
            var expect = Line(t, "Expect Line", -90f, Pal.ExpectText);

            var player = t.Player(
                Anim.New("Pulse", S.Scale(UT.Path("Dot")).FromBase(Vector3.zero).ToBase(V3(0.5f, 0.5f, 0f)).Dur(0.75f).Ease(Ease.InOutSine)).Loop(2, LoopType.Yoyo),
                Anim.New("Home", S.Scale(UT.Path("Dot")).ToRest().Dur(0.2f)));
            var fixture = t.Go.AddComponent<CodeApiTest>();
            fixture.Player = player;
            fixture.Probe = probe;
            fixture.Status = status;
            fixture.Expect = expect;

            Buttons(t, 5f, 170f, ("Play Pulse", fixture.PlayPulse), ("Play with onComplete", fixture.OnCompleteCallback), ("Stop All", fixture.StopAll));

            t.Summary("The checking calls, read live, and Play's onComplete overload (only on a natural finish).")
             .Watch("Dot, the blue probe lines", dot, probe)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode: Play Pulse and watch the probe. Play with onComplete and let it finish. Play with onComplete again, and Stop All before it ends.")
             .Expect("IsPlaying(\"Pulse\") and IsAnyPlaying are True only while it pulses; Has(\"Pulse\") is always True and Has(\"Nope\") always False. 'callback ran' appears after a natural finish, never after Stop All.");
        }

        private static void Baseline(LabBoard b)
        {
            var t = b.Test("C4", "ApplyFromState and CaptureBaseline", 900f, 260f);
            var box = UT.Box(t, "Box", new Vector2(0f, 40f), Pal.Teal, 60f);
            UI.Group(box);
            var status = Line(t, "Status", -55f, Pal.White);
            var expect = Line(t, "Expect Line", -95f, Pal.ExpectText);

            var player = t.Player(
                Anim.New("Drop In",
                    Pos(UT.Path("Box")).FromBase(V3(0f, 80f)).ToRest().Dur(0.5f).Ease(Ease.OutBounce),
                    Alpha(UT.Path("Box")).With().From(0f).To(1f).Dur(0.3f)),
                Anim.New("Home", Pos(UT.Path("Box")).ToRest().Dur(0.4f)));
            var fixture = t.Go.AddComponent<CodeApiTest>();
            fixture.Player = player;
            fixture.Box = box.rectTransform;
            fixture.Status = status;
            fixture.Expect = expect;

            Buttons(t, -10f, 190f,
                ("ApplyFromState", fixture.ApplyFrom), ("Play Drop In", fixture.DropIn),
                ("Move + CaptureBaseline", fixture.MoveRestAndCapture), ("Play Home", fixture.Home));

            t.Summary("ApplyFromState jumps to an animation's start without playing. CaptureBaseline re-reads resting values after code moved something.")
             .Watch("Box", box)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode: ApplyFromState, then Play Drop In. Then Move + CaptureBaseline, and Play Drop In and Play Home again.")
             .Expect("ApplyFromState lifts the box and hides it, and nothing moves until Drop In. After Move + CaptureBaseline, Drop In and Home land on the box's new spot.");
        }

        private static void Waiting(LabBoard b)
        {
            var t = b.Test("C5", "Waiting on the returned Sequence", 440f, 240f);
            var dot = UI.Icon(t.Stage, "Dot", LabAssets.Circle, new Vector2(0f, 60f), 50f, Pal.Yellow);
            var status = Line(t, "Status", -40f, Pal.White);
            var expect = Line(t, "Expect Line", -80f, Pal.ExpectText);

            var player = t.Player(Anim.New("Pulse", S.Scale(UT.Path("Dot")).FromBase(Vector3.zero).ToBase(V3(0.5f, 0.5f, 0f)).Dur(0.75f).Ease(Ease.InOutSine)).Loop(2, LoopType.Yoyo));
            var fixture = t.Go.AddComponent<CodeApiTest>();
            fixture.Player = player;
            fixture.Status = status;
            fixture.Expect = expect;
            Buttons(t, 0f, 220f, ("WaitForCompletion", fixture.WaitForIt));

            t.Summary("Play returns the DOTween Sequence, so a coroutine can wait on it.")
             .Watch("Dot, the white line", dot, status)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode press WaitForCompletion.")
             .Expect("The white line reads 'yield return ...' while the dot pulses, then 'done waiting' the moment it stops.");
        }

        private static void SharedAudio(LabBoard b)
        {
            var t = b.Test("C6", "UIAnimationAudio.SetShared", 440f, 240f);
            var speaker = UI.Icon(t.Stage, "Speaker", LabAssets.Circle, new Vector2(0f, 70f), 50f, Pal.Purple);
            var muffled = UI.Rect(t.Stage, "Muffled Source", new Vector2(150f, 70f), new Vector2(10f, 10f));
            var source = muffled.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            var filter = muffled.gameObject.AddComponent<AudioLowPassFilter>();
            filter.cutoffFrequency = 600f;

            var status = Line(t, "Status", -40f, Pal.White);
            var expect = Line(t, "Expect Line", -80f, Pal.ExpectText);
            var player = t.Player(Anim.New("Click", Sound(LabAssets.Jingle), PunchScale(UT.Path("Speaker"), V3(0.3f, 0.3f, 0f)).With()));
            var fixture = t.Go.AddComponent<CodeApiTest>();
            fixture.Player = player;
            fixture.Status = status;
            fixture.Expect = expect;
            fixture.MuffledSource = source;
            Buttons(t, 0f, 160f, ("Click", fixture.Click), ("SetShared", fixture.RouteSound));

            t.Summary("Replacing the shared source routes every sound step that has no source of its own through yours.")
             .Watch("Speaker; DontDestroyOnLoad in the Hierarchy", speaker)
             .Needs(StationNeeds.PlayMode)
             .HowTo("Enter play mode fresh. Press SetShared first, then Click a few times. Exit, enter again, and press Click before SetShared.")
             .Expect("After SetShared the jingle sounds muffled and no UI Animation Audio object appears. Clicking before SetShared plays it clear, from a new UI Animation Audio.");
        }

        private static void FromUnityEvents(LabBoard b)
        {
            var t = b.Test("C7", "PlayAnimation / StopAnimation from UnityEvents", 440f, 240f);
            var bar = UI.Square(t.Stage, "Bar", new Vector2(-150f, 50f), new Vector2(10f, 30f), Pal.Teal);
            bar.rectTransform.pivot = new Vector2(0f, 0.5f);
            var player = t.Player(Anim.New("Fill", Size(UT.Path("Bar")).To(V3(300f, 30f)).Dur(3f).Ease(Ease.Linear)), Anim.New("Empty", Size(UT.Path("Bar")).ToRest().Dur(0.2f)));

            UI.PlayButton(t.Stage, "Fill", new Vector2(-130f, -20f), new Vector2(110f, 34f), player, "Fill");
            var stop = UI.Button(t.Stage, "Button Stop", "Stop", new Vector2(0f, -20f), new Vector2(110f, 34f), 14f);
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(stop.onClick, player.StopAnimation, "Fill");
            UI.PlayButton(t.Stage, "Empty", new Vector2(130f, -20f), new Vector2(110f, 34f), player, "Empty");

            t.Summary("Buttons wired with no code: On Click → UIAnimationPlayer → PlayAnimation (string) / StopAnimation (string).")
             .Watch("Bar; the buttons' On Click", bar)
             .Needs(StationNeeds.PlayMode | StationNeeds.Inspector)
             .HowTo("Select a button and look at its On Click. In play mode press Fill, Stop half way, Fill again, Empty.")
             .Expect("On Click shows UIAnimationPlayer.PlayAnimation with the name typed in. Stop halts the bar where it is; Fill carries on from there.");
        }
    }
}
