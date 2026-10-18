using System;
using TMPro;
using UnityEngine;

namespace UIAnimLab
{
    /// <summary>What a station needs from you to be tested. Drawn as badges on the tile.</summary>
    [Flags]
    public enum StationNeeds
    {
        None = 0,
        Preview = 1,
        PlayMode = 2,
        Inspector = 4,
        SceneView = 8,
        Console = 16,
        PopOut = 32,
    }

    public enum StationStyle
    {
        /// <summary>A hand-test tile: header, stage, how-to and expected result, all on the tile.</summary>
        Test = 0,

        /// <summary>A capture frame for the wiki. Only a caption above it; the directions live in the Inspector.</summary>
        Showcase = 1,

        /// <summary>The scene's own introduction tile.</summary>
        Readme = 2,
    }

    /// <summary>
    /// One tile on a test or showcase board. Holds the text that explains what the tile is for, and lays
    /// the tile out from it: header, stage (where the animated objects live), and footer.
    ///
    /// Select a station and its Inspector shows the same text as a help box, with buttons that select the
    /// objects to watch. Its UI Animation Player, when it has one, sits on the same object, right below.
    /// </summary>
    [DisallowMultipleComponent]
    public class Station : MonoBehaviour
    {
        [Tooltip("Short id shown on the tile and in the index, e.g. S3.")]
        public string Code;

        public string Title;

        [Tooltip("One or two lines: what is being tested.")]
        [TextArea(2, 5)] public string Summary;

        [Tooltip("The objects to keep an eye on.")]
        [TextArea(1, 4)] public string Watch;

        [Tooltip("Shown as Select buttons in the Inspector.")]
        public GameObject[] WatchObjects = new GameObject[0];

        [Tooltip("How to run the test, step by step.")]
        [TextArea(3, 14)] public string HowTo;

        [Tooltip("What should happen. Drawn in yellow.")]
        [TextArea(1, 8)] public string Expect;

        public StationNeeds Needs;
        public StationStyle Style;

        [Tooltip("Showcase only: the wiki page this frame is for.")]
        public string WikiPage;

        [Tooltip("Showcase only: what to capture, and where it goes on the page.")]
        [TextArea(2, 8)] public string Capture;

        [Header("Layout")]
        public float Width = 620f;
        public float StageHeight = 260f;

        [Header("Parts (made by the builder)")]
        public RectTransform Header;
        public RectTransform Stage;
        public RectTransform Footer;
        public TMP_Text TitleText;
        public TMP_Text BadgeText;
        public TMP_Text SummaryText;
        public TMP_Text WatchText;
        public TMP_Text HowToText;
        public TMP_Text ExpectText;

        public const float Pad = 18f;
        public const float Gap = 8f;

        /// <summary>The part of the tile to frame when focusing it: the whole tile, or a showcase's frame.</summary>
        public RectTransform FocusRect
        {
            get { return Style == StationStyle.Showcase && Stage != null ? Stage : (RectTransform)transform; }
        }

        public static string BadgesOf(StationNeeds needs)
        {
            if (needs == StationNeeds.None) return string.Empty;

            var parts = new System.Collections.Generic.List<string>();
            if ((needs & StationNeeds.Preview) != 0) parts.Add("PREVIEW");
            if ((needs & StationNeeds.PlayMode) != 0) parts.Add("PLAY MODE");
            if ((needs & StationNeeds.Inspector) != 0) parts.Add("INSPECTOR");
            if ((needs & StationNeeds.PopOut) != 0) parts.Add("POP-OUT");
            if ((needs & StationNeeds.SceneView) != 0) parts.Add("SCENE VIEW");
            if ((needs & StationNeeds.Console) != 0) parts.Add("CONSOLE");
            return string.Join("  ·  ", parts);
        }

