using DG.Tweening;
using rmf_claude.DOTweenUI;
using TMPro;
using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UIAnimLab.EditorTools.S;

namespace UIAnimLab.Showcase.Builders
{
    /// <summary>
    /// Showcase scenes for the pages about editor features: Previewing, Recording Values, Mirroring, Copy and
    /// Paste, Animation Sets, Scene Gizmos, Warnings and Common Issues. Most of their pictures are of the
    /// Editor rather than the Game view, so each frame is a setup to screenshot around.
    /// </summary>
    public static class EditorShowcase
    {
        private const string MenuSet = SC.Sets + "Menu Buttons.asset";

        [MenuItem(SC.Menu + "Previewing")]
        public static void Previewing()
        {
            const string page = "Previewing";
            var b = SC.Begin("06 Previewing", page, "Edit-mode previews. These are recorded with the whole Editor in shot, not the Game view.");

            var t = SC.Shot(b, page, "Play, Reset, Stop and Restore",
                "A drawer that opens and closes, for a recording of the Editor: press Play on Open, then Play on Close, then Stop and Restore.",
                "A GIF of the Editor (Scene view and Inspector together, out of play mode) for the top of the page: select Drawer, press Open's Play, then Close's, then Stop and Restore.");
            var drawer = Drawer(t.Stage);
            var player = t.PlayerOn(drawer.gameObject,
                Anim.New("Open", Pos(null).ToBase(V3(360f, 0f)).Dur(0.45f).Ease(Ease.OutCubic)),
                Anim.New("Close", Pos(null).ToRest().Dur(0.45f).Ease(Ease.OutCubic)));
            t.Needs(StationNeeds.Preview | StationNeeds.Inspector);
            t.Watch("Drawer (its player)", drawer);

            var t2 = SC.Shot(b, page, "Previews carry on from each other",
                "Open, then Close mid-way: the preview finishes Open first, so Close always starts from fully open.",
                "A GIF of the Editor for 'Previews carry on from each other': press Open's Play and, half way, Close's Play.");
            var drawer2 = Drawer(t2.Stage);
            t2.PlayerOn(drawer2.gameObject,
                Anim.New("Open", Pos(null).ToBase(V3(360f, 0f)).Dur(1.2f).Ease(Ease.Linear)),
                Anim.New("Close", Pos(null).ToRest().Dur(0.45f).Ease(Ease.OutCubic)));
            t2.Needs(StationNeeds.Preview | StationNeeds.Inspector);
            t2.Watch("Drawer (its player)", drawer2);

            b.Save();
        }

        private static RectTransform Drawer(Transform stage)
        {
            var drawer = SC.CardAt(stage, "Drawer", new Vector2(-200f, 0f), new Vector2(260f, 360f), SC.Card);
            SC.Title(drawer, "Inventory", new Vector2(0f, 130f), 30f, SC.Ink, 240f);
            for (int i = 0; i < 6; i++)
            {
                var slot = UI.Box(drawer, "Slot " + (i + 1), new Vector2(-60f + (i % 2) * 120f, 50f - (i / 2) * 100f), new Vector2(96f, 84f), Pal.Hex("E8EAF4"));
                UI.Icon(slot.transform, "Item", i % 3 == 0 ? LabAssets.Heart : i % 3 == 1 ? LabAssets.Diamond : LabAssets.Star, Vector2.zero, 50f, i % 3 == 0 ? SC.Red : i % 3 == 1 ? SC.Sky : SC.Amber);
            }
            return drawer;
        }

