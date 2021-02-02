using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotator : MonoBehaviour
{

    public Vector3 rotationSpeed;
    
    void Update()
    {
        transform.Rotate(rotationSpeed);
    }
}
