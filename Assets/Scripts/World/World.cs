using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public struct PerlinSettings
{
    public float heightScale;
    public float scale;
    public int octaves;
    public float heightOffset;
    public float probability;

    public PerlinSettings(float hs, float s, int o, float ho, float p)
    {
        heightScale = hs;
        scale = s;
        octaves = o;
        heightOffset = ho;
        probability = p;
    }
}


public class World : MonoBehaviour
{
    public static Vector3Int worldDimensions = new Vector3Int(5, 7, 5);
    public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);
    public static int radius = 5;
    public GameObject chunkPrefab;

    [SerializeField] GameObject loadingCam;
    [SerializeField] GameObject player;
    [SerializeField] Slider progressLoadingBar;

    public static PerlinSettings surfaceSettings;
    public PerlinGrapher surface;

    public static PerlinSettings stoneSettings;
    public PerlinGrapher stone;

    public static PerlinSettings diamondTopSettings;
    public PerlinGrapher diamondT;
    public static PerlinSettings diamondBottomSettings;
    public PerlinGrapher diamondB;

    public static PerlinSettings goldTopSettings;
    public PerlinGrapher goldT;
    public static PerlinSettings goldBottomSettings;
    public PerlinGrapher goldB;

    HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>();
    HashSet<Vector2Int> chunkCollumns = new HashSet<Vector2Int>();
    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    Vector3Int lastBuildPos;
    int drawRadius = 5;

    Queue<IEnumerator> buildQueue = new Queue<IEnumerator>();

    IEnumerator BuildCoordinator()
    {
        while (true)
        {
            while (buildQueue.Count > 0)
            {
                yield return StartCoroutine(buildQueue.Dequeue());
            }
            yield return null;
        }
    }

    void Start()
    {
        progressLoadingBar.maxValue = worldDimensions.x * worldDimensions.z;

        surfaceSettings = new PerlinSettings(surface.heightScale, surface.scale, surface.octaves, surface.heightOffset, surface.probability);

        stoneSettings = new PerlinSettings(stone.heightScale, stone.scale, stone.octaves, stone.heightOffset, stone.probability);

        diamondTopSettings = new PerlinSettings(diamondT.heightScale, diamondT.scale, diamondT.octaves, diamondT.heightOffset, diamondT.probability);
        diamondBottomSettings = new PerlinSettings(diamondB.heightScale, diamondB.scale, diamondB.octaves, diamondB.heightOffset, diamondB.probability);

        goldTopSettings = new PerlinSettings(goldT.heightScale, goldT.scale, goldT.octaves, goldT.heightOffset, goldT.probability);
        goldBottomSettings = new PerlinSettings(goldB.heightScale, goldB.scale, goldB.octaves, goldB.heightOffset, goldB.probability);

        StartCoroutine(BuildWorld());
    }

    void BuildChunkColumn(int x, int z)
    {
        for (int y = 0; y <= worldDimensions.y; y++)
        {
            Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);

            if (!chunkChecker.Contains(position))
            {
                GameObject chunk = Instantiate(chunkPrefab, this.transform);
                chunk.gameObject.name = BuildChunkName(position);
                Chunk c = chunk.GetComponent<Chunk>();
                c.CreateChunk(chunkDimensions, position);
                chunkChecker.Add(position);
                chunks.Add(position, c);
            }
            else
            {
                chunks[position].meshRenderer.enabled = true;
            }
        }
        chunkCollumns.Add(new Vector2Int(x, z));
    }

    IEnumerator BuildWorld()
    {
        for (int z = 0; z <= worldDimensions.z; z++)
        {
            for (int x = 0; x < worldDimensions.x; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z);

                progressLoadingBar.value++;
                yield return null;
            }
        }
        loadingCam.SetActive(false);

        int xPos = (worldDimensions.x * chunkDimensions.x) / 2;
        int zPos = (worldDimensions.z * chunkDimensions.z) / 2;

        int yPos = (int)MeshUtils.FractalBrownianMotion(xPos, zPos, surfaceSettings.scale, surfaceSettings.heightScale, surfaceSettings.octaves, surfaceSettings.heightOffset) + 10;

        player.transform.position = new Vector3Int(xPos, yPos, zPos);
        progressLoadingBar.gameObject.SetActive(false);
        player.SetActive(true);
        lastBuildPos = Vector3Int.CeilToInt(player.transform.position);
        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());
    }

    WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);
    IEnumerator UpdateWorld()
    {
        while (true)
        {
            if((lastBuildPos - player.transform.position).magnitude > chunkDimensions.x)
            {
                lastBuildPos = Vector3Int.CeilToInt(player.transform.position);
                int posX = (int)(player.transform.position.x / chunkDimensions.x) * chunkDimensions.x;
                int posZ = (int)(player.transform.position.z / chunkDimensions.z) * chunkDimensions.z;
                buildQueue.Enqueue(BuildRecursiveWorld(posX, posZ, drawRadius));
                buildQueue.Enqueue(HideColumns(posX, posZ));
            }
            yield return waitForSeconds;
        }

    }

    public void HideChunkColumn(int x, int z)
    {
        for (int y = 0; y < worldDimensions.y; y++)
        {
            Vector3Int pos = new Vector3Int(x, y * chunkDimensions.y, z);
            if (chunkChecker.Contains(pos))
            {
                chunks[pos].meshRenderer.enabled = false;
            }
        }
    }

    IEnumerator HideColumns(int x, int z)
    {
        Vector2Int playerPos = new Vector2Int(x, z);
        foreach (Vector2Int cc in chunkCollumns)
        {
            if((cc - playerPos).magnitude >= drawRadius * chunkDimensions.x)
            {
                HideChunkColumn(cc.x, cc.y);
            }
        }
        yield return null;
    }

    IEnumerator BuildRecursiveWorld(int x, int z, int radius)
    {
        int nextRad = radius - 1;
        if (radius <= 0) yield break;

        BuildChunkColumn(x, z + chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z + chunkDimensions.z, nextRad));
        yield return null;

        BuildChunkColumn(x, z - chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z - chunkDimensions.z, nextRad));
        yield return null;

        BuildChunkColumn(x + chunkDimensions.x, z);
        buildQueue.Enqueue(BuildRecursiveWorld(x + chunkDimensions.x, z, nextRad));
        yield return null;

        BuildChunkColumn(x - chunkDimensions.x, z);
        buildQueue.Enqueue(BuildRecursiveWorld(x - chunkDimensions.x, z, nextRad));
        yield return null;
    }


    public static string BuildChunkName(Vector3Int v)
    {
        return $"{(int)v.x}_{(int)v.y}_{(int)v.z}";
    }

}
