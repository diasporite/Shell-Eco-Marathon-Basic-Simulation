using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualTwin
{
    public class HUD : MonoBehaviour
    {
        [Header("Text")]
        public Text timeElapsed;
        public Text vehicleSpeed;
        public Text distanceTravelled;
        public Text numSamples;

        public void UpdateUI(int numSample, DataPoint sample)
        {
            if (sample == null) sample = new DataPoint(0, 0, 0);

            timeElapsed.text = "Time Elapsed, t = " + 
                sample._time.ToString("0.00") + "s";
            vehicleSpeed.text = "Vehicle Speed, |v| = " + 
                sample._speed.ToString("0.000") + "m/s";
            distanceTravelled.text = "Distance Travelled, s = " + 
                sample._distance.ToString("0.000") + "m";
            numSamples.text = "# of Data Samples, n = " + numSample;
        }
    }
}