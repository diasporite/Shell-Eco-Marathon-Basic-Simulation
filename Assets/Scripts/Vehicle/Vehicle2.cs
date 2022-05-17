﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    [RequireComponent(typeof(Rigidbody))]
    public class Vehicle2 : MonoBehaviour
    {
        InputManager inputManager;

        [Header("Settings")]
        public bool enableDrag = true;
        public bool enableLift = false;
        public bool enableReaction = false;

        [Header("Constants")]
        public float bodyMass = 40;
        public float driverMass = 50;
        public float frontalArea = 0.39f;

        [Header("Dimensions")]
        [SerializeField] Transform com;
        [SerializeField] Transform cos;

        public Vector3 centreOfMass;
        public Vector3 centreOfSteering;

        [SerializeField] float wheelSeparation = 1f;
        [SerializeField] float rearToCoM;
        [SerializeField] float frontToCoM;

        [SerializeField] bool grounded;

        [Header("Coefficients")]
        [Range(0f, 1f)] public float liftCoefficent = 0.02f;
        [Range(0f, 1f)] public float dragCoefficent = 0.12f;

        [Header("Environment")]
        public float airDensity = 1.225f;

        [Header("Wheels")]
        public Wheel2 frontLeftWheel;
        public Wheel2 frontRightWheel;
        public Wheel2 backWheel;
        Wheel2[] wheels;

        [Header("Other Components")]
        public GameObject vehicleBody;
        public Motor motor;
        public FuelCell fuelCell;
        public BoxCollider undercarriage;

        [Header("Variables - Vectors")]
        public Vector3 velocity;
        public Vector3 velocityDir;
        public Vector3 motionCentre;        // Centre of circular motion at centre of mass
        public Vector3 dirOfCircularMotion;

        [Header("Variables - Speed")]
        public float speed = 0;

        [Header("Variables - Linear")]
        public float distanceTravelled;
        public float turningRadius;
        public float turningRadiusCoM;

        [Header("Variables - Rotation")]
        public float rearAngularVelocity;   // Speed at which back wheel rotates wheen steered
        public float angularVelocity;       // Speed at which vehicle CoM rotates when steered
        public float velocityAngle;         // Angle between vehicle axis (CoM) and direction of velocity (beta)
        public float globalAngle;           // Angle between vehicle axis (CoM) and world z axis

        [Header("Variables - Forces")]
        public float wheelDriveForce = 0;
        public float wheelBrakeForce = 0;
        public float wheelRollRes = 0;
        public float dragForce = 0;
        public float liftForce = 0;
        public float resultantDriveForce = 0;

        [Header("Variables - Lateral Forces")]
        public float corneringResistanceForce = 0;
        public float centripetalForce = 0;
        public float lateralForce = 0;
        public float tensionForce = 0;
        public float resultantLateralForce = 0;

        [Header("Variables - Acceleration")]
        public float resultantAcceleration = 0;
        public float liftAcceleration = 0;

        [Header("Variables - Motor & Fuel Cell")]
        [SerializeField] float currentRpm;
        [SerializeField] float currentTorque;

        [Header("Inputs")]
        [SerializeField] float steerInput = 0;
        [SerializeField] float accelerateInput = 0;
        [SerializeField] float brakeInput = 0;

        float gravity = 9.81f;

        Rigidbody rb;

        public Rigidbody Rb => rb;

        public float WheelSeparation => wheelSeparation;
        public float RearToCoM => rearToCoM;
        public float FrontToCoM => frontToCoM;

        public float SteerInput => steerInput;
        public float AccelerateInput => accelerateInput;
        public float BrakeInput => brakeInput;

        public float VehicleMass => bodyMass + driverMass + frontLeftWheel.mass +
            frontRightWheel.mass + backWheel.mass + fuelCell.TotalMass;

        public float InverseVehicleMass => 1 / VehicleMass;

        public bool Stationary => speed <= 0.01f;

        public float CurrentRpm => currentRpm;
        public float CurrentTorque => currentTorque;

        private void Awake()
        {
            inputManager = GetComponent<InputManager>();

            rb = GetComponent<Rigidbody>();

            motor = GetComponent<Motor>();
            fuelCell = GetComponent<FuelCell>();

            wheels = new Wheel2[] { frontLeftWheel, frontRightWheel, backWheel };

            wheelSeparation = (cos.position - backWheel.transform.position).z;

            rearToCoM = com.localPosition.z - backWheel.transform.localPosition.z;
            frontToCoM = cos.localPosition.z - com.localPosition.z;
        }

        private void Start()
        {
            rb.mass = VehicleMass;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.angularDrag = 0;

            undercarriage.size = new Vector3(0.1f, 0.25f, 0.1f);
        }

        private void Update()
        {
            GetInputs();
        }

        private void FixedUpdate()
        {
            Drive();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position + wheelSeparation * transform.forward,
                1f * transform.forward);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cos.position, 0.15f);
            Gizmos.DrawRay(cos.position, 10f * (motionCentre - cos.position).normalized);
            Gizmos.DrawRay(frontLeftWheel.transform.position, 
                frontRightWheel.transform.position - frontLeftWheel.transform.position);

            if (rb != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(com.position, 2.5f * rb.velocity.normalized);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(com.position, turningRadiusCoM * dirOfCircularMotion);
            Gizmos.DrawWireSphere(motionCentre, 0.4f);

            Gizmos.color = Color.white;
            Gizmos.DrawRay(com.position, 3f * velocityDir);
        }

        void GetInputs()
        {
            steerInput = inputManager.Steer;

            accelerateInput = inputManager.Accelerate;
            brakeInput = inputManager.Brake;
        }

        bool IsGrounded()
        {
            foreach (var wheel in wheels)
                if (!wheel.groundCheck.IsGrounded) return false;

            return true;
        }

        void Drive()
        {
            GetVariables();

            GetPower();

            CalculateWheelVariables();

            CalculateVariables();
            ApplyVariables();

            motor.CalculateData(Time.fixedDeltaTime);
            fuelCell.CalculateFuelUsage(accelerateInput, Time.fixedDeltaTime);
        }

        //void CalculateCentreOfMotion()
        //{
        //    var zeta = frontLeftWheel.steerAngle;

        //    if (zeta != 0)
        //    {
        //        var r0 = wheelSeparation / Mathf.Tan(zeta * Mathf.Deg2Rad);
        //        motionCentre = transform.position + r0 * transform.right;
        //        turningRadiusCoM = r0 / Mathf.Cos(velocityAngle * Mathf.Deg2Rad);

        //        dirOfCircularMotion = (motionCentre - centreOfMass).normalized;
        //        dirOfCircularMotion.y = 0;

        //        centripetalForce = VehicleMass * speed * speed / turningRadiusCoM;
        //        lateralForce = centripetalForce * Mathf.Cos(Mathf.Atan(dirOfCircularMotion.z / dirOfCircularMotion.x));
        //        tensionForce = centripetalForce * Mathf.Sin(Mathf.Atan(dirOfCircularMotion.z / dirOfCircularMotion.x));
        //    }
        //}

        void GetPower()
        {
            // Calculate fuel usage based on accelerate input
        }

        void GetVariables()
        {
            velocity = rb.velocity;
            velocity.y = 0;
            speed = velocity.magnitude;

            grounded = IsGrounded();

            centreOfMass = com.position;
            centreOfSteering = cos.position;

            currentTorque = motor.currentTorque;
            currentRpm = motor.currentRpm;
        }

        void CalculateWheelVariables()
        {
            float drive = 0;
            float brake = 0;
            float roll = 0;
            float corn = 0;

            foreach (var wheel in wheels)
            {
                wheel.driveTorque = 0.5f * motor.currentTorque;

                wheel.Steer(steerInput, Time.fixedDeltaTime);
                wheel.Accelerate(accelerateInput, brakeInput, Time.fixedDeltaTime);

                drive += wheel.drivingForce;
                brake += wheel.brakingForce;
                roll += wheel.rollingResForce;
                corn += wheel.cornerResForce;
            }

            wheelDriveForce = drive;
            wheelBrakeForce = brake;
            wheelRollRes = roll;
            corneringResistanceForce = corn;
        }

        void CalculateVariables()
        {
            // ================ LINEAR MOTION ================ 
            dragForce = 0.5f * airDensity * speed * speed * dragCoefficent * frontalArea;
            liftForce = 0.5f * airDensity * speed * speed * liftCoefficent * frontalArea;

            float drag = 1f;

            if (!enableDrag) drag = 0;

            resultantDriveForce = wheelDriveForce - wheelBrakeForce - wheelRollRes - drag * dragForce;
            resultantAcceleration = resultantDriveForce * InverseVehicleMass;
            liftAcceleration = liftForce * InverseVehicleMass;

            speed += resultantDriveForce * InverseVehicleMass * Time.fixedDeltaTime;
            distanceTravelled += speed * Time.fixedDeltaTime;

            // ================ CIRCULAR MOTION ================ 
            var zeta = frontLeftWheel.steerAngle;
            globalAngle = transform.eulerAngles.y;

            var tan_zeta = Mathf.Tan(zeta * Mathf.Deg2Rad);

            // [3, 4]           
            velocityAngle = Mathf.Atan2(rearToCoM * tan_zeta, wheelSeparation) * Mathf.Rad2Deg;

            var cos_velAngle = Mathf.Cos(velocityAngle * Mathf.Deg2Rad);

            rearAngularVelocity = speed * Mathf.Rad2Deg / wheelSeparation;
            angularVelocity = speed * tan_zeta * cos_velAngle * Mathf.Rad2Deg / wheelSeparation;
            globalAngle += angularVelocity * Time.fixedDeltaTime;

            velocityDir.x = Mathf.Sin((globalAngle + velocityAngle) * Mathf.Deg2Rad);
            velocityDir.z = Mathf.Cos((globalAngle + velocityAngle) * Mathf.Deg2Rad);

            if (tan_zeta != 0 && cos_velAngle != 0)
            {
                turningRadius = wheelSeparation / tan_zeta;
                turningRadiusCoM = turningRadius / cos_velAngle;
                centripetalForce = VehicleMass * turningRadiusCoM * (angularVelocity *
                    Mathf.Deg2Rad) * (angularVelocity * Mathf.Deg2Rad);
                motionCentre = backWheel.transform.position + Mathf.Sign(zeta) *
                    turningRadius * backWheel.transform.right;
                dirOfCircularMotion = (motionCentre - centreOfMass).normalized;
                dirOfCircularMotion.y = 0;
            }
        }

        void ApplyVariables()
        {
            rb.mass = VehicleMass;

            rb.rotation = Quaternion.Euler(0, globalAngle, 0);

            rb.velocity = speed * transform.forward;

            if (enableLift)
                rb.AddRelativeForce(liftForce * transform.up, ForceMode.Force);

            if (enableReaction && rb.useGravity && grounded)
                rb.AddRelativeForce(VehicleMass * 9.81f * transform.up, ForceMode.Force);
        }

        Vector3 GetRelCentreOfMass()
        {
            var com = Vector3.zero;

            com += (bodyMass + driverMass + fuelCell.TotalMass) * vehicleBody.transform.localPosition;

            foreach (var wheel in wheels) com += wheel.mass * wheel.transform.localPosition;

            com *= InverseVehicleMass;

            return com;
        }
    }
}