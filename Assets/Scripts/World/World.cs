using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public static Vector3Int worldDimensions = new Vector3Int(5, 7, 5);
    public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);
    public static int radius = 2;
    public static Dictionary<string, GameObject> chunks;
    public GameObject chunkPrefab;

    [SerializeField] GameObject loadingCam;
    [SerializeField] GameObject player;
    [SerializeField] Slider progressLoadingBar;

    void Start()
    {
        progressLoadingBar.maxValue = worldDimensions.x * worldDimensions.z;
        StartCoroutine(BuildWorld());
    }

    void BuildChunkColumn(int x, int z)
    {
        for (int y = 0; y <= worldDimensions.y; y++)
        {
            GameObject chunk = Instantiate(chunkPrefab, this.transform);
            Vector3Int position = new Vector3Int(x * chunkDimensions.x, y * chunkDimensions.y, z * chunkDimensions.z);
            chunk.gameObject.name = BuildChunkName(position);
            chunk.GetComponent<Chunk>().CreateChunk(chunkDimensions, position);
        }
    }

    IEnumerator BuildWorld()
    {
        int playerPosX = Mathf.FloorToInt(player.transform.position.x / chunkDimensions.x);
        int playerPosZ = Mathf.FloorToInt(player.transform.position.z / chunkDimensions.z);

        for (int z = 0; z <= worldDimensions.z; z++)
        {
            for (int x = 0; x < worldDimensions.x; x++)
            {
                BuildChunkColumn(x, z);

                progressLoadingBar.value++;
                yield return null;
            }
        }
        loadingCam.SetActive(false);

        int xPos = (worldDimensions.x * chunkDimensions.x) / 2;
        int zPos = (worldDimensions.z * chunkDimensions.z) / 2;

        Chunk c = chunkPrefab.GetComponent<Chunk>();
        int yPos = (int)MeshUtils.FractalBrownianMotion(xPos, zPos, c.Scale, c.HeightScale, c.Octaves, c.HeightOffset) + 5;

        player.transform.position = new Vector3(xPos, yPos, zPos);
        progressLoadingBar.gameObject.SetActive(false);
        player.SetActive(true);
    }

    public static string BuildChunkName(Vector3Int v)
    {
        return $"{(int)v.x}_{(int)v.y}_{(int)v.z}";
    }

}
