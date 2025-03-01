﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualTwin
{
    public class Motor : MonoBehaviour
    {
        public Vehicle2 vehicle;
        FuelCell fuelCell;

        [Header("Wheels")]
        public Wheel2 frontLeftWheel;
        public Wheel2 frontRightWheel;
        public Wheel2 backWheel;

        [Header("Engine Variables")]
        public float topRpm = 1000;

        public float currentAngularVelocity;
        public float currentTorque;
        public float currentRpm;
        public float pRpm;

        float gearRatio = 6f;
        float torqueCutoff = 3.57f;

        [Header("Requirements")]
        public float reqVoltage = 24;
        public float reqCurrent = 49.7f;
        public float reqPower;

        [Header("Constants")]
        public float p = 16;
        public float kt = 0.076f;
        public float ke = 0.015f;
        public float kv;
        public float r = 0.035f;

        public float trueVoltage;
        public float xl;

        public float inductance = 0.0000658f;

        public float outputCurrent;
        public float inducedResistance;
        public float resistance = 0.035f;
        public float outputPower;

        [Header("Efficiencies")]
        public float transientEfficiency;
        public float motorEfficiency;
        public float averageMotorEfficiency;

        [Header("Variables")]
        public float pTorque;
        public float energyConsumed;
        public float energyIn;
        public float energyLossDrag;
        public float energyLossBrake;

        public float motorOutEnergy;
        public float energyWastedDrag;
        public float energyWastedBrake;

        public float deltaKe;
        public float usefulEnergy;

        const float RPM_TO_RADPS = Mathf.PI / 30f;
        const float RADPS_TO_RPM = 30f / Mathf.PI;

        public float GearRatio => gearRatio;

        private void Awake()
        {
            vehicle = GetComponent<Vehicle2>();
            fuelCell = GetComponent<FuelCell>();

            reqPower = reqVoltage * reqCurrent;
            currentTorque = torqueCutoff;
            kv = 1 / ke;

            topRpm = reqPower / 8.69f;

            energyConsumed = 0;
            energyWastedDrag = 0;
            motorOutEnergy = 0;
        }

        public void CalculateData(float dt)
        {
            currentAngularVelocity = vehicle.speed / (gearRatio * frontLeftWheel.radius);
            currentRpm = currentAngularVelocity * RADPS_TO_RPM;
            currentRpm = Mathf.Min(currentRpm, topRpm);
            pRpm = currentRpm * gearRatio / topRpm;

            xl = currentRpm * RPM_TO_RADPS * inductance * p;
            trueVoltage = (reqVoltage - (ke * currentAngularVelocity)) / 1.2f;

            inducedResistance = 2 * Mathf.PI * currentAngularVelocity * inductance * outputPower;
            outputCurrent = trueVoltage / 
                Mathf.Sqrt(inducedResistance * inducedResistance + resistance * resistance);
            outputPower = resistance / (2 * Mathf.PI * currentAngularVelocity * outputCurrent);

            currentTorque = kt * outputCurrent * gearRatio;
            pTorque = currentTorque / gearRatio;

            reqPower = reqVoltage * reqCurrent;
            outputPower = currentTorque * currentRpm * RPM_TO_RADPS;
            transientEfficiency = outputPower / reqPower;

            energyIn = vehicle.AccelerateInput > 0 ? reqPower * dt : 0f;
            energyLossDrag = vehicle.dragForce * vehicle.speed * dt;
            energyLossBrake = vehicle.wheelBrakeForce * vehicle.speed * dt;

            energyConsumed += energyIn;
            energyWastedDrag += energyLossDrag;
            energyWastedBrake += energyLossBrake;
            motorOutEnergy = vehicle.AccelerateInput > 0 ? outputPower * dt : 0f;

            usefulEnergy = transientEfficiency * energyConsumed;

            motorEfficiency = energyIn / (energyIn + energyLossDrag + energyLossBrake);
            averageMotorEfficiency = energyConsumed / (energyConsumed + energyWastedDrag + energyWastedBrake);
        }
    }
}