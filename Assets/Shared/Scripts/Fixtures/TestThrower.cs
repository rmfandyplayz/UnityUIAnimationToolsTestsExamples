using System;
using UnityEngine;

namespace UIAnimLab
{
    /// <summary>
    /// A fixture for Custom Property steps: a property whose getter always throws. The step using it should
    /// warn once and be skipped, and every other step on the same player should still play.
    /// </summary>
    public class TestThrower : MonoBehaviour
    {
        public float Explodes
        {
            get { throw new InvalidOperationException("this getter throws on purpose"); }
            set { }
        }
    }
}