        [MenuItem(SC.Menu + "Recording Values")]
        public static void Recording()
        {
            const string page = "Recording Values";
            var b = SC.Begin("07 Recording Values", page, "Posing objects and recording them into From and To rows, from an animation's pop-out window.");

            var t = SC.Shot(b, page, "Recording a pose",
                "A card whose Deal animation flies it to the table. Its To is recorded by dragging the card in the Scene view.",
                "A GIF of the Editor for 'Recording a value': select Card, Pop Out Deal, press record on step 0's To, drag the card with the Move tool, press Finish, then Play in the window.",
                "Dock the pop-out window beside the Scene view so both are in shot.");
            var card = SC.CardAt(t.Stage, "Card", new Vector2(-300f, 120f), new Vector2(120f, 170f), SC.Card);
            UI.Icon(card, "Suit", LabAssets.Heart, Vector2.zero, 60f, SC.Red);
            t.PlayerOn(card.gameObject, Anim.New("Deal",
                Pos(null).To(V3(100f, -60f)).Dur(0.6f).Ease(Ease.OutCubic),
                Rot(null).With().To(V3(0f, 0f, -8f)).Dur(0.6f).Ease(Ease.OutCubic)));
            t.Needs(StationNeeds.PopOut | StationNeeds.SceneView);
            t.Watch("Card (its player)", card);

            var t2 = SC.Shot(b, page, "Several values at once",
                "A badge to move, scale and fade in one recording: three rows in the Recording box, one Finish.",
                "A screenshot of the pop-out window with the Recording box showing three rows, for 'Several values at once'.");
            var badge = SC.CardAt(t2.Stage, "Badge", Vector2.zero, new Vector2(200f, 200f), SC.Lilac);
            UI.Group(badge);
            UI.Icon(badge, "Star", LabAssets.Star, Vector2.zero, 110f, SC.Amber);
            t2.PlayerOn(badge.gameObject, Anim.New("Award",
                Pos(null).ToBase(V3(0f, 0f)).Dur(0.4f).Ease(Ease.OutBack),
                S.Scale(null).With().To(V3(1f, 1f, 1f)).Dur(0.4f).Ease(Ease.OutBack),
                Alpha(null).With().To(1f).Dur(0.3f)));
            t2.Needs(StationNeeds.PopOut | StationNeeds.SceneView);
            t2.Watch("Badge (its player)", badge);

            b.Save();
        }

        [MenuItem(SC.Menu + "Mirroring")]
        public static void Mirroring()
        {
            const string page = "Mirroring";
            var b = SC.Begin("17 Mirroring", page, "Show and its mirror, Hide, made with Duplicate as Mirrored. Also covers Mirroring Rules.");

            var t = SC.Shot(b, page, "Show, then its mirror",
                "A menu whose Show slides three buttons in, staggered, after fading the panel in. Hide is Show mirrored: the last button in is the first out, then the panel fades.",
                "A GIF of the Game view for the top of the page, and a before/after screenshot of Show and Show Mirrored in the Inspector for Mirroring Rules.");
            var menu = SC.CardAt(t.Stage, "Menu", Vector2.zero, new Vector2(360f, 420f), SC.Panel);
            UI.Group(menu);
            SC.Title(menu, "Paused", new Vector2(0f, 150f), 40f, SC.Text, 320f);
            string[] names = { "Resume", "Options", "Quit" };
            Color[] colors = { SC.Mint, SC.Sky, SC.Pink };
            var steps = new System.Collections.Generic.List<UIAnimationStep>
            {
                Alpha(null).From(0f).To(1f).Dur(0.2f),
            };
            for (int i = 0; i < 3; i++)
            {
                var button = SC.Pill(menu, names[i], names[i], new Vector2(0f, 50f - i * 95f), new Vector2(260f, 70f), colors[i]);
                UI.Group(button);
                var move = Pos(names[i]).FromBase(V3(-320f, 0f)).ToRest().Dur(0.35f).Ease(Ease.OutCubic).Delay(i * 0.08f);
                if (i > 0) move.With();
                steps.Add(move);
                steps.Add(Alpha(names[i]).With().From(0f).To(1f).Dur(0.35f).Delay(i * 0.08f));
            }

            var show = Anim.New("Show", steps.ToArray());
            var hideSteps = new System.Collections.Generic.List<UIAnimationStep>();
            for (int i = 2; i >= 0; i--)
            {
                var move = Pos(names[i]).FromBase(Vector3.zero).ToBase(V3(-320f, 0f)).Dur(0.35f).Ease(Ease.OutCubic).Delay((2 - i) * 0.08f);
                if (i < 2) move.With();
                hideSteps.Add(move);
                hideSteps.Add(Alpha(names[i]).With().From(1f).To(0f).Dur(0.35f).Delay((2 - i) * 0.08f));
            }
            hideSteps.Add(Alpha(null).From(1f).To(0f).Dur(0.2f));
            var player = t.PlayerOn(menu.gameObject, show, Anim.New("Hide", hideSteps.ToArray()).Note("What right-click Show → Duplicate as Mirrored gives, renamed. Try it on Show to compare."));
            SC.Cues(t, (player, "Show", 0.3f), (player, "Hide", 1.6f)).LoopEvery = 2.8f;
            t.Watch("Menu (its player)", menu);

            var t2 = SC.Shot(b, page, "When it has to guess",
                "A Show with a step that has no From: mirroring it warns in the Console, naming the step.",
                "A screenshot of the Console warning after right-click Show → Duplicate as Mirrored, for the WARNING. Undo the duplicate afterwards.");
            var heart = UI.Icon(t2.Stage, "Heart", LabAssets.Heart, Vector2.zero, 180f, SC.Pink);
            t2.PlayerOn(heart.gameObject, Anim.New("Show",
                Tint(null).To(SC.Red).Dur(0.4f),
                S.Scale(null).With().From(V3(0.5f, 0.5f, 1f)).ToRest().Dur(0.4f).Ease(Ease.OutBack)));
            t2.Needs(StationNeeds.Inspector | StationNeeds.Console);
            t2.Watch("Heart (its player)", heart);

            b.Save();
        }

