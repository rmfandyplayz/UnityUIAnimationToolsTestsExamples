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
    /// <summary>08 Preview: the edit-mode Play / Reset / Stop and Restore buttons, and how carefully they treat the scene.</summary>
    public static class PreviewScene
    {
        [MenuItem(UT.Menu + "08 Preview")]
        public static void Build()
        {
            var b = UT.Begin("08 Preview", "08  Preview",
                "The edit-mode preview moves real scene objects, so it has to put everything back exactly, step aside for saves, play mode and recompiles, and never run game code. " +
                "These tiles check that, plus how previews chain and how the buttons behave in play mode.");

            Chaining(b);
            ReplaySame(b);
            FinishRunning(b);
            UndoStep(b);
            Selection(b);
            Unwinds(b);
            SaveMidPreview(b);
            MaterialRelease(b);
            NoGameCode(b);
            PlayModeButtons(b);
            EndlessLeft(b);
            CloseFromRest(b);

            b.Save();
        }

        private static void Chaining(LabBoard b)
        {
            var t = b.Test("V1", "Previews carry on from each other");
            var drawer = UI.Box(t.Stage, "Drawer", new Vector2(-150f, 0f), new Vector2(140f, 160f), Pal.Blue);
            t.Player(
                Anim.New("Open", Pos(UT.Path("Drawer")).ToBase(V3(300f, 0f)).Dur(0.5f).Ease(Ease.OutCubic)),
                Anim.New("Close", Pos(UT.Path("Drawer")).ToRest().Dur(0.5f).Ease(Ease.OutCubic)));

            t.Summary("Open slides the drawer right; Close slides it back. Neither has a From.")
             .Watch("Drawer", drawer)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Open, wait, then Play Close. Then Stop and Restore. Then Play Close on its own.")
             .Expect("Close starts from where Open finished and slides back. Stop and Restore returns to rest. Close on its own from rest does nothing visible - it's already home.");
        }

        private static void ReplaySame(LabBoard b)
        {
            var t = b.Test("V2", "Playing the same animation again");
            var box = UT.Box(t, "Box", new Vector2(-180f, 0f), Pal.Green, 60f);
            t.Player(Anim.New("Nudge", Pos(UT.Path("Box")).ToCur(V3(120f, 0f)).Dur(0.4f)));

            t.Summary("Nudge moves 120 right of wherever the box is (To: Current).")
             .Watch("Box", box)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play Nudge three times. Change its X to 60 and Play again.")
             .Expect("Every Play replays from the same start: the box moves 120 from its rest each time, never drifting off. After editing to 60, the next Play uses the new value without Stop and Restore.");
        }

        private static void FinishRunning(LabBoard b)
        {
            var t = b.Test("V3", "Pressing another animation mid-play");
            var box = UT.Box(t, "Box", new Vector2(-180f, 20f), Pal.Orange, 50f);
            var flag = UI.Icon(t.Stage, "Flag", LabAssets.Check, new Vector2(180f, 20f), 50f, Pal.Green);
            flag.gameObject.SetActive(false);
            UT.Mark(t, "Flag spot", new Vector2(180f, 20f), new Vector2(50f, 50f));
            t.Player(
                Anim.New("Long Trip", Pos(UT.Path("Box")).ToBase(V3(300f, 0f)).Dur(3f).Ease(Ease.Linear), Active(UT.Path("Flag"), true)),
                Anim.New("Spin", Rot(UT.Path("Box")).ToCur(V3(0f, 0f, -180f)).Dur(0.5f)));

            t.Summary("Long Trip takes 3 s and then switches the flag on.")
             .Watch("Box, Flag", box, flag)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Long Trip, and Play Spin a second in. Stop and Restore.")
             .Expect("Pressing Spin first finishes Long Trip - the box jumps to the end and the tick appears - then spins there. Where Spin starts never depends on when you clicked. Stop and Restore hides the tick and puts the box home.");
        }

        private static void UndoStep(LabBoard b)
        {
            var t = b.Test("V4", "The whole preview is one Undo step");
            var box = UT.Box(t, "Box", new Vector2(0f, 0f), Pal.Purple, 70f);
            UI.Group(box);
            t.Player(Anim.New("Mess Up",
                Pos(UT.Path("Box")).ToBase(V3(150f, 40f)).Dur(0.4f),
                Rot(UT.Path("Box")).With().To(V3(0f, 0f, 45f)),
                Alpha(UT.Path("Box")).With().To(0.3f),
                Tint(UT.Path("Box")).With().To(Pal.Red)));

            t.Summary("Mess Up moves, turns, fades and tints the box.")
             .Watch("Box", box)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Mess Up and, without stopping, press Ctrl+Z once.")
             .Expect("One Ctrl+Z puts everything back - position, rotation, alpha and colour - and the scene isn't left changed.");
        }

        private static void Selection(LabBoard b)
        {
            var t = b.Test("V5", "Selecting things during a preview");
            var box = UT.Box(t, "Box", new Vector2(-150f, 0f), Pal.Teal, 60f);
            var other = UT.Box(t, "Unrelated", new Vector2(160f, 0f), Pal.Grey, 50f);
            t.Player(Anim.New("Slide", Pos(UT.Path("Box")).ToBase(V3(150f, 0f)).Dur(0.4f)));

            t.Summary("Posing means selecting the animated objects, so that mustn't end a preview. Selecting anything unrelated should.")
             .Watch("Box, Unrelated", box, other)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Slide. Select Box (it stays where the preview left it). Select the tile again. Then select Unrelated.")
             .Expect("Selecting Box keeps the preview: Box stays slid. Selecting Unrelated ends it and Box goes home.");
        }

        private static void Unwinds(LabBoard b)
        {
            var t = b.Test("V6", "Play mode and recompiles end a preview");
            var box = UT.Box(t, "Box", new Vector2(0f, 0f), Pal.Pink, 70f);
            t.Player(Anim.New("Grow", S.Scale(UT.Path("Box")).To(V3(2f, 2f, 1f)).Dur(0.4f)));

            t.Summary("A preview left running must be put back before play mode backs the scene up, and before scripts reload.")
             .Watch("Box", box)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode)
             .HowTo("Play Grow, then enter play mode, then exit. Play Grow again, then make a script recompile (e.g. add a space to any .cs file and save).")
             .Expect("In play mode Box is its normal size, and still is after exiting. After the recompile, Box is back to normal size too.");
        }

        private static void SaveMidPreview(LabBoard b)
        {
            var t = b.Test("V7", "Saving the scene mid-preview");
            var box = UT.Box(t, "Box", new Vector2(-150f, 20f), Pal.Blue, 60f);
            var bar = UI.Square(t.Stage, "Fill Bar", new Vector2(80f, 60f), new Vector2(260f, 30f), Pal.Green);
            bar.material = LabAssets.FillMaterial;
            bar.gameObject.AddComponent<UIMaterialInstance>();
            var text = UI.Label(t.Stage, "TMP", "Thick", new Vector2(80f, -20f), new Vector2(260f, 60f), 40f, Pal.White);
            text.gameObject.AddComponent<UIMaterialInstance>();

            t.Player(Anim.New("Change",
                Pos(UT.Path("Box")).ToBase(V3(0f, -60f)).Dur(0.3f),
                MatFloat(UT.Path("Fill Bar"), "_Progress").With().To(0.2f).Dur(0.3f),
                MatFloat(UT.Path("TMP"), "_FaceDilate").With().To(0.5f).Dur(0.3f)));

            t.Summary("Saving while a preview runs must save the scene at rest - including each element's own material - and carry on.")
             .Watch("Box, Fill Bar, TMP", box, bar, text)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Change. Press Ctrl+S. Then Stop and Restore. (Optionally: close and reopen the scene.)")
             .Expect("Saving doesn't visibly change anything - the preview carries on. After Stop and Restore the scene doesn't show as changed. Reopened, the box is at rest, the bar full, the text thin, and the Fill Bar's material is Lab Fill, not an (Instance).");
        }

        private static void MaterialRelease(LabBoard b)
        {
            var t = b.Test("V8", "Materials go back after a preview");
            var heart = UI.Icon(t.Stage, "Heart", LabAssets.Heart, new Vector2(0f, 10f), 120f, Pal.Pink);
            heart.material = LabAssets.GlowMaterial;
            heart.gameObject.AddComponent<UIMaterialInstance>();
            t.Player(Anim.New("Glow", MatFloat(UT.Path("Heart"), "_Glow").From(0f).To(1f).Dur(0.5f)));

            t.Summary("A material step gives its element a material copy while previewing; ending the preview must give the element its own material back.")
             .Watch("Heart", heart)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Select Heart. Note its Image's Material: Lab Glow. Select this tile, Play Glow, select Heart again and look at the material at the bottom of its Inspector. Stop and Restore, then look again.")
             .Expect("During the preview the material reads Lab Glow (Instance), and the material editor at the bottom shows the instance. After Stop and Restore, it reads Lab Glow again, and the material editor shows the asset.");
        }

        private static void NoGameCode(LabBoard b)
        {
            var t = b.Test("V9", "No On Complete, no sounds in the preview");
            var box = UT.Box(t, "Box", new Vector2(-120f, 10f), Pal.Orange, 60f);
            var counterText = UI.Label(t.Stage, "Counter", "", new Vector2(120f, 10f), new Vector2(240f, 40f), 18f, Pal.White);
            var counter = t.Go.AddComponent<EventCounter>();
            counter.Text = counterText;
            counter.Label = "On Complete fired";

            var player = t.Player(Anim.New("Beep",
                Sound(LabAssets.Coin),
                PunchScale(UT.Path("Box"), V3(0.3f, 0.3f, 0f))).OnDone(counter.Count));
            UI.PlayButton(t.Stage, "Beep", new Vector2(0f, -80f), new Vector2(110f, 32f), player, "Beep");

            t.Summary("Beep plays a coin sound and punches; its On Complete counts.")
             .Watch("Box, the counter", box, counterText)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode)
             .HowTo("Preview Beep a few times. Then in play mode, click the Beep button.")
             .Expect("Preview: silent, the counter text doesn't change, and no UI Animation Audio object appears in the Hierarchy. Play mode: the coin sounds and the counter goes up with each click.");
        }

        private static void PlayModeButtons(LabBoard b)
        {
            var t = b.Test("V10", "The preview buttons in play mode");
            var box = UT.Box(t, "Box", new Vector2(-150f, 0f), Pal.Purple, 60f);
            t.Player(
                Anim.New("Away", Pos(UT.Path("Box")).ToBase(V3(300f, 0f)).Dur(0.5f)),
                Anim.New("Pop In", S.Scale(UT.Path("Box")).From(Vector3.zero).ToRest().Dur(0.4f).Ease(Ease.OutBack)));

            t.Summary("In play mode the buttons call the player itself, and the footer adds Stop and Restore.")
             .Watch("Box", box)
             .Needs(StationNeeds.PlayMode | StationNeeds.Inspector)
             .HowTo("In play mode, select this tile. Play Away, then Play Away again. Press Reset on Pop In. Then Stop and Restore.")
             .Expect("The second Away does nothing visible - it has no From and is already there; a note under the buttons says so. Reset on Pop In hides the box (it has a From). Stop and Restore puts the box back where it started.");
        }

        private static void EndlessLeft(LabBoard b)
        {
            var t = b.Test("V11", "An endless loop pressed over");
            var box = UT.Box(t, "Box", new Vector2(0f, 0f), Pal.Teal, 60f);
            t.Player(
                Anim.New("Spin Forever", Rot(UT.Path("Box")).ToCur(V3(0f, 0f, -360f)).Dur(2f).Ease(Ease.Linear)).Loop(-1, LoopType.Incremental),
                Anim.New("Pop", PunchScale(UT.Path("Box"), V3(0.4f, 0.4f, 0f))));

            t.Summary("An endless loop has no end to finish to.")
             .Watch("Box", box)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Spin Forever, then Pop half way round.")
             .Expect("The spin stops where it stands (it can't be finished), and the box punches at that angle.");
        }

        private static void CloseFromRest(LabBoard b)
        {
            var t = b.Test("V12", "Every preview Play starts from the first frame");
            var credits = UI.Box(t.Stage, "Credits", new Vector2(0f, 20f), new Vector2(360f, 150f), Pal.FramePanel);
            UI.Group(credits, 0.4f);
            UI.Label(credits.transform, "Text", "Credits", Vector2.zero, new Vector2(300f, 60f), 30f, Pal.White);

            var player = t.Player(
                Anim.New("Close Credits",
                    Alpha(UT.Path("Credits")).From(1f).To(0f).Dur(0.4f).Delay(0.6f),
                    Active(UT.Path("Credits"), false)).NoFromImmediately(),
                Anim.New("Show Again", Active(UT.Path("Credits"), true), Alpha(UT.Path("Credits")).ToRest().Dur(0.01f)));
            UI.PlayButton(t.Stage, "Close Credits", new Vector2(-80f, -95f), new Vector2(150f, 32f), player, "Close Credits");
            UI.PlayButton(t.Stage, "Show Again", new Vector2(80f, -95f), new Vector2(150f, 32f), player, "Show Again");

            t.Summary("Credits rests dimmed (alpha 0.4). Close Credits fades it from 1 to 0 after a 0.6 s delay, with Apply From Values Immediately off.")
             .Watch("Credits", credits)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode)
             .HowTo("Preview Close Credits. Then in play mode click Close Credits, then Show Again.")
             .Expect("Preview: Credits jumps to fully shown at once, holds, then fades out - every preview Play is Reset then Play. Play mode: it stays dim for 0.6 s, then flashes to full and fades - the setting is off, and the preview deliberately doesn't copy that.");
        }
    }
}
