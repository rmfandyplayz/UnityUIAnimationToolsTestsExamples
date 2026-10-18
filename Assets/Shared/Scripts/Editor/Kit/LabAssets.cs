using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIAnimLab.EditorTools
{
    /// <summary>
    /// Makes the shared sprites, sounds and materials the builders use, so the project needs no art.
    /// Everything is generated into Assets/Shared once; an asset already there is left alone, so its GUID
    /// (and every scene pointing at it) survives a rebuild.
    /// </summary>
    public static class LabAssets
    {
        public const string Root = "Assets/Shared";
        public const string SpriteDir = Root + "/Sprites";
        public const string AudioDir = Root + "/Audio";
        public const string MaterialDir = Root + "/Materials";
        public const string ShaderDir = Root + "/Shaders";

        public static Sprite Rounded { get { return LoadSprite("Rounded"); } }
        public static Sprite Circle { get { return LoadSprite("Circle"); } }
        public static Sprite Ring { get { return LoadSprite("Ring"); } }
        public static Sprite Glow { get { return LoadSprite("Glow"); } }
        public static Sprite Star { get { return LoadSprite("Star"); } }
        public static Sprite Heart { get { return LoadSprite("Heart"); } }
        public static Sprite Diamond { get { return LoadSprite("Diamond"); } }
        public static Sprite Chevron { get { return LoadSprite("Chevron"); } }
        public static Sprite Check { get { return LoadSprite("Check"); } }
        public static Sprite Cross { get { return LoadSprite("Cross"); } }
        public static Sprite Pixel { get { return LoadSprite("PixelHero"); } }

        public static AudioClip Click { get { return LoadClip("Click"); } }
        public static AudioClip Pop { get { return LoadClip("Pop"); } }
        public static AudioClip Whoosh { get { return LoadClip("Whoosh"); } }
        public static AudioClip Jingle { get { return LoadClip("Jingle"); } }
        public static AudioClip Error { get { return LoadClip("Error"); } }
        public static AudioClip Coin { get { return LoadClip("Coin"); } }

        public static Material FillMaterial { get { return LoadMaterial("Lab Fill"); } }
        public static Material WipeMaterial { get { return LoadMaterial("Lab Wipe"); } }
        public static Material GlowMaterial { get { return LoadMaterial("Lab Glow"); } }

        [MenuItem("Tools/UI Anim Lab/Generate Shared Assets", priority = 0)]
        public static void Ensure()
        {
            Folder(Root);
            Folder(SpriteDir);
            Folder(AudioDir);
            Folder(MaterialDir);

            bool made = false;
            made |= SpriteAsset("Rounded", 96, 32, (x, y, s) => RoundedRect(x, y, s, 32f));
            made |= SpriteAsset("Circle", 128, 0, (x, y, s) => Disc(x, y, s, 0f));
            made |= SpriteAsset("Ring", 128, 0, (x, y, s) => Disc(x, y, s, 14f));
            made |= SpriteAsset("Glow", 128, 0, GlowAt, supersample: 1);
            made |= SpriteAsset("Star", 128, 0, (x, y, s) => Polygon(x, y, s, StarPoints()));
            made |= SpriteAsset("Heart", 128, 0, HeartAt);
            made |= SpriteAsset("Diamond", 128, 0, (x, y, s) => Polygon(x, y, s, new[] { V(64, 6), V(122, 64), V(64, 122), V(6, 64) }));
            made |= SpriteAsset("Chevron", 128, 0, (x, y, s) => Polygon(x, y, s, new[] { V(30, 10), V(98, 64), V(30, 118), V(14, 102), V(62, 64), V(14, 26) }));
            made |= SpriteAsset("Check", 128, 0, (x, y, s) => Polygon(x, y, s, new[] { V(8, 66), V(26, 48), V(50, 72), V(104, 18), V(122, 36), V(50, 108) }));
            made |= SpriteAsset("Cross", 128, 0, CrossAt);
            made |= PixelSprite();

            made |= Wav("Click", 0.03f, t => Math.Sin(t * 2 * Math.PI * 1800) * Math.Exp(-t * 180) * 0.8);
            made |= Wav("Pop", 0.12f, t => Math.Sin(2 * Math.PI * (380 * t + 2600 * t * t)) * Math.Exp(-t * 28) * 0.8);
            made |= Wav("Whoosh", 0.35f, Whoosh01);
            made |= Wav("Jingle", 0.42f, t => Notes(t, 0.12, 523.25, 659.25, 783.99, 1046.5));
            made |= Wav("Error", 0.32f, t => Square(t < 0.14 ? 220 : 165, t) * (t % 0.16 < 0.13 ? 1 : 0) * 0.35);
            made |= Wav("Coin", 0.3f, t => Square(t < 0.07 ? 987.77 : 1318.5, t) * Math.Exp(-t * 9) * 0.3);

            if (made) AssetDatabase.Refresh();

            MaterialAsset("Lab Fill", "UI/Lab Fill");
            MaterialAsset("Lab Wipe", "UI/Lab Wipe");
            MaterialAsset("Lab Glow", "UI/Lab Glow");
        }

        // ------------------------------------------------------------ loading

        public static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + "/" + name + ".png");
        }

        public static AudioClip LoadClip(string name)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(AudioDir + "/" + name + ".wav");
        }

        public static Material LoadMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(MaterialDir + "/" + name + ".mat");
        }

        public static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // ------------------------------------------------------------ sprites

        private delegate float Coverage(float x, float y, int size);

        private static bool SpriteAsset(string name, int size, int border, Coverage cover, int supersample = 4)
        {
            string path = SpriteDir + "/" + name + ".png";
            if (File.Exists(path)) return false;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float a = 0f;
                    for (int sy = 0; sy < supersample; sy++)
                    {
                        for (int sx = 0; sx < supersample; sx++)
                        {
                            a += cover(x + (sx + 0.5f) / supersample, y + (sy + 0.5f) / supersample, size);
                        }
                    }

                    a /= supersample * supersample;
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f));
                }
            }

            tex.SetPixels32(pixels);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
            return true;
        }

        private static bool PixelSprite()
        {
            string path = SpriteDir + "/PixelHero.png";
            if (File.Exists(path)) return false;

            // A 12x12 space invader, for snapping: whole-pixel movement shows on its hard edges.
            string[] rows =
            {
                "...#....#...",
                "....#..#....",
                "...######...",
                "..##.##.##..",
                ".##########.",
                ".#.######.#.",
                ".#.#....#.#.",
                "....##.##...",
                "............",
                "............",
                "............",
                "............",
            };

            // rows[0] is the top line; the figure sits two pixels up from the bottom.
            var tex = new Texture2D(12, 12, TextureFormat.RGBA32, false);
            for (int y = 0; y < 12; y++)
            {
                for (int x = 0; x < 12; x++)
                {
                    int row = 11 - y - 2;
                    bool on = row >= 0 && row < 8 && rows[row][x] == '#';
                    tex.SetPixel(x, y, on ? Color.white : Color.clear);
                }
            }

            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 12f;
            importer.SaveAndReimport();
            return true;
        }

        private static Vector2 V(float x, float y)
        {
            return new Vector2(x, 128f - y);
        }

        private static float RoundedRect(float x, float y, int size, float radius)
        {
            float cx = Mathf.Clamp(x, radius, size - radius);
            float cy = Mathf.Clamp(y, radius, size - radius);
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
            return d <= radius ? 1f : 0f;
        }

        private static float Disc(float x, float y, int size, float thickness)
        {
            float r = size * 0.5f - 1f;
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(size * 0.5f, size * 0.5f));
            if (d > r) return 0f;
            return thickness > 0f && d < r - thickness ? 0f : 1f;
        }

        private static float GlowAt(float x, float y, int size)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(size * 0.5f, size * 0.5f)) / (size * 0.5f);
            float a = Mathf.Clamp01(1f - d);
            return a * a * (3f - 2f * a);
        }

        private static float HeartAt(float x, float y, int size)
        {
            // The classic implicit heart, (x^2 + y^2 - 1)^3 - x^2 y^3 <= 0, fitted into the square.
            float u = (x / size - 0.5f) * 2.6f;
            float v = (y / size - 0.45f) * 2.6f;
            float a = u * u + v * v - 1f;
            return a * a * a - u * u * v * v * v <= 0f ? 1f : 0f;
        }

        private static float CrossAt(float x, float y, int size)
        {
            float u = x - size * 0.5f, v = y - size * 0.5f;
            float d1 = Mathf.Abs(u - v) / 1.4142f, d2 = Mathf.Abs(u + v) / 1.4142f;
            bool inBox = Mathf.Abs(u) < size * 0.42f && Mathf.Abs(v) < size * 0.42f;
            return inBox && (d1 < 12f || d2 < 12f) ? 1f : 0f;
        }

        private static Vector2[] StarPoints()
        {
            var points = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                float r = i % 2 == 0 ? 60f : 25f;
                points[i] = new Vector2(64f + Mathf.Cos(angle) * r, 66f + Mathf.Sin(angle) * r);
            }

            return points;
        }

        private static float Polygon(float x, float y, int size, Vector2[] points)
        {
            bool inside = false;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                Vector2 a = points[i], b = points[j];
                if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }

            return inside ? 1f : 0f;
        }

        // ------------------------------------------------------------ sounds

        private const int Rate = 44100;

        private static bool Wav(string name, float seconds, Func<double, double> sample)
        {
            string path = AudioDir + "/" + name + ".wav";
            if (File.Exists(path)) return false;

            int count = Mathf.CeilToInt(seconds * Rate);
            using (var stream = new FileStream(path, FileMode.Create))
            using (var w = new BinaryWriter(stream))
            {
                w.Write(new[] { 'R', 'I', 'F', 'F' });
                w.Write(36 + count * 2);
                w.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                w.Write(16);
                w.Write((short)1);
                w.Write((short)1);
                w.Write(Rate);
                w.Write(Rate * 2);
                w.Write((short)2);
                w.Write((short)16);
                w.Write(new[] { 'd', 'a', 't', 'a' });
                w.Write(count * 2);

                for (int i = 0; i < count; i++)
                {
                    double t = (double)i / Rate;
                    // A short fade at both ends so nothing clicks.
                    double edge = Math.Min(1.0, Math.Min(t / 0.002, (seconds - t) / 0.004));
                    double v = Math.Max(-1, Math.Min(1, sample(t) * edge));
                    w.Write((short)(v * short.MaxValue));
                }
            }

            return true;
        }

        private static readonly System.Random noise = new System.Random(7);
        private static double lowpass;

        private static double Whoosh01(double t)
        {
            double env = Math.Sin(Math.PI * Math.Min(1, t / 0.35));
            double cutoff = 0.02 + 0.25 * env;
            lowpass += cutoff * ((noise.NextDouble() * 2 - 1) - lowpass);
            return lowpass * env * 2.2;
        }

        private static double Square(double frequency, double t)
        {
            return Math.Sin(2 * Math.PI * frequency * t) >= 0 ? 1 : -1;
        }

        private static double Notes(double t, double each, params double[] frequencies)
        {
            int index = Math.Min(frequencies.Length - 1, (int)(t / each));
            double local = t - index * each;
            double tail = index == frequencies.Length - 1 ? 4 : 14;
            return (Math.Sin(2 * Math.PI * frequencies[index] * t) * 0.6 + Math.Sin(4 * Math.PI * frequencies[index] * t) * 0.15)
                   * Math.Exp(-local * tail) * 0.8;
        }

        // ------------------------------------------------------------ materials

        private static void MaterialAsset(string name, string shaderName)
        {
            string path = MaterialDir + "/" + name + ".mat";
            if (File.Exists(path)) return;

            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError("UI Anim Lab: shader '" + shaderName + "' not found - is " + ShaderDir + " imported?");
                return;
            }

            AssetDatabase.CreateAsset(new Material(shader) { name = name }, path);
        }
    }
}
