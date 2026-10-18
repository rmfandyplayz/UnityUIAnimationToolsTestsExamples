using rmf_claude.DOTweenUI;
using TMPro;
using UnityEngine;

namespace UIAnimLab.UnitTests
{
    /// <summary>
    /// Play-mode fixture for Play(name, onEnd): every way an animation can end, and the reason it reports.
    /// Each button starts "Slide" (2 s) with an onEnd that writes the reason, then ends it a different way.
    /// The player lives on a child, Mover, so it can be switched off or destroyed and rebuilt.
    /// </summary>
    public class EndReasonTest : MonoBehaviour
    {
        public UIAnimationPlayer Player;
        public GameObject MoverTemplate;
        public TMP_Text Status;
        public TMP_Text Expect;

        private int ends;
        private string log = "";

        private void Start()
        {
            Write("press a button", "");
        }

        private void Arm(string expected)
        {
            log = "";
            ends = 0;
            Expect.text = "Expect: " + expected;
            Player.Play("Slide", reason =>
            {
                ends++;
                log += (log.Length > 0 ? ", " : "") + reason;
                Write(log, null);
            });
            Write("playing...", null);
        }

        public void Completed()
        {
            Arm("Completed, once, when the slide reaches the right after 2 s.");
        }

        public void Stopped()
        {
            Arm("Stopped, once, straight away; the box stays where it was.");
            Player.Stop("Slide");
        }

        public void StoppedComplete()
        {
            Arm("Completed, once, straight away; the box jumps to the right.");
            Player.Stop("Slide", true);
        }

        public void Interrupted()
        {
            Arm("Interrupted, once, straight away (Other is set to STOP), then Other wiggles the box.");
            Player.Play("Other");
        }

        public void Replayed()
        {
            Arm("Interrupted, once, for the first play when the second starts; the second then finishes with Completed. Two lines in total: Interrupted, Completed.");
            Player.Play("Slide", reason =>
            {
                ends++;
                log += (log.Length > 0 ? ", " : "") + reason;
                Write(log, null);
            });
        }

        public void Disabled()
        {
            Arm("Disabled, once; the box stays hidden until you press Show Mover.");
            Player.gameObject.SetActive(false);
        }

        public void ShowMover()
        {
            Player.gameObject.SetActive(true);
            Write("Mover shown again", "");
        }

        public void Destroyed()
        {
            // An enabled player is turned off before it is destroyed, and nothing says a destroy is coming.
            Arm("Disabled (not Destroyed - Unity disables an enabled object first), once. A fresh Mover appears.");
            var parent = Player.transform.parent;
            var copy = Instantiate(MoverTemplate, parent);
            copy.name = "Mover";
            copy.SetActive(true);
            Destroy(Player.gameObject);
            Player = copy.GetComponent<UIAnimationPlayer>();
        }

        public void NotFound()
        {
            log = "";
            Expect.text = "Expect: NotFound, once, straight away - a caller is never left waiting on a mistyped name.";
            Player.Play("Nope", reason =>
            {
                log += (log.Length > 0 ? ", " : "") + reason;
                Write(log, null);
            });
        }

        private void Write(string status, string expect)
        {
            Status.text = "onEnd: " + status;
            if (expect != null) Expect.text = expect.Length == 0 ? "" : "Expect: " + expect;
        }
    }
}
