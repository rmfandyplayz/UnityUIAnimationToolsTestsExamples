using rmf_claude.DOTweenUI;
using TMPro;
using UnityEngine;

namespace UIAnimLab.UnitTests
{
    /// <summary>
    /// Play-mode fixture for SetTo / SetFrom / ClearOverrides.
    ///
    /// Three animations, one Custom Property step each: Level (the float property below, drawn as the bar),
    /// Coins (the int property below, which has its From ticked) and Message (the text's own .text, typed
    /// out). Each button sets a step's To - and for Coins its From - from code, then plays it. The Inspector
    /// values are 100, 0 to 50 and "Authored in the Inspector.", which Clear goes back to. The white line
    /// shows what the calls returned, the yellow line what should happen.
    ///
    /// The coin total is kept apart from the number on screen, the way a game would keep it. Playing an
    /// animation that is already running stops it where it stands, so a click mid-count sees the
    /// half-counted number on screen; counting from there to the real total is what makes every click add 7.
    /// </summary>
    public class SetToFromTest : MonoBehaviour
    {
        private static readonly string[] Messages =
        {
            "Set from code.",
            "A second message, a little longer than the first.",
            "<b>Rich text</b> tags are never shown half typed.",
            "Short.",
        };

        public UIAnimationPlayer Player;
        public RectTransform LevelFill;
        public TMP_Text LevelLabel;
        public TMP_Text CoinsLabel;
        public TMP_Text Status;
        public TMP_Text Expect;
        [SerializeField] private float level = 60f;
        [SerializeField] private int coins = 12;

        private float levelTarget;
        private int coinTotal;
        private int messageIndex;

        public float Level
        {
            get { return level; }
            set { level = Mathf.Clamp(value, 0f, 100f); Redraw(); }
        }

        public int Coins
        {
            get { return coins; }
            set { coins = Mathf.Max(0, value); Redraw(); }
        }

        private void Awake()
        {
            levelTarget = level;
            coinTotal = coins;
        }

        public void LevelDown() { ChangeLevel(-25f); }
        public void LevelUp() { ChangeLevel(25f); }

        private void ChangeLevel(float delta)
        {
            levelTarget = Mathf.Clamp(levelTarget + delta, 0f, 100f);

            bool ok = Player.SetTo("Level", 0, levelTarget);
            Player.Play("Level");

            Show("SetTo(\"Level\", 0, " + levelTarget + ") returned " + ok,
                "the bar slides from where it is to " + levelTarget + ", not to the Inspector's 100.");
        }

        public void AddCoins()
        {
            int from = coins;
            coinTotal += 7;

            bool okFrom = Player.SetFrom("Coins", 0, (float)from);
            bool okTo = Player.SetTo("Coins", 0, (float)coinTotal);
            Player.Play("Coins");

            Show("SetFrom(\"Coins\", 0, " + from + ") returned " + okFrom + ", SetTo(..., " + coinTotal + ") returned " + okTo,
                "counts " + from + " to " + coinTotal + " in whole numbers. Click again mid-count: it carries on from where it has got to, and still lands 7 higher per click.");
        }

        public void NextMessage()
        {
            string text = Messages[messageIndex];
            messageIndex = (messageIndex + 1) % Messages.Length;

            bool ok = Player.SetTo("Message", 0, text);
            Player.Play("Message");

            Show("SetTo(\"Message\", 0, \"" + text + "\") returned " + ok, "the message is typed over the old one.");
        }

        public void Clear()
        {
            Player.ClearOverrides("Level");
            Player.ClearOverrides("Coins");
            Player.ClearOverrides("Message");
            levelTarget = 100f;
            coinTotal = 50;

            Player.Play("Level");
            Player.Play("Coins");
            Player.Play("Message");

            Show("ClearOverrides on all three, then played them",
                "the Inspector values: the bar goes to 100, coins count 0 to 50, \"Authored in the Inspector.\"");
        }

        public void Refused()
        {
            bool wrongKind = Player.SetTo("Level", 0, Color.red);
            bool fromOff = Player.SetFrom("Level", 0, 10f);
            bool noStep = Player.SetTo("Level", 5, 10f);

            Show("wrong kind returned " + wrongKind + ", From off returned " + fromOff + ", no step 5 returned " + noStep,
                "all three False, nothing moves, and three warnings in the Console saying why.");
        }

        private void Show(string happened, string expected)
        {
            Status.text = happened;
            Expect.text = "Expect: " + expected;
        }

        public void Redraw()
        {
            if (LevelFill != null) LevelFill.anchorMax = new Vector2(level / 100f, 1f);
            if (LevelLabel != null) LevelLabel.text = "Level " + Mathf.RoundToInt(level);
            if (CoinsLabel != null) CoinsLabel.text = coins + " coins";
        }
    }
}
