using UIAnimLab.EditorTools;
using UnityEditor;
using UnityEngine;

namespace UIAnimLab.UnitTests.Builders
{
    /// <summary>Shared bits of the unit-test scene builders.</summary>
    public static class UT
    {
        public const string Scenes = "Assets/UnitTests/Scenes/";
        public const string Sets = "Assets/UnitTests/Animation Sets/";
        public const string Menu = "Tools/UI Anim Lab/Build Unit Tests/";

        public const string HowToPreview =
            "Select this tile. Under its animations, press <b>Play</b> beside an animation to preview it without play mode.";

        public static LabBoard Begin(string scene, string title, string intro, int columns = 4)
        {
            var board = LabBoard.Begin(Scenes + scene + ".unity", false, columns);
            board.Readme(title, intro +
                "\n\n<b>Using this board:</b> select a tile. Its Inspector shows these directions at the top, with Select buttons for the objects to watch, and its UI Animation Player underneath. " +
                "Badges in a tile's corner say what it needs: <color=#FF9E5E>PREVIEW</color> (the Play buttons, out of play mode), <color=#FF9E5E>PLAY MODE</color>, <color=#FF9E5E>INSPECTOR</color>, <color=#FF9E5E>SCENE VIEW</color>, <color=#FF9E5E>CONSOLE</color>, <color=#FF9E5E>POP-OUT</color> (an animation's own window)." +
                "\n\nIn play mode: right-drag pans, the wheel zooms, Tab steps through tiles, Home shows them all.");
            return board;
        }

        /// <summary>A faint outline marking where something should end up.</summary>
        public static void Mark(Tile tile, string name, Vector2 position, Vector2 size)
        {
            UI.Marker(tile.Stage, "Mark " + name, position, size, Pal.White);
        }

        /// <summary>A square box on a tile's stage, named so Target Paths read "Stage/name".</summary>
        public static UnityEngine.UI.Image Box(Tile tile, string name, Vector2 position, Color color, float size = 64f)
        {
            return UI.Box(tile.Stage, name, position, new Vector2(size, size), color);
        }

        public static string Path(string name)
        {
            return "Stage/" + name;
        }

        [MenuItem(Menu + "Build All", priority = 0)]
        public static void BuildAll()
        {
            StepTypesScene.Build();
            TimingScene.Build();
            MotionScene.Build();
            PlaybackScene.Build();
            CodeApiScene.Build();
            TargetsScene.Build();
            InspectorScene.Build();
            PreviewScene.Build();
            RecordingScene.Build();
            GizmosScene.Build();
            EdgeCasesScene.Build();
        }
    }
}
