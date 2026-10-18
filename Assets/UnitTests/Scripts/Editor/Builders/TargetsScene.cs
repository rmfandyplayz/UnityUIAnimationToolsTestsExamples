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
    /// <summary>06 Targets and Animation Sets: what a step animates, and sharing animations between players.</summary>
    public static class TargetsScene
    {
        private const string ButtonSet = UT.Sets + "Button Set.asset";
        private const string PanelSet = UT.Sets + "Panel Set.asset";
        private const string SpinSet = UT.Sets + "Spin Set.asset";

        // Fixed so a rebuild keeps every player's own On Complete pointing at the same animation.
        private const string ShowId = "5b1c0e6a9f7d4c2e8a3b6d0f1e2c4a71";

        [MenuItem(UT.Menu + "06 Targets and Animation Sets")]
        public static void Build()
        {
            var buttonSet = Anim.Asset(ButtonSet,
                Anim.New("Hover", S.Scale("..").ToBase(V3(0.08f, 0.08f, 0f)).Dur(0.15f)).InterruptStop(),
                Anim.New("Unhover", S.Scale("..").ToRest().Dur(0.15f)).InterruptStop(),
                Anim.New("Press", PunchRot("../Icon", V3(0f, 0f, 25f), 0.4f), Tint("../Label").With().From(Pal.Yellow).ToRest().Dur(0.4f)));
            var panelSet = Anim.Asset(PanelSet,
                Anim.New("Show", Alpha("..").From(0f).To(1f).Dur(0.4f), S.Scale("..").With().From(V3(0.8f, 0.8f, 1f)).ToRest().Dur(0.4f).Ease(Ease.OutBack)).Id(ShowId),
                Anim.New("Hide", Alpha("..").To(0f).Dur(0.3f)));
            var spinSet = Anim.Asset(SpinSet,
                Anim.New("Spin", Rot(null).ToCur(V3(0f, 0f, -360f)).Dur(1.5f).Ease(Ease.InOutCubic)),
                Anim.New("Wobble", ShakeRot(null, V3(0f, 0f, 20f), 0.6f)));

            var b = UT.Begin("06 Targets and Animation Sets", "06  Targets and Animation Sets",
                "Which object a step animates - the target slot, a Target Path (down, up with .., hidden objects, misses) - and Animation Sets: shared animations, local overrides, " +
                "each player's own On Complete, Override, Save as Animation Set and Preview On. The sets live in Assets/UnitTests/Animation Sets.");

            ChildPaths(b);
            ParentPaths(b);
            Miss(b);
            SlotWins(b);
            SharedButtons(b, buttonSet);
            LocalOverride(b, buttonSet);
            PlayerOnComplete(b, panelSet);
            Orphans(b, panelSet);
            SaveAsSet(b);
            PreviewOn(b);
            Independent(b, spinSet);
            DuplicateNames(b);

            b.Save();
        }

        private static void ChildPaths(LabBoard b)
        {
            var t = b.Test("G1", "Target Path: children and hidden objects");
            var panel = UI.Box(t.Stage, "Panel", new Vector2(-60f, 0f), new Vector2(300f, 180f), Pal.FramePanel);
            var icon = UI.Icon(panel.transform, "Icon", LabAssets.Star, new Vector2(-80f, 0f), 80f, Pal.Yellow);
            var hidden = UI.Icon(panel.transform, "Hidden", LabAssets.Heart, new Vector2(70f, 0f), 80f, Pal.Pink);
            hidden.gameObject.SetActive(false);
            UT.Mark(t, "Hidden spot", new Vector2(10f, 0f), new Vector2(80f, 80f));

            t.Player(Anim.New("Go",
                Rot("Stage/Panel/Icon").ToCur(V3(0f, 0f, -360f)).Dur(0.8f),
                Active("Stage/Panel/Hidden", true).Delay(0.1f),
                S.Scale("Stage/Panel/Hidden").With().From(Vector3.zero).ToRest().Dur(0.4f).Ease(Ease.OutBack),
                Pos("Stage/Panel").With().ToBase(V3(120f, 0f)).Dur(0.8f)));

            t.Summary("Paths reach down through children by name, and find inactive objects too.")
             .Watch("Panel, Icon, Hidden (starts off)", panel, icon, hidden)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Go</b>. Fold the steps closed and read the headers.")
             .Expect("The star spins, the hidden heart switches on and pops in, and the whole panel slides right. Headers show the end of each path, like Panel/Icon.");
        }

        private static void ParentPaths(LabBoard b)
        {
            var t = b.Test("G2", "Target Path: up with ..");
            var button = UI.Box(t.Stage, "Button", new Vector2(0f, 10f), new Vector2(220f, 80f), Pal.Blue);
            var icon = UI.Icon(button.transform, "Icon", LabAssets.Star, new Vector2(-70f, 0f), 44f, Pal.Yellow);
            var label = UI.Label(button.transform, "Label", "Play", new Vector2(25f, 0f), new Vector2(120f, 40f), 26f, Pal.White);
            var sibling = UI.Icon(t.Stage, "Badge", LabAssets.Circle, new Vector2(140f, 50f), 30f, Pal.Red);
            var animations = UI.Rect(button.transform, "Animations", Vector2.zero, Vector2.zero);

            t.PlayerOn(animations.gameObject, Anim.New("Press",
                PunchScale("..", V3(-0.12f, -0.12f, 0f), 0.35f),
                PunchRot("../Icon", V3(0f, 0f, 30f), 0.45f).With(),
                Tint("../Label").With().From(Pal.Yellow).ToRest().Dur(0.45f),
                PunchScale("../../Badge", V3(0.6f, 0.6f, 0f), 0.4f).With()));

            t.Summary("The player sits on an Animations child of the button. '..' is the button, '../Icon' a child of it, '../../Badge' a child of the stage.")
             .Watch("Animations (select it to play), Button, Icon, Label, Badge", animations.gameObject, button.gameObject, icon.gameObject, label.gameObject, sibling.gameObject)
             .Needs(StationNeeds.Preview)
             .HowTo("Select <b>Animations</b> (Select button above) and Play <b>Press</b>.")
             .Expect("The button squishes, its star twists, its label flashes yellow, and the red badge outside it pops - all from the one player on Animations.");
        }

        private static void Miss(LabBoard b)
        {
            var t = b.Test("G3", "A path that matches nothing");
            var panel = UI.Box(t.Stage, "Panel", new Vector2(0f, 0f), new Vector2(260f, 150f), Pal.FramePanel);
            var icon = UI.Icon(panel.transform, "Icon", LabAssets.Star, new Vector2(0f, 0f), 70f, Pal.Yellow);

            var player = t.Player(Anim.New("Spin",
                Rot("Stage/Panel/Iconn").ToCur(V3(0f, 0f, -360f)).Dur(0.8f),
                PunchScale("Stage/Panel", V3(0.15f, 0.15f, 0f)).With()));
            UI.PlayButton(t.Stage, "Spin", new Vector2(0f, -100f), new Vector2(110f, 32f), player, "Spin");

            t.Summary("Step 0 aims at Panel/Iconn - a typo. It must do nothing: never spin some other object instead.")
             .Watch("Panel, Icon", panel, icon)
             .Needs(StationNeeds.Inspector | StationNeeds.Preview | StationNeeds.PlayMode | StationNeeds.Console)
             .HowTo("Look at Spin's steps. Preview Spin. Enter play mode, click Spin twice, and check the Console.")
             .Expect("Step 0's header ends in a warning sign, its fields below the target are greyed, and a warning says Target Path \"Stage/Panel/Iconn\" matches nothing. Playing only punches the panel; nothing spins. The Console gets ONE warning about it, at start-up, however often Spin is played.");
        }

        private static void SlotWins(LabBoard b)
        {
            var t = b.Test("G4", "A filled slot wins over the path");
            var a = UT.Box(t, "Box A", new Vector2(-120f, 0f), Pal.Blue, 70f);
            var bb = UT.Box(t, "Box B", new Vector2(120f, 0f), Pal.Green, 70f);
            UI.Label(a.transform, "L", "A", Vector2.zero, new Vector2(70f, 70f), 30f, Pal.Stage);
            UI.Label(bb.transform, "L", "B", Vector2.zero, new Vector2(70f, 70f), 30f, Pal.Stage);

            t.Player(Anim.New("Spin", Rot(UT.Path("Box A")).Target(bb).ToCur(V3(0f, 0f, -360f)).Dur(0.8f)));

            t.Summary("The step's Rect Transform slot holds Box B; its Target Path still says Stage/Box A.")
             .Watch("Box A, Box B", a, bb)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Open Spin's step and Play.")
             .Expect("The Target Path field is greyed out. B spins; A never moves. The header names Box B.");
        }

        private static GameObject MakeSetButton(Tile t, string name, Vector2 at, Color color, UIAnimationAsset set, out UIAnimationPlayer player)
        {
            var button = UI.Box(t.Stage, name, at, new Vector2(160f, 70f), color);
            button.raycastTarget = true;
            UI.Icon(button.transform, "Icon", LabAssets.Star, new Vector2(-50f, 0f), 36f, Pal.Yellow);
            UI.Label(button.transform, "Label", name, new Vector2(20f, 0f), new Vector2(100f, 40f), 20f, Pal.White);
            var animations = UI.Rect(button.transform, "Animations", Vector2.zero, Vector2.zero);
            player = t.PlayerOn(animations.gameObject);
            Anim.SetShared(player, set);

            UI.Trigger(button, UnityEngine.EventSystems.EventTriggerType.PointerEnter, player, "Hover");
            UI.Trigger(button, UnityEngine.EventSystems.EventTriggerType.PointerExit, player, "Unhover");
            UI.Trigger(button, UnityEngine.EventSystems.EventTriggerType.PointerClick, player, "Press");
            return animations.gameObject;
        }

        private static void SharedButtons(LabBoard b, UIAnimationAsset set)
        {
            var t = b.Test("A1", "One set, many players");
            var first = MakeSetButton(t, "Start", new Vector2(-170f, 0f), Pal.Blue, set, out _);
            var second = MakeSetButton(t, "Options", new Vector2(0f, 0f), Pal.Purple, set, out _);
            var third = MakeSetButton(t, "Quit", new Vector2(170f, 0f), Pal.Red, set, out _);

            t.Summary("Three buttons, each with a player on its Animations child using Button Set (Hover, Unhover, Press), and no animations of its own.")
             .Watch("each button's Animations", first, second, third)
             .Needs(StationNeeds.Preview | StationNeeds.PlayMode | StationNeeds.Inspector)
             .HowTo("Select one button's Animations: the Inspector lists Shared Animations (Button Set), greyed out, with a Play row for each. Play Press on each. In play mode, hover and click the buttons.")
             .Expect("Each button only animates itself, though they share one set. Shared steps can be opened but not edited; right-click offers only Copy and Override.");
        }

        private static void LocalOverride(LabBoard b, UIAnimationAsset set)
        {
            var t = b.Test("A2", "A player's own animation wins");
            UIAnimationPlayer player;
            var animations = MakeSetButton(t, "Special", new Vector2(0f, 10f), Pal.Teal, set, out player);
            Anim.Set(player, Anim.New("Press", ShakeRot("../Icon", V3(0f, 0f, 40f), 0.6f), PunchScale("..", V3(0.2f, 0.2f, 0f)).With()));

            t.Summary("This player uses Button Set, but has its own Press as well.")
             .Watch("Special's Animations", animations)
             .Needs(StationNeeds.Inspector | StationNeeds.Preview)
             .HowTo("Select Special's Animations. Read the grey notes on both Press entries, and the note above the Play buttons. Play Press.")
             .Expect("The player's own Press reads 'overrides shared'; the set's, under Shared Animations, reads 'overridden locally'. Playing Press shakes the star and grows the button (the local one), not the set's tint.");
        }

        private static void PlayerOnComplete(LabBoard b, UIAnimationAsset set)
        {
            var t = b.Test("A3", "Each player's own On Complete, and Override");
            var left = MakePanel(t, "Panel L", new Vector2(-140f, 20f), set, out var leftPlayer, out var leftDone);
            var right = MakePanel(t, "Panel R", new Vector2(140f, 20f), set, out var rightPlayer, out var rightDone);

            var show = set.Animations[0];
            UnityEventTools.AddBoolPersistentListener(Anim.SharedOnComplete(leftPlayer, show), leftDone.SetActive, true);
            UnityEventTools.AddBoolPersistentListener(Anim.SharedOnComplete(rightPlayer, show), rightDone.SetActive, true);

            UI.PlayButton(t.Stage, "Show L", new Vector2(-140f, -95f), new Vector2(110f, 32f), leftPlayer, "Show");
            UI.PlayButton(t.Stage, "Show R", new Vector2(140f, -95f), new Vector2(110f, 32f), rightPlayer, "Show");

            t.Summary("Two panels share Panel Set's Show. Each player has its own On Complete for it, switching on its own tick.")
             .Watch("each panel's Animations", left, right)
             .Needs(StationNeeds.PlayMode | StationNeeds.Inspector)
             .HowTo("In play mode click Show L, then Show R. Out of play mode: open Show under Panel L's Shared Animations, rename Show to Appear in Panel Set.asset, and check the On Complete is still attached. Then press Override on it. Undo both afterwards.")
             .Expect("Each Show ticks only its own panel. After the rename the entry stays attached. After Override, Show appears in the player's own list with the On Complete moved onto it.");
        }

        private static GameObject MakePanel(Tile t, string name, Vector2 at, UIAnimationAsset set, out UIAnimationPlayer player, out GameObject done)
        {
            var panel = UI.Box(t.Stage, name, at, new Vector2(200f, 120f), Pal.FramePanel);
            UI.Group(panel);
            UI.Label(panel.transform, "Title", name, new Vector2(0f, 25f), new Vector2(180f, 30f), 20f, Pal.White);
            done = UI.Icon(panel.transform, "Done", LabAssets.Check, new Vector2(0f, -20f), 40f, Pal.Green).gameObject;
            done.SetActive(false);
            var animations = UI.Rect(panel.transform, "Animations", Vector2.zero, Vector2.zero);
            player = t.PlayerOn(animations.gameObject);
            Anim.SetShared(player, set);
            return animations.gameObject;
        }

        private static void Orphans(LabBoard b, UIAnimationAsset set)
        {
            var t = b.Test("A4", "Leftover On Completes");
            var animations = MakePanel(t, "Panel", new Vector2(0f, 10f), set, out var player, out var done);
            var gone = new UIAnimation { Name = "Old Show", Id = "0000000000000000000000000000dead" };
            UnityEventTools.AddBoolPersistentListener(Anim.SharedOnComplete(player, gone), done.SetActive, true);

            t.Summary("This player has its own On Complete for 'Old Show', an animation no longer in Panel Set.")
             .Watch("Panel's Animations", animations)
             .Needs(StationNeeds.Inspector)
             .HowTo("Select Panel's Animations and look under Shared Animations. Press Remove, then Undo.")
             .Expect("A note says the player has its own On Complete for an animation no longer in 'Panel Set': 'Old Show'. It never runs. Remove deletes it; Undo brings it back.");
        }

        private static void SaveAsSet(LabBoard b)
        {
            var t = b.Test("A5", "Save as Animation Set", 620f, 280f);
            var card = UI.Box(t.Stage, "Card", new Vector2(-120f, 20f), new Vector2(220f, 150f), Pal.FramePanel);
            var icon = UI.Icon(card.transform, "Icon", LabAssets.Star, new Vector2(-60f, 30f), 50f, Pal.Yellow);
            var twinA = UI.Icon(card.transform, "Twin", LabAssets.Circle, new Vector2(20f, 30f), 30f, Pal.Teal);
            var twinB = UI.Icon(card.transform, "Twin", LabAssets.Circle, new Vector2(70f, 30f), 30f, Pal.Orange);
            var animations = UI.Rect(card.transform, "Animations", Vector2.zero, Vector2.zero);
            var far = UI.Icon(t.Stage, "Elsewhere", LabAssets.Heart, new Vector2(170f, 20f), 60f, Pal.Pink);

            var player = t.PlayerOn(animations.gameObject, Anim.New("Pop",
                PunchScale(null, V3(0.1f, 0.1f, 0f)).Target(card),
                PunchRot(null, V3(0f, 0f, 30f)).With().Target(icon),
                PunchScale(null, V3(0.5f, 0.5f, 0f)).With().Target(twinB),
                PunchScale(null, V3(0.5f, 0.5f, 0f)).With().Target(far)));
            Anim.Add(player, Anim.New("Notify", Active(null, true).Target(far)).OnDone(far.gameObject.SetActive, true));

            t.Summary("The Card's player drives its slots by reference: the card, its icon, the second of two circles both named Twin, and a heart outside the card.")
             .Watch("Card's Animations", animations.gameObject)
             .Needs(StationNeeds.Inspector | StationNeeds.Console)
             .HowTo("Select Card's Animations. Right-click the Animations list → <b>Save as Animation Set…</b> and save it into Assets/UnitTests/Animation Sets/_Scratch. Open the new set. Delete it when done.")
             .Expect("The set's Pop has paths '..' (card) and '../Icon', and '../../Elsewhere' for the heart (up to the stage, then down). The second Twin can't be reached by name: the Console lists it, and that step warns. Notify's On Complete was left out, since it pointed at a scene object. The player itself is unchanged.");
        }

        private static void PreviewOn(LabBoard b)
        {
            var t = b.Test("A6", "Previewing a set with Preview On");
            UIAnimationPlayer player;
            var animations = MakeSetButton(t, "Preview Me", new Vector2(0f, 10f), Pal.Orange, AssetDatabase.LoadAssetAtPath<UIAnimationAsset>(ButtonSet), out player);

            t.Summary("A set has nothing of its own to animate, so its Inspector borrows a player that uses it.")
             .Watch("Preview Me's Animations", animations)
             .Needs(StationNeeds.Inspector | StationNeeds.Preview | StationNeeds.SceneView)
             .HowTo("Select Assets/UnitTests/Animation Sets/Button Set.asset. Drag Preview Me's Animations into <b>Preview On</b>. Play Press from the set's Inspector. Then clear Preview On.")
             .Expect("Play animates Preview Me. With Preview On filled, step warnings check against that player, and the Scene view draws the set's movement gizmos on it. With it empty, the Play buttons have nothing to play on.");
        }

        private static void Independent(LabBoard b, UIAnimationAsset set)
        {
            var t = b.Test("A7", "Players sharing a set don't interfere");
            var one = UT.Box(t, "One", new Vector2(-120f, 20f), Pal.Blue, 70f);
            var two = UT.Box(t, "Two", new Vector2(120f, 20f), Pal.Green, 70f);
            var p1 = t.PlayerOn(one.gameObject);
            var p2 = t.PlayerOn(two.gameObject);
            Anim.SetShared(p1, set);
            Anim.SetShared(p2, set);

            UI.PlayButton(t.Stage, "Spin One", new Vector2(-180f, -80f), new Vector2(110f, 32f), p1, "Spin");
            UI.PlayButton(t.Stage, "Wobble One", new Vector2(-60f, -80f), new Vector2(110f, 32f), p1, "Wobble");
            UI.PlayButton(t.Stage, "Spin Two", new Vector2(60f, -80f), new Vector2(110f, 32f), p2, "Spin");
            UI.PlayButton(t.Stage, "Wobble Two", new Vector2(180f, -80f), new Vector2(110f, 32f), p2, "Wobble");

            t.Summary("Two players, one Spin Set. Each takes its own copy of the set at start-up.")
             .Watch("One, Two", one, two)
             .Needs(StationNeeds.PlayMode)
             .HowTo("In play mode: Spin One, then Spin Two straight after; Wobble One while Two spins. While playing, change Spin's Duration in Spin Set.asset to 5 and Spin One again; undo it afterwards.")
             .Expect("Each spins and wobbles on its own, never stopping or moving the other. The Duration change doesn't reach players already running (it shows next time you enter play mode).");
        }

        private static void DuplicateNames(LabBoard b)
        {
            var t = b.Test("A8", "Two animations with one name in a set", 620f, 140f);
            UI.Label(t.Stage, "Note", "Nothing to animate here: open the asset.", Vector2.zero, new Vector2(500f, 40f), 18f, Pal.BodyText);

            t.Summary("If two animations in one set share a name, the second can never play, and the set says so.")
             .Needs(StationNeeds.Inspector | StationNeeds.Console)
             .HowTo("Select Assets/UnitTests/Animation Sets/Spin Set.asset and rename Wobble to Spin. Then Undo.")
             .Expect("A warning appears in the Console: two animations are named 'Spin', and the second never runs. Undo removes the problem.");
        }
    }
}
