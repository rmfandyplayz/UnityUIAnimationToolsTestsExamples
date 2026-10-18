using System.Globalization;
using TMPro;
using UnityEngine;

namespace UIAnimLab
{
    /// <summary>
    /// Writes a live value into a text every frame, edit mode included, so a test can be read rather than
    /// eyeballed: a scale that should come back to exactly 1, a position, whether something is active.
    /// </summary>
    [ExecuteAlways]
    public class LiveReadout : MonoBehaviour
    {
        public enum Value
        {
            Scale = 0,
            AnchoredPosition = 1,
            RotationZ = 2,
            Active = 3,
            SizeDelta = 4,
            CanvasGroupAlpha = 5,
            TimeScale = 6,
        }

        public TMP_Text Text;
        public Value Show;
        public Transform Target;
        public string Prefix;

        private string last;

        private void Update()
        {
            Write();
        }

    #if UNITY_EDITOR
        private void OnEnable()
        {
            if (!Application.isPlaying) UnityEditor.EditorApplication.update += Write;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.update -= Write;
        }
    #endif

        private void Write()
        {
            if (this == null || Text == null) return;

            string value = Read();
            if (value == last) return;
            last = value;
            Text.text = Prefix + value;
        }

        private string Read()
        {
            var c = CultureInfo.InvariantCulture;
            var rect = Target as RectTransform;

            switch (Show)
            {
                case Value.Scale:
                    return Target == null ? "-" : Target.localScale.x.ToString("0.000", c) + ", " + Target.localScale.y.ToString("0.000", c);
                case Value.AnchoredPosition:
                    return rect == null ? "-" : rect.anchoredPosition.x.ToString("0.0", c) + ", " + rect.anchoredPosition.y.ToString("0.0", c);
                case Value.RotationZ:
                    return Target == null ? "-" : Mathf.DeltaAngle(0f, Target.localEulerAngles.z).ToString("0.0", c);
                case Value.Active:
                    return Target == null ? "-" : (Target.gameObject.activeSelf ? "on" : "off");
                case Value.SizeDelta:
                    return rect == null ? "-" : rect.sizeDelta.x.ToString("0.0", c) + " x " + rect.sizeDelta.y.ToString("0.0", c);
                case Value.CanvasGroupAlpha:
                    var group = Target == null ? null : Target.GetComponent<CanvasGroup>();
                    return group == null ? "-" : group.alpha.ToString("0.00", c);
                case Value.TimeScale:
                    return Time.timeScale.ToString("0.##", c);
            }

            return "-";
        }
    }
}
