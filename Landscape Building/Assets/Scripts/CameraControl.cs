using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    [SerializeField] private float mouseSpeed;
    [SerializeField] private float moveSpeed;
    private float maxRoll = 15f;
    private float minRoll = -15f;
    private float pitch;
    private float yaw;
    private float roll;
    private Transform cam;

	// Use this for initialization
	void Start () {
        cam = transform.GetChild(0);
        // remove visibility of the cursor when testing
        Cursor.visible = false;

        // will began with initial rotation
        yaw = transform.rotation.eulerAngles.y;
        pitch = transform.rotation.eulerAngles.x;
	}
	
	// Update is called once per frame
	void Update () {

        // Moving the pitch and yaw using mouse movement and applying a certain speed
        yaw += mouseSpeed * Input.GetAxis("Mouse X");
        pitch -= mouseSpeed * Input.GetAxis("Mouse Y");

        // Moving the camera transform
        // Moving forward or backward using rigidbody
        if (Input.GetKey(KeyCode.W))
        {
            GetComponent<Rigidbody>().AddForce(transform.forward*moveSpeed);
        } else if (Input.GetKey(KeyCode.S))
        {
            GetComponent<Rigidbody>().AddForce(transform.forward*moveSpeed*-1);
        }

        // Moving left or right using rigidbody, added roll movement by lerping
        if (Input.GetKey(KeyCode.A))
        {
            GetComponent<Rigidbody>().AddForce(transform.right*moveSpeed*-1);
            
            roll = Mathf.Lerp(roll, maxRoll, Time.deltaTime * 2);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            GetComponent<Rigidbody>().AddForce(transform.right*moveSpeed);

            roll = Mathf.Lerp(roll, minRoll, Time.deltaTime * 2);
        }
        else
        {
            // If the user stopped moving left or right
            roll = Mathf.Lerp(roll, 0f, Time.deltaTime * 2);
        }

        // rotating the camera based on the roll, yaw and pitch values
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        cam.eulerAngles = new Vector3(pitch, yaw, roll);
    }
}
