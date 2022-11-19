using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WorldBuilder
{
    public event System.Action WorldLoaded;

    public int ChunksLoaded { get; private set; }
    public readonly int TotalChunks;
    public readonly byte ChunkSize;
    bool worldLoaded;

    readonly Dictionary<Vector2Int, Chunk> chunkMap;
    readonly ChunkRenderer chunkRendererPrefab;
    readonly Queue<Chunk> toAssignMesh;
    /// The amount of chunks around the player on a given side
    readonly byte renderDistance;
    readonly Player playerPrefab;
    readonly World world;
    Player player;


    public WorldBuilder(byte renderDistance, byte chunkSize, World world, Player playerPrefab)
    {
        TotalChunks = (int)Mathf.Pow(renderDistance * 2 + 1, 2);
        this.renderDistance = renderDistance;
        this.playerPrefab = playerPrefab;
        this.ChunkSize = chunkSize;
        this.world = world;

        chunkRendererPrefab = DataLibrary.I.ChunkRendererPrefab;
        chunkMap = new Dictionary<Vector2Int, Chunk>();
        toAssignMesh = new Queue<Chunk>();

        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int y = -renderDistance; y <= renderDistance; y++)
                CreateChunkAt(x, y);

        // Do this in a separate loop just in case we finish before we subscribe
        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int y = -renderDistance; y <= renderDistance; y++)
                TryGetChunkAt(x, y).InitializeValues();

    }

    void LoadingScreen()
    {
        if (ChunksLoaded == TotalChunks * 3)
        {
            for (int x = -renderDistance; x <= renderDistance; x++)
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    Chunk chunk = TryGetChunkAt(x, y);

                    chunk.MeshAssigned -= ChunkInitialized;
                    chunk.MeshGenerated -= ChunkInitialized;
                }

            // Everything is initialized, spawn the player
            int localMiddle = ChunkSize / 2;
            player = Object.Instantiate(playerPrefab, new Vector3(localMiddle, TryGetChunkAt(0, 0).GetHeightMapAt(localMiddle, localMiddle), localMiddle), Quaternion.identity);

            worldLoaded = true;

            WorldLoaded?.Invoke();
            ChunksLoaded = 0;
        }
        else
            while (toAssignMesh.Count > 0)
                lock (toAssignMesh)
                    toAssignMesh.Dequeue().ChunkRenderer.AssignMesh();
    }

    void CreateChunkAt(int x, int y)
    {
        // Chunk automatically generates in a separate thread
        Chunk newChunk = new Chunk(x, y, ChunkSize, world, this, chunkRendererPrefab);
        chunkMap.Add(new Vector2Int(x, y), newChunk);

        // Assign the mesh outside of the thread
        newChunk.MeshGenerated += ChunkMeshGenerated;

        // For loading screen
        newChunk.MeshGenerated += ChunkInitialized;
        newChunk.MeshAssigned += ChunkInitialized;
        newChunk.ValuesGenerated += ChunkValuesInitialized;
    }

    void ChunkInitialized(Chunk chunk)
    {
        // This must be locked because the chunks can call this at the same time and it will not increment
        lock (this) ChunksLoaded++;
    }

    void ChunkValuesInitialized(Chunk c)
    {
        ChunkInitialized(c);

        // All values have been generated, now we can initialize the meshes
        if (ChunksLoaded == TotalChunks)
        {
            for (int x = -renderDistance; x <= renderDistance; x++)
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    Chunk chunk = TryGetChunkAt(x, y);

                    chunk.InitializeMesh();
                    chunk.ValuesGenerated -= ChunkValuesInitialized;
                }
        }
    }

    void ChunkMeshGenerated(Chunk chunk)
    {
        lock (toAssignMesh) toAssignMesh.Enqueue(chunk);
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

                chunkMap.Remove(new Vector2Int(x, nyCoord - 1));
                chunkMap.Add(newChunkCoords, chunk);

                chunk.GenerateAt(newChunkCoords.x, newChunkCoords.y);
            }
        // Went South
        else if (prevPlayerChunkCoords.y > chunkPosition.y)
            for (int x = nxCoord; x <= pxCoord; x++)
            {
                Chunk chunk = chunkMap[new Vector2Int(x, pyCoord + 1)];

                Vector2Int newChunkCoords = new Vector2Int(x, nyCoord);

                chunkMap.Remove(new Vector2Int(x, pyCoord + 1));
                chunkMap.Add(newChunkCoords, chunk);

                chunk.GenerateAt(newChunkCoords.x, newChunkCoords.y);
            }
        // Went West
        if (prevPlayerChunkCoords.x > chunkPosition.x)
            for (int y = nyCoord; y <= pyCoord; y++)
            {
                Chunk chunk = chunkMap[new Vector2Int(pxCoord + 1, y)];

                Vector2Int newChunkCoords = new Vector2Int(nxCoord, y);

                chunkMap.Remove(new Vector2Int(pxCoord + 1, y));
                chunkMap.Add(newChunkCoords, chunk);

                chunk.GenerateAt(newChunkCoords.x, newChunkCoords.y);
            }
        // Went East
        else if (prevPlayerChunkCoords.x < chunkPosition.x)
            for (int y = nyCoord; y <= pyCoord; y++)
            {
                Chunk chunk = chunkMap[new Vector2Int(nxCoord - 1, y)];

                Vector2Int newChunkCoords = new Vector2Int(pxCoord, y);

                chunkMap.Remove(new Vector2Int(nxCoord - 1, y));
                chunkMap.Add(newChunkCoords, chunk);

                chunk.GenerateAt(newChunkCoords.x, newChunkCoords.y);
            }

        prevPlayerChunkCoords = chunkPosition;
    }

    public void Update(float deltaTime)
    {
        if (worldLoaded == false)
        {
            LoadingScreen();
            return;
        }

        int maxAmount = Mathf.CeilToInt(toAssignMesh.Count / 4f);
        byte amount = 0;

        // Assign the meshes in the main thread.
        while (amount < maxAmount)
        {
            lock (toAssignMesh)
                toAssignMesh.Dequeue().ChunkRenderer.AssignMesh();
            amount++;
        }

        if (player != null)
            PlayerMoved(GlobalToChunkCoords(player.transform.position));
    }

    public Chunk TryGetChunkAt(int x, int y)
    {
        chunkMap.TryGetValue(new Vector2Int(x, y), out Chunk chunk);
        return chunk;
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
