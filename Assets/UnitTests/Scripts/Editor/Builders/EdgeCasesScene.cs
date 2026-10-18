using DG.Tweening;
using rmf_claude.DOTweenUI;
using TMPro;
using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using static UIAnimLab.EditorTools.S;

namespace UIAnimLab.UnitTests.Builders
{
    /// <summary>11 Edge Cases and Warnings: the ways things go wrong, and what the tool does about each.</summary>
    public static class EdgeCasesScene
    {
        [MenuItem(UT.Menu + "11 Edge Cases and Warnings")]
        public static void Build()
        {
            var b = UT.Begin("11 Edge Cases and Warnings", "11  Edge Cases and Warnings",
                "Things that go wrong on purpose: warnings in play mode, a property that throws, a stencil Mask, a Layout Group, steps that fight, a destroyed object mid-play, two components of one type, " +
                "and a check that every step type still has the number it was saved with.");

            ConsoleWarnings(b);
            Thrower(b);
            StencilMask(b);
            LayoutGroupFight(b);
            BoxVsEdge(b);
            TwoScales(b);
            DestroyMidPlay(b);
            TwoComponents(b);
            EnumIntegrity(b);

            b.Save();
        }

        private static void ConsoleWarnings(LabBoard b)
        {
            var t = b.Test("X1", "Warnings in play mode");
            var box = UT.Box(t, "Box", new Vector2(-150f, 30f), Pal.Grey, 60f);
            var player = t.Player(
                Anim.New("Broken",
                    Alpha(UT.Path("Box")).To(0f),
                    Rot("Stage/Boxx").ToCur(V3(0f, 0f, 90f)),
                    PunchScale(UT.Path("Box"), V3(0.3f, 0.3f, 0f))));
            UI.PlayButton(t.Stage, "Play Broken", new Vector2(-150f, -60f), new Vector2(150f, 34f), player, "Broken");
            var nope = UI.Button(t.Stage, "Button Nope", "Play 'Nope'", new Vector2(40f, -60f), new Vector2(150f, 34f), 14f);
            UnityEventTools.AddStringPersistentListener(nope.onClick, player.PlayAnimation, "Nope");

            t.Summary("Broken has a Canvas Group step on a box with none, and a path to 'Boxx'. Its last step (a punch) is fine.")
             .Watch("Box", box)
             .Needs(StationNeeds.PlayMode | StationNeeds.Console)
             .HowTo("Clear the Console. Enter play mode. Click Play Broken five times, then Play 'Nope' twice.")
             .Expect("Each warning reads '<who>: CONSEQUENCE IN CAPITALS. details' and clicking it selects the tile. One warning for each broken step when the game starts - no more however often Broken plays - and the punch plays every time. Play 'Nope' warns that there's no animation called Nope. Nothing throws.");
        }

        private static void Thrower(LabBoard b)
        {
            var t = b.Test("X2", "A property that throws");
            var bomb = UI.Icon(t.Stage, "Bomb", LabAssets.Circle, new Vector2(-100f, 20f), 70f, Pal.Red);
            bomb.gameObject.AddComponent<TestThrower>();
            var safe = UT.Box(t, "Safe", new Vector2(100f, 20f), Pal.Green, 60f);

            var player = t.Player(Anim.New("Go",
                Prop(UT.Path("Bomb"), typeof(TestThrower), "Explodes", UIAnimationPropertyKind.Float).To(5f),
                Rot(UT.Path("Safe")).With().ToCur(V3(0f, 0f, -360f)).Dur(0.8f)));
            UI.PlayButton(t.Stage, "Go", new Vector2(0f, -80f), new Vector2(110f, 32f), player, "Go");

            t.Summary("Bomb's Explodes property throws whenever it is read. Go animates it, and spins Safe.")
             .Watch("Bomb, Safe", bomb, safe)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode | StationNeeds.Console)
             .HowTo("Preview Go, then Stop and Restore. Enter play mode and click Go a few times.")
             .Expect("The Explodes step warns once and is skipped; Safe spins every time; no exception escapes, and Stop and Restore still puts everything back.");
        }

