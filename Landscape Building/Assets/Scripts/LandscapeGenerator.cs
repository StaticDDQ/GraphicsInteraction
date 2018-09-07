using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{
    // Default landscape generation constants
    private const float DEFAULT_SIZE = 100.0f;
    private const int DEFAULT_ITER = 6;
    private const float DEFAULT_HEIGHTLIM = 80.0f;
    private const float DEFAULT_SMOOTHNESS = 0.6f;

    // Smoothness range constants
    private const float SMOOTHNESS_LOWER_BOUND = 0.0f;
    private const float SMOOTHNESS_UPPER_BOUND = 1.0f;

    // Landscape coloring constants
    private const float SHORE_UPPER_LIM = 0.02f;
    private const float GRASS_UPPER_LIM = 0.25f;
    private const float BOUNDARY_WIDTH = 0.05f;
    private readonly Color SAND_COLOR = new Color(0.75f, 0.65f, 0.33f);
    private readonly Color GRASS_COLOR = new Color(0.16f, 0.35f, 0.16f);
    private readonly Color SNOW_COLOR = new Color(0.9f, 0.9f, 0.9f);

    public float size = DEFAULT_SIZE;
    public int iterations = DEFAULT_ITER;
    public float heightLimit = DEFAULT_HEIGHTLIM;
    public float smoothness = DEFAULT_SMOOTHNESS;

    // Use this for initialization
    void Start()
    {
        // Range from 0 to 1, set default if user input is out of bounds
        if (smoothness < SMOOTHNESS_LOWER_BOUND || smoothness > SMOOTHNESS_UPPER_BOUND)
        {
            smoothness = DEFAULT_SMOOTHNESS;
        }

        // Range from 0 to 6, set defaults if user input is out of bounds
        if (iterations < 0 || iterations > DEFAULT_ITER)
        {
            iterations = DEFAULT_ITER;
        }
        
        GetComponent<MeshFilter>().sharedMesh = this.CreateLandscapeMesh();
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }

    Mesh CreateLandscapeMesh()
    {
        Mesh m = new Mesh();
        m.name = "Landscape";

        // Initialise height map of required size and run diamond-square algorithm
        float[,] heightMap = CreateHeightMap(iterations);
        int size = heightMap.GetLength(0);
        DiamondSquareGenerator(heightMap, size);
        SetZeroAvgHeight(heightMap, size);

        // Initialise variables for vector creation
        List<Vector3> vectorList = new List<Vector3>();
        List<Color> colorList = new List<Color>();
        List<Vector3> normalList = new List<Vector3>();

        // Draw triangles and set color for every pair of 4 heights in a square
        for (int i = 0; i < size - 1; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                DrawTriangles(vectorList, heightMap, i, j);
                SetColors(colorList, heightMap, i, j);
            }
        }
        
        m.vertices = vectorList.ToArray();
        m.colors = colorList.ToArray();

        // Calculate polygon normals for shader
        for (int i = 0; i < m.vertices.Length; i += 3)
        {
            Vector3 side1 = vectorList[i + 1] - vectorList[i];
            Vector3 side2 = vectorList[i + 2] - vectorList[i];

            Vector3 n = Vector3.Cross(side1, side2);
            Vector3 nn = n / n.magnitude;
            normalList.Add(nn);
            normalList.Add(nn);
            normalList.Add(nn);
        }

        m.normals = normalList.ToArray();

        // Set indices as triangle points
        int[] triangles = new int[m.vertices.Length];
        for (int i = 0; i < m.vertices.Length; i++)
        {
            triangles[i] = i;
        }
        m.triangles = triangles;

        return m;
    }

    /// <summary>
    /// Initialize the heightmap by setting the number of equally divided points
    /// and setting the corner values with random amount
    /// </summary>
    /// <param name="iter"> Iteration to determine the grid size </param>
    /// <returns> Initialized heightmap </returns>
    float[,] CreateHeightMap(int iter)
    {
        // Create height map of size 2^#ofiterations + 1
        int size = (int)Mathf.Pow(2, iter) + 1;
        float[,] heightMap = new float[size, size];

        // Initialize corners with random heights within bounds
        heightMap[0, 0] = Random.Range(-heightLimit * 0.5f, heightLimit * 0.5f);
        heightMap[0, size - 1] = Random.Range(-heightLimit * 0.5f, heightLimit * 0.5f);
        heightMap[size - 1, 0] = Random.Range(-heightLimit * 0.5f, heightLimit * 0.5f);
        heightMap[size - 1, size - 1] = Random.Range(-heightLimit * 0.5f, heightLimit * 0.5f);

        return heightMap;
    }

    /// <summary>
    /// Populate the heightmap using the Diamond Square Algorithm
    /// </summary>
    /// <param name="heightMap"></param>
    /// <param name="size"></param>
    void DiamondSquareGenerator(float[,] heightMap, int size)
    {
        // Decrease iteration scale (step) by factor of 2 per iteration
        for (int step = (size - 1) / 2; step >= 1; step /= 2)
        {
            // Diamond Step
            for (int i = step; i < size; i += (2 * step))
            {
                for (int j = step; j < size; j += (2 * step))
                {
                    // Get average of surrounding coordinates diagonally
                    heightMap[i, j] = (heightMap[i - step, j - step] + heightMap[i - step, j + step] + heightMap[i + step, j - step] + heightMap[i + step, j + step]) / 4;
                    heightMap[i, j] += calcWeightedOffset(step, size);
                }
            }

            // Square Step
            for (int i = 0; i < size; i += step)
            {
                for (int j = ((i + step) % (2 * step)); j < size; j += 2 * step)
                {
                    if (i > 0 && j > 0 && i < size - 1 && j < size - 1)
                    {
                        // Get average of surrounding coordinates horizontally/vertically
                        heightMap[i, j] = (heightMap[i, j - step] + heightMap[i, j + step] + heightMap[i - step, j] + heightMap[i + step, j]) / 4;
                    }
                    else // If code yields error from out of range index, must be border coordinate, move to catch
                    {
                        // Pushes coordinates back into target coordinate, initialized to 0 so doesn't affect average
                        heightMap[i, j] = (heightMap[i, Mathf.Max(0, j - step)] + heightMap[i, Mathf.Min(size - 1, j + step)] + heightMap[Mathf.Max(0, i - step), j] + heightMap[Mathf.Min(size - 1, i + step), j]) / 3;
                    }
                    heightMap[i, j] += calcWeightedOffset(step, size);
                }
            }
        }
    }

    // Takes the height map size and the current iteration scale, outputs a weighted random offset for the height
    float calcWeightedOffset(int step, int size)
    {
        // Weight decrease is proportional to iteration scale (step) decrease
        float baseWeight = 2 * (float)step / (size - 1);

        // Factor smoothness into weight as linear function (see Readme)
        float gradient = 1 - Mathf.Abs(1 - 2 * smoothness);
        float intercept = Mathf.Max(0, 1 - 2 * smoothness);
        float realWeight = gradient * baseWeight + intercept;

        // Apply weight to random offset
        return realWeight * calcRandOffset();
    }

    // Calculate height offset within height bounds
    float calcRandOffset()
    {
        return heightLimit * Random.value - heightLimit * 0.5f;
    }

    // Takes the height map and apply offset such that average height is 0
    void SetZeroAvgHeight(float[,] heightMap, int size)
    {
        float totalHeight = 0f;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                totalHeight += heightMap[i, j];
            }
        }

        float avg = totalHeight / Mathf.Pow(size, 2);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                heightMap[i, j] -= avg;
            }
        }
    }

    // Add 6 vertices to the list that correspond to 2 triangles formed by the square with top-left i and j
    void DrawTriangles(List<Vector3> list, float[,] map, int i, int j)
    {
        list.Add(CreateVectorFromMap(i + 1, j, map));
        list.Add(CreateVectorFromMap(i, j, map));
        list.Add(CreateVectorFromMap(i, j + 1, map));

        list.Add(CreateVectorFromMap(i, j + 1, map));
        list.Add(CreateVectorFromMap(i + 1, j + 1, map));
        list.Add(CreateVectorFromMap(i + 1, j, map));
    }

    // Takes the height map and coordinates as input, outputs a vector corresponding with coordinates' height
    Vector3 CreateVectorFromMap(int i, int j, float[,] map)
    {
        float x = (-size / 2) + (i * (size / (map.GetLength(0) - 1)));
        float y = map[i, j];
        float z = (-size / 2) + (j * (size / (map.GetLength(0) - 1)));
        return new Vector3(x, y, z);
    }

    // Add 6 colors to the list that correspond to the triangle points
    void SetColors(List<Color> list, float[,] map, int i, int j)
    {
        list.Add(getColorFromHeight(map[i + 1, j]));
        list.Add(getColorFromHeight(map[i, j]));
        list.Add(getColorFromHeight(map[i, j + 1]));

        list.Add(getColorFromHeight(map[i, j + 1]));
        list.Add(getColorFromHeight(map[i + 1, j + 1]));
        list.Add(getColorFromHeight(map[i + 1, j]));
    }

    // Set different colors based on height
    Color getColorFromHeight(float height)
    {
        if (height < heightLimit * SHORE_UPPER_LIM)
        {
            return SAND_COLOR;
        }
        else if (height < heightLimit * (SHORE_UPPER_LIM + BOUNDARY_WIDTH))
        {
            return Color.Lerp(SAND_COLOR, GRASS_COLOR, (height - heightLimit * SHORE_UPPER_LIM) / (heightLimit * BOUNDARY_WIDTH));
        }
        else if (height < heightLimit * GRASS_UPPER_LIM)
        {
            return GRASS_COLOR;
        }
        else if (height < heightLimit * (GRASS_UPPER_LIM + BOUNDARY_WIDTH))
        {
            return Color.Lerp(GRASS_COLOR, SNOW_COLOR, (height - heightLimit * GRASS_UPPER_LIM) / (heightLimit * BOUNDARY_WIDTH));
        }
        else
        {
            return SNOW_COLOR;
        }
    }
}
