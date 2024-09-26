using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    public class Steering : MonoBehaviour
    {
        [Header("Properties")]
        public float steeringSpeed = 45;
        public float wheelLock = 75f;
        public float steerLock = 75f;

        [Header("Variables")]
        [SerializeField] float frontWheelTurningAngle;
        [SerializeField] float vehicleVelocityAngle;
        [SerializeField] float slipAngle;
        [SerializeField] float steerAngle;

        float turningSpeed;

        float frontWheelMin;
        float frontWheelMax;

        [SerializeField] Vector3 steerDir = new Vector3(0, 0, 0);

        [SerializeField] float dtheta = 0;
        [SerializeField] float dx = 0;

        Vehicle vehicle;

        Wheel frontWheel;
        Wheel backWheel;

        public float FrontWheelTurningAngle => frontWheelTurningAngle;
        public float VehicleVelocityAngle => vehicleVelocityAngle;

        private void Awake()
        {
            vehicle = GetComponentInParent<Vehicle>();

            frontWheel = vehicle.frontWheel;
            backWheel = vehicle.backWheel;

            frontWheelMax = Mathf.Abs(wheelLock);
            frontWheelMin = -Mathf.Abs(wheelLock);
        }

        private void Start()
        {
            steerDir = transform.forward;
        }

        private void Update()
        {
            //print(transform.eulerAngles.y);
            //print(transform.rotation.y);
            //if (Input.GetKey("space")) transform.Rotate(0, vehicleVelocityAngle * Time.deltaTime, 0);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, 5f * steerDir);
        }

        // Calculate direction of travel of body
        public Vector3 SteerDir()
        {
            if (frontWheel.driving)
            {
                var input = Input.GetAxisRaw("Horizontal");

                if (Mathf.Abs(input) > Mathf.Epsilon)
                {
                    dtheta = steeringSpeed * input * Time.deltaTime;

                    // Turn front wheel object
                    frontWheelTurningAngle += dtheta;
                    if (Mathf.Abs(frontWheelTurningAngle) > wheelLock)
                        frontWheelTurningAngle = wheelLock * Mathf.Sign(frontWheelTurningAngle);
                    frontWheel.transform.localRotation = Quaternion.Euler(0, frontWheelTurningAngle, 0);

                    // Temporary - see Vehicle Dynamics doc
                    vehicleVelocityAngle = GetVelocityAngle();
                    slipAngle = 0.2f * frontWheelTurningAngle;
                    steerAngle = frontWheelTurningAngle - slipAngle;

                    steerDir.x = Mathf.Sin(transform.eulerAngles.y + vehicleVelocityAngle * Mathf.Deg2Rad);
                    steerDir.z = Mathf.Cos(transform.eulerAngles.y + vehicleVelocityAngle * Mathf.Deg2Rad);

                    dx = vehicleVelocityAngle * Time.deltaTime;

                    //transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + vehicleVelocityAngle * Time.deltaTime, 0);
                    transform.Rotate(0, dx, 0);
                    //rotation.y += GetEulerRotation(dtheta);
                    //transform.eulerAngles = rotation;

                    return steerDir.normalized;
                }
            }

            return transform.forward;
        }

        public float GetVelocityAngle()
        {
            // Temporary
            return frontWheelTurningAngle;
        }

        float GetEulerRotation(float rot)
        {
            var r = transform.eulerAngles.y;
            r += rot;
            if (r >= 360) r -= 360;
            if (r < 0) r += 360;
            return r;
        }
    }
}