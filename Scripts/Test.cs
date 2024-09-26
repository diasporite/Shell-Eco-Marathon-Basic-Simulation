using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float rotation;
    public float eulerAngle;

    // Update is called once per frame
    void Update()
    {
        rotation = transform.rotation.y;
        eulerAngle = transform.eulerAngles.y;
    }
}
