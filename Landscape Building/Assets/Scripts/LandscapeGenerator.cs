using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{
    private const float DEFAULT_SIZE = 100.0f;
    private const int DEFAULT_ITER = 6;
    private const float DEFAULT_HEIGHTLIM = 80.0f;
    private const float DEFAULT_SMOOTHNESS = 0.6f;

    private const float SMOOTHNESS_LOWER_BOUND = 0.0f;
    private const float SMOOTHNESS_UPPER_BOUND = 1.0f;

    private const float SHORE_GRASS_BOUNDARY = 0.05f;
    private const float GRASS_SNOW_BOUNDARY = 0.25f;

    public float size = DEFAULT_SIZE;
    public int iterations = DEFAULT_ITER;
    public float heightLimit = DEFAULT_HEIGHTLIM;
    public float smoothness = DEFAULT_SMOOTHNESS;

    // Use this for initialization
    void Start()
    {
        if (smoothness <= SMOOTHNESS_LOWER_BOUND || smoothness >= SMOOTHNESS_UPPER_BOUND)
        {
            smoothness = DEFAULT_SMOOTHNESS;
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

        // Draw 2 triangles (6 vectors) for every pair of 4 heights in a square
        for (int i = 0; i < size - 1; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                DrawTriangles(vectorList, heightMap, i, j, colorList);
            }
        }

        m.vertices = vectorList.ToArray();
        m.colors = colorList.ToArray();

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

        int[] triangles = new int[m.vertices.Length];
        for (int i = 0; i < m.vertices.Length; i++)
        {
            triangles[i] = i;
        }
        m.triangles = triangles;

        return m;
    }

    // Takes the height map and coordinates as input, outputs a vector corresponding with coordinates' height
    Vector3 CreateVectorFromMap(int i, int j, float[,] map, List<Color> clist)
    {
        float x = (-size / 2) + (i * (size / (map.GetLength(0) - 1)));
        float y = map[i, j];
        float z = (-size / 2) + (j * (size / (map.GetLength(0) - 1)));
        SetColors(clist, map, i, j);
        return new Vector3(x, y, z);
    }

    #region DiamondSquare
    /// <summary>
    /// Populate the heightmap using the Diamond Square Algorithm
    /// </summary>
    /// <param name="heightMap"></param>
    /// <returns> Generated heightmap </returns>
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
    #endregion

    #region CreateHeightMap
    /// <summary>
    /// Initialize the heightmap by setting the number of equally divided points
    /// and setting the corner values with random amount
    /// </summary>
    /// <param name="iter"> Iteration to determine the grid size </param>
    /// <returns> Initilized heatmap </returns>
    float[,] CreateHeightMap(int iter)
    {
        // Create height map of size 2^#ofiterations + 1
        int size = (int)Mathf.Pow(2, iter) + 1;
        float[,] heightMap = new float[size, size];

        // Initialize corners with random heights within bounds
        heightMap[0, 0] = Random.Range(-heightLimit * 0.5f, heightLimit*0.5f);
        heightMap[0, size - 1] = Random.Range(-heightLimit * 0.5f, heightLimit * 0.5f);
        heightMap[size - 1, 0] = Random.Range(-heightLimit * 0.5f, heightLimit * 0.5f);
        heightMap[size - 1, size - 1] = Random.Range(-heightLimit * 0.5f, heightLimit * 0.5f);

        return heightMap;
    }
    #endregion

    // Takes the height map size and the current iteration scale, outputs a weighted random offset for the height
    float calcWeightedOffset(int step, int size)
    {
        // Weight decrease is proportional to iteration scale (step) decrease
        float baseWeight = 2 * (float)step / (size - 1);

        // Factor smoothness into weight as linear function. For example, as step size decreases:
        // MOVE THIS TO README.MD
        // 0 < smoothness < 0.5 : starting weight is constant at 1, y-intercept decreases, gradient steepens
        // 0.5 < smoothness < 1 : starting weight decreases, y-intercept is constant at 0, gradient flattens
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

    void DrawTriangles(List<Vector3> list, float[,] map, int i, int j, List<Color> clist)
    {
        list.Add(CreateVectorFromMap(i + 1, j, map, clist));
        list.Add(CreateVectorFromMap(i, j, map, clist));
        list.Add(CreateVectorFromMap(i, j + 1, map, clist));

        list.Add(CreateVectorFromMap(i, j + 1, map, clist));
        list.Add(CreateVectorFromMap(i + 1, j + 1, map, clist));
        list.Add(CreateVectorFromMap(i + 1, j, map, clist));
    }

    void SetColors(List<Color> list, float[,] map, int i, int j)
    {
        float y = map[i, j];

        if (y < heightLimit * SHORE_GRASS_BOUNDARY)
        {
            list.Add(Random.ColorHSV(0.11f, 0.14f, 0.5f, 0.6f, 0.7f, 0.8f));
        }
        else if (y < heightLimit * GRASS_SNOW_BOUNDARY)
        {
            list.Add(Random.ColorHSV(0.3f, 0.36f, 0.5f, 0.6f, 0.3f, 0.4f));
        }
        else
        {
            list.Add(Random.ColorHSV(0.8f, 1f, 0f, 0f, 0.8f, 1f));
        }
    }
}
