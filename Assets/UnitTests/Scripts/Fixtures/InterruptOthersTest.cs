using rmf_claude.DOTweenUI;
using TMPro;
using UnityEngine;

namespace UIAnimLab.UnitTests
{
    /// <summary>
    /// Play-mode fixture for Interrupt Others and Stop(complete). The edit-mode preview never reaches the
    /// player's interrupt code, so this only means anything in play mode.
    ///
    /// Travel moves the runner across, then shows DONE with a jingle, and its On Complete counts here.
    /// Travel Endless swings back and forth forever. The three interrupt buttons each play a small punch on
    /// the runner, set to STOP, COMPLETE + REPORT or COMPLETE + SILENT beside its Interrupt Others. The white
    /// line shows what happened, the yellow line what should have. The buttons are wired in the Inspector.
    /// </summary>
    public class InterruptOthersTest : MonoBehaviour
    {
        private const string Travel = "Travel";
        private const string TravelEndless = "Travel Endless";

        public UIAnimationPlayer Player;
        public RectTransform Runner;
        public GameObject Done;
        public TMP_Text Status;
        public TMP_Text Expect;

        private string current;
        private string ended = "-";
        private int plays;
        private int onCompleteCount;
        private int onCompleteAtPress;

        /// <summary>Wired to Travel's On Complete.</summary>
        public void CountOnComplete()
        {
            onCompleteCount++;
        }

        public void PlayTravel() { Begin(Travel); }
        public void PlayEndless() { Begin(TravelEndless); }

        public void Stop()
        {
            EndCurrent(running => Player.Stop(running),
                "Stops where it is. No DONE, no jingle. On Complete +0, ends Stopped.",
                "Stops where it is. On Complete +0, ends Stopped.");
        }

        public void StopComplete()
        {
            EndCurrent(running => Player.Stop(running, true),
                "Jumps to the right end and DONE shows, with NO jingle. On Complete +1, ends Completed.",
                "An endless loop has no end: stops where it is. On Complete +0, ends Stopped.");
        }

        public void StopAllComplete()
        {
            EndCurrent(running => Player.StopAll(true),
                "Jumps to the right end and DONE shows, with NO jingle. On Complete +1, ends Completed.",
                "An endless loop has no end: stops where it is. On Complete +0, ends Stopped.");
        }

        public void InterruptStop()
        {
            Interrupt("Interrupt STOP",
                "Stops where it is, then the runner punches. No DONE. On Complete +0, ends Interrupted.",
                "Stops where it is, then the runner punches. On Complete +0, ends Interrupted.");
        }

        public void InterruptReport()
        {
            Interrupt("Interrupt COMPLETE REPORT",
                "Jumps to the right end, DONE shows, NO jingle, then the runner punches. On Complete +1, ends Completed.",
                "An endless loop has no end: stops where it is, then punches. On Complete +0, ends Interrupted.");
        }

        public void InterruptSilent()
        {
            Interrupt("Interrupt COMPLETE SILENT",
                "Jumps to the right end, DONE shows, NO jingle, then the runner punches. On Complete +0, ends Interrupted.",
                "An endless loop has no end: stops where it is, then punches. On Complete +0, ends Interrupted.");
        }

        private void Begin(string animationName)
        {
            // Playing an animation that is already running always stops it where it stands, whatever its
            // Interrupt Others buttons say - they are about the others.
            if (current == animationName && Player.IsPlaying(animationName))
            {
                Press(animationName == Travel
                    ? "Already running: stops where it is and starts over from the left. On Complete +0."
                    : "Already running: stops where it is and starts swinging again from the left.");
            }
            else
            {
                Press(animationName == Travel
                    ? "Crosses in 2.5 s, then DONE shows with a jingle. On Complete +1, ends Completed."
                    : "Swings back and forth until something ends it.");
            }

            current = animationName;
            ended = "-";

            // Only the latest play's end is shown: restarting reports Interrupted for the play it replaces.
            int play = ++plays;
            Player.Play(animationName, reason =>
            {
                if (play == plays) ended = reason.ToString();
            });
        }

        private void EndCurrent(System.Action<string> end, string whenTravel, string whenEndless)
        {
            if (Running(whenTravel, whenEndless)) end(current);
        }

        private void Interrupt(string animationName, string whenTravel, string whenEndless)
        {
            Running(whenTravel, whenEndless);
            Player.Play(animationName);
        }

        private bool Running(string whenTravel, string whenEndless)
        {
            bool running = current != null && Player.IsPlaying(current);

            if (!running) Press("Nothing is running - press Play Travel or Play Endless first.");
            else Press(current == Travel ? whenTravel : whenEndless);

            return running;
        }

        private void Press(string expected)
        {
            onCompleteAtPress = onCompleteCount;
            Expect.text = "Expect: " + expected;
        }

        private void Update()
        {
            string state = current == null ? "nothing played yet"
                : current + (Player.IsPlaying(current) ? ": running" : ": ended " + ended);

            Status.text = state
                + "     On Complete: " + onCompleteCount + " (+" + (onCompleteCount - onCompleteAtPress) + ")"
                + "     DONE: " + (Done.activeSelf ? "shown" : "hidden")
                + "     runner x: " + Mathf.RoundToInt(Runner.anchoredPosition.x);
        }
    }
}
