﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    [RequireComponent(typeof(Rigidbody))]
    public class Vehicle : MonoBehaviour
    {
        [Header("Constants")]
        public float topSpeed = 10;
        public float bodyLength = 2;
        public float length_fcm = 1;    // Length between front and centre of mass (l_f)
        public float length_bcm = 1;    // Length between back and centre of mass (l_r)
        public float bodyMass = 200;
        public float rideHeight = 0.25f;
        public float surfaceArea = 0.2f;

        [Range(0.001f, 0.1f)]
        public float stationaryThreshold = 0.04f;

        [Header("Coefficients")]
        [Range(0f, 1f)] public float liftCoefficent = 0.1f;
        [Range(0f, 1f)] public float dragCoefficent = 0.1f;

        [Header("Environment")]
        public float airDensity = 1000f;

        [Header("Components")]
        public Wheel frontWheel;
        public Wheel backWheel;
        public BoxCollider undercarriage;

        Rigidbody rb;

        [Header("Variables")]
        [SerializeField] float speed;
        [SerializeField] float distance;
        [SerializeField] float driveAngle = 0;  // beta in vehicle dynamics doc
        [SerializeField] Vector3 velocity = new Vector3(0, 0, 0);
        [SerializeField] float driveForce = 0;
        [SerializeField] float liftForce = 0;
        [SerializeField] float dragForce = 0;
        [SerializeField] float dragAcceleration = 0;

        public float Speed => speed;
        public float Distance => distance;

        public Rigidbody Rb => rb;

        public float VehicleMass => bodyMass + frontWheel.mass + backWheel.mass;

        public float InverseVehicleMass => 1 / VehicleMass;

        public bool Stationary => speed <= stationaryThreshold;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            //accelerator = GetComponent<Accelerator>();
            //steering = GetComponent<Steering>();

            bodyLength = length_fcm + length_bcm;
        }

        private void Start()
        {
            rb.mass = bodyMass;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.angularDrag = 0;

            undercarriage.size = new Vector3(0.1f, rideHeight, 0.1f);
        }

        private void Update()
        {
            //velocity = CalcDriveDir();

            //accelerator.Accelerate();
            //accelerator.Accelerate(velocity);

            Steer();
            Accelerate();
        }

        private void FixedUpdate()
        {
            Drive();

            LogData();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, 5f * transform.forward);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, 5f * velocity.normalized);
        }

        void Steer()
        {
            var input = Input.GetAxisRaw("Horizontal");
            frontWheel.SteerWheel(input * Time.deltaTime);
        }

        void Accelerate()
        {
            var sign = 0;

            if (Input.GetKey("j")) sign = 1;
            else if (Input.GetKey("l")) sign = -1;

            frontWheel.DriveWheel(sign, Time.deltaTime);
        }

        void Drive()
        {
            //transform.forward = driveDir;

            // Get velocity of front wheel
            var v = frontWheel.Velocity;

            // Calculate velocity and angle of body
            velocity = CalculateVelocity(v);
            driveAngle = Vector3.SignedAngle(velocity, transform.forward, transform.up);

            // Account for drag (placeholder)
            //velocity *= (1 - dragCoefficent);
            dragForce = 0.5f * airDensity * velocity.sqrMagnitude * surfaceArea;
            dragAcceleration = dragForce * InverseVehicleMass;

            // Apply to rb
            speed -= dragAcceleration * Time.fixedDeltaTime;
            rb.velocity = speed * velocity.normalized;

            // Turn vehicle by small amount
            var delta = -driveAngle * Time.fixedDeltaTime;
            //print(driveAngle);
            transform.Rotate(0, delta, 0);
        }

        void LogData()
        {
            speed = rb.velocity.magnitude;
            distance += speed * Time.fixedDeltaTime;
        }

        Vector3 CalculateVelocity(Vector3 frontWheelVelocity)
        {
            var velocity = frontWheelVelocity;
            var dv = Vector3.zero;
            velocity += dv;

            return velocity;
        }
    }
}