        private static void StencilMask(LabBoard b)
        {
            var t = b.Test("X3", "Stencil Mask vs RectMask2D", 620f, 280f);
            var masked = MakeMaskedGlow(t, "Under Mask", new Vector2(-140f, 20f), stencil: true);
            var clipped = MakeMaskedGlow(t, "Under RectMask2D", new Vector2(140f, 20f), stencil: false);

            t.Player(Anim.New("Glow Both",
                MatFloat("Stage/Under Mask/Heart", "_Glow").From(0f).To(1f).Dur(0.6f),
                MatFloat("Stage/Under RectMask2D/Heart", "_Glow").With().From(0f).To(1f).Dur(0.6f)));

            t.Summary("Two hearts with the Lab Glow material and a UI Material Instance. One sits under a stencil Mask, the other under a RectMask2D.")
             .Watch("Under Mask, Under RectMask2D", masked, clipped)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode | StationNeeds.Console | StationNeeds.Inspector)
             .HowTo("Select the masked Heart: look at its UI Material Instance. Play Glow Both (preview, then play mode).")
             .Expect("The UI Material Instance under the stencil Mask warns about it. Only the heart under the RectMask2D lights up; the other doesn't change.");
        }

        private static GameObject MakeMaskedGlow(Tile t, string name, Vector2 at, bool stencil)
        {
            var holder = UI.Box(t.Stage, name, at, new Vector2(200f, 160f), Pal.FramePanel);
            if (stencil) holder.gameObject.AddComponent<Mask>();
            else holder.gameObject.AddComponent<RectMask2D>();
            var heart = UI.Icon(holder.transform, "Heart", LabAssets.Heart, Vector2.zero, 110f, Pal.Pink);
            heart.material = LabAssets.GlowMaterial;
            heart.gameObject.AddComponent<UIMaterialInstance>();
            UI.Caption(t.Stage, name, at + new Vector2(0f, -95f));
            return heart.gameObject;
        }

        private static void LayoutGroupFight(LabBoard b)
        {
            var t = b.Test("X4", "Inside a Layout Group");
            var list = UI.Rect(t.Stage, "List", new Vector2(0f, 0f), new Vector2(300f, 170f));
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            for (int i = 0; i < 3; i++)
            {
                var row = UI.Box(list, "Row " + (i + 1), Vector2.zero, new Vector2(300f, 50f), Pal.Blue);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 50f;
                UI.Box(row.transform, "Inner", Vector2.zero, new Vector2(280f, 40f), Pal.Teal);
            }

            t.Player(
                Anim.New("Move Row", Pos("Stage/List/Row 1").ToBase(V3(150f, 0f)).Dur(0.5f)),
                Anim.New("Move Inner", Pos("Stage/List/Row 2/Inner").ToBase(V3(150f, 0f)).Dur(0.5f)),
                Anim.New("Back", Pos("Stage/List/Row 1").ToRest().Dur(0.2f), Pos("Stage/List/Row 2/Inner").With().ToRest().Dur(0.2f)));

            t.Summary("A Layout Group places its children itself, and fights position steps on them.")
             .Watch("List", list.gameObject)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode)
             .HowTo("Preview Move Row and Move Inner. Then try both in play mode (select the tile and use its Play buttons), and resize the Game view while Row 1 is moved.")
             .Expect("Moving the row itself is undone by the layout whenever it rebuilds (in play mode at the latest); moving the Inner child of Row 2 works and stays. The recommended fix: animate a child inside each item.");
        }

        private static void BoxVsEdge(LabBoard b)
        {
            var t = b.Test("X5", "Whole-box and edge steps fight");
            var frame = UI.Square(t.Stage, "Frame", Vector2.zero, new Vector2(460f, 160f), Pal.Ghost);
            var panel = UI.Square(frame.transform, "Panel", Vector2.zero, Vector2.zero, Pal.Purple);
            panel.rectTransform.anchorMin = new Vector2(0f, 0f);
            panel.rectTransform.anchorMax = new Vector2(0.4f, 1f);
            panel.rectTransform.offsetMin = new Vector2(10f, 10f);
            panel.rectTransform.offsetMax = new Vector2(-10f, -10f);

            t.Player(Anim.New("Slide And Hold",
                Pos("Stage/Frame/Panel").ToBase(V3(150f, 0f)).Dur(0.6f),
                OffMin("Stage/Frame/Panel").With().ToRest().Dur(0.6f)));

            t.Summary("Slide And Hold slides the panel 150 right with Anchored Position while an Offset Min step holds its left edge where it rests.")
             .Watch("Panel", panel)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Slide And Hold.")
             .Expect("Instead of moving, the panel stretches to the right: its left edge stays put. This is the documented fight - mix Anchored Position with Size Delta, or Offset Min with Offset Max, not across.");
        }

        private static void TwoScales(LabBoard b)
        {
            var t = b.Test("X6", "Two steps on one value at once");
            var box = UT.Box(t, "Box", new Vector2(-100f, 0f), Pal.Orange, 70f);
            var fixedBox = UT.Box(t, "Fixed", new Vector2(100f, 0f), Pal.Green, 70f);
            UI.Icon(fixedBox.transform, "Icon", LabAssets.Star, Vector2.zero, 40f, Pal.Yellow);

            t.Player(
                Anim.New("Fight", S.Scale(UT.Path("Box")).To(V3(1.5f, 1.5f, 1f)).Dur(0.6f), PunchScale(UT.Path("Box"), V3(0.4f, 0.4f, 0f), 0.6f).With()),
                Anim.New("Fixed", S.Scale(UT.Path("Fixed")).To(V3(1.5f, 1.5f, 1f)).Dur(0.6f), PunchScale("Stage/Fixed/Icon", V3(0.6f, 0.6f, 0f), 0.6f).With()),
                Anim.New("Reset", S.Scale(UT.Path("Box")).ToRest().Dur(0.2f), S.Scale(UT.Path("Fixed")).With().ToRest().Dur(0.2f)));

            t.Summary("Fight runs a Scale and a Punch Scale on Box at the same time. Fixed punches a child instead.")
             .Watch("Box, Fixed", box, fixedBox)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Fight, then Fixed, then Reset.")
             .Expect("Fight: the two steps overwrite each other - the growth stutters or the punch is lost, and the box doesn't end cleanly at 1.5. Fixed: the box grows smoothly while its star punches.");
        }

        private static void DestroyMidPlay(LabBoard b)
        {
            var t = b.Test("X7", "Destroying an object mid-play");
            var holder = UI.Rect(t.Stage, "Holder", new Vector2(-80f, 20f), new Vector2(10f, 10f));
            var victim = UT.Box(t, "Victim", Vector2.zero, Pal.Red, 60f);
            victim.transform.SetParent(holder, false);
            var victimPlayer = t.PlayerOn(victim.gameObject, Anim.New("Spin", Rot(null).ToCur(V3(0f, 0f, -360f)).Dur(2f).Ease(Ease.Linear)).Loop(-1, LoopType.Incremental));
            var auto = victim.gameObject.AddComponent<PlayOnEnable>();
            auto.Animation = "Spin";

            var button = UI.Button(t.Stage, "Button Destroy", "Destroy Victim", new Vector2(100f, -60f), new Vector2(170f, 34f), 14f);
            var actions = button.gameObject.AddComponent<TestActions>();
            actions.Toggled = victim.gameObject;
            UI.OnClick(button, actions.DestroyObject);

            t.Summary("Victim spins forever from the moment play mode starts.")
             .Watch("Victim", victim)
             .Needs(StationNeeds.PlayMode | StationNeeds.Console)
             .HowTo("In play mode click Destroy Victim. Then exit play mode.")
             .Expect("Victim disappears with no error in the Console, then or later. Nothing is left running in the background.");
        }

        private static void TwoComponents(LabBoard b)
        {
            var t = b.Test("X8", "Two components of one type");
            var gauges = UI.Rect(t.Stage, "Gauges", Vector2.zero, new Vector2(400f, 140f));
            var first = MakeBar(gauges, "First Bar", 30f, Pal.Green);
            var second = MakeBar(gauges, "Second Bar", -30f, Pal.Orange);
            gauges.gameObject.AddComponent<TestGauge>().Setup(first.Item1, first.Item2);
            gauges.gameObject.AddComponent<TestGauge>().Setup(second.Item1, second.Item2);

            t.Player(Anim.New("Drain", Prop(UT.Path("Gauges"), typeof(TestGauge), "Health", UIAnimationPropertyKind.Float).To(10f).Dur(0.6f)));

            t.Summary("Gauges has two TestGauge components, each drawing its own bar. A Custom Property step on TestGauge.Health uses the first.")
             .Watch("Gauges", gauges.gameObject)
             .Needs(StationNeeds.Preview)
             .HowTo("Play Drain.")
             .Expect("Only the first (green) bar drains; the orange one stays at 75.");
        }

        private static (Image, TMP_Text) MakeBar(Transform parent, string name, float y, Color color)
        {
            UI.Square(parent, name + " Back", new Vector2(-40f, y), new Vector2(300f, 22f), Pal.Ghost);
            var bar = UI.Square(parent, name, new Vector2(-40f, y), new Vector2(300f, 22f), color);
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            var label = UI.Label(parent, name + " Label", "", new Vector2(170f, y), new Vector2(160f, 26f), 15f, Pal.White, TextAlignmentOptions.Left);
            return (bar, label);
        }

        private static void EnumIntegrity(LabBoard b)
        {
            var t = b.Test("X9", "Every step type keeps its number", 620f, 120f);
            // The player sits on its own inactive object, so it never starts up and never warns in play mode;
            // the Inspector still shows each step's header.
            var holder = UI.Rect(t.Stage, "Numbers", Vector2.zero, new Vector2(10f, 10f));

            // One step per stored number, 0 to 20, in number order. If an enum value were ever renumbered, these
            // headers would stop matching the list below.
            var steps = new UIAnimationStep[21];
            for (int i = 0; i <= 20; i++) steps[i] = Of((UIAnimationStepType)i);
            t.PlayerOn(holder.gameObject, Anim.New("Numbers 0 to 20", steps).Note("One step per stored type number, in order. See the tile for the expected list."));
            holder.gameObject.SetActive(false);
            UI.Label(t.Stage, "Note", "Nothing to play: compare the headers with the list below.", Vector2.zero, new Vector2(560f, 40f), 16f, Pal.BodyText);

            t.Summary("A step saved with each type number from 0 to 20, in order. Unity stores the number, so the headers must still read these types.")
             .Needs(StationNeeds.Inspector)
             .Watch("Numbers (inactive; holds the player)", holder.gameObject)
             .HowTo("Select Numbers. Open Numbers 0 to 20 and read the type in brackets on each header, top to bottom. Warning signs on them are expected (Numbers has none of the components).")
             .Expect("0 Anchored Position, 1 Canvas Group Alpha, 2 Graphic Alpha, 3 Graphic Color, 4 Local Position, 5 Material Color, 6 Material Float, 7 Offset Min, 8 Offset Max, 9 Punch Anchored Position, 10 Punch Scale, 11 Rotation, 12 Scale, 13 Set Active, 14 Shake Anchored Position, 15 Size Delta, 16 Play Sound, 17 Punch Rotation, 18 Shake Rotation, 19 Shake Scale, 20 Custom Property.");
        }
    }
}
