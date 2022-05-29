using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour
{
    LineRenderer lineRenderer;
    [SerializeField] float heightScale = 2f;
    [SerializeField] float scale = 0.5f;
    [SerializeField] int octaves = 1;
    [SerializeField] float heightOffset = -40;

    public float HeightScale { get => heightScale; set => heightScale = value; }
    public float Scale { get => scale; set => scale = value; }
    public int Octaves { get => octaves; set => octaves = value; }
    public float HeightOffset { get => heightOffset; set => heightOffset = value; }

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
