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
    /// <summary>
    /// 09 Recording: posing objects and recording From / To values from an animation's pop-out window.
    /// One station per kind of value, R1-R12 as they were in the jam. Each animation's Notes say what to do;
    /// the window shows them.
    /// </summary>
    public static class RecordingScene
    {
        private const string RecordSet = UT.Sets + "Record Set.asset";
        private const string MaterialPath = "Assets/UnitTests/Materials/Record Fill.mat";

        private const string Common =
            "Select this tile, press <b>Pop Out</b> on an animation, and follow its Notes, shown in the window. Record with the red dot at the end of a From or To row, pose, then Finish.";

        [MenuItem(UT.Menu + "09 Recording")]
        public static void Build()
        {
            var b = UT.Begin("09 Recording", "09  Recording",
                "Recording values from a pose: pop an animation out, press record on a From or To row, pose the object in the Scene view or its Inspector, press Finish. " +
                "Each tile covers one kind of value; each animation's Notes (shown in its window) say exactly what to do and what gets stored. " +
                "After Finish, Play the animation in the window: it should land exactly on the pose.");

            Move(b);
            SizeScaleTurn(b);
            Fades(b);
            MaterialValues(b);
            CsProperties(b);
            Fields(b);
            CounterVector2(b);
            SeveralAtOnce(b);
            SharedSet(b);
            WindowAnywhere(b);
            Snap(b);
            UndoCancel(b);

            b.Save();
        }

        private static Tile Station(LabBoard b, string code, string title, string summary, string expect, StationNeeds extra = StationNeeds.None)
        {
            return b.Test(code, title, 620f, 260f)
                .Summary(summary)
                .Needs(StationNeeds.PopOut | StationNeeds.SceneView | extra)
                .HowTo(Common)
                .Expect(expect);
        }

        private static void Move(LabBoard b)
        {
            var t = Station(b, "R1", "Record: move",
                "Position in each mode, and a From. The Ball rests at (-60, 0).",
                "Each mode stores what its Notes say, and every animation lands exactly where the Ball was dropped.");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-60f, 0f), 44f, Pal.Yellow);
            t.Watch("Ball", ball);

            t.Player(
                Anim.New("To Absolute", Pos(UT.Path("Ball")).To(V3(120f, 40f)).Dur(0.5f))
                    .Note("Pop Out, press record on step 0's To, drag the Ball with the Move tool, press Finish. Play: it lands exactly where you dropped it. Absolute stores the position itself."),
                Anim.New("To Baseline", Pos(UT.Path("Ball")).ToBase(V3(100f, 0f)).Dur(0.5f))
                    .Note("Baseline stores the distance from where the Ball rests, (-60, 0). Record To, drop the Ball 30 to the right of where it sits: To reads (30, 0). Play: it lands where you dropped it."),
                Anim.New("To Current",
                        Pos(UT.Path("Ball")).To(V3(-20f, 30f)).Dur(0.4f),
                        Pos(UT.Path("Ball")).ToCur(V3(60f, 0f)).Dur(0.4f))
                    .Note("Step 1 moves on from wherever step 0 leaves the Ball, (-20, 30). Record step 1's To and drop the Ball anywhere: it is stored as the distance from (-20, 30). Recording starts where things stand - press Reset or Play first if you want to pose from the animation's own frames. Play: it ends where you dropped it."),
                Anim.New("From Baseline", Pos(UT.Path("Ball")).FromBase(V3(-150f, 0f)).ToRest().Dur(0.5f))
                    .Note("Record the From (the row with the FROM button) and drag the Ball to where the animation should start. Reset shows it there; Play slides it back home."));
        }

        private static void SizeScaleTurn(LabBoard b)
        {
            var t = Station(b, "R2", "Record: size, scale, turn",
                "Size Delta, Scale, Rotation and Offset Max. The Block rests at 40 x 30.",
                "Each stores what its Notes say. The turn is stored the short way round (-40, not 320).");
            var block = UI.Square(t.Stage, "Block", new Vector2(0f, 0f), new Vector2(40f, 30f), Pal.Teal);
            t.Watch("Block", block);

            t.Player(
                Anim.New("Size Baseline", Size(UT.Path("Block")).ToBase(V3(60f, 40f)).Dur(0.4f))
                    .Note("Record To, then resize the Block with the Rect tool (or type Width and Height): stored as the difference from 40 x 30."),
                Anim.New("Scale Absolute", S.Scale(UT.Path("Block")).To(V3(2f, 2f, 1f)).Dur(0.4f))
                    .Note("Record To, scale the Block with the Scale tool, Finish: stored as the scale itself."),
                Anim.New("Turn Current",
                        Rot(UT.Path("Block")).To(V3(0f, 0f, 30f)).Dur(0.3f),
                        Rot(UT.Path("Block")).ToCur(V3(0f, 0f, 45f)).Dur(0.3f))
                    .Note("Step 1 turns on from step 0's 30 degrees. Record step 1, rotate the Block to -10 (the Inspector shows 350): stored as -40, the short way round."),
                Anim.New("Corner Baseline", OffMax(UT.Path("Block")).ToBase(V3(40f, 30f)).Dur(0.4f))
                    .Note("Offset Max moves the top-right corner. Record To, drag that corner with the Rect tool: stored as how far the corner moved from rest."));
        }

        private static void Fades(LabBoard b)
        {
            var t = Station(b, "R3", "Record: fades and colours",
                "Canvas Group alpha, Graphic alpha and Graphic colour, in Current, Baseline and From.",
                "Each stores what its Notes say. A colour stored as a difference can have negative parts; hover it in the Recording box for the numbers.", StationNeeds.Inspector);
            var box = UI.Box(t.Stage, "Box", new Vector2(-120f, 0f), new Vector2(120f, 120f), Pal.Purple);
            UI.Group(box);
            var image = UI.Box(t.Stage, "Image", new Vector2(120f, 0f), new Vector2(120f, 120f), Color.white);
            t.Watch("Box (Canvas Group), Image", box, image);

            t.Player(
                Anim.New("Group alpha Current",
                        Alpha(UT.Path("Box")).To(0.5f).Dur(0.3f),
                        Alpha(UT.Path("Box")).ToCur(0.2f).Dur(0.3f))
                    .Note("Step 1 fades on from step 0's 0.5. Record step 1's To, then set the Box's Canvas Group Alpha to 0.8 in its Inspector: stored as +0.3. Play: ends at 0.8."),
                Anim.New("Image alpha Baseline", GAlpha(UT.Path("Image")).ToBase(-0.3f).Dur(0.3f))
                    .Note("Record To, set the Image colour's alpha to 0.4: stored as -0.6, the difference from its resting 1."),
                Anim.New("Colour Current",
                        Tint(UT.Path("Image")).To(Pal.Red).Dur(0.3f),
                        Tint(UT.Path("Image")).ToCur(new Color(0f, 0.3f, 0f, 0f)).Dur(0.3f))
                    .Note("Step 1 changes colour on from step 0's red. Record step 1, pick a colour for the Image: stored as the difference from red. Play: ends on your colour."),
                Anim.New("Colour From", Tint(UT.Path("Image")).From(Pal.Blue).ToRest().Dur(0.5f))
                    .Note("Record the From, pick a colour for the Image: Reset shows it, Play fades from it back to white."));
        }

        private static void MaterialValues(LabBoard b)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                LabAssets.Folder("Assets/UnitTests/Materials");
                material = new Material(LabAssets.FillMaterial);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.SetColor("_Color", Pal.Green);
            material.SetFloat("_Progress", 1f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);

            var t = Station(b, "R4", "Record: material values",
                "A Material Float (_Progress) and a Material Color (_Color) on the Bar's own material copy.",
                "Each stores what its Notes say. Change the copy, not the asset: if you change Record Fill.mat instead, the Recording box warns.", StationNeeds.Inspector);
            var bar = UI.Square(t.Stage, "Bar", new Vector2(0f, 0f), new Vector2(400f, 50f), Color.white);
            bar.material = material;
            bar.gameObject.AddComponent<UIMaterialInstance>();
            t.Watch("Bar", bar);

            t.Player(
                Anim.New("Fill Current",
                        MatFloat(UT.Path("Bar"), "_Progress").To(0.3f).Dur(0.3f),
                        MatFloat(UT.Path("Bar"), "_Progress").ToCur(0.2f).Dur(0.3f))
                    .Note("Step 1 fills on from step 0's 0.3. Record step 1's To, select the Bar - its material is at the bottom of its Inspector - and drag Progress to 0.9: stored as +0.6. Play: fills to 0.9."),
                Anim.New("Tint Baseline", MatColor(UT.Path("Bar"), "_Color").ToBase(new Color(0f, 0f, 0f, 0f)).Dur(0.3f))
                    .Note("Record To, change the material's Tint: stored as the difference from its resting green."));
        }

        private static void CsProperties(LabBoard b)
        {
            var t = Station(b, "R5", "Record: C# properties",
                "Custom Property on a float property (Health) and an int property (Coins).",
                "Rows say 'read on Finish' instead of a live value; Finish reads them and stores what the Notes say.", StationNeeds.Inspector);
            var gauge = MakeGauge(t);
            t.Watch("Gauge", gauge);

            t.Player(
                Anim.New("Health Current",
                        Prop(UT.Path("Gauge"), typeof(TestGauge), "Health", UIAnimationPropertyKind.Float).To(40f).Dur(0.4f),
                        Prop(UT.Path("Gauge"), typeof(TestGauge), "Health", UIAnimationPropertyKind.Float).ToCur(10f).Dur(0.4f))
                    .Note("Health is a C# property. Step 1 goes on from step 0's 40. Record step 1's To, then set Health to 90 in the Gauge's Inspector: stored as +50. The row says 'read on Finish' - a property's getter is only called when you press Finish."),
                Anim.New("Coins Baseline", Prop(UT.Path("Gauge"), typeof(TestGauge), "Coins", UIAnimationPropertyKind.Int).ToBase(3f).Dur(0.4f))
                    .Note("Coins is an int property. Record To, set Coins to 20 in the Gauge's Inspector: stored as +8, the difference from its resting 12."));
        }

        private static GameObject MakeGauge(Tile t)
        {
            var rect = UI.Rect(t.Stage, "Gauge", Vector2.zero, new Vector2(400f, 70f));
            UI.Square(rect, "Back", new Vector2(0f, 12f), new Vector2(400f, 26f), Pal.Ghost);
            var bar = UI.Square(rect, "Bar", new Vector2(0f, 12f), new Vector2(400f, 26f), Pal.Green);
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            var label = UI.Label(rect, "Readout", "", new Vector2(0f, -22f), new Vector2(400f, 30f), 20f, Pal.White);
            rect.gameObject.AddComponent<TestGauge>().Setup(bar, label);
            return rect.gameObject;
        }

        private static void Fields(LabBoard b)
        {
            var t = Station(b, "R6", "Record: fields",
                "Custom Property on plain public fields: a Vector3, a Color, and typewriter text in Baseline.",
                "Each stores what its Notes say. 'Bye' over a resting 'Hello' can't be stored in Baseline: the row says why and Finish leaves it alone, saying so in the Console.", StationNeeds.Inspector | StationNeeds.Console);
            var fields = UI.Box(t.Stage, "Fields", new Vector2(-100f, 15f), new Vector2(110f, 110f), Color.white);
            UI.Icon(fields.transform, "Up", LabAssets.Chevron, new Vector2(0f, 22f), 40f, Pal.Stage).rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
            var caption = UI.Label(t.Stage, "Caption", "", new Vector2(120f, 15f), new Vector2(260f, 40f), 22f, Pal.White);
            var fixture = fields.gameObject.AddComponent<TestFields>();
            fixture.Tint = Color.white;
            fixture.Caption = "Hello";
            fixture.Setup(fields, caption);
            t.Watch("Fields", fields);

            t.Player(
                Anim.New("Angles Absolute", Prop(UT.Path("Fields"), typeof(TestFields), "Angles", UIAnimationPropertyKind.Vector3).To(V3(0f, 0f, 20f)).Dur(0.4f))
                    .Note("Angles is a public Vector3 field. Record To, type Angles Z 45 in the Fields Inspector, Finish: To is (0, 0, 45)."),
                Anim.New("Tint Current",
                        Prop(UT.Path("Fields"), typeof(TestFields), "Tint", UIAnimationPropertyKind.Color).To(Pal.Yellow).Dur(0.3f),
                        Prop(UT.Path("Fields"), typeof(TestFields), "Tint", UIAnimationPropertyKind.Color).ToCur(new Color(0f, 0f, 0f, 0f)).Dur(0.3f))
                    .Note("Step 1 goes on from step 0's yellow. Record step 1's To, pick a Tint in the Fields Inspector: stored as the difference from yellow."),
                Anim.New("Caption Baseline", Prop(UT.Path("Fields"), typeof(TestFields), "Caption", UIAnimationPropertyKind.Text).ToBase("!").Dur(0.5f).Ease(Ease.Linear))
                    .Note("Typewriter text: Baseline adds text onto the end of the resting caption 'Hello'. Record To, set Caption to 'Hello world': stored as ' world'. Set it to 'Bye' instead and the row shows why it cannot be recorded; Finish then leaves To alone and says so in the Console."));
        }

        private static void CounterVector2(LabBoard b)
        {
            var t = Station(b, "R7", "Record: counter, Vector2",
                "A text counted as a number, and RectTransform.anchoredPosition as a Vector2 Custom Property.",
                "Each stores what its Notes say; Count plays 120, 200, 300.", StationNeeds.Inspector);
            var score = UI.Label(t.Stage, "Score", "120", new Vector2(-120f, 0f), new Vector2(200f, 70f), 48f, Pal.Yellow, TextAlignmentOptions.Center, FontStyles.Bold);
            var dot = UI.Icon(t.Stage, "Dot", LabAssets.Circle, new Vector2(140f, 0f), 40f, Pal.Pink);
            t.Watch("Score, Dot", score, dot);

            t.Player(
                Anim.New("Count Current",
                        Prop(UT.Path("Score"), typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.NumberText).To(200f).Dur(0.5f),
                        Prop(UT.Path("Score"), typeof(TextMeshProUGUI), "text", UIAnimationPropertyKind.NumberText).ToCur(50f).Dur(0.5f))
                    .Note("The Score text counts as a number. Step 1 counts on from step 0's 200. Record step 1's To, then change the Score's text to 300: stored as +100. Play counts 120, 200, 300."),
                Anim.New("Nudge Baseline", Prop(UT.Path("Dot"), typeof(RectTransform), "anchoredPosition", UIAnimationPropertyKind.Vector2).ToBase(V3(0f, 40f)).Dur(0.4f))
                    .Note("A Vector2 member: the Dot's anchoredPosition, driven as a Custom Property. Record To, drag the Dot: stored as how far it moved from rest."));
        }

        private static void SeveralAtOnce(LabBoard b)
        {
            var t = Station(b, "R8", "Record: several at once",
                "Three rows recorded together: position, scale and Canvas Group alpha of one Card.",
                "All three are written on one Finish, and one Ctrl+Z takes them all back, Card included.", StationNeeds.Inspector);
            var card = UI.Box(t.Stage, "Card", new Vector2(0f, 0f), new Vector2(140f, 100f), Pal.Blue);
            UI.Group(card);
            t.Watch("Card", card);

            t.Player(Anim.New("Pop",
                    Pos(UT.Path("Card")).ToBase(V3(0f, 0f)).Dur(0.4f),
                    S.Scale(UT.Path("Card")).With().To(V3(1f, 1f, 1f)).Dur(0.4f),
                    Alpha(UT.Path("Card")).With().To(1f).Dur(0.4f))
                .Note("Record all three To's - each click adds a row to the box at the top of the window. Move and scale the Card, set its Canvas Group Alpha, then Finish once: all three are written, and one Ctrl+Z takes them all back, Card included."));
        }

        private static void SharedSet(LabBoard b)
        {
            var set = Anim.Asset(RecordSet,
                Anim.New("Slide", Pos(null).ToBase(V3(80f, 0f)).Dur(0.5f))
                    .Note("Recorded from the set: the set's own Inspector needs Preview On (this station's Ball) for there to be anything to pose. Record To, drag the Ball, Finish: the set's To changes."));

            var t = Station(b, "R9", "Record: a shared set",
                "Recording into an Animation Set, through Preview On.",
                "Pop Out works from the set's Inspector with Preview On filled; Finish writes into Record Set.asset. The Ball's player shows no Pop Out under Shared Animations (read-only there).", StationNeeds.Inspector);
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-60f, 0f), 44f, Pal.Teal);
            var player = t.PlayerOn(ball.gameObject);
            Anim.SetShared(player, set);
            t.Watch("Ball (its player uses Record Set)", ball);
            t.HowTo("Select Assets/UnitTests/Animation Sets/Record Set.asset. Drag Ball into <b>Preview On</b>. Press Pop Out on Slide there, and follow its Notes.");
        }

        private static void WindowAnywhere(LabBoard b)
        {
            var t = Station(b, "R10", "Window: drawn anywhere",
                "A popped-out animation is drawn by its window, whatever is selected.",
                "The line, dots and path stay drawn and draggable with something else selected; the path editor stays on; a preview from the window keeps playing.");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(-200f, -60f), 36f, Pal.Orange);
            t.Watch("Ball", ball);

            t.Player(Anim.New("Wander",
                    Pos(UT.Path("Ball")).To(V3(200f, 60f)).Dur(1.4f).Ease(Ease.InOutSine).Path(UIAnimationPathShape.Curved, V3(-80f, 80f), V3(60f, -60f)))
                .Note("Pop Out, then select another station: this animation's line, dots and path stay drawn and draggable, because the window draws them. Press Edit Path in Scene on step 0 and select something else: the path editor stays on while the window is open. Play in the window keeps playing whatever you select."));
        }

        private static void Snap(LabBoard b)
        {
            var t = Station(b, "R11", "Snap, close to keep",
                "Recording a snapping step, and closing the window instead of Finish.",
                "The recorded value is whole numbers (halves away from zero), and closing the window kept it, like Finish.");
            var ball = UI.Icon(t.Stage, "Ball", LabAssets.Circle, new Vector2(0f, 0f), 36f, Pal.Green);
            t.Watch("Ball", ball);

            t.Player(Anim.New("Snap", Pos(UT.Path("Ball")).ToBase(V3(60f, 0f)).Dur(0.5f)).SnapAll()
                .Note("Snapping is on, so a recorded value is rounded to whole numbers, halves away from zero. Record To, drag the Ball, then close the window instead of pressing Finish: closing keeps the value, like Finish."));
        }

        private static void UndoCancel(LabBoard b)
        {
            var t = Station(b, "R12", "Undo, Cancel, play mode",
                "How a recording ends: Finish then Undo, Cancel, play mode, and selecting away.",
                "One Ctrl+Z undoes a whole recording and leaves the Box at rest. Cancel and play mode write nothing and put the Box back (play mode also warns in the Console). Selecting away and back: still recording.", StationNeeds.PlayMode | StationNeeds.Console);
            var box = UT.Box(t, "Box", new Vector2(0f, 0f), Pal.Pink, 60f);
            t.Watch("Box", box);

            t.Player(Anim.New("Wiggle",
                    Pos(UT.Path("Box")).ToBase(V3(0f, 30f)).Dur(0.3f),
                    Rot(UT.Path("Box")).ToBase(V3(0f, 0f, 15f)).Dur(0.3f))
                .Note("Record both To's, pose the Box, Finish, then Ctrl+Z once: both values go back. Record again and press Cancel: nothing is written and the Box goes back. Record again and enter play mode: that cancels too. Record again, select another object, then come back: still recording."));
        }
    }
}
