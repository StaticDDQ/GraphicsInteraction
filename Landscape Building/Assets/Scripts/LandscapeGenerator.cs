﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscapeGenerator : MonoBehaviour
{
    public float size = 50;
    public int iterations = 6;
    public float heightLimit = 50;
    public float smoothness = 0.5f;
    private System.Random rng;

    // Use this for initialization
    void Start()
    {
        rng = new System.Random();

        MeshFilter landscapeMesh = this.gameObject.AddComponent<MeshFilter>();
        landscapeMesh.mesh = this.CreateLandscapeMesh();

        MeshRenderer renderer = this.gameObject.AddComponent<MeshRenderer>();
        renderer.material.shader = Shader.Find("Custom/LandscapeShader");
    }

    // Update is called once per frame
    void Update()
    {

    }

    Mesh CreateLandscapeMesh()
    {
        Mesh m = new Mesh();
        m.name = "Landscape";

        // Initialise height map of required size and run diamond-square algorithm
        float[,] heightMap = CreateHeightMap(iterations);
        heightMap = DiamondSquareGenerator(heightMap);

        // Initialise variables for vector creation
        int size = heightMap.GetLength(0);
        List<Vector3> vectorList = new List<Vector3>();
        List<Color> colorList = new List<Color>();

        // Draw 2 triangles (6 vectors) for every pair of 4 heights in a square
        for (int i = 0; i < size - 1; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                vectorList.Add(CreateVectorFromMap(i + 1, j, heightMap, colorList));
                vectorList.Add(CreateVectorFromMap(i, j, heightMap, colorList));
                vectorList.Add(CreateVectorFromMap(i, j + 1, heightMap, colorList));

                vectorList.Add(CreateVectorFromMap(i, j + 1, heightMap, colorList));
                vectorList.Add(CreateVectorFromMap(i + 1, j + 1, heightMap, colorList));
                vectorList.Add(CreateVectorFromMap(i + 1, j, heightMap, colorList));
            }
        }

        m.vertices = vectorList.ToArray();
        m.colors = colorList.ToArray();

        int[] triangles = new int[m.vertices.Length];
        for (int i = 0; i < m.vertices.Length; i++)
        {
            triangles[i] = i;
        }
        m.triangles = triangles;

        return m;
    }

    // Takes the initialized height map as input, returns the height map after running diamond-square algorithm
    float[,] DiamondSquareGenerator(float[,] heightMap)
    {
        // Decrease iteration scale (step) by factor of 2 per iteration
        int size = heightMap.GetLength(0);
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
                    try
                    {
                        // Get average of surrounding coordinates horizontally/vertically
                        heightMap[i, j] = (heightMap[i, j - step] + heightMap[i, j + step] + heightMap[i - step, j] + heightMap[i + step, j]) / 4;
                    }
                    catch // If code yields error from out of range index, must be border coordinate, move to catch
                    {
                        // Pushes coordinates back into target coordinate, initialized to 0 so doesn't affect average
                        heightMap[i, j] = (heightMap[i, Math.Max(0, j - step)] + heightMap[i, Math.Min(size - 1, j + step)] + heightMap[Math.Max(0, i - step), j] + heightMap[Math.Min(size - 1, i + 1), j]) / 3;
                    }
                    heightMap[i, j] += calcWeightedOffset(step, size);
                }
            }
        }

        return heightMap;
    }

    // Takes # of iterations as input and outputs a height map of required size with corners initialized
    float[,] CreateHeightMap(int iter)
    {
        // Create height map of size 2^#ofiterations + 1
        int size = (int)Math.Pow(2, iter) + 1;
        float[,] heightMap = new float[size, size];

        // Initialize corners with random heights within bounds
        heightMap[0, 0] = calcRandOffset();
        heightMap[0, size - 1] = calcRandOffset();
        heightMap[size - 1, 0] = calcRandOffset();
        heightMap[size - 1, size - 1] = calcRandOffset();

        return heightMap;
    }
    
    // Takes the height map and coordinates as input, outputs a vector corresponding with coordinates' height
    // FIX COLOR
    Vector3 CreateVectorFromMap(int i, int j, float[,] map, List<Color> list)
    {
        float x = (-size / 2) + (i * (size / (map.GetLength(0) - 1)));
        float y = map[i, j];
        float z = (-size / 2) + (j * (size / (map.GetLength(0) - 1)));

        //FIX THIS, COLOR LIST CURRENTLY UPDATED HERE THROUGH PARAM REFERENCE
        //float a = (y + heightLimit) / (heightLimit * 2f);
        //list.Add(new Color(Math.Max(0, Math.Min(1, 4 * a - 2)), Math.Max(0, 1 - Math.Abs(4 * a - 2)), Math.Max(0, Math.Min(1, 2 - 4 * a))));
        float a = Math.Max(0, Math.Min(1, 2 * ((y + heightLimit) / (heightLimit * 2f)) - 0.5f));
        list.Add(new Color(a, a, a));

        return new Vector3(x, y, z);
    }

    // Takes the height map size and the current iteration scale, outputs a weighted random offset for the height
    float calcWeightedOffset(int step, int size)
    {
        // Weight decrease is proportional to iteration scale (step) decrease
        float baseWeight = 2 * (float)step / (size - 1);

        // Factor smoothness into weight as linear function. For example, as step size decreases:
        // MOVE THIS TO README.MD
        // 0 < smoothness < 0.5 : starting weight is constant at 1, y-intercept decreases, gradient steepens
        // 0.5 < smoothness < 1 : starting weight decreases, y-intercept is constant at 0, gradient flattens
        float gradient = 1 - Math.Abs(1 - 2 * smoothness);
        float intercept = Math.Max(0, 1 - 2 * smoothness);
        float realWeight = gradient * baseWeight + intercept;

        // Apply weight to random offset
        return realWeight * calcRandOffset();
    }

    // Calculate height offset within height bounds
    float calcRandOffset()
    {
        return (float)(heightLimit * rng.NextDouble() - heightLimit / 2);
    }
}
