using DG.Tweening;
using rmf_claude.DOTweenUI;
using TMPro;
using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UIAnimLab.EditorTools.S;

namespace UIAnimLab.Showcase.Builders
{
    /// <summary>The Examples page: every recipe built exactly as the page lists it, one frame each.</summary>
    public static class ExamplesShowcase
    {
        private const string Page = "Examples";

        [MenuItem(SC.Menu + "Examples")]
        public static void Build()
        {
            var b = SC.Begin("08 Examples", Page, "Each recipe on the page, built exactly as the page lists it, so the GIF shows what the table makes.");

            FadeAndPop(b);
            SlideIn(b);
            HoverGrow(b);
            ButtonPress(b);
            Cascade(b);
            Pulse(b);
            Transition(b);
            Counter(b);
            Typewriter(b);

            b.Save();
        }

        private static Tile Recipe(LabBoard b, string title, string shows)
        {
            return SC.Shot(b, Page, title, shows, "A GIF of the Game view, under the '" + title + "' heading.");
        }

        private static void FadeAndPop(LabBoard b)
        {
            var t = Recipe(b, "Fade and pop in", "The panel from Your First Animation: Canvas Group Alpha 0 → 1 and Scale 0.8 → Baseline 0 together, 0.25 s, then the mirrored Hide.");
            var panel = BasicsShowcase.SettingsPanel(t.Stage, "Panel", Vector2.zero);
            var p = t.PlayerOn(panel.gameObject, BasicsShowcase.ShowAnimation(), BasicsShowcase.HideAnimation());
            SC.Cues(t, (p, "Show", 0.3f), (p, "Hide", 1.6f)).LoopEvery = 2.6f;
        }

        private static void SlideIn(LabBoard b)
        {
            var t = Recipe(b, "Slide in from off-screen", "Anchored Position, Baseline (0, -600) → Baseline (0, 0), 0.4 s Out Cubic: the panel rises into place from below the frame.");
            var panel = SC.CardAt(t.Stage, "Panel", new Vector2(0f, -20f), new Vector2(600f, 300f));
            SC.Title(panel, "New quest", new Vector2(0f, 80f), 40f, SC.Ink, 500f);
            SC.Body(panel, "Text", "Find the lost lighthouse keeper before the storm.", new Vector2(0f, 10f), 24f, Pal.Hex("5A6185"), 500f);
            SC.Pill(panel, "Accept", "Accept", new Vector2(0f, -90f), new Vector2(200f, 60f), SC.Mint);
            var p = t.PlayerOn(panel.gameObject,
                Anim.New("Slide In", Pos(null).FromBase(V3(0f, -600f)).ToRest().Dur(0.4f).Ease(Ease.OutCubic)),
                Anim.New("Slide Out", Pos(null).FromBase(Vector3.zero).ToBase(V3(0f, -600f)).Dur(0.4f).Ease(Ease.OutCubic)));
            SC.Cues(t, (p, "Slide In", 0.3f), (p, "Slide Out", 1.6f)).LoopEvery = 2.6f;
        }

        private static void HoverGrow(LabBoard b)
        {
            var t = Recipe(b, "Hover grow", "Hover: Scale → Baseline 0.05, Unhover: Scale → Baseline 0, both 0.15 s and Interrupt Others STOP, from Pointer Enter / Pointer Exit. Hover the cards in play mode; Space fakes a quick pass over all three.");
            Color[] colors = { SC.Mint, SC.Sky, SC.Pink };
            string[] names = { "Sword", "Shield", "Potion" };
            Sprite[] icons = { LabAssets.Diamond, LabAssets.Star, LabAssets.Heart };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            for (int i = 0; i < 3; i++)
            {
                var card = SC.CardAt(t.Stage, names[i], new Vector2(-260f + i * 260f, 0f), new Vector2(200f, 260f), SC.Card);
                UI.Icon(card, "Icon", icons[i], new Vector2(0f, 30f), 90f, colors[i]);
                SC.Title(card, names[i], new Vector2(0f, -80f), 28f, SC.Ink, 180f);
                var p = t.PlayerOn(card.gameObject,
                    Anim.New("Hover", S.Scale(null).ToBase(V3(0.05f, 0.05f, 0f)).Dur(0.15f)).InterruptStop(),
                    Anim.New("Unhover", S.Scale(null).ToRest().Dur(0.15f)).InterruptStop());
                var body = card.Find("Body");
                body.GetComponent<Image>().raycastTarget = true;
                UI.Trigger(body, EventTriggerType.PointerEnter, p, "Hover");
                UI.Trigger(body, EventTriggerType.PointerExit, p, "Unhover");
                cues.Add((p, "Hover", 0.3f + i * 0.4f));
                cues.Add((p, "Unhover", 0.6f + i * 0.4f));
            }
            SC.Cues(t, cues.ToArray()).LoopEvery = 2.2f;
        }

