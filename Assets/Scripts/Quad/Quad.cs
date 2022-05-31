using UnityEngine;

public class Quad
{
    public Mesh mesh;

    /// <summary>
    /// Creating a quad
    /// </summary>
    /// <param name="offset">Used for the verts position</param>
    /// <param name="blockside">Which side is building</param>
    /// <param name="blocktype">What type the side/block is</param>
    public Quad(Vector3 offset, MeshUtils.EBlockSide blockside, MeshUtils.EBlockType blocktype)
    {
        mesh = new Mesh();
        mesh.name = "Single_Quad";

        Vector3[] verts = new Vector3[4];
        Vector3[] norms = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] tris = new int[6];
        tris = new int[] { 3, 1, 0, 3, 2, 1 };

        //all verts of a cube
        Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f) + offset;
        Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f) + offset;
        Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f) + offset;
        Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f) + offset;
        Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f) + offset;
        Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f) + offset;
        Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f) + offset;
        Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f) + offset;

        switch (blockside)
        {
            case MeshUtils.EBlockSide.Top:
                verts = new Vector3[] { p7, p6, p5, p4 };
                norms = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                break;
            case MeshUtils.EBlockSide.Bottom:
                verts = new Vector3[] { p0, p1, p2, p3 };
                norms = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                break;
            case MeshUtils.EBlockSide.Right:
                verts = new Vector3[] { p5, p6, p2, p1 };
                norms = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                break;
            case MeshUtils.EBlockSide.Left:
                verts = new Vector3[] { p7, p4, p0, p3 };
                norms = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                break;
            case MeshUtils.EBlockSide.Front:
                verts = new Vector3[] { p4, p5, p1, p0 };
                norms = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                break;
            case MeshUtils.EBlockSide.Back:
                verts = new Vector3[] { p6, p7, p3, p2 };
                norms = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                break;
        }
        
        Vector2 uv00 = MeshUtils.blockUvData[(int)blocktype, 0];
        Vector2 uv10 = MeshUtils.blockUvData[(int)blocktype, 1];
        Vector2 uv01 = MeshUtils.blockUvData[(int)blocktype, 2];
        Vector2 uv11 = MeshUtils.blockUvData[(int)blocktype, 3];
        uvs = new Vector2[] { uv11, uv01, uv00, uv10 };

        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
    }
}
