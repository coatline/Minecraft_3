using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WorldBuilder
{
    public readonly byte ChunkSize;

    readonly Dictionary<Vector2Int, Chunk> chunkMap;
    readonly ChunkRenderer chunkRendererPrefab;
    readonly Queue<Chunk> toAssignMesh;
    readonly Queue<Chunk> recycleQueue;
    /// The amount of chunks around the player on a given side
    readonly byte renderDistance;
    readonly Player player;
    readonly Thread thread;
    readonly World world;


    public WorldBuilder(byte renderDistance, byte chunkSize, World world, Player playerPrefab)
    {
        chunkRendererPrefab = DataLibrary.I.ChunkRendererPrefab;
        chunkMap = new Dictionary<Vector2Int, Chunk>();
        toAssignMesh = new Queue<Chunk>();
        thread = new Thread(GenerateChunks);
        recycleQueue = new Queue<Chunk>();

        thread.Name = "Chunk Generation Thread";
        thread.Start();

        this.renderDistance = renderDistance;
        this.ChunkSize = chunkSize;
        this.world = world;

        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int y = -renderDistance; y <= renderDistance; y++)
                CreateChunkAt(x, y);

        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Chunk chunk = TryGetChunkAt(new Vector2Int(x, y));
                chunk.Generated -= OnChunkGenerated;
                // Generate the mesh
                chunk.CallGenerated();
                chunk.Generated += OnChunkGenerated;
                // Assign the mesh
                chunk.CallMeshDataReady();
            }

        int localMiddle = chunkSize / 2;

        player = Object.Instantiate(playerPrefab, new Vector3(localMiddle, TryGetChunkAt(new Vector2Int(0, 0)).GetHeightMapAt(localMiddle, localMiddle), localMiddle), Quaternion.identity);
    }

    Chunk CreateChunkAt(int x, int y)
    {
        Chunk newChunk = new Chunk(x, y, ChunkSize, world, this);

        newChunk.Unloaded += OnChunkUnloaded;
        newChunk.Generated += OnChunkGenerated;

        newChunk.GenerateAt(x, y);

        ChunkRenderer chunkRenderer = Object.Instantiate(chunkRendererPrefab, new Vector3(newChunk.WorldX, 0, newChunk.WorldY), Quaternion.identity);
        chunkRenderer.Setup(newChunk);

        return newChunk;
    }

    Vector2Int prevPlayerChunkCoords;

    public void PlayerMoved(Vector2Int chunkPosition)
    {
        if (chunkPosition == prevPlayerChunkCoords) return;

        // If we crossed a diagonal chunk border since the last frame, don't let it happen
        if (prevPlayerChunkCoords.x != chunkPosition.x && chunkPosition.y != prevPlayerChunkCoords.y)
            chunkPosition.y = prevPlayerChunkCoords.y;
        else
            chunkPosition.y = Mathf.Clamp(chunkPosition.y, prevPlayerChunkCoords.y - 1, prevPlayerChunkCoords.y + 1);

        chunkPosition.x = Mathf.Clamp(chunkPosition.x, prevPlayerChunkCoords.x - 1, prevPlayerChunkCoords.x + 1);


        int nxCoord = chunkPosition.x - renderDistance;
        int nyCoord = chunkPosition.y - renderDistance;
        int pxCoord = chunkPosition.x + renderDistance;
        int pyCoord = chunkPosition.y + renderDistance;

        // I may be able to switch all of these to be else ifs since two may not be able to happen at once

        // Went North
        if (prevPlayerChunkCoords.y < chunkPosition.y)
            for (int x = nxCoord; x <= pxCoord; x++)
            {
                // Get all of the chunks behind the player and recycle them towards the front
                Chunk chunk = chunkMap[new Vector2Int(x, nyCoord - 1)];

                Vector2Int newChunkCoords = new Vector2Int(x, pyCoord);
                chunk.MarkAsRecycled(newChunkCoords);

                //chunkMap.Remove(new Vector2Int(x, nyCoord - 1));
                //chunkMap.Add(newChunkCoords, chunk);

                if (!recycleQueue.Contains(chunk))
                    recycleQueue.Enqueue(chunk);
                else
                    Debug.LogError("huh");
            }
        // Went South
        else if (prevPlayerChunkCoords.y > chunkPosition.y)
            for (int x = nxCoord; x <= pxCoord; x++)
            {
                Chunk chunk = chunkMap[new Vector2Int(x, pyCoord + 1)];

                Vector2Int newChunkCoords = new Vector2Int(x, nyCoord);
                chunk.MarkAsRecycled(newChunkCoords);

                //chunkMap.Remove(new Vector2Int(x, pyCoord + 1));
                //chunkMap.Add(newChunkCoords, chunk);

                if (!recycleQueue.Contains(chunk))
                    recycleQueue.Enqueue(chunk);
                else
                    Debug.LogError("huh");
            }
        // Went West
        if (prevPlayerChunkCoords.x > chunkPosition.x)
            for (int y = nyCoord; y <= pyCoord; y++)
            {
                Chunk chunk = chunkMap[new Vector2Int(pxCoord + 1, y)];

                Vector2Int newChunkCoords = new Vector2Int(nxCoord, y);
                chunk.MarkAsRecycled(newChunkCoords);

                //chunkMap.Remove(new Vector2Int(pxCoord + 1, y));
                //chunkMap.Add(newChunkCoords, chunk);

                if (!recycleQueue.Contains(chunk))
                    recycleQueue.Enqueue(chunk);
                else
                    Debug.LogError("huh");
            }
        // Went East
        else if (prevPlayerChunkCoords.x < chunkPosition.x)
            for (int y = nyCoord; y <= pyCoord; y++)
            {
                Chunk chunk = chunkMap[new Vector2Int(nxCoord - 1, y)];

                Vector2Int newChunkCoords = new Vector2Int(pxCoord, y);
                chunk.MarkAsRecycled(newChunkCoords);

                //chunkMap.Remove(new Vector2Int(nxCoord - 1, y));
                //chunkMap.Add(newChunkCoords, chunk);

                if (!recycleQueue.Contains(chunk))
                    recycleQueue.Enqueue(chunk);
                else
                    Debug.LogError("huh");
            }

        prevPlayerChunkCoords = chunkPosition;
    }


    void GenerateChunks()
    {
        while (true)
        {
            if (recycleQueue.Count == 0)
                continue;

            Chunk c = recycleQueue.Dequeue();
            c.Unload();
            c.Recycle();

            toAssignMesh.Enqueue(c);
        }
    }

    public void Update(float deltaTime)
    {
        PlayerMoved(GlobalToChunkCoords(player.transform.position));

        if (toAssignMesh.Count > 0)
            toAssignMesh.Dequeue().CallMeshDataReady();
    }

    void OnChunkGenerated(Vector2Int newPosition, Chunk chunk)
    {
        chunkMap.Add(newPosition, chunk);
    }

    void OnChunkUnloaded(Vector2Int position)
    {
        chunkMap.Remove(position);
    }

    public Chunk TryGetChunkAt(Vector2Int pos)
    {
        chunkMap.TryGetValue(pos, out Chunk chunk);
        return chunk;
    }

    public Vector2Int GlobalToChunkCoords(Vector3 pos) => new Vector2Int(Mathf.FloorToInt(pos.x / ChunkSize), Mathf.FloorToInt(pos.z / ChunkSize));
    public Vector3Int RoundGlobalCoords(Vector3 pos) => new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
    public Vector3Int GlobalToLocalCoords(Vector3 globalPos, Vector2Int chunkCoords) => new Vector3Int(Mathf.FloorToInt(globalPos.x) - (chunkCoords.x * ChunkSize), Mathf.FloorToInt(globalPos.y), Mathf.FloorToInt(globalPos.z) - (chunkCoords.y * ChunkSize));

    public Vector3Int GetLocalCoordsFromGlobalCoords(int x, int y, int z)
    {
        // Negative is not handled correctly
        x %= ChunkSize;
        z %= ChunkSize;

        if (x < 0)
            x += ChunkSize;
        if (z < 0)
            z += ChunkSize;

        return new Vector3Int(x, y, z);
    }

    public Block TryGetBlockAtGlobal(Vector3Int position)
    {
        Chunk chunk = TryGetChunkAt(GlobalToChunkCoords(position));
        if (chunk == null) { return null; }

        Vector3Int localCoords = GetLocalCoordsFromGlobalCoords(position.x, position.y, position.z);
        Block block = BlockLoader.I.GetBlock(chunk[localCoords.x, localCoords.y, localCoords.z]);
        //print($"global coords: {x}, {y}, {z} chunk coords: {GetChunkCoordsFromGlobalCoords(new Vector3(x, y, z))} local coords outputed: {localCoords} block detected: { block.name}");
        return block;
    }
}
