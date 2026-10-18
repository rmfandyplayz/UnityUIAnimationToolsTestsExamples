using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UIAnimLab
{
    /// <summary>
    /// Lets you move around a board of stations in play mode, where the Scene view's own navigation is
    /// not available:
    ///
    ///   Right-drag      pan
    ///   Wheel           zoom around the cursor
    ///   Tab / Shift+Tab focus the next / previous station
    ///   1 - 9, 0        focus tile 1 - 10 (the intro doesn't count)
    ///   F               focus the station under the cursor
    ///   Home            fit every station on screen
    ///   H               show or hide the index bar
    ///
    /// Moves the Board (a plain RectTransform holding every tile), never the tiles themselves, so no
    /// animation's resting values are touched. Runs on unscaled time with plain code rather than DOTween,
    /// so a test that pauses the game or kills every tween leaves navigation working.
    /// </summary>
    public class StationBoard : MonoBehaviour
    {
        public RectTransform Board;

        [Tooltip("The index of jump buttons and the controls hint. Toggled with H.")]
        public GameObject IndexBar;

        [Tooltip("Focus the first station when play mode starts, rather than fitting all of them.")]
        public bool StartFocused;

        [Tooltip("Margin kept around a focused station, as a fraction of the screen. 0 fills the screen exactly, " +
                 "which is what a showcase frame wants for recording.")]
        public float FocusMargin = 0.06f;

        public float MinZoom = 0.1f;
        public float MaxZoom = 4f;

        private readonly List<Station> stations = new List<Station>();
        private RectTransform canvasRect;
        private Canvas canvas;
        private Vector2 targetPosition;
        private float targetZoom = 1f;
        private bool gliding;
        private int focused = -1;

        public IReadOnlyList<Station> Stations
        {
            get { return stations; }
        }

        /// <summary>The station last focused with Tab, a number key, F or the index; null after Home.</summary>
        public Station FocusedStation
        {
            get { return focused >= 0 && focused < stations.Count ? stations[focused] : null; }
        }

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasRect = (RectTransform)canvas.transform;
            Collect();
        }

        private void Start()
        {
            if (StartFocused && stations.Count > 0) Focus(FirstFocusable(), true);
            else FitAll(true);
        }

        public void Collect()
        {
            stations.Clear();
            if (Board == null) return;
            Board.GetComponentsInChildren(true, stations);
        }

        private int FirstFocusable()
        {
            for (int i = 0; i < stations.Count; i++)
            {
                if (stations[i].Style != StationStyle.Readme) return i;
            }

            return 0;
        }

        private int NthTile(int n)
        {
            for (int i = 0; i < stations.Count; i++)
            {
                if (stations[i].Style == StationStyle.Readme) continue;
                if (n-- == 0) return i;
            }

            return -1;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    if (delta.sqrMagnitude > 0f)
                    {
                        gliding = false;
                        Board.anchoredPosition += delta / canvas.scaleFactor;
                    }
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    gliding = false;
                    ZoomAround(mouse.position.ReadValue(), Mathf.Pow(1.1f, Mathf.Sign(scroll) * Mathf.Min(3f, Mathf.Abs(scroll) / 120f + 0.5f)));
                }
            }

            if (keyboard != null)
            {
                bool shift = keyboard.shiftKey.isPressed;

                if (keyboard.tabKey.wasPressedThisFrame && stations.Count > 0)
                {
                    int next = focused < 0 ? 0 : (focused + (shift ? -1 : 1) + stations.Count) % stations.Count;
                    Focus(next);
                }

                if (keyboard.homeKey.wasPressedThisFrame) FitAll();
                if (keyboard.hKey.wasPressedThisFrame && IndexBar != null) IndexBar.SetActive(!IndexBar.activeSelf);
                if (keyboard.fKey.wasPressedThisFrame && mouse != null) FocusUnder(mouse.position.ReadValue());

                // Number keys count tiles, not the intro: 1 is the first tile after it.
                for (int i = 0; i < 10; i++)
                {
                    var key = keyboard[i == 9 ? Key.Digit0 : Key.Digit1 + i];
                    if (key.wasPressedThisFrame) Focus(NthTile(i));
                }
            }

            if (gliding)
            {
                float t = 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime);
                Board.anchoredPosition = Vector2.Lerp(Board.anchoredPosition, targetPosition, t);
                float zoom = Mathf.Lerp(Board.localScale.x, targetZoom, t);
                Board.localScale = new Vector3(zoom, zoom, 1f);

                if ((Board.anchoredPosition - targetPosition).sqrMagnitude < 0.01f && Mathf.Abs(zoom - targetZoom) < 0.0005f)
                {
                    Board.anchoredPosition = targetPosition;
                    Board.localScale = new Vector3(targetZoom, targetZoom, 1f);
                    gliding = false;
                }
            }
        }

        /// <summary>Focus station number index (counting from 0, in board order). Wired to the index buttons.</summary>
        public void Focus(int index)
        {
            Focus(index, false);
        }

        public void Focus(int index, bool instant)
        {
            if (index < 0 || index >= stations.Count) return;
            focused = index;
            Frame(BoundsOf(stations[index].FocusRect), stations[index].Style == StationStyle.Showcase ? 0f : FocusMargin, instant);
        }

        public void FitAll()
        {
            FitAll(false);
        }

        public void FitAll(bool instant)
        {
            if (stations.Count == 0) return;

            Rect all = BoundsOf((RectTransform)stations[0].transform);
            for (int i = 1; i < stations.Count; i++)
            {
                Rect r = BoundsOf((RectTransform)stations[i].transform);
                all = Rect.MinMaxRect(Mathf.Min(all.xMin, r.xMin), Mathf.Min(all.yMin, r.yMin),
                                      Mathf.Max(all.xMax, r.xMax), Mathf.Max(all.yMax, r.yMax));
            }

            focused = -1;
            Frame(all, 0.03f, instant);
        }

        private void FocusUnder(Vector2 screen)
        {
            for (int i = 0; i < stations.Count; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)stations[i].transform, screen, null))
                {
                    Focus(i);
                    return;
                }
            }
        }

        /// <summary>A rect in the Board's own space, whatever the board's current pan and zoom.</summary>
        private Rect BoundsOf(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 4; i++)
            {
                Vector2 p = Board.InverseTransformPoint(corners[i]);
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private void Frame(Rect area, float margin, bool instant)
        {
            // Keep clear of the index bar while it shows.
            float top = IndexBar != null && IndexBar.activeSelf ? ((RectTransform)IndexBar.transform).rect.height + 24f : 0f;
            Vector2 screen = canvasRect.rect.size - new Vector2(0f, top);
            float zoom = Mathf.Min(screen.x / area.width, screen.y / area.height) * (1f - margin * 2f);
            targetZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);

            // The board sits at the canvas centre, so putting the area's centre there is a matter of
            // moving the board by minus that centre, scaled (and down by half the bar).
            targetPosition = -area.center * targetZoom + BoardOrigin() + new Vector2(0f, -top * 0.5f);
            gliding = !instant;

            if (instant)
            {
                Board.anchoredPosition = targetPosition;
                Board.localScale = new Vector3(targetZoom, targetZoom, 1f);
            }
        }

        // The anchored position that puts the board's origin at the canvas centre.
        private Vector2 BoardOrigin()
        {
            Vector2 anchor = (Board.anchorMin + Board.anchorMax) * 0.5f;
            return (new Vector2(0.5f, 0.5f) - anchor) * canvasRect.rect.size;
        }

        private void ZoomAround(Vector2 screenPoint, float factor)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Board.parent, screenPoint, null, out local);

            float zoom = Mathf.Clamp(Board.localScale.x * factor, MinZoom, MaxZoom);
            factor = zoom / Board.localScale.x;

            // Board.localPosition is where the board origin sits in its parent; scaling around `local`
            // moves it along the line from there to the cursor.
            Vector2 origin = Board.localPosition;
            Vector2 moved = local + (origin - local) * factor;
            Board.anchoredPosition += moved - origin;
            Board.localScale = new Vector3(zoom, zoom, 1f);
        }
    }
}