        /// <summary>Writes the text into the tile and stacks header, stage and footer from it.</summary>
        public void Refresh()
        {
            var rect = (RectTransform)transform;
            float inner = Width - Pad * 2f;

            SetText(TitleText, Style == StationStyle.Showcase
                ? "<color=#8A93A6>" + WikiPage + "  ·</color>  " + Title
                : (string.IsNullOrEmpty(Code) ? Title : "<color=#7FD1FF>" + Code + "</color>   " + Title));
            SetText(BadgeText, BadgesOf(Needs));
            SetText(SummaryText, Summary);
            SetText(WatchText, string.IsNullOrEmpty(Watch) ? string.Empty : "<b>Watch:</b> " + Watch);
            SetText(HowToText, HowTo);
            SetText(ExpectText, string.IsNullOrEmpty(Expect) ? string.Empty : "<b>Expect:</b> " + Expect);

            // Header: title row, then summary and watch lines, each as tall as its text.
            float y = Style == StationStyle.Showcase ? 0f : Pad;
            float titleWidth = inner - (BadgeText != null && !string.IsNullOrEmpty(BadgeText.text) ? Preferred(BadgeText, 9999f).x + Gap : 0f);
            y = Place(TitleText, y, titleWidth, 0f);
            if (BadgeText != null)
            {
                Rect(BadgeText).anchoredPosition = new Vector2(-Pad, -(Style == StationStyle.Showcase ? 0f : Pad) - 4f);
                Rect(BadgeText).sizeDelta = new Vector2(Preferred(BadgeText, 9999f).x + 2f, 24f);
            }
            y = Place(SummaryText, y, inner, Gap * 0.5f);
            y = Place(WatchText, y, inner, Gap * 0.5f);
            y += Style == StationStyle.Showcase ? Gap : Pad * 0.75f;

            if (Header != null)
            {
                Header.anchoredPosition = Vector2.zero;
                Header.sizeDelta = new Vector2(0f, y);
            }

            if (Stage != null)
            {
                float side = Style == StationStyle.Showcase ? 0f : Pad;
                Stage.anchorMin = Stage.anchorMax = new Vector2(0.5f, 1f);
                Stage.pivot = new Vector2(0.5f, 1f);
                Stage.anchoredPosition = new Vector2(0f, -y);
                Stage.sizeDelta = new Vector2(Width - side * 2f, StageHeight);
                y += StageHeight;
            }

            // Footer: how-to, then the expected result.
            float footerTop = y;
            float f = 0f;
            bool anyFooter = !string.IsNullOrEmpty(HowTo) || !string.IsNullOrEmpty(Expect);
            if (anyFooter && Style != StationStyle.Showcase)
            {
                f = Pad * 0.75f;
                f = Place(HowToText, f, inner, 0f);
                f = Place(ExpectText, f, inner, string.IsNullOrEmpty(HowTo) ? 0f : Gap);
                f += Pad;
            }
            else
            {
                Hide(HowToText);
                Hide(ExpectText);
            }

            if (Footer != null)
            {
                Footer.anchoredPosition = new Vector2(0f, -footerTop);
                Footer.sizeDelta = new Vector2(0f, f);
            }

            y += f;
            if (Style == StationStyle.Showcase) y += 0f;
            else if (!anyFooter) y += Pad;

            rect.sizeDelta = new Vector2(Width, y);
        }

        private static RectTransform Rect(TMP_Text text)
        {
            return (RectTransform)text.transform;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text == null) return;
            text.text = value ?? string.Empty;
            text.gameObject.SetActive(!string.IsNullOrEmpty(value));
        }

        private static void Hide(TMP_Text text)
        {
            if (text != null) text.gameObject.SetActive(false);
        }

        private static Vector2 Preferred(TMP_Text text, float width)
        {
            return text.GetPreferredValues(text.text, width, 0f);
        }

        /// <summary>Puts a top-anchored text at y (measured down from its parent's top), sized to fit; returns the y below it.</summary>
        private static float Place(TMP_Text text, float y, float width, float gapBefore)
        {
            if (text == null || !text.gameObject.activeSelf) return y;

            y += gapBefore;
            float height = Mathf.Ceil(Preferred(text, width).y) + 2f;
            var rect = Rect(text);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(Pad, -y);
            rect.sizeDelta = new Vector2(width, height);
            return y + height;
        }
    }
}
