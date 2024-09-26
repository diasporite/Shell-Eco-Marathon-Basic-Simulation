using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    [System.Serializable]
    public class DataPoint
    {
        [SerializeField] float time;
        [SerializeField] float speed;
        [SerializeField] float distance;

        public float _time => time;
        public float _speed => speed;
        public float _distance => distance;

        public DataPoint(float t, float v, float s)
        {
            time = t;
            speed = v;
            distance = s;
        }
    }
}