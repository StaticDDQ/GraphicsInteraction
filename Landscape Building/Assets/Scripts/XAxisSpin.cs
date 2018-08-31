using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XAxisSpin : MonoBehaviour {

	public float spinSpeed = 20.0f;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
    this.transform.localRotation *= Quaternion.AngleAxis(Time.deltaTime*spinSpeed, Vector3.right);
	}
}
