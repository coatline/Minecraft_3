using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WorldBuilder
{
    readonly Dictionary<Vector2Int, Chunk> chunkMap;
    readonly ChunkRenderer chunkRendererPrefab;
    /// The amount of chunks around the player on a given side
    readonly List<Chunk> recycleQueue;
    readonly byte renderDistance;
    readonly byte chunkSize;
    readonly Thread thread;
    readonly World world;

    public WorldBuilder(byte renderDistance, byte chunkSize, World world)
    {
        chunkRendererPrefab = DataLibrary.I.ChunkRendererPrefab;
        chunkMap = new Dictionary<Vector2Int, Chunk>();
        thread = new Thread(GenerateChunks);
        recycleQueue = new List<Chunk>();

        thread.Name = "Chunk Generation Thread";
        thread.Start();

        this.renderDistance = renderDistance;
        this.chunkSize = chunkSize;
        this.world = world;

        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int y = -renderDistance; y <= renderDistance; y++)
                CreateChunkAt(x, y);
    }

    void CreateChunkAt(int x, int y)
    {
        Chunk newChunk = new Chunk(x, y, chunkSize, world, this);

        newChunk.Unloaded += OnChunkUnloaded;
        newChunk.Generated += OnChunkGenerated;

        //Debug.Log($"{newChunk.WorldX}, {newChunk.WorldY}, {newChunk.X}, {newChunk.Y}");
        ChunkRenderer chunkRenderer = Object.Instantiate(chunkRendererPrefab, new Vector3(newChunk.WorldX, 0, newChunk.WorldY), Quaternion.identity);
        chunkRenderer.Setup(newChunk);

        newChunk.GenerateAt(x, y);
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

        // Went Up
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
                    recycleQueue.Add(chunk);
                else
                    Debug.LogError("huh");
            }
        // Went Down
        else if (prevPlayerChunkCoords.y > chunkPosition.y)
            for (int x = nxCoord; x <= pxCoord; x++)
            {
                Chunk chunk = chunkMap[new Vector2Int(x, pyCoord + 1)];

                Vector2Int newChunkCoords = new Vector2Int(x, nyCoord);
                chunk.MarkAsRecycled(newChunkCoords);

                //chunkMap.Remove(new Vector2Int(x, pyCoord + 1));
                //chunkMap.Add(newChunkCoords, chunk);

                if (!recycleQueue.Contains(chunk))
                    recycleQueue.Add(chunk);
                else
                    Debug.LogError("huh");
            }
        // Went Left
        if (prevPlayerChunkCoords.x > chunkPosition.x)
            for (int y = nyCoord; y <= pyCoord; y++)
            {
                Chunk chunk = chunkMap[new Vector2Int(pxCoord + 1, y)];

                Vector2Int newChunkCoords = new Vector2Int(nxCoord, y);
                chunk.MarkAsRecycled(newChunkCoords);

                //chunkMap.Remove(new Vector2Int(pxCoord + 1, y));
                //chunkMap.Add(newChunkCoords, chunk);

                if (!recycleQueue.Contains(chunk))
                    recycleQueue.Add(chunk);
                else
                    Debug.LogError("huh");
            }
        // Went Right
        else if (prevPlayerChunkCoords.x < chunkPosition.x)
            for (int y = nyCoord; y <= pyCoord; y++)
            {
                Chunk chunk = chunkMap[new Vector2Int(nxCoord - 1, y)];

                Vector2Int newChunkCoords = new Vector2Int(pxCoord, y);
                chunk.MarkAsRecycled(newChunkCoords);

                //chunkMap.Remove(new Vector2Int(nxCoord - 1, y));
                //chunkMap.Add(newChunkCoords, chunk);

                if (!recycleQueue.Contains(chunk))
                    recycleQueue.Add(chunk);
                else
                    Debug.LogError("huh");
            }
    }

    void GenerateChunks()
    {
        while (true)
        {
            if (recycleQueue.Count == 0)
                continue;

            Debug.Log("Yes!");
            recycleQueue[0].Unload();
            recycleQueue[0].Recycle();
        }
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
}
