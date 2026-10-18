using TMPro;
using UnityEngine;

namespace UIAnimLab
{
    /// <summary>
    /// Counts how often an event reached it - an On Complete, a button - and shows the count. Wire Count()
    /// to the event in the Inspector. The count is not saved, so every play mode session starts at 0.
    /// </summary>
    public class EventCounter : MonoBehaviour
    {
        public TMP_Text Text;
        public string Label = "On Complete fired";

        [System.NonSerialized] public int Value;

        private void OnEnable()
        {
            Write();
        }

        public void Count()
        {
            Value++;
            Write();
        }

        public void ResetCount()
        {
            Value = 0;
            Write();
        }

        private void Write()
        {
            if (Text != null) Text.text = Label + ": " + Value;
        }
    }
}
