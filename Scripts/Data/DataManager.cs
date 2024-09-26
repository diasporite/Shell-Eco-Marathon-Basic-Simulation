using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    public class DataManager : MonoBehaviour
    {
        [SerializeField] bool recording = false;

        public Vehicle subject;
        public HUD hud;

        [SerializeField] float timeElapsed = 0;
        [Range(1, 5)]
        [SerializeField] int sampleEveryFixedFrame = 1;
        int framesSinceLog = 0;

        [SerializeField] List<DataPoint> data = new List<DataPoint>();

        public DataPoint LastSample
        {
            get
            {
                if (data.Count > 0)
                    return data[data.Count - 1];
                return null;
            }
        }

        private void Start()
        {
            timeElapsed = 0;
        }

        private void FixedUpdate()
        {
            TickFixed();
        }

        void TickFixed()
        {
            timeElapsed += Time.fixedDeltaTime;

            if (recording)
            {
                framesSinceLog++;
                if (framesSinceLog >= sampleEveryFixedFrame)
                {
                    LogData(subject.Speed, subject.Distance);
                    hud.UpdateUI(data.Count, LastSample);
                    framesSinceLog = 0;
                }
            }
        }

        public void StartRecording()
        {
            data.Clear();

            recording = true;
        }

        public void StopRecording()
        {
            recording = false;
        }

        public void LogData(float speed, float distance)
        {
            data.Add(new DataPoint(timeElapsed, speed, distance));
        }

        public void LogData(float time, float speed, float distance)
        {
            data.Add(new DataPoint(time, speed, distance));
        }
    }
}