        private static void ButtonPress(LabBoard b)
        {
            var t = Recipe(b, "Button press with a click", "Press: Play Sound (Pitch Variation 0.08), then Punch Scale -0.1 with it, 0.25 s, from the button's On Click.");
            var button = SC.Pill(t.Stage, "Button", "Buy", new Vector2(0f, 0f), new Vector2(300f, 100f), SC.Amber);
            var p = t.PlayerOn(button.gameObject, Anim.New("Press", S.Sound(LabAssets.Click, 1f, 1f, 0.08f), PunchScale(null, V3(-0.1f, -0.1f, 0f), 0.25f).With()));
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(UI.Add<Button>(button).onClick, p.PlayAnimation, "Press");
            SC.Cues(t, (p, "Press", 0.3f), (p, "Press", 0.8f), (p, "Press", 1.3f)).LoopEvery = 2f;
        }

        private static void Cascade(LabBoard b)
        {
            var t = Recipe(b, "Cascading menu buttons", "One player on the menu, three Anchored Position steps by Target Path, Baseline (-300, 0) → Baseline 0, delays 0 / 0.05 / 0.1, 0.3 s Out Cubic - plus a Graphic Alpha with each.");
            var menu = UI.Rect(t.Stage, "Menu", new Vector2(0f, 0f), new Vector2(400f, 400f));
            string[] names = { "Play", "Options", "Quit" };
            Color[] colors = { SC.Mint, SC.Sky, SC.Pink };
            var steps = new System.Collections.Generic.List<UIAnimationStep>();
            for (int i = 0; i < 3; i++)
            {
                var pill = SC.Pill(menu, names[i], names[i], new Vector2(0f, 110f - i * 110f), new Vector2(320f, 84f), colors[i]);
                var move = Pos(names[i]).FromBase(V3(-300f, 0f)).ToRest().Dur(0.3f).Ease(Ease.OutCubic).Delay(i * 0.05f);
                if (i > 0) move.With();
                steps.Add(move);
                steps.Add(GAlpha(names[i] + "/Body").With().From(0f).To(1f).Dur(0.3f).Delay(i * 0.05f));
                steps.Add(GAlpha(names[i] + "/Label").With().From(0f).To(1f).Dur(0.3f).Delay(i * 0.05f));
            }
            var p = t.PlayerOn(menu.gameObject, Anim.New("Show", steps.ToArray()));
            SC.Cues(t, (p, "Show", 0.3f)).LoopEvery = 1.8f;
        }

        private static void Pulse(LabBoard b)
        {
            var t = Recipe(b, "Attention pulse", "Scale Baseline 0 → Baseline 0.08, 0.6 s In Out Sine, Loops -1, Yoyo, Interrupt Others unticked.");
            var gift = UI.Icon(t.Stage, "Gift", LabAssets.Star, new Vector2(0f, 20f), 180f, SC.Amber);
            var badge = SC.Dot(gift.transform, "Badge", new Vector2(70f, 70f), 46f, SC.Red);
            SC.Title(badge.transform, "1", Vector2.zero, 28f, SC.Text, 46f);
            SC.Caption(t.Stage, "Daily reward ready", new Vector2(0f, -120f), 400f).fontSize = 26f;
            var p = t.PlayerOn(gift.gameObject, Anim.New("Attention",
                S.Scale(null).FromBase(Vector3.zero).ToBase(V3(0.08f, 0.08f, 0f)).Dur(0.6f).Ease(Ease.InOutSine)).Loop(-1, LoopType.Yoyo).NoInterrupt());
            SC.Cues(t, (p, "Attention", 0.1f)).LoopFromStart = true;
        }

