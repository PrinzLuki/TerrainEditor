using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public Mesh mesh;
    Chunk parentChunk;

    /// <summary>
    /// Creates a quad and merges the quads in the "quads" list
    /// </summary>
    /// <param name="offset">Where is the block</param>
    /// <param name="blocktype">Which type is the block</param>
    /// <param name="chunk">The chunk that mains the block</param>
    public Block(Vector3 offset, MeshUtils.EBlockType blocktype, Chunk chunk)
    {
        parentChunk = chunk;
        Vector3 blockLocalpos = offset - chunk.location;

        if (blocktype != MeshUtils.EBlockType.Air)
        {
            List<Quad> quads = new List<Quad>();

            if (!HasNeighbour((int)blockLocalpos.x, (int)blockLocalpos.y + 1, (int)blockLocalpos.z))
                quads.Add(new Quad(offset, MeshUtils.EBlockSide.Top, blocktype));

            if (!HasNeighbour((int)blockLocalpos.x, (int)blockLocalpos.y - 1, (int)blockLocalpos.z))
                quads.Add(new Quad(offset, MeshUtils.EBlockSide.Bottom, blocktype));

            if (!HasNeighbour((int)blockLocalpos.x - 1, (int)blockLocalpos.y, (int)blockLocalpos.z))
                quads.Add(new Quad(offset, MeshUtils.EBlockSide.Left, blocktype));

            if (!HasNeighbour((int)blockLocalpos.x + 1, (int)blockLocalpos.y, (int)blockLocalpos.z))
                quads.Add(new Quad(offset, MeshUtils.EBlockSide.Right, blocktype));

            if (!HasNeighbour((int)blockLocalpos.x, (int)blockLocalpos.y, (int)blockLocalpos.z + 1))
                quads.Add(new Quad(offset, MeshUtils.EBlockSide.Front, blocktype));

            if (!HasNeighbour((int)blockLocalpos.x, (int)blockLocalpos.y, (int)blockLocalpos.z - 1))
                quads.Add(new Quad(offset, MeshUtils.EBlockSide.Back, blocktype));

            if (quads.Count <= 0) return;

            Mesh[] meshSides = new Mesh[quads.Count];
            int mIdx = 0;
            foreach (Quad q in quads)
            {
                meshSides[mIdx] = q.mesh;
                mIdx++;
            }

            mesh = MeshUtils.MergeMeshes(meshSides);
        }
    }

    /// <summary>
    /// Checking if the block has a block next to him
    /// </summary>
    /// <param name="x">Local block position X</param>
    /// <param name="y">Local block position Y</param>
    /// <param name="z">Local block position Z</param>
    /// <returns>bool</returns>
    public bool HasNeighbour(int x, int y, int z)
    {
        if (x < 0 || x >= parentChunk.Width ||
            y < 0 || y >= parentChunk.Height ||
            z < 0 || z >= parentChunk.Depth)
        {
            return false;
        }

        if (parentChunk.chunkData[x + parentChunk.Width * (y + parentChunk.Depth * z)] == MeshUtils.EBlockType.Air ||
            parentChunk.chunkData[x + parentChunk.Width * (y + parentChunk.Depth * z)] == MeshUtils.EBlockType.Water)
        {
            return false;
        }

        return true;
    }

}
