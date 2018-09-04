using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAnim : MonoBehaviour
{
    private Vector2 uvOffset = Vector2.zero;
    private new MeshRenderer renderer;

    public Vector2 tileScale =  new Vector2(1,1);
    public Vector2 moveRate = new Vector2(1.0f, 0.0f);

    public float size;
    public int gridSize;

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        // Used to generate more vertices which creates smoother wave motion
        InitMesh();
        
    }

    private void LateUpdate()
    {
        // Texture moves continuously
        uvOffset += (moveRate * Time.deltaTime);
        renderer.sharedMaterial.SetTextureOffset("_MainTex", uvOffset);
        renderer.sharedMaterial.SetTextureScale("_MainTex", tileScale);
    }

    [ContextMenu("Create Mesh")]
    public void InitMesh()
    {
        GetComponent<MeshFilter>().mesh = CreateMesh();
    }

    private Mesh CreateMesh()
    {
        Mesh water = new Mesh();

        // Assign a list of positions to place the vertices
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector3> uvs = new List<Vector3>();

        for (int x = 0; x < gridSize + 1; x++)
        {
            for (int y = 0; y < gridSize + 1; y++)
            {
                // Equally distribute the amount of vertices by the given grid
                vertices.Add(new Vector3(-size * 0.5f + size * (x / (float)gridSize), 0, -size * 0.5f + size * (y / (float)gridSize)));
                // Set normals for each vertices
                normals.Add(Vector3.up);
                // Add UV for each vertices
                uvs.Add(new Vector2(x / (float)gridSize, y / (float)gridSize));
            }
        }

        List<int> triangles = new List<int>();
        var vertCount = gridSize + 1;

        // Go through all vertices except the last line
        for (int i = 0; i < vertCount * vertCount- vertCount; i++)
        {
            // Skip if it is the last vertices, therefore it doesn't have another vertice to form
            // a triangle, prevents out of bound error
            if ((i + 1) % vertCount == 0)
            {
                continue;
            }
            
            // Defines the triangles by clockwise implementation
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + vertCount + 1);
          
            triangles.Add(i);
            triangles.Add(i + vertCount + 1);
            triangles.Add(i + vertCount);
        }

        // Set all data to map the mesh as a plane
        water.SetVertices(vertices);
        water.SetNormals(normals);
        water.SetUVs(0, uvs);

        water.triangles = triangles.ToArray();

        return water;
    }
}
