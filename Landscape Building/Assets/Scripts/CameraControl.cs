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

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
        yaw = transform.rotation.eulerAngles.y;
        pitch = transform.rotation.eulerAngles.x;
	}
	
	// Update is called once per frame
	void Update () {

        // Moving the pitch and yaw using mouse movement and applying a certain speed
        yaw += mouseSpeed * Input.GetAxis("Mouse X");
        pitch -= mouseSpeed * Input.GetAxis("Mouse Y");

        // Moving the camera transform
        // Moving forward or backward
        if (Input.GetKey(KeyCode.W))
        {
            //transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
            GetComponent<Rigidbody>().AddForce(transform.forward*moveSpeed);
        } else if (Input.GetKey(KeyCode.S))
        {
            //transform.Translate(Vector3.back * Time.deltaTime * moveSpeed);
            GetComponent<Rigidbody>().AddForce(transform.forward*moveSpeed*-1);
        }

        // Moving left or right
        if (Input.GetKey(KeyCode.A))
        {
            //transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
            GetComponent<Rigidbody>().AddForce(transform.right*moveSpeed*-1);
            
            roll = Mathf.Lerp(roll, maxRoll, Time.deltaTime * 2);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            //transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
            GetComponent<Rigidbody>().AddForce(transform.right*moveSpeed);

            roll = Mathf.Lerp(roll, minRoll, Time.deltaTime * 2);
        }
        else
        {
            roll = Mathf.Lerp(roll, 0f, Time.deltaTime * 2);
        }

        // rotating the camera based on the yaw and pitch values
        transform.eulerAngles = new Vector3(pitch, yaw, roll);
    }
}
