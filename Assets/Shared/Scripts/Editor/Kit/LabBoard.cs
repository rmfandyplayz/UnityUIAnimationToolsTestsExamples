using System;
using System.Collections.Generic;
using rmf_claude.DOTweenUI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIAnimLab.EditorTools
{
    /// <summary>The colours every board is drawn in.</summary>
    public static class Pal
    {
        public static readonly Color Background = Hex("1B1E25");
        public static readonly Color Tile = Hex("2A2E38");
        public static readonly Color Stage = Hex("20232B");
        public static readonly Color ReadmeTile = Hex("26324A");
        public static readonly Color TitleText = Hex("F2F3F5");
        public static readonly Color BodyText = Hex("B9BEC9");
        public static readonly Color HowToText = Hex("D3D7DF");
        public static readonly Color WatchText = Hex("7FD1FF");
        public static readonly Color ExpectText = Hex("FFD866");
        public static readonly Color BadgeText = Hex("FF9E5E");
        public static readonly Color Faint = Hex("6B7280");

        // Things to animate. Bright enough to read against Stage, distinct from each other.
        public static readonly Color Blue = Hex("5DA9FF");
        public static readonly Color Green = Hex("5CD68E");
        public static readonly Color Orange = Hex("FF9F43");
        public static readonly Color Pink = Hex("FF6B9A");
        public static readonly Color Purple = Hex("A78BFA");
        public static readonly Color Yellow = Hex("FFD84D");
        public static readonly Color Teal = Hex("2DD4BF");
        public static readonly Color Red = Hex("F25F5C");
        public static readonly Color White = Hex("F2F3F5");
        public static readonly Color Grey = Hex("8A93A6");
        public static readonly Color Ghost = new Color(1f, 1f, 1f, 0.08f);

        // Showcase frames: a deep indigo backdrop with a soft palette.
        public static readonly Color FrameBg = Hex("232946");
        public static readonly Color FramePanel = Hex("2F3760");
        public static readonly Color FrameCard = Hex("FFFFFE");
        public static readonly Color FrameInk = Hex("232946");
        public static readonly Color FrameText = Hex("FFFFFE");
        public static readonly Color FrameMuted = Hex("B8C1EC");
        public static readonly Color FramePink = Hex("EEBBC3");
        public static readonly Color FrameAmber = Hex("F9BC60");
        public static readonly Color FrameMint = Hex("ABD1C6");
        public static readonly Color FrameRed = Hex("E16162");

        public static Color Hex(string hex)
        {
            Color c;
            ColorUtility.TryParseHtmlString("#" + hex, out c);
            return c;
        }

        public static Color A(this Color c, float alpha)
        {
            c.a = alpha;
            return c;
        }
    }

    /// <summary>
    /// Builds one scene: a camera, a Screen Space Overlay canvas holding a Board of station tiles, an
    /// index bar and an EventSystem. Tiles are laid out on a grid in the order they are made.
    ///
    ///     var board = LabBoard.Begin("Assets/UnitTests/Scenes/01 Step Types.unity", "Step Types", "...");
    ///     var t = board.Test("S1", "Anchored Position").Summary("...").HowTo("...");
    ///     ...
    ///     board.Save();
    ///
    /// The scene is built in a new additive scene and saved over the old file (same .meta, same GUID),
    /// so the scene you have open is never touched. A target scene with unsaved changes is refused.
    /// </summary>
    public sealed class LabBoard
    {
        public readonly string Path;
        public readonly bool Showcase;
        public Scene Scene;
        public Canvas Canvas;
        public RectTransform Root;
        public StationBoard Nav;

        private readonly List<Station> stations = new List<Station>();
        private readonly List<int> rowBreaks = new List<int>();
        private readonly int columns;
        private Scene previousActive;
        private bool wasOpen;

        public const float Gap = 44f;

        private LabBoard(string path, bool showcase, int columns)
        {
            Path = path;
            Showcase = showcase;
            this.columns = columns;
        }

        public IReadOnlyList<Station> Stations
        {
            get { return stations; }
        }

        public static LabBoard Begin(string path, bool showcase = false, int columns = 4)
        {
            LabAssets.Ensure();
            LabAssets.Folder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));

            var board = new LabBoard(path, showcase, columns);
            var open = SceneManager.GetSceneByPath(path);
            if (open.IsValid() && open.isLoaded)
            {
                if (open.isDirty) throw new InvalidOperationException(path + " is open with unsaved changes. Save or discard them, then build again.");
                board.wasOpen = true;
            }

            board.previousActive = SceneManager.GetActiveScene();
            board.Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(board.Scene);

            if (board.wasOpen) EditorSceneManager.CloseScene(open, true);

            board.MakeRig();
            return board;
        }

        private void MakeRig()
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Showcase ? Pal.Hex("15182A") : Pal.Background;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            Canvas = canvasGo.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            Root = UI.Rect(canvasGo.transform, "Board", Vector2.zero, Vector2.zero);

            Nav = canvasGo.AddComponent<StationBoard>();
            Nav.Board = Root;
            Nav.StartFocused = Showcase;
            Nav.FocusMargin = Showcase ? 0f : 0.06f;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        /// <summary>Starts a new row before the next tile.</summary>
        public LabBoard Row()
        {
            rowBreaks.Add(stations.Count);
            return this;
        }

        public Tile Readme(string title, string body, float width = 0f)
        {
            var tile = Make(StationStyle.Readme, string.Empty, title, width > 0f ? width : (Showcase ? 960f : 620f), 0f);
            tile.Station.Summary = body;
            return tile;
        }

        public Tile Test(string code, string title, float width = 620f, float stageHeight = 260f)
        {
            return Make(StationStyle.Test, code, title, width, stageHeight);
        }

        /// <summary>A showcase frame. 960 x 540 is 16:9, so a 16:9 Game view focused on it shows exactly the frame.</summary>
        public Tile Shot(string page, string title, float width = 960f, float height = 540f)
        {
            var tile = Make(StationStyle.Showcase, string.Empty, title, width, height);
            tile.Station.WikiPage = page;
            return tile;
        }

        private Tile Make(StationStyle style, string code, string title, float width, float stageHeight)
        {
            string name = (string.IsNullOrEmpty(code) ? title : code + "  " + title).Replace('/', '-');
            if (style == StationStyle.Readme) name = "README  " + title.Replace('/', '-');
            if (style == StationStyle.Showcase) name = (FrameCount() + 1).ToString("00") + "  " + title.Replace('/', '-');

            var rect = UI.Rect(Root, name, Vector2.zero, new Vector2(width, 100f));
            rect.pivot = new Vector2(0f, 1f);

            var bg = rect.gameObject.AddComponent<Image>();
            bg.sprite = LabAssets.Rounded;
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 2f;
            bg.color = style == StationStyle.Readme ? Pal.ReadmeTile : style == StationStyle.Showcase ? Color.clear : Pal.Tile;
            bg.raycastTarget = false;

            var station = rect.gameObject.AddComponent<Station>();
            station.Code = code;
            station.Title = title;
            station.Style = style;
            station.Width = width;
            station.StageHeight = style == StationStyle.Readme ? 0f : stageHeight;

            station.Header = UI.TopStretch(rect, "Header");
            station.TitleText = UI.Text(station.Header, "Title", "", style == StationStyle.Showcase ? 24f : 26f, Pal.TitleText, FontStyles.Bold);
            station.BadgeText = UI.Text(station.Header, "Needs", "", 14f, Pal.BadgeText, FontStyles.Bold);
            var badge = (RectTransform)station.BadgeText.transform;
            badge.anchorMin = badge.anchorMax = badge.pivot = new Vector2(1f, 1f);
            station.BadgeText.alignment = TextAlignmentOptions.TopRight;
            station.BadgeText.textWrappingMode = TextWrappingModes.NoWrap;
            station.SummaryText = UI.Text(station.Header, "Summary", "", style == StationStyle.Readme ? 19f : 18f, Pal.BodyText);
            station.WatchText = UI.Text(station.Header, "Watch", "", 16f, Pal.WatchText);

            if (style != StationStyle.Readme)
            {
                var stage = UI.Rect(rect, "Stage", Vector2.zero, new Vector2(width, stageHeight));
                var stageBg = stage.gameObject.AddComponent<Image>();
                if (style != StationStyle.Showcase)
                {
                    // A showcase frame stays square, so a recording of it has no corners showing through.
                    stageBg.sprite = LabAssets.Rounded;
                    stageBg.type = Image.Type.Sliced;
                    stageBg.pixelsPerUnitMultiplier = 2.5f;
                }
                stageBg.color = style == StationStyle.Showcase ? Pal.FrameBg : Pal.Stage;
                stageBg.raycastTarget = false;
                if (style == StationStyle.Showcase) stage.gameObject.AddComponent<RectMask2D>();
                station.Stage = stage;
            }

            station.Footer = UI.TopStretch(rect, "Footer");
            station.HowToText = UI.Text(station.Footer, "How To", "", 16f, Pal.HowToText);
            station.ExpectText = UI.Text(station.Footer, "Expect", "", 16f, Pal.ExpectText);

            stations.Add(station);
            return new Tile(this, station);
        }

        private int FrameCount()
        {
            int n = 0;
            foreach (var s in stations) if (s.Style != StationStyle.Readme) n++;
            return n;
        }

        /// <summary>Lays the tiles out, builds the index bar, saves the scene and closes it (unless it was open before).</summary>
        public void Save()
        {
            Layout();
            MakeIndex();
            Nav.Collect();

            EditorSceneManager.MarkSceneDirty(Scene);
            EditorSceneManager.SaveScene(Scene, Path);

            if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
            if (!wasOpen && SceneManager.sceneCount > 1) EditorSceneManager.CloseScene(Scene, true);

            Debug.Log("UI Anim Lab: built " + Path + " (" + stations.Count + " tiles)");
        }

        private void Layout()
        {
            foreach (var s in stations) s.Refresh();

            // Grid from the top-left of the 1920 x 1080 canvas, so the first row is what the Game view
            // shows before anything is moved.
            float left = -960f + 60f;
            float x = left;
            float y = 540f - 110f;
            float rowHeight = 0f;
            int inRow = 0;

            for (int i = 0; i < stations.Count; i++)
            {
                var rect = (RectTransform)stations[i].transform;
                bool breakHere = i > 0 && (rowBreaks.Contains(i) || inRow >= columns);
                if (breakHere)
                {
                    x = left;
                    y -= rowHeight + Gap;
                    rowHeight = 0f;
                    inRow = 0;
                }

                rect.anchoredPosition = new Vector2(x, y);
                x += rect.sizeDelta.x + Gap;
                rowHeight = Mathf.Max(rowHeight, rect.sizeDelta.y);
                inRow++;
            }
        }

        private void MakeIndex()
        {
            var bar = UI.Rect(Canvas.transform, "Index", Vector2.zero, Vector2.zero);
            bar.anchorMin = bar.anchorMax = bar.pivot = new Vector2(0f, 1f);
            bar.anchoredPosition = new Vector2(16f, -14f);

            var bg = bar.gameObject.AddComponent<Image>();
            bg.sprite = LabAssets.Rounded;
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 3f;
            bg.color = new Color(0f, 0f, 0f, 0.55f);

            var layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 14, 8, 8);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fitter = bar.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = UI.Text(bar, "Scene", System.IO.Path.GetFileNameWithoutExtension(Path), 18f, Pal.TitleText, FontStyles.Bold);
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.gameObject.AddComponent<LayoutElement>().minHeight = 30f;

            int frame = 0;
            for (int i = 0; i < stations.Count; i++)
            {
                var s = stations[i];
                if (s.Style != StationStyle.Readme) frame++;
                string label = s.Style == StationStyle.Readme ? "Intro" : s.Style == StationStyle.Showcase ? frame.ToString() : s.Code;
                var button = UI.Button(bar, "Go " + label, label, Vector2.zero, new Vector2(10f, 30f), 15f);
                var element = button.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = Mathf.Max(34f, label.Length * 10f + 14f);
                element.preferredHeight = 30f;
                UnityEventTools.AddIntPersistentListener(button.onClick, Nav.Focus, i);
            }

            var hint = UI.Text(bar, "Hint", "Right-drag pan · Wheel zoom · Tab next · F focus · Home all · H hide", 14f, Pal.Faint);
            hint.textWrappingMode = TextWrappingModes.NoWrap;

            Nav.IndexBar = bar.gameObject;
            if (Showcase) bar.gameObject.SetActive(false);
        }
    }

    /// <summary>One tile being built. Every setter returns the tile, so calls chain.</summary>
    public sealed class Tile
    {
        public readonly LabBoard Board;
        public readonly Station Station;

        public Tile(LabBoard board, Station station)
        {
            Board = board;
            Station = station;
        }

        public RectTransform Stage { get { return Station.Stage; } }
        public GameObject Go { get { return Station.gameObject; } }

        public Tile Summary(string text) { Station.Summary = text; return this; }
        public Tile HowTo(string text) { Station.HowTo = text; return this; }
        public Tile Expect(string text) { Station.Expect = text; return this; }
        public Tile Needs(StationNeeds needs) { Station.Needs = needs; return this; }
        public Tile Capture(string text) { Station.Capture = text; return this; }

        public Tile Watch(string text, params Component[] objects)
        {
            var list = new List<GameObject>();
            foreach (var o in objects) if (o != null) list.Add(o.gameObject);
            return Watch(text, list.ToArray());
        }

        public Tile Watch(string text, params GameObject[] objects)
        {
            Station.Watch = text;
            Station.WatchObjects = objects;
            return this;
        }

        /// <summary>Adds a UI Animation Player to the tile itself (or to `on`) holding these animations.</summary>
        public UIAnimationPlayer Player(params UIAnimation[] animations)
        {
            return PlayerOn(Go, animations);
        }

        public UIAnimationPlayer PlayerOn(GameObject on, params UIAnimation[] animations)
        {
            var player = on.GetComponent<UIAnimationPlayer>();
            if (player == null) player = on.AddComponent<UIAnimationPlayer>();
            Anim.Set(player, animations);
            return player;
        }
    }
}