        [MenuItem(SC.Menu + "Copy and Paste")]
        public static void CopyPaste()
        {
            const string page = "Copy and Paste";
            var b = SC.Begin("18 Copy and Paste", page, "The right-click menus, and pasting between objects.");

            var t = SC.Shot(b, page, "The right-click menus",
                "Two buttons: Play has a Press animation (a click sound and a squish), Options has none yet.",
                "Screenshots of the right-click menu on Press's header, on a step header, and on the Animations list, for the table. Then copy Press onto Options and screenshot the pasted animation, sound clip intact.");
            var play = SC.Pill(t.Stage, "Play", "Play", new Vector2(-200f, 0f), new Vector2(260f, 90f), SC.Mint);
            var options = SC.Pill(t.Stage, "Options", "Options", new Vector2(200f, 0f), new Vector2(260f, 90f), SC.Sky);
            var p = t.PlayerOn(play.gameObject, Anim.New("Press", S.Sound(LabAssets.Click, 1f, 1f, 0.08f), PunchScale(null, V3(-0.1f, -0.1f, 0f), 0.25f).With()));
            t.PlayerOn(options.gameObject);
            SC.Cues(t, (p, "Press", 0.3f));
            t.Needs(StationNeeds.Inspector);
            t.Watch("Play, Options", play, options);

            b.Save();
        }

        [MenuItem(SC.Menu + "Animation Sets")]
        public static void AnimationSets()
        {
            const string page = "Animation Sets";
            var set = Anim.Asset(MenuSet,
                Anim.New("Hover", S.Scale("..").ToBase(V3(0.06f, 0.06f, 0f)).Dur(0.15f)).InterruptStop(),
                Anim.New("Unhover", S.Scale("..").ToRest().Dur(0.15f)).InterruptStop(),
                Anim.New("Press", PunchRot("../Icon", V3(0f, 0f, 30f), 0.4f), PunchScale("..", V3(-0.08f, -0.08f, 0f), 0.3f).With()));

            var b = SC.Begin("19 Animation Sets", page, "One set of animations used by many players: Menu Buttons.asset in Assets/Showcase/Animation Sets.");

            var t = SC.Shot(b, page, "One set, many buttons",
                "Four menu buttons, each with a player on its Animations child using Menu Buttons and nothing of its own. Space presses each in turn; in play mode hover them too.",
                "A GIF of the Game view for the top of the page, and a screenshot of one button's player showing 'Shared Animations (Menu Buttons)' for 'Shared Animations in the player's Inspector'.");
            string[] names = { "New Game", "Continue", "Options", "Quit" };
            Color[] colors = { SC.Mint, SC.Sky, SC.Lilac, SC.Pink };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            GameObject firstAnimations = null;
            for (int i = 0; i < 4; i++)
            {
                var pill = SC.Pill(t.Stage, names[i], names[i], new Vector2(0f, 165f - i * 110f), new Vector2(340f, 84f), colors[i]);
                ((RectTransform)pill.Find("Label")).anchoredPosition = new Vector2(25f, 0f);
                UI.Icon(pill, "Icon", LabAssets.Star, new Vector2(-120f, 0f), 44f, SC.Amber);
                var animations = UI.Rect(pill, "Animations", Vector2.zero, Vector2.zero);
                var player = t.PlayerOn(animations.gameObject);
                Anim.SetShared(player, set);
                var body = pill.Find("Body");
                UI.Trigger(body, UnityEngine.EventSystems.EventTriggerType.PointerEnter, player, "Hover");
                UI.Trigger(body, UnityEngine.EventSystems.EventTriggerType.PointerExit, player, "Unhover");
                UI.Trigger(body, UnityEngine.EventSystems.EventTriggerType.PointerClick, player, "Press");
                cues.Add((player, "Press", 0.3f + i * 0.3f));
                if (i == 0) firstAnimations = animations.gameObject;
            }
            SC.Cues(t, cues.ToArray()).LoopEvery = 2f;
            t.Watch("New Game's Animations", firstAnimations);

            var t2 = SC.Shot(b, page, "A player's own animation wins",
                "A Quit button using the same set but with its own Press (a red shake): 'overrides shared' on its own, 'overridden locally' under Shared Animations.",
                "A screenshot of the player's Inspector showing both grey notes, for 'A player's own animations win'.");
            var quit = SC.Pill(t2.Stage, "Quit", "Quit", new Vector2(0f, 0f), new Vector2(340f, 84f), SC.Red);
            UI.Icon(quit, "Icon", LabAssets.Cross, new Vector2(-120f, 0f), 40f, SC.Text);
            var quitAnimations = UI.Rect(quit, "Animations", Vector2.zero, Vector2.zero);
            var quitPlayer = t2.PlayerOn(quitAnimations.gameObject, Anim.New("Press", ShakePos("..", 12f, 0.4f)));
            Anim.SetShared(quitPlayer, set);
            SC.Cues(t2, (quitPlayer, "Press", 0.3f));
            t2.Watch("Quit's Animations", quitAnimations.gameObject);

            b.Save();
        }

