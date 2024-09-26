using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;

public class SpeedOmeterScript : MonoBehaviour
{
    public Transform SpeedNeedle;
    public const float Maxangle =50f ;
    public const float MinAngle =-250f;
    public float MaxSpeed;
    public float Speed;
    public float DistanceAngle;
    public float new_angle;
    public float Cspeed;
    public Quaternion Target;
    public Vector3 Target2;
    public VirtualTwin.Vehicle subject;
    public List<float> speeds = new List<float>();
    public Rigidbody NathSpeed;
    
    // Start is called before the first frame update
    void Awake()
    { 
        
        GameObject a = GameObject.Find("Car");
        VirtualTwin.Vehicle NathScript = a.GetComponent<VirtualTwin.Vehicle>();
        MaxSpeed = NathScript.topSpeed;
        NathSpeed = a.GetComponent<Rigidbody>();
       
        DistanceAngle = Mathf.Abs(Maxangle) -(MinAngle);
        GameObject needtarget = GameObject.Find("Needle");
        SpeedNeedle = needtarget.transform;
        
    }

    // Update is called once per frame
    void Update()
    {
        Cspeed = Vector3.Magnitude(NathSpeed.velocity);
        if (Cspeed <= 0)
        {
            new_angle = MinAngle;
        }
        else
        {
         
            new_angle=(((Cspeed/MaxSpeed))*DistanceAngle); 
        }
        if(Cspeed>MaxSpeed)
        {
            new_angle = Maxangle-MinAngle;
        }

        Vector3 axis = Vector3.forward;
         Target = Quaternion.Euler(0, 0, -(MinAngle+new_angle));
        Target2 = Target.eulerAngles;
          SpeedNeedle.rotation=Target;

        speeds.Add(Cspeed);
        
    }
    void OnApplicationQuit()
    {
        string path = PathMethod();
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine("Speed");

        for(int i=0; i < speeds.Count;++i)
        {
            writer.WriteLine(speeds[i]);
        }
        writer.Flush();
        writer.Close();
    }
    private string PathMethod()
    {
        return Application.dataPath + "/Data" + "TestDdata.csv";
    }
}
