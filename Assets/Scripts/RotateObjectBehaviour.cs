using UnityEngine;
using System;


public class RotateObjectBehaviour : MonoBehaviour {

    private float sensitivity = 3f;

     // array for storing if the 3 mouse buttons are dragging
     bool[] isDragActive;
 
     // for remembering if a button was down in previous frame
     bool[] downInPreviousFrame;
 
     void Start () {
         isDragActive = new bool[] {false, false, false};
         downInPreviousFrame = new bool[] {false, false, false};
     }
     
     void Update () {
         for (int i=0; i<isDragActive.Length; i++)
         {
             if (Input.GetMouseButton(i))
             {
                 if (downInPreviousFrame[i])
                 {
                     if (isDragActive[i])
                     {
                         OnDragging(i);
                     }
                     else
                     {
                         isDragActive[i] = true;
                         OnDraggingStart(i);
                     }
                 }
                 downInPreviousFrame[i] = true;
             }
             else
             {
                 if (isDragActive[i])
                 {
                     isDragActive[i] = false;
                     OnDraggingEnd(i);
                 }
                 downInPreviousFrame[i] = false;
             }
         }
     }
 
     public virtual void OnDraggingStart(int mouseButton)
     {
         // implement this for start of dragging
     }
 
     public virtual void OnDragging(int mouseButton)
     {
         if (mouseButton != 1) return;
         
         // implement this for dragging
         if (Input.GetMouseButtonDown(0)) return;  // Right button only
         float rotX = Input.GetAxis ("Mouse X") * sensitivity * Mathf.Deg2Rad;
         float rotY = Input.GetAxis ("Mouse Y") * sensitivity * Mathf.Deg2Rad;

         transform.RotateAround (transform.up, -rotX);
         transform.RotateAround (Vector3.right, rotY);     }
 
     public virtual void OnDraggingEnd(int mouseButton)
     {
         // implement this for end of dragging
     }
 }