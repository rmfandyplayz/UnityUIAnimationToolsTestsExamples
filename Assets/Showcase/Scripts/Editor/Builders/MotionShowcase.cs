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
    /// <summary>Showcase scenes for Punch and Shake, Sizing a RectTransform, Movement Paths, Stylised Motion, Interrupting and Chaining.</summary>
    public static class MotionShowcase
    {
        private static void Lane(Tile t, string name, float y, float width = 700f)
        {
            UI.Box(t.Stage, name + " Lane", new Vector2(40f, y), new Vector2(width, 4f), Pal.Hex("39406E"));
        }

        [MenuItem(SC.Menu + "Punch and Shake")]
        public static void PunchAndShake()
        {
            const string page = "Punch and Shake";
            var b = SC.Begin("09 Punch and Shake", page, "The four effects from 'Good starting values', and spamming one.");

            var t = SC.Shot(b, page, "Good starting values",
                "Button squish (Punch Scale), 'Wrong!' headshake (Shake Anchored Position), Wobble (Shake Rotation) and Jelly (Shake Scale), with the table's values.",
                "A GIF of the Game view for 'Good starting values'.");
            string[] names = { "Squish", "Wrong!", "Wobble", "Jelly" };
            Color[] colors = { SC.Amber, SC.Red, SC.Sky, SC.Pink };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            for (int i = 0; i < 4; i++)
            {
                float x = -330f + i * 220f;
                var subject = i == 0 ? SC.Pill(t.Stage, names[i], "OK", new Vector2(x, 20f), new Vector2(160f, 64f), colors[i])
                    : i == 1 ? (RectTransform)UI.Icon(t.Stage, names[i], LabAssets.Cross, new Vector2(x, 20f), 110f, colors[i]).transform
                    : i == 2 ? (RectTransform)UI.Icon(t.Stage, names[i], LabAssets.Star, new Vector2(x, 20f), 120f, colors[i]).transform
                    : (RectTransform)UI.Icon(t.Stage, names[i], LabAssets.Heart, new Vector2(x, 20f), 120f, colors[i]).transform;
                SC.Caption(t.Stage, names[i], new Vector2(x, -90f), 200f);
                UIAnimationStep step = i == 0 ? PunchScale(null, V3(-0.1f, -0.1f, 0f), 0.25f)
                    : i == 1 ? ShakePos(null, 10f, 0.4f)
                    : i == 2 ? ShakeRot(null, V3(0f, 0f, 15f), 0.4f)
                    : ShakeScale(null, V3(0.25f, 0.25f, 0f), 0.5f);
                var p = t.PlayerOn(subject.gameObject, Anim.New("Go", step));
                cues.Add((p, "Go", 0.3f + i * 0.45f));
            }
            SC.Cues(t, cues.ToArray()).LoopEvery = 2.6f;

            var t2 = SC.Shot(b, page, "Spamming is safe",
                "A button squished 20 times in a second and a half. However it's cut short, it ends exactly at its normal size.",
                "A GIF of the Game view for 'Spamming is safe'.");
            var button = SC.Pill(t2.Stage, "Button", "Mash me", new Vector2(0f, 30f), new Vector2(260f, 90f), SC.Mint);
            var readout = UI.Label(t2.Stage, "Readout", "", new Vector2(0f, -80f), new Vector2(400f, 40f), 24f, SC.Muted);
            var live = readout.gameObject.AddComponent<LiveReadout>();
            live.Text = readout;
            live.Target = button;
            live.Show = LiveReadout.Value.Scale;
            live.Prefix = "scale ";
            var p2 = t2.PlayerOn(button.gameObject, Anim.New("Squish", PunchScale(null, V3(-0.15f, -0.15f, 0f), 0.3f)));
            var spam = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            for (int i = 0; i < 20; i++) spam.Add((p2, "Squish", 0.2f + i * 0.07f));
            SC.Cues(t2, spam.ToArray()).LoopEvery = 3f;

            b.Save();
        }

        [MenuItem(SC.Menu + "Sizing a RectTransform")]
        public static void Sizing()
        {
            const string page = "Sizing a RectTransform";
            var b = SC.Begin("10 Sizing a RectTransform", page, "The four steps that move or resize a UI element, and the fight between whole-box and edge steps.");

            var t = SC.Shot(b, page, "Four ways to change a rect",
                "Anchored Position slides the box; Size Delta grows it around its pivot; Offset Min pulls its left and bottom edges; Offset Max its right and top.",
                "A GIF of the Game view for the table at the top of the page.");
            string[] names = { "Anchored Position", "Size Delta", "Offset Min", "Offset Max" };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            for (int i = 0; i < 4; i++)
            {
                float x = -345f + i * 230f;
                var cell = UI.Box(t.Stage, "Cell " + (i + 1), new Vector2(x, 20f), new Vector2(200f, 200f), SC.Panel);
                SC.Caption(t.Stage, names[i], new Vector2(x, -110f), 220f).fontSize = 18f;
                var box = UI.Square(cell.transform, "Box", Vector2.zero, new Vector2(100f, 100f), i == 0 ? SC.Sky : i == 1 ? SC.Mint : i == 2 ? SC.Pink : SC.Amber);
                UIAnimation go;
                switch (i)
                {
                    case 0: go = Anim.New("Go", Pos("Box").ToBase(V3(40f, 40f)).Dur(0.5f), Pos("Box").ToRest().Dur(0.5f).Delay(0.4f)); break;
                    case 1: go = Anim.New("Go", Size("Box").ToBase(V3(60f, 60f)).Dur(0.5f), Size("Box").ToRest().Dur(0.5f).Delay(0.4f)); break;
                    case 2: go = Anim.New("Go", OffMin("Box").ToBase(V3(-40f, -40f)).Dur(0.5f), OffMin("Box").ToRest().Dur(0.5f).Delay(0.4f)); break;
                    default: go = Anim.New("Go", OffMax("Box").ToBase(V3(40f, 40f)).Dur(0.5f), OffMax("Box").ToRest().Dur(0.5f).Delay(0.4f)); break;
                }
                var p = t.PlayerOn(cell.gameObject, go);
                cues.Add((p, "Go", 0.3f));
            }
            SC.Cues(t, cues.ToArray()).LoopEvery = 2.4f;

            var t2 = SC.Shot(b, page, "Whole-box and edge steps fight",
                "A panel slid 200 right with Anchored Position while an Offset Min step holds its left edge: it stretches instead of moving.",
                "A GIF of the Game view for 'Don't mix whole-box and edge steps'.");
            var ghost = UI.Box(t2.Stage, "Where It Should Go", new Vector2(100f, 20f), new Vector2(300f, 200f), new Color(1f, 1f, 1f, 0.08f));
            var panel = SC.CardAt(t2.Stage, "Panel", new Vector2(-100f, 20f), new Vector2(300f, 200f), SC.Lilac, false);
            SC.Caption(t2.Stage, "dashed: where a plain slide would land", new Vector2(100f, -110f), 400f);
            var p2 = t2.PlayerOn(panel.gameObject,
                Anim.New("Slide And Hold", Pos(null).ToBase(V3(200f, 0f)).Dur(0.8f), OffMin(null).With().ToRest().Dur(0.8f)),
                Anim.New("Back", Pos(null).ToRest().Dur(0.01f), OffMin(null).With().ToRest().Dur(0.01f)));
            SC.Cues(t2, (p2, "Slide And Hold", 0.4f)).LoopEvery = 2.4f;

            b.Save();
        }

        [MenuItem(SC.Menu + "Movement Paths")]
        public static void MovementPaths()
        {
            const string page = "Movement Paths";
            var b = SC.Begin("11 Movement Paths", page, "Curved and Linear paths, a reusable Current-mode hop, and the path editor.");

            var t = SC.Shot(b, page, "A card flying into a hand",
                "Three cards arc from a deck into a fanned hand along Curved paths, staggered.",
                "A GIF of the Game view for the top of the page.");
            var deck = SC.CardAt(t.Stage, "Deck", new Vector2(-340f, 150f), new Vector2(100f, 140f), SC.Panel);
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            for (int i = 0; i < 3; i++)
            {
                var card = SC.CardAt(t.Stage, "Card " + (i + 1), new Vector2(-340f, 150f), new Vector2(100f, 140f), SC.Card);
                UI.Icon(card, "Suit", i == 1 ? LabAssets.Heart : LabAssets.Diamond, Vector2.zero, 50f, i == 1 ? SC.Red : SC.Sky);
                var end = new Vector3(-110f + i * 110f, -140f, 0f);
                var p = t.PlayerOn(card.gameObject, Anim.New("Deal",
                    Pos(null).FromBase(Vector3.zero).To(end).Dur(0.7f).Ease(Ease.OutCubic).Path(UIAnimationPathShape.Curved, new Vector3(-60f + i * 60f, 200f, 0f)),
                    Rot(null).With().FromBase(Vector3.zero).To(V3(0f, 0f, 12f - i * 12f)).Dur(0.7f).Ease(Ease.OutCubic)));
                cues.Add((p, "Deal", 0.3f + i * 0.2f));
            }
            SC.Cues(t, cues.ToArray()).LoopEvery = 2.4f;

            var t2 = SC.Shot(b, page, "Curved and Linear",
                "The same four points, Curved (top) and Linear (bottom).",
                "A GIF of the Game view for the Custom Path table.");
            Vector3[] points = { V3(-160f, 60f), V3(-40f, -60f), V3(80f, 60f), V3(200f, -60f) };
            var curvedDot = SC.Dot(t2.Stage, "Curved", new Vector2(-330f, 120f), 40f, SC.Pink);
            var linearDot = SC.Dot(t2.Stage, "Linear", new Vector2(-330f, -130f), 40f, SC.Mint);
            SC.Caption(t2.Stage, "Curved", new Vector2(-380f, 200f), 160f);
            SC.Caption(t2.Stage, "Linear", new Vector2(-380f, -50f), 160f);
            var shifted = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++) shifted[i] = points[i] + V3(0f, 120f);
            var pc = t2.PlayerOn(curvedDot.gameObject, Anim.New("Go", Pos(null).FromBase(Vector3.zero).To(V3(330f, 120f)).Dur(1.6f).Ease(Ease.InOutSine).Path(UIAnimationPathShape.Curved, shifted)));
            for (int i = 0; i < points.Length; i++) shifted[i] = points[i] + V3(0f, -130f);
            var pl = t2.PlayerOn(linearDot.gameObject, Anim.New("Go", Pos(null).FromBase(Vector3.zero).To(V3(330f, -130f)).Dur(1.6f).Ease(Ease.InOutSine).Path(UIAnimationPathShape.Linear, shifted)));
            SC.Cues(t2, (pc, "Go", 0.3f), (pl, "Go", 0.3f)).LoopEvery = 2.6f;

            var t3 = SC.Shot(b, page, "A hop that works from anywhere",
                "One animation with To on Current and one point: the same hop, played four times, carries the frog across.",
                "A GIF of the Game view for 'Points use To's mode'.");
            var frog = UI.Icon(t3.Stage, "Frog", LabAssets.Circle, new Vector2(-330f, -120f), 60f, SC.Mint);
            Lane(t3, "Ground", -152f);
            var hop = t3.PlayerOn(frog.gameObject, Anim.New("Hop", Pos(null).ToCur(V3(160f, 0f)).Dur(0.45f).Ease(Ease.Linear).Path(UIAnimationPathShape.Curved, V3(80f, 140f))));
            SC.Cues(t3, (hop, "Hop", 0.3f), (hop, "Hop", 0.85f), (hop, "Hop", 1.4f), (hop, "Hop", 1.95f)).LoopEvery = 3f;

            var t4 = SC.Shot(b, page, "Editing a path in the Scene view",
                "A swoop with two points, for a Scene view screenshot of Edit Path in Scene.",
                "A screenshot of the Scene view while editing (select the frame, open Swoop's step, press Edit Path in Scene, hover a point) for 'Editing a path in the Scene view'.");
            var ball = SC.Dot(t4.Stage, "Ball", new Vector2(-300f, -120f), 50f, SC.Amber);
            t4.PlayerOn(ball.gameObject, Anim.New("Swoop", Pos(null).To(V3(300f, 120f)).Dur(1.2f).Ease(Ease.InOutSine).Path(UIAnimationPathShape.Curved, V3(-100f, 160f), V3(120f, -140f))));
            t4.Station.Needs |= StationNeeds.SceneView;
            t4.Watch("Ball (its player)", ball);

            b.Save();
        }

        [MenuItem(SC.Menu + "Stylised Motion")]
        public static void StylisedMotion()
        {
            const string page = "Stylised Motion";
            var b = SC.Begin("15 Stylised Motion", page, "Play At Custom FPS, and Snapping.");

            var t = SC.Shot(b, page, "Play At Custom FPS",
                "The same spin and slide, smooth, at 12 FPS and at 6 FPS.",
                "A GIF of the Game view for 'Play At Custom FPS'. Record at 30 FPS or more, or the smooth one stutters too.");
            string[] labels = { "smooth", "12 FPS", "6 FPS" };
            float[] fps = { 0f, 12f, 6f };
            var cues = new System.Collections.Generic.List<(UIAnimationPlayer, string, float)>();
            for (int i = 0; i < 3; i++)
            {
                float y = 140f - i * 140f;
                SC.Caption(t.Stage, labels[i], new Vector2(-380f, y), 140f);
                Lane(t, labels[i], y);
                var star = UI.Icon(t.Stage, labels[i], LabAssets.Star, new Vector2(-280f, y), 70f, i == 0 ? SC.Sky : i == 1 ? SC.Amber : SC.Pink);
                var go = Anim.New("Go",
                    Pos(null).FromBase(Vector3.zero).ToBase(V3(600f, 0f)).Dur(1.5f).Ease(Ease.InOutSine),
                    Rot(null).With().FromBase(Vector3.zero).ToBase(V3(0f, 0f, -360f)).Dur(1.5f).Ease(Ease.InOutSine));
                if (fps[i] > 0f) go.Fps(fps[i]);
                cues.Add((t.PlayerOn(star.gameObject, go), "Go", 0.3f));
            }
            SC.Cues(t, cues.ToArray()).LoopEvery = 2.6f;

            var t2 = SC.Shot(b, page, "Snapping",
                "Two pixel-art invaders creep 6 pixels right over 1.5 s inside an 8x zoomed parent: off glides, ALL STEPS steps a whole pixel at a time.",
                "A GIF of the Game view for 'Snapping'.");
            var zoom = UI.Rect(t2.Stage, "Zoom x8", Vector2.zero, new Vector2(100f, 60f));
            zoom.localScale = new Vector3(8f, 8f, 1f);
            var smooth = UI.Icon(zoom, "Off", LabAssets.Pixel, new Vector2(-10f, 12f), 12f, SC.Mint);
            var snapped = UI.Icon(zoom, "All Steps", LabAssets.Pixel, new Vector2(-10f, -12f), 12f, SC.Amber);
            SC.Caption(t2.Stage, "Snapping off", new Vector2(-330f, 96f), 200f);
            SC.Caption(t2.Stage, "ALL STEPS", new Vector2(-330f, -96f), 200f);
            var ps = t2.PlayerOn(smooth.gameObject, Anim.New("Creep", Pos(null).FromBase(Vector3.zero).ToBase(V3(6f, 0f)).Dur(1.5f).Ease(Ease.Linear)));
            var pp = t2.PlayerOn(snapped.gameObject, Anim.New("Creep", Pos(null).FromBase(Vector3.zero).ToBase(V3(6f, 0f)).Dur(1.5f).Ease(Ease.Linear)).SnapAll());
            SC.Cues(t2, (ps, "Creep", 0.3f), (pp, "Creep", 0.3f)).LoopEvery = 2.4f;

            b.Save();
        }

        [MenuItem(SC.Menu + "Interrupting and Chaining")]
        public static void Interrupting()
        {
            const string page = "Interrupting and Chaining";
            var b = SC.Begin("16 Interrupting and Chaining", page, "Interrupt Others' COMPLETE and STOP, layering an animation on top, and chaining with On Complete.");

            var t = SC.Shot(b, page, "COMPLETE vs STOP",
                "Two cards are hovered and left half way through growing. COMPLETE (left) jumps to full size first, then shrinks; STOP (right) shrinks from where it got to.",
                "A GIF of the Game view for 'How the others end'.");
            var complete = HoverCard(t, "COMPLETE", new Vector2(-200f, 20f), SC.Amber, true, out var pc);
            var stop = HoverCard(t, "STOP", new Vector2(200f, 20f), SC.Mint, false, out var pst);
            SC.Cues(t, (pc, "Hover", 0.3f), (pst, "Hover", 0.3f), (pc, "Unhover", 0.55f), (pst, "Unhover", 0.55f)).LoopEvery = 2f;

            var t2 = SC.Shot(b, page, "Playing on top: Interrupt Others unticked",
                "A bell rings with a looping pulse (Interrupt Others unticked) while its other animations come and go.",
                "A GIF of the Game view for 'Untick it'.");
            var bell = UI.Icon(t2.Stage, "Bell", LabAssets.Star, new Vector2(0f, 20f), 160f, SC.Amber);
            var pb = t2.PlayerOn(bell.gameObject,
                Anim.New("Pulse", S.Scale(null).FromBase(Vector3.zero).ToBase(V3(0.12f, 0.12f, 0f)).Dur(0.4f).Ease(Ease.InOutSine)).Loop(-1, LoopType.Yoyo).NoInterrupt(),
                Anim.New("Ring", ShakeRot(null, V3(0f, 0f, 25f), 0.6f)).NoInterrupt(),
                Anim.New("Tint", Tint(null).To(SC.Pink).Dur(0.3f), Tint(null).ToRest().Dur(0.3f).Delay(0.2f)).NoInterrupt());
            SC.Cues(t2, (pb, "Pulse", 0.1f), (pb, "Ring", 0.8f), (pb, "Tint", 1.6f), (pb, "Ring", 2.4f)).LoopEvery = 3.2f;

            var t3 = SC.Shot(b, page, "Chaining with On Complete",
                "Three steps of a tutorial light up in turn: each animation's On Complete plays the next.",
                "A GIF of the Game view for 'Chaining animations', and a screenshot of One's On Complete in the Inspector.");
            var holder = UI.Rect(t3.Stage, "Chain", Vector2.zero, new Vector2(10f, 10f));
            var chain = t3.PlayerOn(holder.gameObject);
            string[] names = { "One", "Two", "Three" };
            var animations = new UIAnimation[3];
            for (int i = 0; i < 3; i++)
            {
                var node = SC.CardAt(t3.Stage, names[i], new Vector2(-260f + i * 260f, 20f), new Vector2(180f, 180f), SC.Panel);
                SC.Title(node, (i + 1).ToString(), Vector2.zero, 64f, SC.Muted, 160f);
                animations[i] = Anim.New(names[i],
                    Tint("../" + names[i] + "/Body").To(SC.Sky).Dur(0.25f),
                    PunchScale("../" + names[i], V3(0.15f, 0.15f, 0f), 0.4f).With());
            }
            animations[0].Then(chain, "Two");
            animations[1].Then(chain, "Three");
            Anim.Set(chain, animations);
            SC.Cues(t3, (chain, "One", 0.4f)).LoopEvery = 2.4f;
            t3.Watch("Chain (its player)", holder.gameObject);

            b.Save();
        }

        private static RectTransform HoverCard(Tile t, string name, Vector2 at, Color color, bool complete, out UIAnimationPlayer player)
        {
            var card = SC.CardAt(t.Stage, name, at, new Vector2(220f, 220f), color);
            SC.Caption(t.Stage, name, at + new Vector2(0f, -170f), 220f);
            var hover = Anim.New("Hover", S.Scale(null).ToBase(V3(0.3f, 0.3f, 0f)).Dur(0.6f).Ease(Ease.Linear));
            var unhover = Anim.New("Unhover", S.Scale(null).ToRest().Dur(0.6f).Ease(Ease.Linear));
            if (complete) { hover.InterruptComplete(); unhover.InterruptComplete(); }
            else { hover.InterruptStop(); unhover.InterruptStop(); }
            player = t.PlayerOn(card.gameObject, hover, unhover);

            var body = card.Find("Body");
            body.GetComponent<Image>().raycastTarget = true;
            UI.Trigger(body, EventTriggerType.PointerEnter, player, "Hover");
            UI.Trigger(body, EventTriggerType.PointerExit, player, "Unhover");
            return card;
        }
    }
}
