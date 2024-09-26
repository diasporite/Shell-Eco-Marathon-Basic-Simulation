using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    public class Accelerator : MonoBehaviour
    {
        [Header("Input")]
        public string accelerateKey = "j";
        public string brakeKey = "l";

        [Header("Properties")]
        public float acceleration = 20f;
        [SerializeField] float speed = 0;

        [Header("Variables")]
        [SerializeField] Vector3 velocity;

        float topSpeed;

        Vehicle vehicle;
        Vector3 ds;

        Rigidbody rb;

        private void Awake()
        {
            vehicle = GetComponentInParent<Vehicle>();

            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            topSpeed = vehicle.topSpeed;

            rb.velocity = Vector3.zero;
            speed = 0;
        }

        private void FixedUpdate()
        {
            velocity = rb.velocity;
        }

        public float Accelerate()
        {
            var sign = 0;

            if (Input.GetKey(accelerateKey))
            { sign = 1; }
            else if (Input.GetKey(brakeKey))
            { sign = -1; }

            speed += sign * acceleration * Time.deltaTime;

            if (speed > topSpeed) speed = topSpeed;
            if (speed < 0) speed = 0;

            return speed;
        }

        public void Accelerate(Vector3 drive)
        {
            if (Input.GetKey(accelerateKey))
            {
                ds = acceleration * drive * Time.deltaTime;
                rb.velocity += ds;

                if (rb.velocity.sqrMagnitude > vehicle.topSpeed * vehicle.topSpeed)
                    rb.velocity = rb.velocity.normalized * vehicle.topSpeed;
            }
            else if (Input.GetKey(brakeKey))
            {
                ds = -acceleration * drive * Time.deltaTime;
                rb.velocity += ds;
                if (Vector3.Dot(vehicle.transform.forward, rb.velocity) < 0)
                    rb.velocity = Vector3.zero;
            }
        }
    }
}