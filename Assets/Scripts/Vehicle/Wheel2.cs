﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    public enum WheelOrientation
    {
        FrontLeft = 0,
        FrontRight = 1,
        Back = 2,
    }

    public class Wheel2 : MonoBehaviour
    {
        [Header("Settings")]
        public WheelOrientation orientation;
        public bool steering = true;
        public bool driving = true;
        public bool enableRollingRes = true;
        public bool enableCorneringRes = true;

        [Header("Components")]
        public GameObject wheelModel;
        public GroundCheck groundCheck;

        [Header("Steering")]
        public float steeringSpeed = 15f;
        public float steeringRatio = 15f;
        public float wheelLock = 15f;

        float lowerLock;
        float upperLock;

        [Header("Dimensions")]
        public float mass = 0.4f;
        public float radius = 0.254f;
        public float tyrePressure_Bar = 5f;

        [Header("Power")]
        public float driveTorque = 8.69f;
        public float brakeTorque = 25f;

        [Header("Resistances")]
        [Range(0f, 1f)] public float rollingResistance = 0.0081f;
        [Range(0f, 1f)] public float corneringResistance = 0.12f;

        [Header("Coefficients")]
        [Range(0f, 1f)] public float brakingCoefficient = 0.5f;

        [Header("Variables - Steering")]
        public float globalAngle = 0;
        public float vehicleRotation = 0;
        public float dtheta = 0;
        public float steerAngle = 0;    // delta in dynamics doc
        public float slipAngle = 0;     // alpha in dynamics doc
        public Vector3 steerDir = new Vector3(0, 0, 1);

        [Header("Variables - Forces")]
        public float vertForce = 0;
        public float drivingForce = 0;
        public float brakingForce = 0;
        public float rollingResForce = 0;
        public float cornerResForce = 0;
        public float resultantForce = 0;

        Vehicle2 vehicle;

        float curvature;
        float vehicleMass;
        float inverseVehicleMass;

        // See vehicle dynamics doc
        float c1 = 148f;
        float c2 = 1000f;

        public Vector3 LocalSteerDir => steerDir;

        private void Awake()
        {
            vehicle = GetComponentInParent<Vehicle2>();
            groundCheck = GetComponentInChildren<GroundCheck>();

            curvature = 1 / radius;

            lowerLock = 360 - wheelLock;
            upperLock = 360 + wheelLock;
        }

        private void Start()
        {
            vehicleMass = vehicle.VehicleMass;
            inverseVehicleMass = vehicle.InverseVehicleMass;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, 1.5f * transform.forward);
            Gizmos.DrawRay(transform.position, 10f * Mathf.Sign(steerAngle) * transform.right);
        }

        public void Steer(float input, float dt)
        {
            if (steering)
            {
                vehicleRotation = vehicle.velocityAngle;
                globalAngle = 360 + Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up);
                //globalAngle = transform.eulerAngles.y;

                if (input != 0)
                {
                    dtheta = input * steeringSpeed * dt;
                    steerAngle += dtheta;
                }
                // Self correcting steering
                // Source: https://carfromjapan.com/article/driving-tips/steering-wheel-returns-to-center-after-turn/
                else
                    steerAngle = Mathf.MoveTowardsAngle(steerAngle, 0, steeringSpeed * dt);


                if (Mathf.Abs(steerAngle) > wheelLock)
                    steerAngle = Mathf.Sign(steerAngle) * wheelLock;

                slipAngle = SlipAngle();

                var angle = transform.localEulerAngles.y + steerAngle;
                if (Mathf.Abs(angle) > wheelLock) angle = Mathf.Sign(angle) * wheelLock;

                transform.localRotation = Quaternion.Euler(0, steerAngle, 0);

            }
        }

        public void Accelerate(float driveInput, float brakeInput, float dt)
        {
            vertForce = VerticalForce();

            driveTorque = vehicle.CurrentTorque;

            drivingForce = DriveForce(driveInput, driveTorque);
            brakingForce = BrakingForce(brakeInput, brakeTorque);
            rollingResForce = RollingResistanceForce();

            cornerResForce = CorneringResistanceForce(); // TBD

            resultantForce = drivingForce - brakingForce - rollingResForce;
        }

        float RollingResistanceForce()
        {
            if (!enableRollingRes) return 0;

            //var speedForce = (0.005f + (0.01f + 0.0095f * (0.01f * 0.01f * vehicle.groundSpeed * vehicle.groundSpeed) / tyrePressure) * weightForce);
            //var speedForce = (0.0095f * (0.01f * 0.01f * vehicle.groundSpeed * vehicle.groundSpeed) / tyrePressure) * weightForce;
            //var speedForce = 0.5f * rollingResistance * vehicleMass * 9.81f * vehicle.speed * vehicle.speed;
            var speedForce = vertForce * (0.0095f * 0.0001f * (1000/3600) * (1000/3600) * 
                vehicle.speed * vehicle.speed / (tyrePressure_Bar * 100000f));

            // Source: https://www.engineeringtoolbox.com/rolling-friction-resistance-d_1303.html
            var flatForce = rollingResistance * vertForce * curvature;

            //if (vehicle.groundSpeed < forceSpeedThreshold) return speedForce;
            if (speedForce < flatForce) return speedForce;

            //return speedForce;
            return flatForce;
        }

        float CorneringResistanceForce()
        {
            if (!enableCorneringRes) return 0;

            var f = c1 * Mathf.Sin(2 * Mathf.Atan2(vertForce, c2)) * slipAngle;
            //return -weightForce * Mathf.Sin(steerAngle * Mathf.Deg2Rad);
            return f;
        }

        float DriveForce(float driveInput, float torque)
        {
            if (driveInput > 0)
                return driveTorque * curvature;

            return 0;
        }

        float BrakingForce(float brakeInput, float torque)
        {
            if (brakeInput > 0)
            {
                var speedForce = 0.5f * brakingCoefficient * vehicleMass * 9.81f * vehicle.speed * vehicle.speed;
                var flatForce = brakeTorque * curvature;

                //if (vehicle.groundSpeed < forceSpeedThreshold) return speedForce;
                if (speedForce < flatForce) return speedForce;

                return flatForce;
            }

            return 0;
        }

        float VerticalForce()
        {
            switch (orientation)
            {
                case WheelOrientation.FrontLeft:
                    return 0.5f * mass * 9.81f * vehicle.RearToCoM / vehicle.WheelSeparation;
                case WheelOrientation.FrontRight:
                    return 0.5f * mass * 9.81f * vehicle.RearToCoM / vehicle.WheelSeparation;
                case WheelOrientation.Back:
                    return mass * 9.81f * vehicle.FrontToCoM / vehicle.WheelSeparation;
                default:
                    return 0;
            }
        }

        float SlipAngle()
        {
            switch (orientation)
            {
                case WheelOrientation.Back:
                    return Mathf.Atan2(-vehicle.WheelSeparation,
                        2 * vehicle.turningRadiusCoM * Mathf.Cos(vehicle.velocityAngle * 
                        Mathf.Deg2Rad)) * Mathf.Rad2Deg;
                default:
                    return steerAngle - Mathf.Atan2(vehicle.WheelSeparation, 
                        2 * vehicle.turningRadiusCoM * Mathf.Cos(vehicle.velocityAngle * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
            }
        }
    }
}