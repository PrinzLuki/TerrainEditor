using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;

public static class MeshUtils
{
    public enum EBlockType { Grass, Dirt, Stone, Sand, Water, Diamonds, Gold, Bedrock, Air }

    public enum EBlockSide
    {
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back
    }

    public static Vector2[,] blockUvData =
    {
        {new Vector2(0.0625f * 2, 0.0625f * 6), new Vector2(0.0625f * 2, 0.0625f * 7), new Vector2(0.0625f * 3, 0.0625f * 6), new Vector2(0.0625f * 3, 0.0625f * 7)},           //Grass 
        {new Vector2(0.0625f * 2, 0.0625f * 15), new Vector2(0.0625f * 2, 0.0625f * 16), new Vector2(0.0625f * 3, 0.0625f * 15), new Vector2(0.0625f * 3, 0.0625f * 16)},       //Dirt
        {new Vector2(0.0625f * 1, 0.0625f * 15), new Vector2(0.0625f * 1, 0.0625f * 16), new Vector2(0.0625f * 2, 0.0625f * 15), new Vector2(0.0625f * 2, 0.0625f * 16)},       //Stone
        {new Vector2(0.0625f * 2, 0.0625f * 14), new Vector2(0.0625f * 2, 0.0625f * 15), new Vector2(0.0625f * 3, 0.0625f * 14), new Vector2(0.0625f * 3, 0.0625f * 15)},       //Sand
        {new Vector2(0.0625f * 15, 0.0625f * 3), new Vector2(0.0625f * 15, 0.0625f * 4), new Vector2(0.0625f * 16, 0.0625f * 3), new Vector2(0.0625f * 16, 0.0625f * 4)},       //Water
        {new Vector2(0.0625f * 2, 0.0625f * 12), new Vector2(0.0625f * 2, 0.0625f * 13), new Vector2(0.0625f * 3, 0.0625f * 12), new Vector2(0.0625f * 3, 0.0625f * 13)},       //Diamonds
        {new Vector2(0.0625f * 0, 0.0625f * 13), new Vector2(0.0625f * 0, 0.0625f * 14), new Vector2(0.0625f * 1, 0.0625f * 13), new Vector2(0.0625f * 1, 0.0625f * 14)},       //Gold
        {new Vector2(0.0625f * 1, 0.0625f * 14), new Vector2(0.0625f * 1, 0.0625f * 15), new Vector2(0.0625f * 2, 0.0625f * 14), new Vector2(0.0625f * 1, 0.0625f * 15)},       //BedRock
    };
    public static Mesh MergeMeshes(Mesh[] meshes)
    {
        Mesh mesh = new Mesh();

        Dictionary<VertexData, int> pointsOrder = new Dictionary<VertexData, int>();
        HashSet<VertexData> pointsHash = new HashSet<VertexData>();
        List<int> tris = new List<int>();

        int pIndex = 0;
        for (int i = 0; i < meshes.Length; i++)
        {
            if (meshes[i] == null) continue;

            for (int j = 0; j < meshes[i].vertices.Length; j++)
            {
                Vector3 v = meshes[i].vertices[j];
                Vector3 n = meshes[i].normals[j];
                Vector2 u = meshes[i].uv[j];
                VertexData p = new VertexData(v, n, u);

                if (!pointsHash.Contains(p))
                {
                    pointsOrder.Add(p, pIndex);
                    pointsHash.Add(p);
                    pIndex++;
                }

            }

            for (int t = 0; t < meshes[i].triangles.Length; t++)
            {
                int triPoint = meshes[i].triangles[t];
                Vector3 v = meshes[i].vertices[triPoint];
                Vector3 n = meshes[i].normals[triPoint];
                Vector2 u = meshes[i].uv[triPoint];
                VertexData p = new VertexData(v, n, u);

                pointsOrder.TryGetValue(p, out int index);
                tris.Add(index);
            }
            meshes[i] = null;
        }
        ExtractArrays(pointsOrder, mesh);
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        mesh.name = "Single_Cube";
        return mesh;
    }

    public static void ExtractArrays(Dictionary<VertexData, int> lists, Mesh mesh)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        foreach (VertexData vData in lists.Keys)
        {
            verts.Add(vData.Item1);
            norms.Add(vData.Item2);
            uvs.Add(vData.Item3);
        }
        mesh.vertices = verts.ToArray();
        mesh.normals = norms.ToArray();
        mesh.uv = uvs.ToArray();
    }

    public static float FractalBrownianMotion(float x, float z, float scale, float heigthScale, int octaves, float heightOffset)
    {
        float total = 0;
        float frequency = 1;
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency) * heigthScale;
            frequency *= 2;
        }
        return total + heightOffset;
    }


}
