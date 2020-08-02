using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera/3RDPerson Camera")]
public class PlayerCamera : MonoBehaviour {

    public Transform target;
    // The distance in the x-z plane to the target
    public float xDistance = 2;
    public float yDistance = 15;
    public float zDistance = 2;

    // the height we want the camera to be above the target
    public float height = 5;
    // How much we
    public float damping = 3;
    public float rotationDamping = 3;

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {
        if (target){
            // Calculate the current rotation angles
            float wantedRotationAngle = target.eulerAngles.y;
            float wantedY = target.position.y + height;
            float wantedX = target.position.x + xDistance;
            float wantedZ = target.position.z + zDistance;

            float currentRotationAngle = transform.eulerAngles.y;
            float currentX = transform.position.x;
            float currentY = transform.position.y;
            float currentZ = transform.position.z;

            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

            // Damp the height
            currentY = Mathf.Lerp (currentY, wantedY, damping * Time.deltaTime);
            currentX = Mathf.Lerp (currentX, wantedX, damping * Time.deltaTime);
            currentZ = Mathf.Lerp (currentZ, wantedZ, damping * Time.deltaTime);

            // Convert the angle into a rotation
            Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target

            Vector3 pos = target.position;
            pos.y = currentY;
            pos.x = currentX;
            pos.z = currentZ;
            //pos -= currentRotation * Vector3.forward * yDistance;
            transform.position = pos;


            // Always look at the target
            transform.LookAt (target);
        }
    }


}