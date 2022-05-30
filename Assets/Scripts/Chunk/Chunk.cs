using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
public enum ChunkState { Build, Done, Keep };

public class Chunk : MonoBehaviour
{
    [SerializeField] Material mat_Atlas;
    [SerializeField] int width;
    [SerializeField] int depth;
    [SerializeField] int height;
    public ChunkState state;
    public Block[,,] blocks; //FlatArray = x + width * (y + depth * z) = blocks[x, y, z]

    public MeshUtils.EBlockType[] chunkData;
    public MeshRenderer meshRenderer;
    public Vector3 chunkLocation;

    public int Width { get => width; set => width = value; }
    public int Depth { get => depth; set => depth = value; }
    public int Height { get => height; set => height = value; }

    void Start()
    {

    }
    public void CreateChunk(Vector3 chunkScale, Vector3 position)
    {
        chunkLocation = position;
        width = (int)chunkScale.x;
        height = (int)chunkScale.y;
        depth = (int)chunkScale.z;


        MeshFilter meshF = this.gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshR = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer = meshR;
        meshR.material = mat_Atlas;

        blocks = new Block[width, height, depth];

        BuildCunk();

        var inputMeshes = new List<Mesh>();
        int vertexStart = 0;
        int triStart = 0;
        int meshCount = width * height * depth;
        int currMeshes = 0;

        var jobs = new ProcessMeshDataJob();
        jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        jobs.triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    blocks[x, y, z] = new Block(new Vector3(x, y, z) + chunkLocation, chunkData[x + width * (y + depth * z)], this);

                    if (blocks[x, y, z].mesh != null)
                    {
                        inputMeshes.Add(blocks[x, y, z].mesh);
                        var vCount = blocks[x, y, z].mesh.vertexCount;
                        var iCount = blocks[x, y, z].mesh.GetIndexCount(0);
                        jobs.vertexStart[currMeshes] = vertexStart;
                        jobs.triStart[currMeshes] = triStart;
                        vertexStart += vCount;
                        triStart += (int)iCount;
                        currMeshes++;
                    }
                }
            }
        }

        jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        var outputMeshData = Mesh.AllocateWritableMeshData(1);
        jobs.outputMesh = outputMeshData[0];
        jobs.outputMesh.SetIndexBufferParams(triStart, IndexFormat.UInt32);
        jobs.outputMesh.SetVertexBufferParams(vertexStart,
            new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2));

        var handle = jobs.Schedule(inputMeshes.Count, 4);

        var newMesh = new Mesh();
        newMesh.name = "Chunk" + chunkLocation.x + "_" + chunkLocation.y + "_" + chunkLocation.z;

        var smDescriptor = new SubMeshDescriptor(0, triStart, MeshTopology.Triangles);
        smDescriptor.firstVertex = 0;
        smDescriptor.vertexCount = vertexStart;

        handle.Complete();

        jobs.outputMesh.subMeshCount = 1;
        jobs.outputMesh.SetSubMesh(0, smDescriptor);

        Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] { newMesh });
        jobs.meshData.Dispose();
        jobs.vertexStart.Dispose();
        jobs.triStart.Dispose();
        newMesh.RecalculateBounds();

        meshF.mesh = newMesh;
        MeshCollider coll = this.gameObject.AddComponent<MeshCollider>();
        coll.sharedMesh = meshF.mesh;
    }

    [BurstCompile]
    struct ProcessMeshDataJob : IJobParallelFor
    {
        [ReadOnly] public Mesh.MeshDataArray meshData;
        public Mesh.MeshData outputMesh;
        public NativeArray<int> vertexStart;
        public NativeArray<int> triStart;

        public void Execute(int index)
        {
            var data = meshData[index];
            var vCount = meshData[index].vertexCount;
            var vStart = vertexStart[index];

            var verts = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetVertices(verts.Reinterpret<Vector3>());

            var norms = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetNormals(norms.Reinterpret<Vector3>());

            var uvs = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetUVs(0, uvs.Reinterpret<Vector3>());

            var outputVerts = outputMesh.GetVertexData<Vector3>(stream: 0);
            var outputNorms = outputMesh.GetVertexData<Vector3>(stream: 1);
            var outputUvs = outputMesh.GetVertexData<Vector3>(stream: 2);

            for (int i = 0; i < vCount; i++)
            {
                outputVerts[i + vStart] = verts[i];
                outputNorms[i + vStart] = norms[i];
                outputUvs[i + vStart] = uvs[i];
            }

            verts.Dispose();
            norms.Dispose();
            uvs.Dispose();

            var tStart = triStart[index];
            var tCount = data.GetSubMesh(0).indexCount;
            var outputTris = outputMesh.GetIndexData<int>();

            if (data.indexFormat == IndexFormat.UInt16)
            {
                var tris = data.GetIndexData<ushort>();
                for (int i = 0; i < tCount; ++i)
                {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            }
            else
            {
                var tris = data.GetIndexData<int>();
                for (int i = 0; i < tCount; ++i)
                {
                    int idx = tris[i];
                    outputTris[i + tStart] = vStart + idx;
                }
            }
        }
    }

    void BuildCunk()
    {
        int blockCount = width * height * depth;
        chunkData = new MeshUtils.EBlockType[blockCount];

        for (int i = 0; i < blockCount; i++)
        {
            int x = i % width + (int)chunkLocation.x;
            int y = (i / width) % height + (int)chunkLocation.y;
            int z = i / (width * height) + (int)chunkLocation.z;

            int surfaceHeight = (int)MeshUtils.FractalBrownianMotion(x, z, World.surfaceSettings.scale, World.surfaceSettings.heightScale, World.surfaceSettings.octaves, World.surfaceSettings.heightOffset);

            int stoneHeight = (int)MeshUtils.FractalBrownianMotion(x, z, World.stoneSettings.scale, World.stoneSettings.heightScale, World.stoneSettings.octaves, World.stoneSettings.heightOffset);

            int diamondTopHeight = (int)MeshUtils.FractalBrownianMotion(x, z, World.diamondTopSettings.scale, World.diamondTopSettings.heightScale, World.diamondTopSettings.octaves, World.diamondTopSettings.heightOffset);
            int diamondBottomHeight = (int)MeshUtils.FractalBrownianMotion(x, z, World.diamondBottomSettings.scale, World.diamondBottomSettings.heightScale, World.diamondBottomSettings.octaves, 
                                                                           World.diamondBottomSettings.heightOffset);

            int goldTopHeight = (int)MeshUtils.FractalBrownianMotion(x, z, World.goldTopSettings.scale, World.goldTopSettings.heightScale, World.goldTopSettings.octaves, World.goldTopSettings.heightOffset);
            int goldBottomHeight = (int)MeshUtils.FractalBrownianMotion(x, z, World.goldBottomSettings.scale, World.goldBottomSettings.heightScale, World.goldBottomSettings.octaves, World.goldBottomSettings.heightOffset);

            //Grass
            if (surfaceHeight == y)
            {
                chunkData[i] = MeshUtils.EBlockType.Grass;
            }
            //Diamond
            else if(y < diamondTopHeight && y > diamondBottomHeight && UnityEngine.Random.Range(0, 100) < World.diamondTopSettings.probability)
            {
                chunkData[i] = MeshUtils.EBlockType.Diamonds;
            }
            //Gold
            else if(y < goldTopHeight && y > goldBottomHeight && UnityEngine.Random.Range(0,100) < World.goldTopSettings.probability)
            {
                chunkData[i] = MeshUtils.EBlockType.Gold;
            }
            //Stone
            else if (y < stoneHeight && UnityEngine.Random.Range(0,100) < World.stoneSettings.probability)
            {
                chunkData[i] = MeshUtils.EBlockType.Stone;
            }
            //Dirt
            else if (y < surfaceHeight)
            {
                chunkData[i] = MeshUtils.EBlockType.Dirt;
            }
            //Air
            else
                chunkData[i] = MeshUtils.EBlockType.Air;
        }
    }

}



