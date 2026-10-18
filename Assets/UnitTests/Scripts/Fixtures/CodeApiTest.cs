using System.Collections;
using rmf_claude.DOTweenUI;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UIAnimLab.UnitTests
{
    /// <summary>
    /// Play-mode fixture for the rest of the code API: IsPlaying / IsAnyPlaying / Has, ApplyFromState,
    /// CaptureBaseline, waiting on the returned Sequence, Play with an onComplete, and
    /// UIAnimationAudio.SetShared. Buttons are wired in the Inspector.
    /// </summary>
    public class CodeApiTest : MonoBehaviour
    {
        public UIAnimationPlayer Player;
        public RectTransform Box;
        public TMP_Text Probe;
        public TMP_Text Status;
        public TMP_Text Expect;
        public AudioSource MuffledSource;

        private void Update()
        {
            if (Probe == null) return;
            Probe.text = "IsPlaying(\"Pulse\"): " + Player.IsPlaying("Pulse")
                + "   IsPlaying(\"Home\"): " + Player.IsPlaying("Home")
                + "   IsAnyPlaying: " + Player.IsAnyPlaying
                + "\nHas(\"Pulse\"): " + Player.Has("Pulse") + "   Has(\"Nope\"): " + Player.Has("Nope");
        }

        private void Show(string happened, string expected)
        {
            Status.text = happened;
            Expect.text = "Expect: " + expected;
        }

        public void PlayPulse()
        {
            Player.Play("Pulse");
            Show("Play(\"Pulse\")", "IsPlaying(\"Pulse\") and IsAnyPlaying read True while it runs (1.5 s), then False. Has(\"Nope\") is always False.");
        }

        public void ApplyFrom()
        {
            Player.ApplyFromState("Drop In");
            Show("ApplyFromState(\"Drop In\")", "the box jumps up and fades to its From values and stays there - nothing plays. Press Drop In to play it from there.");
        }

        public void DropIn()
        {
            Player.Play("Drop In");
            Show("Play(\"Drop In\")", "the box drops back down to where it rests and fades in.");
        }

        public void MoveRestAndCapture()
        {
            Player.StopAll();
            Box.anchoredPosition += new Vector2(120f, 0f);
            if (Box.anchoredPosition.x > 200f) Box.anchoredPosition = new Vector2(-120f, Box.anchoredPosition.y);
            Player.CaptureBaseline();
            Show("moved the box from code, then CaptureBaseline()", "Drop In and Home now land on the box's new place, not the old one.");
        }

        public void Home()
        {
            Player.Play("Home");
            Show("Play(\"Home\")", "the box goes to its resting place: wherever CaptureBaseline last saw it.");
        }

        public void WaitForIt()
        {
            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            Show("yield return Play(\"Pulse\").WaitForCompletion()...", "this line changes to 'done waiting' when the pulse ends, 1.5 s later.");
            yield return Player.Play("Pulse").WaitForCompletion();
            Status.text = "done waiting";
        }

        public void OnCompleteCallback()
        {
            Show("Play(\"Pulse\", () => ...)", "'callback ran' appears when the pulse finishes on its own. Press Stop All before then and it never appears.");
            Player.Play("Pulse", () => Status.text = "callback ran");
        }

        public void StopAll()
        {
            Player.StopAll();
            Status.text = "StopAll()";
        }

        public void RouteSound()
        {
            UIAnimationAudio.SetShared(MuffledSource);
            Show("UIAnimationAudio.SetShared(muffled source)", "Click now sounds muffled (a low-pass filter on the new shared source), and no UI Animation Audio object is made.");
        }

        public void Click()
        {
            Player.Play("Click");
        }
    }
}
