﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    [RequireComponent(typeof(Rigidbody))]
    public class Wheel : MonoBehaviour
    {
        public bool driving = true;

        public float mass = 5;
        public float radius = 1;

        public float torque = 1;
        public float rollingRes = 0.5f;

        // Rotation about axis, in degrees
        public float toe = 0;
        public float camber = 0;

        Vehicle vehicle;

        Rigidbody rb;

        public Vector3 Forward
        {
            get => transform.forward;
            set => transform.forward = value;
        }

        // Returns direction of travel of wheel
        public Vector3 Drive
        {
            get
            {
                if (driving) return Forward;
                return vehicle.Forward;
            }
        }

        private void Awake()
        {
            vehicle = GetComponentInParent<Vehicle>();

            rb = GetComponent<Rigidbody>();

            radius = 0.5f * GetComponent<BoxCollider>().size.y;
        }
    }
}