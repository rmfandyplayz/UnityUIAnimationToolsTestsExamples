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
    /// <summary>Showcase scenes for the Basic usage pages: Your First Animation, Steps, From and To, Targets, Easing.</summary>
    public static class BasicsShowcase
    {
        [MenuItem(SC.Menu + "Your First Animation")]
        public static void YourFirstAnimation()
        {
            const string page = "Your First Animation";
            var b = SC.Begin("01 Your First Animation", page, "The page builds a settings panel that fades in while popping up, plays it from a button, and mirrors it into a Hide.");

            // 1. The finished result.
            var t = SC.Shot(b, page, "Show and Hide",
                "The panel the page builds: Show fades it in while it pops up from 80%, Hide is its mirror.",
                "A GIF of the Game view for the top of the page: Space plays Show, then Hide. L loops it.");
            var panel = SettingsPanel(t.Stage, "Settings Panel", Vector2.zero);
            var player = t.PlayerOn(panel.gameObject, ShowAnimation(), HideAnimation());
            var open = SC.Pill(t.Stage, "Open Button", "Open Settings", new Vector2(-330f, -220f), new Vector2(220f, 52f), SC.Amber);
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(UI.Add<Button>(open).onClick, player.PlayAnimation, "Show");
            SC.Cues(t, (player, "Show", 0.3f), (player, "Hide", 1.8f)).LoopEvery = 2.8f;
            t.Watch("Settings Panel (its player)", panel);

            // 2. The Inspector.
            var t2 = SC.Shot(b, page, "The finished Inspector",
                "The same panel, for a screenshot of its UI Animation Player with Show open and both steps folded closed.",
                "A screenshot of the Inspector, for 'What you've built': select Settings Panel, open Show, fold both steps closed so the headers read like a timeline.");
            var panel2 = SettingsPanel(t2.Stage, "Settings Panel", Vector2.zero);
            t2.PlayerOn(panel2.gameObject, ShowAnimation(), HideAnimation());
            t2.Watch("Settings Panel (its player)", panel2);

            // 3. Playing from a button.
            var t3 = SC.Shot(b, page, "Play it from a button",
                "A button whose On Click plays Show, with no code: On Click → UIAnimationPlayer → PlayAnimation (string), 'Show'.",
                "A screenshot of the Open Button's On Click in the Inspector, for step 7. Optionally a GIF of clicking it in play mode.");
            var panel3 = SettingsPanel(t3.Stage, "Settings Panel", new Vector2(120f, 0f));
            var player3 = t3.PlayerOn(panel3.gameObject, ShowAnimation(), HideAnimation());
            var button3 = SC.Pill(t3.Stage, "Open Button", "Open Settings", new Vector2(-300f, 0f), new Vector2(220f, 56f), SC.Amber);
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(UI.Add<Button>(button3).onClick, player3.PlayAnimation, "Show");
            panel3.GetComponent<CanvasGroup>().alpha = 1f;
            t3.Watch("Open Button, Settings Panel", button3, panel3);

            // 4. Duplicate as Mirrored.
            var t4 = SC.Shot(b, page, "Duplicate as Mirrored",
                "A panel with only Show, ready for the right-click menu.",
                "A screenshot of the right-click menu on Show's header with Duplicate as Mirrored highlighted, for step 8. Undo the duplicate afterwards.");
            var panel4 = SettingsPanel(t4.Stage, "Settings Panel", Vector2.zero);
            t4.PlayerOn(panel4.gameObject, ShowAnimation());
            t4.Watch("Settings Panel (its player)", panel4);

            b.Save();
        }

        public static UIAnimation ShowAnimation()
        {
            return Anim.New("Show",
                Alpha(null).From(0f).To(1f).Dur(0.25f).Ease(Ease.OutQuad),
                S.Scale(null).With().From(V3(0.8f, 0.8f, 0.8f)).ToRest().Dur(0.25f).Ease(Ease.OutBack));
        }

        public static UIAnimation HideAnimation()
        {
            return Anim.New("Hide",
                S.Scale(null).FromBase(Vector3.zero).To(V3(0.8f, 0.8f, 0.8f)).Dur(0.25f).Ease(Ease.OutBack),
                Alpha(null).With().From(1f).To(0f).Dur(0.25f).Ease(Ease.OutQuad));
        }

        /// <summary>A settings card: title, three rows with toggles, a close button. Has a Canvas Group.</summary>
        public static RectTransform SettingsPanel(Transform parent, string name, Vector2 position)
        {
            var card = SC.CardAt(parent, name, position, new Vector2(400f, 300f));
            UI.Group(card);
            SC.Title(card, "Settings", new Vector2(0f, 105f), 32f, SC.Ink, 360f);
            string[] rows = { "Music", "Sound effects", "Screen shake" };
            for (int i = 0; i < rows.Length; i++)
            {
                float y = 40f - i * 58f;
                UI.Label(card, rows[i], rows[i], new Vector2(-50f, y), new Vector2(240f, 40f), 22f, SC.Ink, TextAlignmentOptions.Left);
                var track = UI.Box(card, rows[i] + " Toggle", new Vector2(130f, y), new Vector2(64f, 32f), i == 2 ? Pal.Hex("D5D9E8") : SC.Mint);
                track.pixelsPerUnitMultiplier = 2f;
                SC.Dot(track.transform, "Knob", new Vector2(i == 2 ? -16f : 16f, 0f), 24f, Color.white);
            }
            return card;
        }

        [MenuItem(SC.Menu + "Steps")]
        public static void Steps()
        {
            const string page = "Steps";
            var b = SC.Begin("02 Steps", page, "Steps, their timing, and the animation settings below them.");

            // 1. A timeline of five steps, as in the page's Timing section.
            var t = SC.Shot(b, page, "Reading the list like a timeline",
                "The five-step example from Timing: fade and grow the panel together, then slide the title in with its fade, then the button a moment later.",
                "A GIF of the Game view for Timing, and a screenshot of the player's Inspector with the five step headers folded, matching the page's example.");
            var card = SC.CardAt(t.Stage, "Panel", Vector2.zero, new Vector2(460f, 300f));
            UI.Group(card);
            var title = SC.Title(card, "Level Complete", new Vector2(0f, 80f), 36f, SC.Ink, 420f);
            UI.Group(title);
            var stars = UI.Rect(card, "Stars", new Vector2(0f, 10f), new Vector2(300f, 60f));
            for (int i = 0; i < 3; i++) UI.Icon(stars, "Star " + (i + 1), LabAssets.Star, new Vector2(-70f + i * 70f, 0f), 56f, SC.Amber);
            var next = SC.Pill(card, "Button", "Next level", new Vector2(0f, -95f), new Vector2(220f, 56f), SC.Pink);
            UI.Group(next);
            var player = t.PlayerOn(card.gameObject, Anim.New("Show",
                Alpha(null).From(0f).To(1f).Dur(0.25f),
                S.Scale(null).With().From(V3(0.85f, 0.85f, 1f)).ToRest().Dur(0.25f).Ease(Ease.OutBack),
                Pos("Title Level Complete").FromBase(V3(0f, 40f)).ToRest().Dur(0.3f).Ease(Ease.OutCubic),
                Alpha("Title Level Complete").With().From(0f).To(1f).Dur(0.3f),
                Alpha("Button").With().From(0f).To(1f).Dur(0.3f).Delay(0.1f)));
            SC.Cues(t, (player, "Show", 0.3f)).LoopEvery = 2f;
            t.Watch("Panel (its player)", card);

            // 2. A gallery of step types.
            var t2 = SC.Shot(b, page, "What a step can do",
                "Eight tiles, each looping one kind of step: move, turn, scale, resize, fade, tint, a shader value, and switching on and off.",
                "A looping GIF of the Game view for the top of 'Step types'. It loops by itself in play mode.");
            Gallery(t2);

            // 3. Loops.
            var t3 = SC.Shot(b, page, "Loops and Loop Type",
                "The same move with Loops 3 and each Loop Type: Restart jumps back, Yoyo swings back, Incremental keeps going.",
                "A GIF of the Game view for the Loops row of 'Animation settings'.");
            var lanes = new (string name, LoopType type, float y)[] { ("Restart", LoopType.Restart, 120f), ("Yoyo", LoopType.Yoyo, 0f), ("Incremental", LoopType.Incremental, -120f) };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            foreach (var (name, type, y) in lanes)
            {
                SC.Caption(t3.Stage, name, new Vector2(-370f, y), 180f);
                UI.Box(t3.Stage, name + " Lane", new Vector2(70f, y), new Vector2(660f, 6f), Pal.Hex("39406E"));
                var dot = SC.Dot(t3.Stage, name, new Vector2(-240f, y), 44f, name == "Restart" ? SC.Sky : name == "Yoyo" ? SC.Pink : SC.Amber);
                float move = type == LoopType.Incremental ? 160f : 480f;
                var p = t3.PlayerOn(dot.gameObject, Anim.New("Go", Pos(null).ToCur(V3(move, 0f)).Dur(0.6f).Ease(Ease.InOutSine)).Loop(3, type));
                cues.Add((p, "Go", 0.3f));
            }
            SC.Cues(t3, cues.ToArray()).LoopEvery = 2.8f;

            b.Save();
        }

        private static void Gallery(Tile t)
        {
            var director = SC.Cues(t);
            director.LoopFromStart = true;
            director.LoopEvery = 2.4f;

            string[] labels = { "Anchored Position", "Rotation", "Scale", "Size Delta", "Canvas Group Alpha", "Graphic Color", "Material Float", "Set Active" };
            for (int i = 0; i < 8; i++)
            {
                float x = -345f + (i % 4) * 230f;
                float y = i < 4 ? 120f : -130f;
                var cell = SC.CardAt(t.Stage, "Cell " + (i + 1), new Vector2(x, y), new Vector2(210f, 210f), SC.Panel, false);
                SC.Caption(cell, labels[i], new Vector2(0f, -85f), 200f).fontSize = 17f;

                UIAnimation animation;
                Graphic subject;
                switch (i)
                {
                    case 0:
                        subject = SC.Dot(cell, "Subject", new Vector2(-50f, 10f), 44f, SC.Sky);
                        animation = Anim.New("Go", Pos("Subject").ToBase(V3(100f, 0f)).Dur(0.6f).Ease(Ease.InOutSine), Pos("Subject").ToRest().Dur(0.6f).Delay(0.3f).Ease(Ease.InOutSine));
                        break;
                    case 1:
                        subject = UI.Box(cell, "Subject", new Vector2(0f, 10f), new Vector2(70f, 70f), SC.Pink);
                        animation = Anim.New("Go", Rot("Subject").ToCur(V3(0f, 0f, -180f)).Dur(1.2f).Ease(Ease.InOutCubic));
                        break;
                    case 2:
                        subject = UI.Icon(cell, "Subject", LabAssets.Heart, new Vector2(0f, 10f), 80f, SC.Red);
                        animation = Anim.New("Go", S.Scale("Subject").To(V3(1.4f, 1.4f, 1f)).Dur(0.4f).Ease(Ease.OutBack), S.Scale("Subject").ToRest().Dur(0.5f).Delay(0.4f));
                        break;
                    case 3:
                        subject = UI.Box(cell, "Subject", new Vector2(-70f, 10f), new Vector2(30f, 40f), SC.Mint);
                        subject.rectTransform.pivot = new Vector2(0f, 0.5f);
                        animation = Anim.New("Go", Size("Subject").To(V3(140f, 40f)).Dur(0.6f).Ease(Ease.OutCubic), Size("Subject").ToRest().Dur(0.5f).Delay(0.4f));
                        break;
                    case 4:
                        subject = UI.Box(cell, "Subject", new Vector2(0f, 10f), new Vector2(110f, 80f), SC.Lilac);
                        UI.Icon(subject.transform, "Star", LabAssets.Star, Vector2.zero, 50f, SC.Amber);
                        UI.Group(subject);
                        animation = Anim.New("Go", Alpha("Subject").To(0f).Dur(0.5f), Alpha("Subject").ToRest().Dur(0.5f).Delay(0.4f));
                        break;
                    case 5:
                        subject = UI.Icon(cell, "Subject", LabAssets.Diamond, new Vector2(0f, 10f), 80f, SC.Sky);
                        animation = Anim.New("Go", Tint("Subject").To(SC.Pink).Dur(0.6f), Tint("Subject").To(SC.Amber).Dur(0.6f), Tint("Subject").ToRest().Dur(0.6f));
                        break;
                    case 6:
                        subject = UI.Square(cell, "Subject", new Vector2(0f, 10f), new Vector2(150f, 40f), SC.Mint);
                        subject.material = LabAssets.FillMaterial;
                        subject.gameObject.AddComponent<UIMaterialInstance>();
                        animation = Anim.New("Go", MatFloat("Subject", "_Progress").From(0f).To(1f).Dur(1.2f).Ease(Ease.InOutQuad));
                        break;
                    default:
                        subject = UI.Icon(cell, "Subject", LabAssets.Star, new Vector2(0f, 10f), 80f, SC.Amber);
                        animation = Anim.New("Go", Active("Subject", false).Delay(0.5f), Active("Subject", true).Delay(0.6f));
                        break;
                }

                var player = t.PlayerOn(cell.gameObject, animation);
                director.Cues.Add(new ShotDirector.Cue { Player = player, Animation = "Go", At = 0.1f });
            }
        }

        [MenuItem(SC.Menu + "From and To")]
        public static void FromAndTo()
        {
            const string page = "From and To";
            var b = SC.Begin("03 From and To", page, "The FROM / TO button, the three modes, and Apply From Values Immediately.");

            // 1. TO carries on, FROM starts over.
            var t = SC.Shot(b, page, "TO vs FROM",
                "Two hearts, each grown then shrunk quickly. TO (left) carries on smoothly from wherever it is; FROM (right) jumps to its From first, every time.",
                "A GIF of the Game view for 'The FROM / TO button'.");
            var toHeart = UI.Icon(t.Stage, "TO", LabAssets.Heart, new Vector2(-200f, 20f), 150f, SC.Pink);
            var fromHeart = UI.Icon(t.Stage, "FROM", LabAssets.Heart, new Vector2(200f, 20f), 150f, SC.Red);
            SC.Caption(t.Stage, "TO", new Vector2(-200f, -120f));
            SC.Caption(t.Stage, "FROM 0.5", new Vector2(200f, -120f));
            var pTo = t.PlayerOn(toHeart.gameObject,
                Anim.New("Grow", S.Scale(null).To(V3(1.35f, 1.35f, 1f)).Dur(0.6f)).InterruptStop(),
                Anim.New("Shrink", S.Scale(null).ToRest().Dur(0.6f)).InterruptStop());
            var pFrom = t.PlayerOn(fromHeart.gameObject,
                Anim.New("Grow", S.Scale(null).From(V3(0.5f, 0.5f, 1f)).To(V3(1.35f, 1.35f, 1f)).Dur(0.6f)).InterruptStop(),
                Anim.New("Shrink", S.Scale(null).From(V3(1.35f, 1.35f, 1f)).ToRest().Dur(0.6f)).InterruptStop());
            SC.Cues(t, (pTo, "Grow", 0.2f), (pFrom, "Grow", 0.2f), (pTo, "Shrink", 0.5f), (pFrom, "Shrink", 0.5f), (pTo, "Grow", 0.9f), (pFrom, "Grow", 0.9f), (pTo, "Shrink", 1.7f), (pFrom, "Shrink", 1.7f))
                .LoopEvery = 3f;

            // 2. The three modes.
            var t2 = SC.Shot(b, page, "Absolute, Baseline, Current",
                "Each ball is nudged three times. Absolute and Baseline land on the same spot every time; Current goes 120 further each time.",
                "A GIF of the Game view for 'The three modes'.");
            var modes = new (string name, UIAnimationEndpointMode mode, float y, Color color)[]
            {
                ("Absolute", UIAnimationEndpointMode.Absolute, 130f, SC.Sky),
                ("Baseline", UIAnimationEndpointMode.Baseline, 0f, SC.Mint),
                ("Current", UIAnimationEndpointMode.Current, -130f, SC.Amber),
            };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            foreach (var (name, mode, y, color) in modes)
            {
                SC.Caption(t2.Stage, name, new Vector2(-380f, y), 160f);
                UI.Box(t2.Stage, name + " Lane", new Vector2(60f, y), new Vector2(700f, 6f), Pal.Hex("39406E"));
                var ball = SC.Dot(t2.Stage, name, new Vector2(-260f, y), 44f, color);
                var step = Pos(null).Dur(0.4f).Ease(Ease.OutCubic);
                if (mode == UIAnimationEndpointMode.Absolute) step.To(V3(-140f, y));
                else if (mode == UIAnimationEndpointMode.Baseline) step.ToBase(V3(120f, 0f));
                else step.ToCur(V3(120f, 0f));
                var p = t2.PlayerOn(ball.gameObject, Anim.New("Nudge", step), Anim.New("Home", Pos(null).ToRest().Dur(0.4f)));
                cues.Add((p, "Nudge", 0.3f));
                cues.Add((p, "Nudge", 0.9f));
                cues.Add((p, "Nudge", 1.5f));
                cues.Add((p, "Home", 2.4f));
            }
            SC.Cues(t2, cues.ToArray()).LoopEvery = 3.2f;

            // 3. Apply From Values Immediately.
            var t3 = SC.Shot(b, page, "Apply From Values Immediately",
                "Two titles fade in from invisible after a 0.5 s delay. With the setting on (left) the title hides at once; off (right) it sits visible, then flashes.",
                "A GIF of the Game view for 'Apply From Values Immediately'. Play mode only: the preview always applies From at once.");
            var on = SC.Title(t3.Stage, "Welcome!", new Vector2(-220f, 20f), 44f, SC.Text, 380f);
            on.name = "On";
            var off = SC.Title(t3.Stage, "Welcome!", new Vector2(220f, 20f), 44f, SC.Text, 380f);
            off.name = "Off";
            UI.Group(on);
            UI.Group(off);
            SC.Caption(t3.Stage, "on (the default)", new Vector2(-220f, -60f));
            SC.Caption(t3.Stage, "off", new Vector2(220f, -60f));
            var pOn = t3.PlayerOn(on.gameObject, Anim.New("Fade In", Alpha(null).From(0f).To(1f).Dur(0.5f).Delay(0.5f)));
            var pOff = t3.PlayerOn(off.gameObject, Anim.New("Fade In", Alpha(null).From(0f).To(1f).Dur(0.5f).Delay(0.5f)).NoFromImmediately());
            SC.Cues(t3, (pOn, "Fade In", 0.6f), (pOff, "Fade In", 0.6f)).LoopEvery = 2.4f;

            b.Save();
        }

        [MenuItem(SC.Menu + "Targets")]
        public static void Targets()
        {
            const string page = "Targets";
            var b = SC.Begin("04 Targets", page, "Target slots and Target Paths, and the Animations-child layout.");

            var t = SC.Shot(b, page, "One player, the button above it",
                "A button with its player on an Animations child: '..' squishes the button, '../Icon' twists the icon, '../Label' flashes the label.",
                "A GIF of the Game view of the press (Space), and a screenshot of the Hierarchy showing Button > Icon, Label, Animations, for the TIP.");
            var button = SC.Pill(t.Stage, "Button", "Play", new Vector2(0f, 20f), new Vector2(320f, 96f), SC.Sky);
            ((RectTransform)button.Find("Label")).anchoredPosition = new Vector2(30f, 0f);
            UI.Icon(button, "Icon", LabAssets.Star, new Vector2(-100f, 0f), 56f, SC.Amber);
            var animations = UI.Rect(button, "Animations", Vector2.zero, Vector2.zero);
            var player = t.PlayerOn(animations.gameObject, Anim.New("Press",
                PunchScale("..", V3(-0.12f, -0.12f, 0f), 0.35f),
                PunchRot("../Icon", V3(0f, 0f, 35f), 0.5f).With(),
                Tint("../Label").With().From(SC.Red).ToRest().Dur(0.5f)));
            SC.Cues(t, (player, "Press", 0.3f)).LoopEvery = 1.4f;
            t.Watch("Animations", animations.gameObject);

            var t2 = SC.Shot(b, page, "The same animation on three buttons",
                "Three buttons with an identical player each, all paths and no dragged-in slots, so each drives its own icon.",
                "A GIF of the Game view for 'Why bother with paths'.");
            string[] names = { "Start", "Options", "Quit" };
            Color[] colors = { SC.Mint, SC.Lilac, SC.Pink };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            for (int i = 0; i < 3; i++)
            {
                var pill = SC.Pill(t2.Stage, names[i], names[i], new Vector2(0f, 120f - i * 120f), new Vector2(300f, 84f), colors[i]);
                ((RectTransform)pill.Find("Label")).anchoredPosition = new Vector2(25f, 0f);
                UI.Icon(pill, "Icon", LabAssets.Star, new Vector2(-95f, 0f), 46f, SC.Amber);
                var anims = UI.Rect(pill, "Animations", Vector2.zero, Vector2.zero);
                var p = t2.PlayerOn(anims.gameObject, Anim.New("Press",
                    PunchScale("..", V3(-0.1f, -0.1f, 0f), 0.35f),
                    PunchRot("../Icon", V3(0f, 0f, 35f), 0.5f).With()));
                cues.Add((p, "Press", 0.3f + i * 0.35f));
            }
            SC.Cues(t2, cues.ToArray()).LoopEvery = 2f;

            b.Save();
        }

        [MenuItem(SC.Menu + "Easing")]
        public static void Easing()
        {
            const string page = "Easing";
            var b = SC.Begin("05 Easing", page, "Ease presets side by side, and a custom curve.");

            var t = SC.Shot(b, page, "Ease presets",
                "Seven balls make the same 1.2 s move, each with a different ease.",
                "A GIF of the Game view for the top of the page (or 'Presets').");
            Ease[] eases = { Ease.Linear, Ease.OutQuad, Ease.InQuad, Ease.InOutQuad, Ease.OutBack, Ease.OutBounce, Ease.OutElastic };
            string[] labels = { "Linear", "Out Quad", "In Quad", "In Out Quad", "Out Back", "Out Bounce", "Out Elastic" };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            for (int i = 0; i < eases.Length; i++)
            {
                float y = 195f - i * 65f;
                SC.Caption(t.Stage, labels[i], new Vector2(-360f, y), 180f).alignment = TextAlignmentOptions.Right;
                UI.Box(t.Stage, labels[i] + " Lane", new Vector2(80f, y), new Vector2(620f, 4f), Pal.Hex("39406E"));
                var dot = SC.Dot(t.Stage, labels[i], new Vector2(-210f, y), 36f, i % 2 == 0 ? SC.Sky : SC.Pink);
                var p = t.PlayerOn(dot.gameObject, Anim.New("Go", Pos(null).FromBase(Vector3.zero).ToBase(V3(560f, 0f)).Dur(1.2f).Ease(eases[i])));
                cues.Add((p, "Go", 0.3f));
            }
            SC.Cues(t, cues.ToArray()).LoopEvery = 2.6f;

            var t2 = SC.Shot(b, page, "A custom curve",
                "A card that winds up - backs off a little - then rushes past its end and settles: drawn as a Custom Curve.",
                "A GIF of the Game view, plus a screenshot of the curve in Unity's curve editor (click the step's Curve), for 'Custom curves'.");
            var card = SC.CardAt(t2.Stage, "Card", new Vector2(-250f, 0f), new Vector2(160f, 200f), SC.Card);
            UI.Icon(card, "Icon", LabAssets.Diamond, new Vector2(0f, 20f), 70f, SC.Sky);
            var curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, -1.5f), new Keyframe(0.25f, -0.15f), new Keyframe(0.75f, 1.12f), new Keyframe(1f, 1f, 0f, 0f));
            var p2 = t2.PlayerOn(card.gameObject, Anim.New("Wind Up", Pos(null).FromBase(Vector3.zero).ToBase(V3(500f, 0f)).Dur(1f).Curve(curve)));
            SC.Cues(t2, (p2, "Wind Up", 0.3f)).LoopEvery = 2.2f;
            t2.Watch("Card (its player)", card);

            b.Save();
        }
    }
}
