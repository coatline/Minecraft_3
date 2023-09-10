using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
    [SerializeField] TMP_Text playerChunkPosition;
    [SerializeField] TMP_Text selectedBlockInfo;
    [SerializeField] LineRenderer linePrefab;
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] GameObject debugMenu;
    [SerializeField] TMP_Text fpsText;

    [SerializeField] Game game;
    WorldBuilder worldBuilder;
    Player player;
    World world;
    Camera cam;

    int chunkSize;

    private void Start()
    {
        world = game.World;
        worldBuilder = world.WorldBuilder;
        chunkSize = worldBuilder.ChunkSize;
    }

    bool showChunkBorder;

    private void OnDrawGizmos()
    {
        if (showChunkBorder)
        {
            Vector2Int playerChunkCoords = worldBuilder.GlobalToChunkCoords(player.transform.position);

            for (int x = playerChunkCoords.x - 1; x <= playerChunkCoords.x + 1; x++)
                for (int y = playerChunkCoords.y - 1; y <= playerChunkCoords.y + 1; y++)
                {
                    Vector3 pos = new Vector3((x * chunkSize) + (chunkSize / 2), player.transform.position.y, (y * chunkSize) + (chunkSize / 2));

                    Gizmos.DrawWireCube(pos, new Vector3((chunkSize), 100f, (chunkSize)));
                }
        }
    }

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            cam = Camera.main;
            return;
        }

        if (Input.GetKeyDown(KeyCode.F3))
            debugMenu.SetActive(!debugMenu.activeSelf);

        if (Input.GetKeyDown(KeyCode.B))
            showChunkBorder = !showChunkBorder;

        CalculateFPS();

        if (debugMenu.activeSelf)
            ShowChunkAndSelectedDebugging();
    }

    byte frameCount = 0;
    float dt = 0f;
    float fps = 0f;
    float updateRate = 4f;  // 4 updates per sec.

    void CalculateFPS()
    {
        frameCount++;

        dt += Time.deltaTime;
        if (dt > 1.0 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1f / updateRate;
        }

        fpsText.text = ((int)fps).ToString();
    }

    void ShowChunkAndSelectedDebugging()
    {
        Vector3 dir = cam.transform.forward;

        RaycastHit hitInfo;

        if (Physics.Raycast(cam.transform.position, dir, out hitInfo, 25f, terrainLayer))
        {
            Vector3 pointInTargetBlock;
            Vector3 pointOutTargetBlock;

            pointInTargetBlock = (hitInfo.point + dir * .015f);
            pointOutTargetBlock = worldBuilder.RoundGlobalCoords((hitInfo.point - dir * .015f));

            Vector3Int pitb = worldBuilder.RoundGlobalCoords(pointInTargetBlock);
            Vector3Int potb = worldBuilder.RoundGlobalCoords(pointOutTargetBlock);

            Block inblock = worldBuilder.TryGetBlockAtGlobal(pitb);
            Block outblock = worldBuilder.TryGetBlockAtGlobal(potb);

            selectedBlockInfo.text = $"Selected Block: {inblock.name}\n" +
                $"To Build On: {outblock.name}\n" +
                $"Selected Chunk: {worldBuilder.GlobalToChunkCoords(pointInTargetBlock)}\n" +
                $"Selected Global Coords: {pitb}\n" +
                $"Selected Local Coords: {worldBuilder.GetLocalCoordsFromGlobalCoords(pitb.x, pitb.y, pitb.z)}\n";
            //$"To Build On Local Coords: {ChunkLoader.GetLocalCoordsFromGlobalCoords(potb.x, potb.y, potb.z)}";
        }
        else
            selectedBlockInfo.text = "Selected: None";

        Vector3Int playerCoordsRounded = worldBuilder.RoundGlobalCoords(player.transform.position);

        Vector2Int chunkPos = worldBuilder.GlobalToChunkCoords(player.transform.position);
        playerChunkPosition.text = $"Chunk Position: {chunkPos}\n" +
            $"Local Chunk Position: {worldBuilder.GetLocalCoordsFromGlobalCoords(playerCoordsRounded.x, playerCoordsRounded.y, playerCoordsRounded.z)}";
    }
}
