using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAnim : MonoBehaviour
{
    private Vector2 uvOffset = Vector2.zero;
    private new Renderer renderer;

    public Vector2 tileScale =  new Vector2(1,1);
    public Vector2 moveRate = new Vector2(1.0f, 0.0f);

    public float size;
    public int gridSize;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        GetComponent<MeshFilter>().mesh = CreateMesh();
    }

    private void LateUpdate()
    {
        uvOffset += (moveRate * Time.deltaTime);
        renderer.material.SetTextureOffset("_MainTex", uvOffset);
        renderer.material.SetTextureScale("_MainTex", tileScale);
    }

    private Mesh CreateMesh()
    {
        Mesh water = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector3> uvs = new List<Vector3>();

        for (int x = 0; x < gridSize + 1; x++)
        {
            for (int y = 0; y < gridSize + 1; y++)
            {
                vertices.Add(new Vector3(-size * 0.5f + size * (x / (float)gridSize), 0, -size * 0.5f + size * (y / (float)gridSize)));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (float)gridSize, y / (float)gridSize));
            }
        }

        List<int> triangles = new List<int>();
        var vertCount = gridSize + 1;

        for (int i = 0; i < vertCount * vertCount - vertCount; i++)
        {
            if((i+1) % vertCount == 0)
            {
                continue;
            }
            triangles.AddRange(new List<int>()
            {
                i+1+vertCount, i+vertCount, i,
                i, i+1, i+vertCount+1
            });
        }

        water.SetVertices(vertices);
        //water.SetNormals(normals);
        water.SetUVs(0,uvs);
        water.SetTriangles(triangles, 0);

        return water;
    }
}
