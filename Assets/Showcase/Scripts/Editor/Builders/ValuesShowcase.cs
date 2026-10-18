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
    /// <summary>Showcase scenes for Sound, Custom Property, and Material and Shader Properties.</summary>
    public static class ValuesShowcase
    {
        [MenuItem(SC.Menu + "Sound")]
        public static void Sound()
        {
            const string page = "Sound";
            var b = SC.Begin("12 Sound", page, "Sound steps make no picture, so these frames are for Inspector and Hierarchy screenshots (and for listening).");

            var t = SC.Shot(b, page, "A button with a click",
                "A button whose Press plays a click (Pitch Variation 0.08) with a squish. Space presses it four times.",
                "A screenshot of the Play Sound step open in the Inspector, for the example at the top. In play mode, a screenshot of the Hierarchy showing UI Animation Audio under DontDestroyOnLoad, for 'Where the sound plays from'.");
            var button = SC.Pill(t.Stage, "Button", "Click me", new Vector2(0f, 20f), new Vector2(280f, 90f), SC.Amber);
            var player = t.PlayerOn(button.gameObject, Anim.New("Press",
                S.Sound(LabAssets.Click, 1f, 1f, 0.08f),
                PunchScale(null, V3(-0.1f, -0.1f, 0f), 0.25f).With()));
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(UI.Add<Button>(button).onClick, player.PlayAnimation, "Press");
            SC.Cues(t, (player, "Press", 0.3f), (player, "Press", 0.7f), (player, "Press", 1.1f), (player, "Press", 1.5f));
            t.Watch("Button (its player)", button);

            var t2 = SC.Shot(b, page, "Landing with the pop",
                "A Show whose pop sound is delayed 0.15 s, so it lands as the panel overshoots.",
                "A screenshot of the Show animation's steps (folded) in the Inspector, showing the delayed sound step.");
            var panel = SC.CardAt(t2.Stage, "Panel", Vector2.zero, new Vector2(360f, 220f));
            UI.Group(panel);
            SC.Title(panel, "Reward!", new Vector2(0f, 30f), 40f, SC.Ink, 300f);
            UI.Icon(panel, "Coin", LabAssets.Circle, new Vector2(0f, -45f), 60f, SC.Amber);
            var p2 = t2.PlayerOn(panel.gameObject, Anim.New("Show",
                Alpha(null).From(0f).To(1f).Dur(0.25f),
                S.Scale(null).With().From(V3(0.6f, 0.6f, 1f)).ToRest().Dur(0.3f).Ease(Ease.OutBack),
                S.Sound(LabAssets.Pop).With().Delay(0.15f)));
            SC.Cues(t2, (p2, "Show", 0.3f)).LoopEvery = 1.8f;
            t2.Watch("Panel (its player)", panel);

            b.Save();
        }

        [MenuItem(SC.Menu + "Custom Property")]
        public static void CustomProperty()
        {
            const string page = "Custom Property";
            var b = SC.Begin("13 Custom Property", page, "A score counter, typewriter text, a health bar's fill, a slider, and the Property dropdown.");

            var t = SC.Shot(b, page, "Score, typewriter, health, slider",
                "Four Custom Property steps: a TextMeshPro text counted as a number (N0), the same text typed out, an Image's fillAmount, and a Slider's value.",
                "A GIF of the Game view for the top of the page.");
            var score = UI.Label(t.Stage, "Score", "0", new Vector2(-220f, 140f), new Vector2(360f, 90f), 72f, SC.Amber, TextAlignmentOptions.Center, FontStyles.Bold);
            SC.Caption(t.Stage, "text (as number), N0", new Vector2(-220f, 80f), 300f);
            var typed = UI.Label(t.Stage, "Dialogue", "", new Vector2(200f, 140f), new Vector2(420f, 100f), 26f, SC.Text, TextAlignmentOptions.TopLeft);
            SC.Caption(t.Stage, "text (typewriter)", new Vector2(200f, 80f), 300f);

            UI.Box(t.Stage, "Health Back", new Vector2(-220f, -60f), new Vector2(340f, 36f), Pal.Hex("39406E"));
            var health = UI.Square(t.Stage, "Health", new Vector2(-220f, -60f), new Vector2(340f, 36f), SC.Mint);
            health.type = Image.Type.Filled;
            health.fillMethod = Image.FillMethod.Horizontal;
            health.fillAmount = 0.2f;
            SC.Caption(t.Stage, "Image / float fillAmount", new Vector2(-220f, -110f), 300f);

            var slider = MakeSlider(t.Stage, "Volume", new Vector2(200f, -60f));
            SC.Caption(t.Stage, "Slider / float value", new Vector2(200f, -110f), 300f);

            var player = t.Player(Anim.New("Show Off",
                Prop("Stage/Score", typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.NumberText).From(0f).To(12500f).Format("N0").Dur(1.4f).Ease(Ease.OutCubic),
                Prop("Stage/Dialogue", typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.Text).With().From("").To("Welcome, <b>traveller</b>. The road ahead is long.").Dur(1.6f).Ease(Ease.Linear),
                Prop("Stage/Health", typeof(Image), "fillAmount", UIAnimationPropertyKind.Float).With().From(0.2f).To(0.9f).Dur(1f).Ease(Ease.OutBack),
                Prop("Stage/Volume", typeof(Slider), "value", UIAnimationPropertyKind.Float).With().From(0f).To(0.75f).Dur(1.2f).Ease(Ease.InOutSine)));
            SC.Cues(t, (player, "Show Off", 0.3f)).LoopEvery = 3f;

            var t2 = SC.Shot(b, page, "The Property dropdown",
                "A Custom Property step pointed at an object with a TextMeshPro text and a script of your own (TestGauge: Health, Coins).",
                "A screenshot of the open Property dropdown, grouped by component, for 'Setting one up' step 3.");
            var gaugeRoot = UI.Rect(t2.Stage, "Gauge", new Vector2(0f, 20f), new Vector2(420f, 100f));
            UI.Box(gaugeRoot, "Back", new Vector2(0f, 15f), new Vector2(420f, 34f), Pal.Hex("39406E"));
            var bar = UI.Square(gaugeRoot, "Bar", new Vector2(0f, 15f), new Vector2(420f, 34f), SC.Pink);
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            var label = UI.Label(gaugeRoot, "Readout", "", new Vector2(0f, -30f), new Vector2(420f, 40f), 24f, SC.Text);
            gaugeRoot.gameObject.AddComponent<TestGauge>().Setup(bar, label);
            var p2 = t2.PlayerOn(gaugeRoot.gameObject, Anim.New("Heal", Prop(null, typeof(TestGauge), "Health", UIAnimationPropertyKind.Float).From(10f).To(100f).Dur(1f).Ease(Ease.OutCubic)));
            SC.Cues(t2, (p2, "Heal", 0.3f)).LoopEvery = 2f;
            t2.Watch("Gauge (its player)", gaugeRoot.gameObject);

            b.Save();
        }

        private static Slider MakeSlider(Transform parent, string name, Vector2 position)
        {
            var root = UI.Rect(parent, name, position, new Vector2(340f, 30f));
            var slider = root.gameObject.AddComponent<Slider>();
            UI.Box(root, "Background", Vector2.zero, new Vector2(340f, 12f), Pal.Hex("39406E"));
            var fillArea = UI.Rect(root, "Fill Area", Vector2.zero, new Vector2(320f, 12f));
            var fill = UI.Box(fillArea, "Fill", Vector2.zero, Vector2.zero, SC.Sky);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.sizeDelta = new Vector2(10f, 0f);
            var handleArea = UI.Rect(root, "Handle Slide Area", Vector2.zero, new Vector2(320f, 30f));
            var handle = SC.Dot(handleArea, "Handle", Vector2.zero, 30f, Color.white);
            handle.rectTransform.sizeDelta = new Vector2(30f, 30f);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.value = 0f;
            return slider;
        }

        [MenuItem(SC.Menu + "Material and Shader Properties")]
        public static void Materials()
        {
            const string page = "Material and Shader Properties";
            var b = SC.Begin("14 Material and Shader Properties", page, "Material Float and Material Color steps, on an Image and on a TextMeshPro text.");

            var t = SC.Shot(b, page, "A fullscreen transition",
                "A circle wipe (the Lab Wipe shader's _Progress) covers the frame, the scene behind it changes, and it uncovers again.",
                "A GIF of the Game view for the top of the page and for Examples: Fullscreen shader transition.");
            var before = UI.Rect(t.Stage, "Level 1", Vector2.zero, new Vector2(960f, 540f));
            SC.Title(before, "Level 1", new Vector2(0f, 40f), 72f, SC.Text, 600f);
            UI.Icon(before, "Hero", LabAssets.Pixel, new Vector2(0f, -80f), 96f, SC.Mint);
            var after = UI.Rect(t.Stage, "Level 2", Vector2.zero, new Vector2(960f, 540f));
            SC.Title(after, "Level 2", new Vector2(0f, 40f), 72f, SC.Amber, 600f);
            UI.Icon(after, "Hero", LabAssets.Pixel, new Vector2(0f, -80f), 96f, SC.Pink);
            after.gameObject.SetActive(false);

            var wipe = UI.Square(t.Stage, "Wipe", Vector2.zero, new Vector2(960f, 540f), Pal.Hex("121528"));
            wipe.material = LabAssets.WipeMaterial;
            wipe.gameObject.AddComponent<UIMaterialInstance>();
            var player = t.Player(Anim.New("Transition",
                MatFloat("Stage/Wipe", "_Progress").From(0f).To(1f).Dur(0.7f).Ease(Ease.InOutQuad),
                Active("Stage/Level 1", false),
                Active("Stage/Level 2", true),
                MatFloat("Stage/Wipe", "_Progress").To(0f).Dur(0.7f).Ease(Ease.InOutQuad).Delay(0.2f)));
            SC.Cues(t, (player, "Transition", 0.4f)).LoopEvery = 2.6f;

            var t2 = SC.Shot(b, page, "Glow and tint",
                "A Material Float (_Glow) and a Material Color (_GlowColor) on a gem, and TextMeshPro's own _FaceDilate and _FaceColor on a title.",
                "A GIF of the Game view for 'Setting one up'.");
            var gem = UI.Icon(t2.Stage, "Gem", LabAssets.Diamond, new Vector2(-220f, 0f), 200f, SC.Sky);
            gem.material = LabAssets.GlowMaterial;
            gem.gameObject.AddComponent<UIMaterialInstance>();
            var title = SC.Title(t2.Stage, "LEVEL UP", new Vector2(200f, 0f), 72f, SC.Text, 460f);
            title.gameObject.AddComponent<UIMaterialInstance>();
            var p2 = t2.Player(Anim.New("Shine",
                MatFloat("Stage/Gem", "_Glow").From(0f).To(0.8f).Dur(0.4f),
                MatColor("Stage/Gem", "_GlowColor").With().From(Color.white).To(SC.Amber).Dur(0.8f),
                MatFloat("Stage/Gem", "_Glow").To(0f).Dur(0.6f),
                MatFloat("Stage/Title LEVEL UP", "_FaceDilate").With().From(0f).To(0.35f).Dur(0.4f),
                MatColor("Stage/Title LEVEL UP", "_FaceColor").With().From(Color.white).To(SC.Amber).Dur(0.4f)));
            SC.Cues(t2, (p2, "Shine", 0.3f)).LoopEvery = 2.4f;

            var t3 = SC.Shot(b, page, "Under a stencil Mask",
                "The same glow on two hearts: under a RectMask2D (left) it shows; under a stencil Mask (right) it doesn't, and UI Material Instance warns.",
                "A GIF of the Game view for the WARNING, and a screenshot of the right heart's UI Material Instance warning in the Inspector.");
            var left = MaskedHeart(t3, "Under RectMask2D", new Vector2(-220f, 20f), false);
            var right = MaskedHeart(t3, "Under Mask", new Vector2(220f, 20f), true);
            var p3 = t3.Player(Anim.New("Glow",
                MatFloat("Stage/Under RectMask2D/Heart", "_Glow").From(0f).To(1f).Dur(0.6f),
                MatFloat("Stage/Under Mask/Heart", "_Glow").With().From(0f).To(1f).Dur(0.6f)));
            SC.Cues(t3, (p3, "Glow", 0.3f)).LoopEvery = 2f;
            t3.Watch("the two hearts", left, right);

            b.Save();
        }

        private static GameObject MaskedHeart(Tile t, string name, Vector2 at, bool stencil)
        {
            var holder = UI.Box(t.Stage, name, at, new Vector2(300f, 300f), SC.Panel);
            if (stencil) holder.gameObject.AddComponent<Mask>();
            else holder.gameObject.AddComponent<RectMask2D>();
            var heart = UI.Icon(holder.transform, "Heart", LabAssets.Heart, Vector2.zero, 200f, SC.Pink);
            heart.material = LabAssets.GlowMaterial;
            heart.gameObject.AddComponent<UIMaterialInstance>();
            SC.Caption(t.Stage, name, at + new Vector2(0f, -185f), 300f);
            return heart.gameObject;
        }
    }
}
