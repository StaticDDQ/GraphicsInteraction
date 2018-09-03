using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTerrain : MonoBehaviour {

    public PointLight pointLight;

    // Called each frame
    void Update()
    {
        // Pass updated light positions to shader
        GetComponent<MeshRenderer>().sharedMaterial.SetColor("_PointLightColor", this.pointLight.color);
        GetComponent<MeshRenderer>().sharedMaterial.SetVector("_PointLightPosition", this.pointLight.GetWorldPosition());
    }
}
