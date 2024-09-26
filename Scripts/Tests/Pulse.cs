using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    public class Pulse
    {
        [SerializeField] float duration;

        public Pulse(float duration)
        {
            this.duration = duration;
        }
    }
}