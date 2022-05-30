using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float heightScale = 2f;
    [Range(0.0f, 1.0f)]
    public float scale = 0.5f;
    public int octaves = 1;
    public float heightOffset = 1;
    [Range(0.0f, 100f)]
    public float probability = 1;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 100;
        Graph();
    }

    //float FractalBrownianMotion(float x, float z)
    //{
    //    float total = 0;
    //    float frequency = 1;
    //    for (int i = 0; i < octaves; i++)
    //    {
    //        total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * heightScale;
    //        frequency *= 2;
    //    }
    //      return total;
    //}

    void Graph()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 100;

        int z = 11;
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        for (int x = 0; x < lineRenderer.positionCount; x++)
        {
            float y = MeshUtils.FractalBrownianMotion(x, z, scale, heightScale, octaves, heightOffset);
            positions[x] = new Vector3(x, y, z);
        }
        lineRenderer.SetPositions(positions);
    }

    void OnValidate()
    {
        Graph();
    }
}
