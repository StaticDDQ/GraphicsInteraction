using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAnim : MonoBehaviour
{

    private new Renderer renderer;
    public float x;
    public float y;
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
        renderer.material.SetTextureScale("_MainTex", new Vector2(x, y));
    }
}