        private static void Transition(LabBoard b)
        {
            var t = Recipe(b, "Fullscreen shader transition", "A fullscreen Image with the Lab Wipe material and a UI Material Instance: Material Float _Progress 0 → 1, 0.8 s In Out Quad.");
            var scene = UI.Rect(t.Stage, "Level", Vector2.zero, new Vector2(960f, 540f));
            SC.Title(scene, "Stage Clear!", new Vector2(0f, 30f), 72f, SC.Amber, 700f);
            UI.Icon(scene, "Star", LabAssets.Star, new Vector2(0f, -90f), 90f, SC.Amber);
            var wipe = UI.Square(t.Stage, "Transition", Vector2.zero, new Vector2(960f, 540f), Pal.Hex("121528"));
            wipe.material = LabAssets.WipeMaterial;
            wipe.gameObject.AddComponent<UIMaterialInstance>();
            var p = t.PlayerOn(wipe.gameObject, Anim.New("Transition Out", MatFloat(null, "_Progress").From(0f).To(1f).Dur(0.8f).Ease(Ease.InOutQuad)));
            SC.Cues(t, (p, "Transition Out", 0.4f)).LoopEvery = 2f;
        }

        private static void Counter(LabBoard b)
        {
            var t = Recipe(b, "Score counter", "A TextMeshPro text holding only the number: Custom Property text (as number), To Absolute, Format N0, 0.6 s Out Quad, From off - so it counts on from what's showing. The label is a separate text.");
            SC.Title(t.Stage, "SCORE", new Vector2(0f, 90f), 30f, SC.Muted, 400f);
            var score = SC.Title(t.Stage, "1,250", new Vector2(0f, 10f), 96f, SC.Amber, 600f);
            score.name = "Score";
            var p = t.Player(
                Anim.New("Count Up", Prop("Stage/Score", typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.NumberText).To(4800f).Format("N0").Dur(0.6f)),
                Anim.New("Count More", Prop("Stage/Score", typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.NumberText).To(12500f).Format("N0").Dur(0.6f)));
            SC.Cues(t, (p, "Count Up", 0.3f), (p, "Count More", 1.3f)).LoopEvery = 2.6f;
        }

        private static void Typewriter(LabBoard b)
        {
            var t = Recipe(b, "Typewriter dialogue", "Custom Property text (typewriter), From Absolute \"\" → To, 1.5 s Linear. Rich text like bold is never shown half-typed.");
            var box = SC.CardAt(t.Stage, "Dialogue Box", new Vector2(0f, -110f), new Vector2(820f, 200f), SC.Card);
            SC.Dot(box, "Portrait", new Vector2(-320f, 10f), 120f, SC.Lilac);
            UI.Label(box, "Speaker", "Old Sailor", new Vector2(-60f, 60f), new Vector2(420f, 40f), 26f, SC.Pink, TextAlignmentOptions.Left, FontStyles.Bold);
            var line = UI.Label(box, "Line", "", new Vector2(40f, -10f), new Vector2(600f, 110f), 28f, SC.Ink, TextAlignmentOptions.TopLeft);
            var p = t.PlayerOn(box.gameObject,
                Anim.New("Say", Prop("Line", typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.Text).From("").To("Welcome, <b>traveller</b>. Mind the rocks past the lighthouse.").Dur(1.5f).Ease(Ease.Linear)));
            SC.Cues(t, (p, "Say", 0.3f)).LoopEvery = 2.6f;
        }
    }
}
