using rmf_claude.DOTweenUI;
using UnityEngine;

namespace UIAnimLab
{
    /// <summary>
    /// Plays an animation on the UI Animation Player beside it whenever this object is enabled in play mode,
    /// or only the first time. For tiles that should be moving as soon as play mode starts.
    /// </summary>
    [RequireComponent(typeof(UIAnimationPlayer))]
    public class PlayOnEnable : MonoBehaviour
    {
        public string Animation;
        public bool OnlyOnce;

        private bool played;

        private void OnEnable()
        {
            if (OnlyOnce && played) return;
            played = true;
            GetComponent<UIAnimationPlayer>().Play(Animation);
        }
    }
}
