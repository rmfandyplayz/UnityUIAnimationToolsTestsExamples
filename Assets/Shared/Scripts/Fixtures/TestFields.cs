using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UIAnimLab
{
    /// <summary>
    /// A fixture for Custom Property steps on plain public FIELDS, which have no setter to react with. The
    /// values are copied onto the objects every frame - every editor tick too, so the edit-mode preview
    /// shows them moving. Only a changed value is written, so an idle scene is never touched.
    /// </summary>
    [ExecuteAlways]
    public class TestFields : MonoBehaviour
    {
        public Vector3 Angles;
        public Color Tint = Color.white;
        public string Caption = string.Empty;

        [SerializeField] private Image image;
        [SerializeField] private TMP_Text caption;

        private Vector3 appliedAngles;
        private Color appliedTint;
        private string appliedCaption;

        public void Setup(Image image, TMP_Text caption)
        {
            this.image = image;
            this.caption = caption;
            transform.localEulerAngles = Angles;
            if (image != null) image.color = Tint;
            if (caption != null) caption.text = Caption;
        }

        private void OnEnable()
        {
            // The scene is saved with the fields and the objects agreeing, so there is nothing to write yet.
            appliedAngles = Angles;
            appliedTint = Tint;
            appliedCaption = Caption;

        #if UNITY_EDITOR
            if (!Application.isPlaying) EditorApplication.update += Apply;
        #endif
        }

        private void OnDisable()
        {
        #if UNITY_EDITOR
            EditorApplication.update -= Apply;
        #endif
        }

        private void Update()
        {
            Apply();
        }

        private void Apply()
        {
            if (this == null) return;

            if (Angles != appliedAngles)
            {
                transform.localEulerAngles = Angles;
                appliedAngles = Angles;
            }

            if (image != null && Tint != appliedTint)
            {
                image.color = Tint;
                appliedTint = Tint;
            }

            if (caption != null && Caption != appliedCaption)
            {
                caption.text = Caption;
                appliedCaption = Caption;
            }
        }
    }
}
