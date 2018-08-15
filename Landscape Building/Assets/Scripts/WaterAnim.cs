using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAnim : MonoBehaviour
{

    private new Renderer renderer;

    private Vector2 uvOffset = Vector2.zero;
    public Vector2 animRate = new Vector2(1.0f, 0.0f);

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    private void LateUpdate()
    {
        uvOffset += (animRate * Time.deltaTime);
        renderer.material.SetTextureOffset("_MainTex", uvOffset);
    }
}
