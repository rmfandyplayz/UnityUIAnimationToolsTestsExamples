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
    /// <summary>01 Step Types: every step type does what it says, one tile each.</summary>
    public static class StepTypesScene
    {
        [MenuItem(UT.Menu + "01 Step Types")]
        public static void Build()
        {
            var b = UT.Begin("01 Step Types", "01  Step Types",
                "Every step type, one tile each, played on its own. Each tile's animations are previewed with the Play buttons under them, unless the tile says PLAY MODE. " +
                "Faint outlines on a stage mark where things should land.");

            AnchoredPosition(b);
            LocalPosition(b);
            Rotation(b);
            Scale(b);
            SizeDelta(b);
            Offsets(b);
            CanvasGroupAlpha(b);
            GraphicAlpha(b);
            GraphicColor(b);
            MaterialFloat(b);
            MaterialColor(b);
            SetActive(b);
            PlaySound(b);
            Punch(b);
            Shake(b);
            CustomNumbers(b);
            CustomValues(b);

            b.Save();
        }

        private static void AnchoredPosition(LabBoard b)
        {
            var t = b.Test("T1", "Anchored Position");
            UT.Mark(t, "Left", new Vector2(-200f, 0f), new Vector2(64f, 64f));
            UT.Mark(t, "Right", new Vector2(200f, 0f), new Vector2(64f, 64f));
            var box = UT.Box(t, "Box", Vector2.zero, Pal.Blue);

            t.Player(
                Anim.New("Across", Pos(UT.Path("Box")).FromBase(V3(-200f, 0f)).ToBase(V3(200f, 0f)).Dur(0.8f).Ease(Ease.InOutSine)),
                Anim.New("Home", Pos(UT.Path("Box")).ToRest().Dur(0.3f)));

            t.Summary("Moves a UI element by its anchored position.")
             .Watch("Box", box)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Across</b>, then <b>Home</b>.")
             .Expect("Across jumps Box onto the left outline and slides it onto the right one. Home slides it back to the middle, where it rests.");
        }

        private static void LocalPosition(LabBoard b)
        {
            var t = b.Test("T2", "Local Position");

            // A box anchored to the stage's bottom-left corner: its anchored position and its local position differ,
            // so a Local Position of (0, 0, 0) lands on the stage's centre (its pivot), not on its anchor.
            UT.Mark(t, "Centre", Vector2.zero, new Vector2(64f, 64f));
            var box = UT.Box(t, "Box", Vector2.zero, Pal.Green);
            var rect = box.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.anchoredPosition = new Vector2(70f, 60f);
            UI.Caption(t.Stage, "anchored bottom-left", new Vector2(-180f, -110f));

            t.Player(
                Anim.New("To Centre", LocalPos(UT.Path("Box")).To(Vector3.zero).Dur(0.6f).Ease(Ease.OutCubic)),
                Anim.New("Back", LocalPos(UT.Path("Box")).ToRest().Dur(0.4f)));

            t.Summary("Moves an object by its plain Transform position, which ignores anchors.")
             .Watch("Box, anchored to the bottom-left corner", box)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>To Centre</b>, then <b>Back</b>.")
             .Expect("To Centre lands Box exactly on the centre outline: local position (0, 0) is the parent's pivot, whatever the anchors say. Back returns it to the corner.");
        }

        private static void Rotation(LabBoard b)
        {
            var t = b.Test("T3", "Rotation");
            var box = UT.Box(t, "Box", Vector2.zero, Pal.Orange, 80f);
            UI.Icon(box.transform, "Up", LabAssets.Chevron, new Vector2(0f, 18f), 36f, Pal.Stage).rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);

            t.Player(
                Anim.New("Spin", Rot(UT.Path("Box")).ToCur(V3(0f, 0f, -360f)).Dur(1f).Ease(Ease.InOutCubic)),
                Anim.New("Tilt", Rot(UT.Path("Box")).To(V3(0f, 0f, 25f)).Dur(0.4f).Ease(Ease.OutBack)),
                Anim.New("Upright", Rot(UT.Path("Box")).ToRest().Dur(0.3f)));

            t.Summary("Turns an element. On UI only Z is used.")
             .Watch("Box (the arrow shows which way is up)", box)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Spin</b>, <b>Tilt</b>, then <b>Upright</b>.")
             .Expect("Spin turns one full turn clockwise and ends upright (a To beyond 360 really spins). Tilt leans it 25 degrees left. Upright puts it back.");
        }

        private static void Scale(LabBoard b)
        {
            var t = b.Test("T4", "Scale");
            UT.Mark(t, "Rest", Vector2.zero, new Vector2(90f, 90f));
            var box = UT.Box(t, "Box", Vector2.zero, Pal.Pink, 90f);

            t.Player(
                Anim.New("Pop In", S.Scale(UT.Path("Box")).From(Vector3.zero).ToRest().Dur(0.45f).Ease(Ease.OutBack)),
                Anim.New("Squash", S.Scale(UT.Path("Box")).To(V3(1.5f, 0.6f, 1f)).Dur(0.3f)),
                Anim.New("Normal", S.Scale(UT.Path("Box")).ToRest().Dur(0.3f)));

            t.Summary("Grows or shrinks an element. 1 is its normal size.")
             .Watch("Box", box)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Pop In</b>, <b>Squash</b>, then <b>Normal</b>.")
             .Expect("Pop In grows Box from nothing, overshoots a little and settles exactly on the outline. Squash makes it wide and flat. Normal puts it back.");
        }

        private static void SizeDelta(LabBoard b)
        {
            var t = b.Test("T5", "Size Delta");
            UT.Mark(t, "Full", new Vector2(0f, 0f), new Vector2(440f, 40f));
            var bar = UI.Square(t.Stage, "Bar", new Vector2(-220f, 0f), new Vector2(60f, 40f), Pal.Teal);
            bar.rectTransform.pivot = new Vector2(0f, 0.5f);
            UI.Caption(t.Stage, "pivot on the left edge", new Vector2(-150f, -45f));

            t.Player(
                Anim.New("Grow", Size(UT.Path("Bar")).To(V3(440f, 40f)).Dur(0.7f).Ease(Ease.OutCubic)),
                Anim.New("Shrink", Size(UT.Path("Bar")).ToRest().Dur(0.4f)));

            t.Summary("Resizes an element around its pivot.")
             .Watch("Bar", bar)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Grow</b>, then <b>Shrink</b>.")
             .Expect("Grow stretches Bar to the right until it fills the outline; its left edge (the pivot) never moves. Shrink returns it to 60 wide.");
        }

        private static void Offsets(LabBoard b)
        {
            var t = b.Test("T6", "Offset Min / Offset Max");
            var frame = UI.Square(t.Stage, "Frame", Vector2.zero, new Vector2(480f, 200f), Pal.Ghost);
            var panel = UI.Square(frame.transform, "Panel", Vector2.zero, Vector2.zero, Pal.Purple);
            var rect = panel.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 12f);
            rect.offsetMax = new Vector2(-12f, -12f);

            string path = "Stage/Frame/Panel";
            t.Player(
                Anim.New("Inset Left-Bottom", OffMin(path).To(V3(120f, 60f)).Dur(0.5f)),
                Anim.New("Inset Right-Top", OffMax(path).To(V3(-120f, -60f)).Dur(0.5f)),
                Anim.New("Reset Edges", OffMin(path).ToRest().Dur(0.3f), OffMax(path).With().ToRest().Dur(0.3f)));

            t.Summary("Moves an element's left/bottom edges (Offset Min) or right/top edges (Offset Max), holding the opposite corner still.")
             .Watch("Panel, stretched inside Frame", panel)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Inset Left-Bottom</b>, then <b>Inset Right-Top</b>, then <b>Reset Edges</b>. Select Panel during the second and read its Right and Top in the RectTransform.")
             .Expect("The first pulls only the left and bottom edges in; the top-right corner stays put. The second pulls the right and top edges in: the step's (-120, -60) shows as Right 120, Top 60 in the Inspector (Unity flips their sign). Reset Edges puts both back at 12.");
        }

        private static void CanvasGroupAlpha(LabBoard b)
        {
            var t = b.Test("T7", "Canvas Group Alpha");
            var panel = UI.Box(t.Stage, "Panel", Vector2.zero, new Vector2(300f, 160f), Pal.Blue);
            UI.Group(panel);
            UI.Icon(panel.transform, "Star", LabAssets.Star, new Vector2(-80f, 0f), 70f, Pal.Yellow);
            UI.Label(panel.transform, "Text", "children fade too", new Vector2(40f, 0f), new Vector2(160f, 60f), 20f, Pal.White);

            t.Player(
                Anim.New("Fade Out", Alpha(UT.Path("Panel")).To(0f).Dur(0.5f)),
                Anim.New("Fade In", Alpha(UT.Path("Panel")).From(0f).To(1f).Dur(0.5f)));

            t.Summary("Fades a Canvas Group: the panel and everything inside it.")
             .Watch("Panel, with a star and a label inside", panel)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Fade Out</b>, then <b>Fade In</b>.")
             .Expect("The panel, the star and the text fade out together, and back in. The star and text never fade on their own.");
        }

        private static void GraphicAlpha(LabBoard b)
        {
            var t = b.Test("T8", "Graphic Alpha");
            Graphics(t, out var image, out var raw, out var tmp, out var legacy);

            t.Player(
                Anim.New("Fade All",
                    GAlpha(UT.Path("Image")).To(0.15f).Dur(0.5f),
                    GAlpha(UT.Path("RawImage")).With().To(0.15f).Dur(0.5f).Delay(0.1f),
                    GAlpha(UT.Path("TMP Text")).With().To(0.15f).Dur(0.5f).Delay(0.2f),
                    GAlpha(UT.Path("Legacy Text")).With().To(0.15f).Dur(0.5f).Delay(0.3f)),
                Anim.New("Restore",
                    GAlpha(UT.Path("Image")).ToRest(),
                    GAlpha(UT.Path("RawImage")).With().ToRest(),
                    GAlpha(UT.Path("TMP Text")).With().ToRest(),
                    GAlpha(UT.Path("Legacy Text")).With().ToRest()));

            t.Summary("Fades one Graphic: an Image, a RawImage, a TextMeshPro text or a legacy Text.")
             .Watch("Image, RawImage, TMP Text, Legacy Text", image, raw, tmp, legacy)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Fade All</b>, then <b>Restore</b>.")
             .Expect("All four fade to faint one after another, left to right, keeping their colour. Restore brings all four back to full at once.");
        }

        private static void GraphicColor(LabBoard b)
        {
            var t = b.Test("T9", "Graphic Color");
            Graphics(t, out var image, out var raw, out var tmp, out var legacy);

            t.Player(
                Anim.New("Tint",
                    Tint(UT.Path("Image")).To(Pal.Red).Dur(0.5f),
                    Tint(UT.Path("RawImage")).With().To(Pal.Green).Dur(0.5f),
                    Tint(UT.Path("TMP Text")).With().To(Pal.Yellow).Dur(0.5f),
                    Tint(UT.Path("Legacy Text")).With().To(Pal.Purple).Dur(0.5f)),
                Anim.New("Restore",
                    Tint(UT.Path("Image")).ToRest(),
                    Tint(UT.Path("RawImage")).With().ToRest(),
                    Tint(UT.Path("TMP Text")).With().ToRest(),
                    Tint(UT.Path("Legacy Text")).With().ToRest()));

            t.Summary("Tints one Graphic: Image, RawImage, TextMeshPro or legacy Text.")
             .Watch("Image, RawImage, TMP Text, Legacy Text", image, raw, tmp, legacy)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Tint</b>, then <b>Restore</b>.")
             .Expect("Image turns red, RawImage green, the TMP text yellow, the legacy text purple, all together. Restore returns each to its own starting colour.");
        }

        private static void Graphics(Tile t, out Image image, out RawImage raw, out TextMeshProUGUI tmp, out Text legacy)
        {
            image = UI.Box(t.Stage, "Image", new Vector2(-210f, 10f), new Vector2(90f, 90f), Pal.Blue);

            var rawRect = UI.Rect(t.Stage, "RawImage", new Vector2(-70f, 10f), new Vector2(90f, 90f));
            raw = rawRect.gameObject.AddComponent<RawImage>();
            raw.texture = LabAssets.Circle.texture;
            raw.color = Pal.Orange;
            raw.raycastTarget = false;

            tmp = UI.Label(t.Stage, "TMP Text", "TMP", new Vector2(70f, 10f), new Vector2(110f, 90f), 44f, Pal.White, TextAlignmentOptions.Center, FontStyles.Bold);

            var legacyRect = UI.Rect(t.Stage, "Legacy Text", new Vector2(210f, 10f), new Vector2(120f, 90f));
            legacy = legacyRect.gameObject.AddComponent<Text>();
            legacy.font = UI.LegacyFont != null ? UI.LegacyFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            legacy.text = "Text";
            legacy.fontSize = 40;
            legacy.alignment = TextAnchor.MiddleCenter;
            legacy.color = Pal.Teal;
            legacy.raycastTarget = false;

            UI.Caption(t.Stage, "Image", new Vector2(-210f, -60f), 100f);
            UI.Caption(t.Stage, "RawImage", new Vector2(-70f, -60f), 100f);
            UI.Caption(t.Stage, "TextMeshPro", new Vector2(70f, -60f), 100f);
            UI.Caption(t.Stage, "legacy Text", new Vector2(210f, -60f), 100f);
        }

        private static void MaterialFloat(LabBoard b)
        {
            var t = b.Test("T10", "Material Float");
            var bar = UI.Square(t.Stage, "Fill Bar", new Vector2(0f, 50f), new Vector2(440f, 46f), Pal.Green);
            bar.material = LabAssets.FillMaterial;
            bar.gameObject.AddComponent<UIMaterialInstance>();

            var text = UI.Label(t.Stage, "Dilate Text", "Thicker", new Vector2(0f, -55f), new Vector2(400f, 80f), 52f, Pal.White, TextAlignmentOptions.Center);
            text.gameObject.AddComponent<UIMaterialInstance>();

            t.Player(
                Anim.New("Fill",
                    MatFloat(UT.Path("Fill Bar"), "_Progress").From(0f).To(1f).Dur(1f).Ease(Ease.InOutQuad)),
                Anim.New("Thicken",
                    MatFloat(UT.Path("Dilate Text"), "_FaceDilate").From(0f).To(0.45f).Dur(0.5f),
                    MatFloat(UT.Path("Dilate Text"), "_FaceDilate").To(0f).Dur(0.5f).Delay(0.3f)));

            t.Summary("Animates a float on an element's own copy of its material (UI Material Instance), on an Image and on a TextMeshPro text.")
             .Watch("Fill Bar (Lab Fill shader), Dilate Text (TMP's own shader)", bar, text)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Fill</b> and <b>Thicken</b>. Afterwards, look at <b>Assets/Shared/Materials/Lab Fill.mat</b>: its Progress should still read 1.")
             .Expect("Fill sweeps the bar from empty to full, left to right. Thicken makes the letters bolder, then thin again. The material asset itself never changes.");
        }

        private static void MaterialColor(LabBoard b)
        {
            var t = b.Test("T11", "Material Color");
            var heart = UI.Icon(t.Stage, "Glow Heart", LabAssets.Heart, new Vector2(-130f, 0f), 140f, Pal.Pink);
            heart.material = LabAssets.GlowMaterial;
            heart.gameObject.AddComponent<UIMaterialInstance>();

            var text = UI.Label(t.Stage, "Face Text", "Face", new Vector2(130f, 0f), new Vector2(240f, 100f), 64f, Pal.White, TextAlignmentOptions.Center, FontStyles.Bold);
            text.gameObject.AddComponent<UIMaterialInstance>();

            t.Player(
                Anim.New("Glow",
                    MatFloat(UT.Path("Glow Heart"), "_Glow").From(0f).To(1f).Dur(0.4f),
                    MatColor(UT.Path("Glow Heart"), "_GlowColor").With().From(Pal.Yellow).To(Pal.Teal).Dur(0.8f)),
                Anim.New("Face Colour",
                    MatColor(UT.Path("Face Text"), "_FaceColor").To(Pal.Orange).Dur(0.5f),
                    MatColor(UT.Path("Face Text"), "_FaceColor").To(Color.white).Dur(0.5f).Delay(0.3f)),
                Anim.New("No Glow",
                    MatFloat(UT.Path("Glow Heart"), "_Glow").ToRest().Dur(0.3f)));

            t.Summary("Animates a colour on an element's own material copy.")
             .Watch("Glow Heart (Lab Glow shader), Face Text (TMP)", heart, text)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Glow</b>, then <b>No Glow</b>, and <b>Face Colour</b>.")
             .Expect("Glow lights the heart up yellow and shifts the glow to teal. No Glow fades it off. Face Colour turns the letters orange, then white again.");
        }

        private static void SetActive(LabBoard b)
        {
            var t = b.Test("T12", "Set Active");
            UT.Mark(t, "Lamp", new Vector2(-120f, 0f), new Vector2(100f, 100f));
            var lamp = UI.Icon(t.Stage, "Lamp", LabAssets.Star, new Vector2(-120f, 0f), 100f, Pal.Yellow);
            lamp.gameObject.SetActive(false);
            var label = UI.Label(t.Stage, "Clock", "on at 0.5 s, off at 1.5 s", new Vector2(120f, 0f), new Vector2(240f, 80f), 18f, Pal.BodyText);

            t.Player(
                Anim.New("Blink",
                    Active(UT.Path("Lamp"), true).Delay(0.5f),
                    Active(UT.Path("Lamp"), false).Delay(1f)),
                Anim.New("On", Active(UT.Path("Lamp"), true)),
                Anim.New("Off", Active(UT.Path("Lamp"), false)));

            t.Summary("Turns an object on or off at a moment in the animation. The path reaches a hidden object too.")
             .Watch("Lamp (starts hidden - the outline marks it)", lamp.gameObject)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Blink</b>. Then <b>On</b>, and <b>Off</b>. Stop and Restore afterwards.")
             .Expect("Blink: the star appears half a second in and disappears a second later. On / Off switch it straight away. Stop and Restore leaves it hidden, as it was.");
        }

        private static void PlaySound(LabBoard b)
        {
            var t = b.Test("T13", "Play Sound", 620f, 300f);

            // Three players, one per place a sound can come from.
            var shared = UI.Rect(t.Stage, "Shared Source", new Vector2(-190f, 40f), new Vector2(170f, 60f));
            var own = UI.Rect(t.Stage, "Own Source", new Vector2(0f, 40f), new Vector2(170f, 60f));
            var slot = UI.Rect(t.Stage, "Slot Source", new Vector2(190f, 40f), new Vector2(170f, 60f));

            var ownSource = own.gameObject.AddComponent<AudioSource>();
            ownSource.playOnAwake = false;
            ownSource.pitch = 1f;

            var speaker = UI.Rect(t.Stage, "Speaker", new Vector2(190f, -60f), new Vector2(20f, 20f));
            var speakerSource = speaker.gameObject.AddComponent<AudioSource>();
            speakerSource.playOnAwake = false;
            UI.Caption(t.Stage, "Speaker (an AudioSource)", new Vector2(190f, -80f), 200f);

            var sharedPlayer = t.PlayerOn(shared.gameObject,
                Anim.New("Click", Sound(LabAssets.Click, 1f, 1f, 0.12f), PunchScale(null, V3(-0.15f, -0.15f, 0f)).With()));
            var ownPlayer = t.PlayerOn(own.gameObject,
                Anim.New("Pop", Sound(LabAssets.Pop, 1f, 1.4f), PunchScale(null, V3(-0.15f, -0.15f, 0f)).With()));
            var slotPlayer = t.PlayerOn(slot.gameObject,
                Anim.New("Jingle", Sound(LabAssets.Jingle).Target(speakerSource), PunchScale(null, V3(-0.15f, -0.15f, 0f)).With()));

            Fill(shared, "Click\n(shared source)", sharedPlayer, "Click");
            Fill(own, "Pop\n(own source)", ownPlayer, "Pop");
            Fill(slot, "Jingle\n(step's slot)", slotPlayer, "Jingle");

            UI.Caption(t.Stage, "Click the buttons in the Game view", new Vector2(-95f, -40f), 300f);

            t.Summary("Plays a clip from the step's Audio Source, else the player's own, else a shared 2D source the tool makes.")
             .Watch("the three buttons; the Hierarchy's DontDestroyOnLoad", shared, own, slot)
             .Needs(StationNeeds.PlayMode)
             .HowTo("Enter play mode and click each button in the Game view. Click <b>Click</b> rapidly a few times. Then try the Play buttons in the edit-mode preview.")
             .Expect("Click plays a short click, slightly differently pitched each time, and <b>UI Animation Audio</b> appears under DontDestroyOnLoad. Pop plays higher (pitch 1.4) from Own Source's AudioSource. Jingle plays from Speaker. Rapid clicks overlap rather than cut off. In the edit-mode preview, nothing sounds.");
        }

        private static void Fill(RectTransform holder, string text, UIAnimationPlayer player, string animation)
        {
            var button = UI.PlayButton(holder, text, Vector2.zero, new Vector2(170f, 60f), player, animation);
            var r = (RectTransform)button.transform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero;
            button.GetComponentInChildren<TMP_Text>().fontSize = 15f;
            button.GetComponentInChildren<TMP_Text>().textWrappingMode = TextWrappingModes.Normal;
        }

        private static void Punch(LabBoard b)
        {
            var t = b.Test("T14", "Punch");
            var pos = UT.Box(t, "Punch Pos", new Vector2(-180f, 10f), Pal.Blue, 70f);
            var rot = UT.Box(t, "Punch Rot", new Vector2(0f, 10f), Pal.Orange, 70f);
            var scale = UT.Box(t, "Punch Scale", new Vector2(180f, 10f), Pal.Pink, 70f);
            UI.Caption(t.Stage, "Punch Anchored Position", new Vector2(-180f, -50f));
            UI.Caption(t.Stage, "Punch Rotation", new Vector2(0f, -50f));
            UI.Caption(t.Stage, "Punch Scale", new Vector2(180f, -50f));

            t.Player(Anim.New("Punch All",
                PunchPos(UT.Path("Punch Pos"), V3(0f, 40f), 0.5f),
                PunchRot(UT.Path("Punch Rot"), V3(0f, 0f, 30f), 0.5f).With(),
                PunchScale(UT.Path("Punch Scale"), V3(0.3f, 0.3f, 0f), 0.5f).With()));

            t.Summary("A jolt in one direction that springs back to where it started.")
             .Watch("Punch Pos, Punch Rot, Punch Scale", pos, rot, scale)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Punch All</b>. Then press Play again rapidly, several times.")
             .Expect("Each box jolts (up, a twist, bigger) and wobbles back. However fast Play is pressed, every box ends exactly where it started.");
        }

        private static void Shake(LabBoard b)
        {
            var t = b.Test("T15", "Shake");
            var pos = UT.Box(t, "Shake Pos", new Vector2(-180f, 10f), Pal.Blue, 70f);
            var rot = UT.Box(t, "Shake Rot", new Vector2(0f, 10f), Pal.Orange, 70f);
            var scale = UT.Box(t, "Shake Scale", new Vector2(180f, 10f), Pal.Pink, 70f);
            UI.Caption(t.Stage, "Shake Anchored Position", new Vector2(-180f, -50f));
            UI.Caption(t.Stage, "Shake Rotation (Z only)", new Vector2(0f, -50f));
            UI.Caption(t.Stage, "Shake Scale", new Vector2(180f, -50f));

            t.Player(Anim.New("Shake All",
                ShakePos(UT.Path("Shake Pos"), 12f, 0.5f),
                ShakeRot(UT.Path("Shake Rot"), V3(0f, 0f, 20f), 0.5f).With(),
                ShakeScale(UT.Path("Shake Scale"), V3(0.3f, 0.3f, 0f), 0.6f).With()));

            t.Summary("A random wobble that settles back where it started.")
             .Watch("Shake Pos, Shake Rot, Shake Scale", pos, rot, scale)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Shake All</b>, a few times.")
             .Expect("Each box jitters (sideways and up/down, twisting in the plane of the screen only, bigger and smaller) then settles exactly where it started. The middle box never tips over in 3D.");
        }

        private static void CustomNumbers(LabBoard b)
        {
            var t = b.Test("T16", "Custom Property: numbers", 620f, 300f);

            var gaugeRect = UI.Rect(t.Stage, "Gauge", new Vector2(0f, 70f), new Vector2(440f, 70f));
            var barBg = UI.Square(gaugeRect, "Bar Back", new Vector2(0f, 12f), new Vector2(440f, 26f), Pal.Ghost);
            var bar = UI.Square(gaugeRect, "Bar", new Vector2(0f, 12f), new Vector2(440f, 26f), Pal.Green);
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            var gaugeLabel = UI.Label(gaugeRect, "Readout", "", new Vector2(0f, -22f), new Vector2(440f, 30f), 20f, Pal.White);
            var gauge = gaugeRect.gameObject.AddComponent<TestGauge>();
            gauge.Setup(bar, gaugeLabel);

            var score = UI.Label(t.Stage, "Score", "120", new Vector2(-110f, -60f), new Vector2(220f, 70f), 48f, Pal.Yellow, TextAlignmentOptions.Center, FontStyles.Bold);
            UI.Caption(t.Stage, "Score (as number, N0)", new Vector2(-110f, -105f), 220f);
            var typed = UI.Label(t.Stage, "Typed", "", new Vector2(130f, -60f), new Vector2(260f, 70f), 20f, Pal.White, TextAlignmentOptions.Left);
            UI.Caption(t.Stage, "Typed (typewriter)", new Vector2(130f, -105f), 220f);

            t.Player(
                Anim.New("Heal",
                    Prop(UT.Path("Gauge"), typeof(TestGauge), "Health", UIAnimationPropertyKind.Float).From(10f).To(100f).Dur(1f).Ease(Ease.OutCubic),
                    Prop(UT.Path("Gauge"), typeof(TestGauge), "Coins", UIAnimationPropertyKind.Int).With().From(0f).To(250f).Dur(1f)),
                Anim.New("Count Up",
                    Prop(UT.Path("Score"), typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.NumberText).To(15000f).Format("N0").Dur(1.2f)),
                Anim.New("Type",
                    Prop(UT.Path("Typed"), typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.Text).From("").To("Hello, <b>bold</b> world.").Dur(1.2f).Ease(Ease.Linear)));

            t.Summary("Custom Property on a C# float and int (the gauge's Health and Coins), and on a text counted as a number or typed out.")
             .Watch("Gauge, Score, Typed", gaugeRect, score, typed)
             .Needs(StationNeeds.Preview)
             .HowTo("Play <b>Heal</b>, <b>Count Up</b> and <b>Type</b>. Stop and Restore afterwards. Then open the <b>Property</b> dropdown on Heal's first step.")
             .Expect("Heal fills the bar from 10 to 100 HP while Coins counts in whole numbers to 250. Count Up counts from 120 to 15,000 with a thousands separator. Type writes the line letter by letter, and the bold tag inside it never shows half-typed. Stop and Restore puts back 75 HP, 12 coins, 120 and an empty line. The dropdown lists Health and Coins under TestGauge, but not HealthFraction (read-only), OldHealth ([Obsolete]) or IsAlive (a bool).");
        }

        private static void CustomValues(LabBoard b)
        {
            var t = b.Test("T17", "Custom Property: vectors and colour", 620f, 300f);

            var fields = UI.Box(t.Stage, "Fields", new Vector2(-150f, 20f), new Vector2(110f, 110f), Pal.White);
            UI.Icon(fields.transform, "Up", LabAssets.Chevron, new Vector2(0f, 22f), 40f, Pal.Stage).rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
            var caption = UI.Label(t.Stage, "Fields Caption", "", new Vector2(-150f, -70f), new Vector2(240f, 30f), 18f, Pal.White);
            var fixture = fields.gameObject.AddComponent<TestFields>();
            fixture.Tint = Pal.White;
            fixture.Caption = "fields at rest";
            fixture.Setup(fields, caption);

            var target = UI.Box(t.Stage, "Sized", new Vector2(150f, 20f), new Vector2(80f, 80f), Pal.Teal);
            UI.Caption(t.Stage, "Sized (RectTransform.sizeDelta)", new Vector2(150f, -70f), 240f);

            t.Player(
                Anim.New("Animate Fields",
                    Prop(UT.Path("Fields"), typeof(TestFields), "Angles", UIAnimationPropertyKind.Vector3).To(V3(0f, 0f, 135f)).Dur(0.8f),
                    Prop(UT.Path("Fields"), typeof(TestFields), "Tint", UIAnimationPropertyKind.Color).With().To(Pal.Orange).Dur(0.8f),
                    Prop(UT.Path("Fields"), typeof(TestFields), "Caption", UIAnimationPropertyKind.Text).With().To("fields animated").Dur(0.8f)),
                Anim.New("Grow Sized",
                    Prop(UT.Path("Sized"), typeof(RectTransform), "sizeDelta", UIAnimationPropertyKind.Vector2).To(V3(180f, 120f)).Dur(0.6f).Ease(Ease.OutBack)));

            t.Summary("Custom Property on plain public fields - a Vector3, a Color and a string - and on a Unity property, RectTransform's Vector2 sizeDelta.")
             .Watch("Fields, Sized", fields, target)
             .Needs(StationNeeds.Preview | StationNeeds.Inspector)
             .HowTo("Play <b>Animate Fields</b> and <b>Grow Sized</b>. Then open the Property dropdown on a step of Animate Fields: it lists the members of each component on Fields.")
             .Expect("Fields turns 135 degrees, tints orange and retypes its caption, all together. Sized grows to 180 x 120 with a slight overshoot. The dropdown lists Angles, Tint and Caption under TestFields.");
        }
    }
}
