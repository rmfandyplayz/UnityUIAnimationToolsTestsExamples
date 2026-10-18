using System;
using System.Collections;
using System.Collections.Generic;
using rmf_claude.DOTweenUI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UIAnimLab.Showcase
{
    /// <summary>
    /// Plays a showcase frame's demonstration on cue, so a GIF can be recorded in one take. Only the frame
    /// the board is focused on listens:
    ///
    ///   Space   put everything back, then play the cues
    ///   R       put everything back
    ///   L       loop: replay the cues every Loop Every seconds, for a seamless GIF
    ///
    /// A cue plays one animation on one player, a number of seconds after Space. Putting back stops every
    /// player in the frame and restores the values each captured when it started (what the Inspector's
    /// play-mode Stop and Restore button does).
    /// </summary>
    public class ShotDirector : MonoBehaviour
    {
        [Serializable]
        public class Cue
        {
            public UIAnimationPlayer Player;
            public string Animation;
            public float At;
        }

        public Station Station;
        public List<Cue> Cues = new List<Cue>();

        [Tooltip("Seconds from Space to the next replay while looping. Leave at 0 to use the last cue's time plus 1.5 s.")]
        public float LoopEvery;

        [Tooltip("Start looping as soon as play mode starts, while this frame is focused.")]
        public bool LoopFromStart;

        private StationBoard board;
        private UIAnimationPlayer[] players;
        private Coroutine running;
        private bool looping;

        private void Awake()
        {
            board = GetComponentInParent<StationBoard>();
            players = GetComponentsInChildren<UIAnimationPlayer>(true);
            looping = LoopFromStart;
        }

        private bool Focused
        {
            get { return board != null && board.FocusedStation == Station; }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (!Focused)
            {
                if (running != null) Stop();
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame) Play();
            if (keyboard.rKey.wasPressedThisFrame) Stop();
            if (keyboard.lKey.wasPressedThisFrame)
            {
                looping = !looping;
                if (looping) Play();
            }

            if (looping && running == null) Play();
        }

        public void Play()
        {
            // Space also submits whichever button was clicked last; deselect it so it isn't pressed again.
            if (UnityEngine.EventSystems.EventSystem.current != null) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            Stop();
            running = StartCoroutine(Run());
        }

        public void Stop()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            PutBack();
        }

        private void PutBack()
        {
            foreach (var player in players)
            {
                if (player == null) continue;
                player.StopAll();
            #if UNITY_EDITOR
                player.EditorRestoreBaselines();
            #endif
            }
        }

        private IEnumerator Run()
        {
            float start = Time.unscaledTime;
            float last = 0f;
            var cues = new List<Cue>(Cues);
            cues.Sort((a, b) => a.At.CompareTo(b.At));

            foreach (var cue in cues)
            {
                while (Time.unscaledTime - start < cue.At) yield return null;
                if (cue.Player != null) cue.Player.Play(cue.Animation);
                last = cue.At;
            }

            if (!looping)
            {
                running = null;
                yield break;
            }

            float every = LoopEvery > 0f ? LoopEvery : last + 1.5f;
            while (Time.unscaledTime - start < every) yield return null;
            running = null;
        }
    }
}
