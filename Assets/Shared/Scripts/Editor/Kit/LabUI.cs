using TMPro;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIAnimLab.EditorTools
{
    /// <summary>
    /// Small factories for UI objects. Positions are anchored positions from the parent's centre, the
    /// way a Stage is laid out, unless a method says otherwise.
    /// </summary>
    public static class UI
    {
        public static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            go.layer = 5;
            return rect;
        }

        /// <summary>A child stretched across the top of its parent, for stacking header and footer rows.</summary>
        public static RectTransform TopStretch(Transform parent, string name)
        {
            var rect = Rect(parent, name, Vector2.zero, Vector2.zero);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            return rect;
        }

        public static Image Box(Transform parent, string name, Vector2 position, Vector2 size, Color color, Sprite sprite = null)
        {
            var rect = Rect(parent, name, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite != null ? sprite : LabAssets.Rounded;
            if (image.sprite == LabAssets.Rounded)
            {
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 3f;
            }
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>A plain square-cornered box, for things whose exact edges matter (sizes, offsets, snapping).</summary>
        public static Image Square(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var image = Box(parent, name, position, size, color, null);
            image.sprite = null;
            image.type = Image.Type.Simple;
            return image;
        }

        public static Image Icon(Transform parent, string name, Sprite sprite, Vector2 position, float size, Color color)
        {
            var image = Box(parent, name, position, new Vector2(size, size), color, sprite);
            image.preserveAspect = true;
            return image;
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color color, FontStyles style = FontStyles.Normal)
        {
            var rect = Rect(parent, name, Vector2.zero, new Vector2(200f, 40f));
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.fontStyle = style;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.richText = true;
            label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.TopLeft;
            return label;
        }

        /// <summary>A centred label at a position on a stage.</summary>
        public static TextMeshProUGUI Label(Transform parent, string name, string text, Vector2 position, Vector2 size, float fontSize, Color color,
                                            TextAlignmentOptions align = TextAlignmentOptions.Center, FontStyles style = FontStyles.Normal)
        {
            var label = Text(parent, name, text, fontSize, color, style);
            var rect = (RectTransform)label.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            label.alignment = align;
            return label;
        }

        /// <summary>A caption under a stage object, so every object on a stage is named.</summary>
        public static TextMeshProUGUI Caption(Transform parent, string text, Vector2 position, float width = 180f)
        {
            return Label(parent, "Caption " + text, text, position, new Vector2(width, 22f), 14f, Pal.Faint);
        }

        public static Button Button(Transform parent, string name, string text, Vector2 position, Vector2 size, float fontSize = 16f)
        {
            var image = Box(parent, name, position, size, Pal.Hex("3B4252"));
            image.raycastTarget = true;
            image.pixelsPerUnitMultiplier = 4f;

            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.35f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.85f);
            colors.colorMultiplier = 1.2f;
            button.colors = colors;

            var label = Label(image.transform, "Label", text, Vector2.zero, Vector2.zero, fontSize, Pal.TitleText);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return button;
        }

        /// <summary>A button whose On Click plays an animation by name, through PlayAnimation - no code.</summary>
        public static Button PlayButton(Transform parent, string text, Vector2 position, Vector2 size, rmf_claude.DOTweenUI.UIAnimationPlayer player, string animation)
        {
            var button = Button(parent, "Button " + text, text, position, size);
            UnityEventTools.AddStringPersistentListener(button.onClick, player.PlayAnimation, animation);
            return button;
        }

        /// <summary>Adds an Event Trigger entry (Pointer Enter, Pointer Exit...) that plays an animation, as a person would wire it.</summary>
        public static void Trigger(Component on, EventTriggerType type, rmf_claude.DOTweenUI.UIAnimationPlayer player, string animation)
        {
            var trigger = Add<EventTrigger>(on);
            var entry = new EventTrigger.Entry { eventID = type };
            UnityEventTools.AddStringPersistentListener(entry.callback, player.PlayAnimation, animation);
            trigger.triggers.Add(entry);
        }

        public static void OnClick(Button button, UnityAction action)
        {
            UnityEventTools.AddVoidPersistentListener(button.onClick, action);
        }

        public static void OnClick(Button button, UnityAction<string> action, string argument)
        {
            UnityEventTools.AddStringPersistentListener(button.onClick, action, argument);
        }

        public static CanvasGroup Group(Component on, float alpha = 1f)
        {
            var group = on.gameObject.GetComponent<CanvasGroup>();
            if (group == null) group = on.gameObject.AddComponent<CanvasGroup>();
            group.alpha = alpha;
            return group;
        }

        /// <summary>A Graphic-sized dashed-looking marker: a faint outline showing where something rests or lands.</summary>
        public static Image Marker(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var image = Box(parent, name, position, size, color.A(0.18f));
            return image;
        }

        public static T Add<T>(Component on) where T : Component
        {
            var c = on.gameObject.GetComponent<T>();
            return c != null ? c : on.gameObject.AddComponent<T>();
        }
    }
}
