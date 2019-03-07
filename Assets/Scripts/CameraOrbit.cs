using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    //Distance the camera is from world zero
    public float distance = 10f;
    //X and T rotation speed
    public float xSpeed = 120f;
    public float ySpeed = 120f;
    //X and Y rotation limits
    public float yMin = 15f;
    public float yMax = 80f;
    //Current X and Y rotation
    private float x = 0;
    private float y = 0;
    // Use this for initialization
    void Start()
    {
        //Get Current rotaiton of camera
        Vector3 euler = transform.eulerAngles;
        x = euler.y;
        y = euler.x;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(Input.GetMouseButton(1))
        {
            //Hide the cursor
            Cursor.visible = false;
            //Get input X and Y offsets
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            //Offset rotation with mouse X and Y offsets
            x += mouseX * xSpeed * Time.deltaTime;
            y -= mouseY * ySpeed * Time.deltaTime;
            //Clamp the Y between min and max limits
            y = Mathf.Clamp(y, yMin, yMax);
        }
        else
        {
            Cursor.visible = true;
        }
        transform.rotation = Quaternion.Euler(y, x, 0);
        transform.position = -transform.forward * distance;
    }
}
