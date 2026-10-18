using rmf_claude.DOTweenUI;
using TMPro;
using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnimLab.Showcase.Builders
{
    /// <summary>
    /// Shared bits of the showcase scene builders: one scene per wiki page, one 960 x 540 frame per picture
    /// or GIF, all in one visual style (an indigo backdrop, white cards, a soft pastel palette).
    /// </summary>
    public static class SC
    {
        public const string Scenes = "Assets/Showcase/Scenes/";
        public const string Sets = "Assets/Showcase/Animation Sets/";
        public const string Menu = "Tools/UI Anim Lab/Build Showcase/";

        public static readonly Color Bg = Pal.FrameBg;
        public static readonly Color Panel = Pal.FramePanel;
        public static readonly Color Card = Pal.FrameCard;
        public static readonly Color Ink = Pal.FrameInk;
        public static readonly Color Text = Pal.FrameText;
        public static readonly Color Muted = Pal.FrameMuted;
        public static readonly Color Pink = Pal.FramePink;
        public static readonly Color Amber = Pal.FrameAmber;
        public static readonly Color Mint = Pal.FrameMint;
        public static readonly Color Red = Pal.FrameRed;
        public static readonly Color Sky = Pal.Hex("8BD3FF");
        public static readonly Color Lilac = Pal.Hex("C3B1F7");

        public const string Keys = "Focus this frame (Tab, or its number key), then <b>Space</b> plays it, <b>R</b> puts it back, <b>L</b> loops it.";

        public static LabBoard Begin(string scene, string page, string intro)
        {
            var board = LabBoard.Begin(Scenes + scene + ".unity", true, 2);
            board.Readme(page,
                "Demonstrations for the wiki page <b>" + page + "</b>. " + intro +
                "\n\nEach frame is 960 x 540 (16:9). In play mode the board starts focused on frame 1: Tab moves to the next, number keys jump, Home shows them all, H shows the index. " +
                "With a frame focused, <b>Space</b> plays its demonstration, <b>R</b> puts it back and <b>L</b> loops it - set the Game view to 16:9 and the frame fills it exactly. " +
                "\n\nSelect a frame to see in its Inspector what to capture and where it goes on the page. Assets/Showcase/SHOTLIST.md lists every shot.");
            return board;
        }

        public static Tile Shot(LabBoard board, string page, string title, string shows, string capture, string setup = null, string result = null)
        {
            var tile = board.Shot(page, title);
            tile.Summary(shows).Capture(capture);
            if (setup != null) tile.HowTo(setup);
            if (result != null) tile.Expect(result);
            return tile;
        }

        /// <summary>Wires Space in play mode to these cues: (player, animation, seconds after Space).</summary>
        public static ShotDirector Cues(Tile tile, params (UIAnimationPlayer player, string animation, float at)[] cues)
        {
            var director = tile.Go.GetComponent<ShotDirector>();
            if (director == null) director = tile.Go.AddComponent<ShotDirector>();
            director.Station = tile.Station;
            foreach (var (player, animation, at) in cues)
            {
                director.Cues.Add(new ShotDirector.Cue { Player = player, Animation = animation, At = at });
            }

            tile.Station.Needs |= StationNeeds.PlayMode;
            return director;
        }

        /// <summary>A card: a container (what animations move) holding a soft shadow and a white body.</summary>
        public static RectTransform CardAt(Transform parent, string name, Vector2 position, Vector2 size, Color? color = null, bool shadow = true)
        {
            var card = UI.Rect(parent, name, position, size);
            if (shadow)
            {
                var s = UI.Box(card, "Shadow", new Vector2(0f, -8f), size + new Vector2(10f, 10f), new Color(0f, 0f, 0.05f, 0.35f));
                s.pixelsPerUnitMultiplier = 1.4f;
            }

            var body = UI.Box(card, "Body", Vector2.zero, size, color ?? Card);
            body.pixelsPerUnitMultiplier = 2f;
            return card;
        }

        public static TextMeshProUGUI Title(Transform parent, string text, Vector2 position, float size = 34f, Color? color = null, float width = 600f)
        {
            return UI.Label(parent, "Title " + text, text, position, new Vector2(width, size * 1.6f), size, color ?? Text, TextAlignmentOptions.Center, FontStyles.Bold);
        }

        public static TextMeshProUGUI Body(Transform parent, string name, string text, Vector2 position, float size = 22f, Color? color = null, float width = 600f)
        {
            return UI.Label(parent, name, text, position, new Vector2(width, size * 1.8f), size, color ?? Muted);
        }

        /// <summary>A pill-shaped button: a container with a body and a label, so the whole thing can be animated.</summary>
        public static RectTransform Pill(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color, Color? ink = null)
        {
            var pill = UI.Rect(parent, name, position, size);
            var body = UI.Box(pill, "Body", Vector2.zero, size, color);
            body.pixelsPerUnitMultiplier = 64f / size.y;
            body.raycastTarget = true;
            UI.Label(pill, "Label", label, Vector2.zero, size, size.y * 0.42f, ink ?? Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            return pill;
        }

        public static Image Dot(Transform parent, string name, Vector2 position, float size, Color color)
        {
            return UI.Icon(parent, name, LabAssets.Circle, position, size, color);
        }

        /// <summary>A small grey caption under something in a frame, for frames that compare side by side.</summary>
        public static TextMeshProUGUI Caption(Transform parent, string text, Vector2 position, float width = 240f)
        {
            return UI.Label(parent, "Caption " + text, text, position, new Vector2(width, 30f), 20f, Muted);
        }
    }
}