        [MenuItem(SC.Menu + "Scene Gizmos")]
        public static void SceneGizmos()
        {
            const string page = "Scene Gizmos";
            var b = SC.Begin("20 Scene Gizmos", page, "Scene view drawings: these frames are set up for Scene view screenshots, with the frame's player selected.");

            var t = SC.Shot(b, page, "Movement and resizing",
                "A slide with a curved path, and a panel that grows: a movement line with arrows, and outlines fading in.",
                "A Scene view screenshot for the top of the page: select the frame and expand both animations. Frame the Scene view on the frame (select it, press F).");
            var ball = SC.Dot(t.Stage, "Ball", new Vector2(-330f, -150f), 50f, SC.Amber);
            var panel = UI.Box(t.Stage, "Panel", new Vector2(200f, 40f), new Vector2(120f, 80f), SC.Lilac);
            t.Player(
                Anim.New("Fly", Pos("Stage/Ball").To(V3(-60f, 150f)).Dur(0.8f).Path(UIAnimationPathShape.Curved, V3(-300f, 100f))),
                Anim.New("Grow", Size("Stage/Panel").To(V3(300f, 260f)).Dur(0.6f).Ease(Ease.OutBack)));
            t.Needs(StationNeeds.SceneView | StationNeeds.Inspector);

            var t2 = SC.Shot(b, page, "What the lines mean",
                "Three moves ending in each mode: solid Absolute, dashed Baseline (with its faint ghost cross at rest), dotted Current.",
                "A Scene view screenshot for 'What the lines mean': select the frame, expand all three animations.");
            string[] modes = { "Absolute", "Baseline", "Current" };
            for (int i = 0; i < 3; i++)
            {
                float y = 150f - i * 150f;
                SC.Caption(t2.Stage, modes[i], new Vector2(-370f, y), 160f);
                SC.Dot(t2.Stage, modes[i], new Vector2(-260f, y), 40f, i == 0 ? SC.Sky : i == 1 ? SC.Mint : SC.Amber);
            }
            t2.Player(
                Anim.New("Absolute", Pos("Stage/Absolute").To(V3(300f, 150f)).Dur(0.5f)),
                Anim.New("Baseline", Pos("Stage/Baseline").To(V3(-150f, -40f)).Dur(0.4f), Pos("Stage/Baseline").ToBase(V3(500f, 0f)).Dur(0.5f)),
                Anim.New("Current", Pos("Stage/Current").ToCur(V3(560f, 0f)).Dur(0.5f)));
            t2.Needs(StationNeeds.SceneView | StationNeeds.Inspector);

            var t3 = SC.Shot(b, page, "Dragging the start and end",
                "A move with a From (ring) and a To (dot), and a resizing step with corner and edge handles.",
                "Scene view screenshots for 'Dragging the start and end': hover a handle so its white ring and name show, and hold Shift while dragging a corner.");
            SC.Dot(t3.Stage, "Ball", new Vector2(-250f, 0f), 50f, SC.Pink);
            UI.Square(t3.Stage, "Box", new Vector2(250f, 0f), new Vector2(100f, 100f), SC.Mint);
            t3.Player(
                Anim.New("Slide", Pos("Stage/Ball").FromBase(V3(-100f, -120f)).ToBase(V3(150f, 120f)).Dur(0.5f)),
                Anim.New("Grow", Size("Stage/Box").FromBase(V3(-40f, -40f)).ToBase(V3(80f, 60f)).Dur(0.5f)));
            t3.Needs(StationNeeds.SceneView | StationNeeds.Inspector);

            var t4 = SC.Shot(b, page, "The selected step",
                "Five scale steps whose outlines overlap; click one step in the Inspector and it's drawn on top with a dark halo.",
                "A Scene view screenshot with the Inspector beside it, for 'The selected step': click step 3's header, so its blue bar shows in the Inspector and its halo in the Scene view.");
            Color[] colors = { SC.Sky, SC.Mint, SC.Amber, SC.Pink, SC.Lilac };
            var crowd = new System.Collections.Generic.List<UIAnimationStep>();
            for (int i = 0; i < 5; i++)
            {
                UI.Square(t4.Stage, "Member " + (i + 1), new Vector2(-240f + i * 120f, 0f), new Vector2(80f, 80f), colors[i]);
                var s = S.Scale("Stage/Member " + (i + 1)).To(V3(1.8f, 1.8f, 1f)).Dur(0.4f);
                if (i > 0) s.With();
                crowd.Add(s);
            }
            t4.Player(Anim.New("Crowd", crowd.ToArray()));
            t4.Needs(StationNeeds.SceneView | StationNeeds.Inspector);

            b.Save();
        }

