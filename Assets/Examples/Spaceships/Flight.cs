using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flight : MonoBehaviour
{
    private Rigidbody rb;
    private TrailRenderer tr;
    public float turnAmount = 1f;
    public float thrustAmount = 1f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tr = GetComponentInChildren<TrailRenderer>();
    }

    void LateUpdate()
    {
        tr.emitting = false;

        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(Vector3.up * turnAmount);
            rb.AddTorque(Vector3.left * (turnAmount * 0.1f));
        }
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(Vector3.down * turnAmount);
            rb.AddTorque(Vector3.right * (turnAmount * 0.1f));
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddTorque(Vector3.up * turnAmount);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddTorque(Vector3.down * turnAmount);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(transform.rotation * Vector3.forward * thrustAmount);
            tr.emitting = true;
        }
        if (Input.GetKey(KeyCode.Return))
        {
            GetComponent<RandomSpaceshipGenerator>().Generate();
        }


    }
}
