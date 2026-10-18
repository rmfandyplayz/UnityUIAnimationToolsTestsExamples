using System.Collections;
using rmf_claude.DOTweenUI;
using UnityEngine;

namespace UIAnimLab
{
    /// <summary>
    /// Small actions for buttons to call in play mode, wired in the Inspector: pausing the game, switching
    /// an object off and on, playing an animation many times in quick succession.
    /// </summary>
    public class TestActions : MonoBehaviour
    {
        public UIAnimationPlayer Player;
        public GameObject Toggled;

        public void TogglePause()
        {
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
        }

        public void ToggleObject()
        {
            if (Toggled != null) Toggled.SetActive(!Toggled.activeSelf);
        }

        public void DestroyObject()
        {
            if (Toggled != null) Destroy(Toggled);
        }

        /// <summary>Plays the animation 20 times, one every 50 ms, without waiting for any to finish.</summary>
        public void Spam(string animation)
        {
            StartCoroutine(SpamRoutine(animation));
        }

        private IEnumerator SpamRoutine(string animation)
        {
            for (int i = 0; i < 20; i++)
            {
                Player.Play(animation);
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }

        public void StopAll()
        {
            Player.StopAll();
        }

        public void StopAnimation(string animation)
        {
            Player.Stop(animation);
        }

        public void StopComplete(string animation)
        {
            Player.Stop(animation, true);
        }

        private void OnDestroy()
        {
            // Leaving play mode paused would surprise the next test.
            if (Time.timeScale == 0f) Time.timeScale = 1f;
        }
    }
}