        [MenuItem(SC.Menu + "Warnings and Common Issues")]
        public static void Warnings()
        {
            const string page = "Warnings and Common Issues";
            var b = SC.Begin("21 Warnings and Common Issues", page, "A broken step for the Inspector warning screenshot, and two common problems in motion.");

            var t = SC.Shot(b, page, "An Inspector warning",
                "A step aimed at 'Box', which has no Canvas Group, and one with a mistyped path: both show a warning sign, a faded header and a reason.",
                "A screenshot of the Inspector with Fade's steps open, for 'Inspector warnings'.");
            UI.Box(t.Stage, "Box", new Vector2(0f, 0f), new Vector2(200f, 200f), SC.Lilac);
            t.Player(Anim.New("Fade",
                Alpha("Stage/Box").To(0f),
                S.Scale("Stage/Iconn").To(V3(1.2f, 1.2f, 1f))));
            t.Needs(StationNeeds.Inspector);

            var t2 = SC.Shot(b, page, "Animating inside a Layout Group",
                "Three list rows in a Vertical Layout Group. Sliding a row itself is undone by the layout; sliding a child inside each row works.",
                "A GIF of the Game view for 'Animating inside a Layout Group': Space slides the inner children in, staggered.");
            var list = UI.Rect(t2.Stage, "List", Vector2.zero, new Vector2(420f, 330f));
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            var cascade = new System.Collections.Generic.List<UIAnimationStep>();
            for (int i = 0; i < 3; i++)
            {
                var row = UI.Rect(list, "Row " + (i + 1), Vector2.zero, new Vector2(420f, 96f));
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 96f;
                var inner = SC.CardAt(row, "Inner", Vector2.zero, new Vector2(420f, 96f), SC.Card, false);
                inner.anchorMin = Vector2.zero;
                inner.anchorMax = Vector2.one;
                inner.sizeDelta = Vector2.zero;
                inner.Find("Body").GetComponent<RectTransform>().anchorMin = Vector2.zero;
                inner.Find("Body").GetComponent<RectTransform>().anchorMax = Vector2.one;
                inner.Find("Body").GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                UI.Label(inner, "Text", "Quest " + (i + 1), new Vector2(-40f, 0f), new Vector2(300f, 50f), 28f, SC.Ink, TextAlignmentOptions.Left);
                UI.Group(inner);
                var move = Pos("Stage/List/Row " + (i + 1) + "/Inner").FromBase(V3(-500f, 0f)).ToRest().Dur(0.4f).Ease(Ease.OutCubic).Delay(i * 0.1f);
                if (i > 0) move.With();
                cascade.Add(move);
                cascade.Add(Alpha("Stage/List/Row " + (i + 1) + "/Inner").With().From(0f).To(1f).Dur(0.4f).Delay(i * 0.1f));
            }
            var p2 = t2.Player(Anim.New("Cascade In", cascade.ToArray()));
            SC.Cues(t2, (p2, "Cascade In", 0.3f)).LoopEvery = 2f;

            b.Save();
        }
    }
}
