using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WorldBuilder
{
    public event System.Action WorldLoaded;

    public int ChunksLoaded { get; private set; }
    public readonly int RenderDistanceLength;
    public readonly int TotalChunks;
    public readonly int ChunkSize;
    bool worldLoaded;

    readonly Dictionary<Vector2Int, Chunk> chunkMap;
    readonly ChunkRenderer chunkRendererPrefab;
    readonly List<Chunk> toAssignMesh;
    /// The amount of chunks around the player on a given side
    readonly byte renderDistance;
    readonly Player playerPrefab;
    readonly List<Chunk> allChunks;
    readonly World world;
    Player player;


    public WorldBuilder(byte renderDistance, World world, Player playerPrefab)
    {
        RenderDistanceLength = renderDistance * 2 + 1;
        TotalChunks = RenderDistanceLength * RenderDistanceLength;

        this.ChunkSize = world.WorldSettings.ChunkSize;
        this.renderDistance = renderDistance;
        this.playerPrefab = playerPrefab;
        this.world = world;

        chunkRendererPrefab = DataLibrary.I.ChunkRendererPrefab;
        chunkMap = new Dictionary<Vector2Int, Chunk>();
        toAssignMesh = new List<Chunk>();
        allChunks = new List<Chunk>();

        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int y = -renderDistance; y <= renderDistance; y++)
                CreateChunkAt(x, y);

        // Do this in a separate loop just in case we finish before we subscribe
        for (int x = -renderDistance; x <= renderDistance; x++)
            for (int y = -renderDistance; y <= renderDistance; y++)
                TryGetChunkAt(x, y).InitializeValues();
    }

    void CreateChunkAt(int x, int y)
    {
        // Chunk automatically generates in a separate thread
        Chunk newChunk = new Chunk(x, y, world, this, chunkRendererPrefab);
        chunkMap.Add(new Vector2Int(x, y), newChunk);
        allChunks.Add(newChunk);

        // Assign the mesh outside of the thread
        newChunk.MeshGenerated += ChunkMeshGenerated;
        newChunk.StartedGenerating += ChunkStartedGenerating;

        // For loading screen
        newChunk.MeshGenerated += ChunkWasInitialized;
        newChunk.MeshAssigned += ChunkWasInitialized;
        newChunk.ValuesGenerated += ChunkValuesInitialized;
    }

    void ChunkValuesInitialized(Chunk c)
    {
        ChunkWasInitialized(c);

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

    void ChunkWasInitialized(Chunk chunk)
    {
        // This must be locked because the chunks can call this at the same time and it will not increment
        lock (this) ChunksLoaded++;
    }

    void LoadingScreen()
    {
        if (ChunksLoaded >= TotalChunks * 3)
        {
            for (int x = -renderDistance; x <= renderDistance; x++)
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    Chunk chunk = TryGetChunkAt(x, y);

                    chunk.MeshAssigned -= ChunkWasInitialized;
                    chunk.MeshGenerated -= ChunkWasInitialized;
                }

            // Everything is initialized, spawn the player
            int localMiddle = ChunkSize / 2;
            player = Object.Instantiate(playerPrefab, new Vector3(localMiddle, TryGetChunkAt(0, 0).Data.GetHeightMapAt(localMiddle, localMiddle), localMiddle), Quaternion.identity);

            worldLoaded = true;

            WorldLoaded?.Invoke();
            ChunksLoaded = 0;
        }
        else
            while (toAssignMesh.Count > 0)
                AssignMesh();
    }

    void ChunkMeshGenerated(Chunk chunk)
    {
        lock (toAssignMesh)
            if (toAssignMesh.Contains(chunk) == false)
                toAssignMesh.Add(chunk);
    }

    void ChunkStartedGenerating(Chunk chunk)
    {
        lock (toAssignMesh)
            if (toAssignMesh.Contains(chunk))
                toAssignMesh.Remove(chunk);
    }

    Vector2Int prevPlayerChunkCoords;

    public void PlayerMoved(Vector2Int playerChunkCoords)
    {
        if (playerChunkCoords == prevPlayerChunkCoords) return;

        int nxCoord = playerChunkCoords.x - renderDistance;
        int nyCoord = playerChunkCoords.y - renderDistance;
        int pxCoord = playerChunkCoords.x + renderDistance;
        int pyCoord = playerChunkCoords.y + renderDistance;

        //int prevNXCoord = prevPlayerChunkCoords.x - renderDistance;
        //int prevNYCoord = prevPlayerChunkCoords.y - renderDistance;
        //int prevPXCoord = prevPlayerChunkCoords.x + renderDistance;
        //int prevPYCoord = prevPlayerChunkCoords.y + renderDistance;

        //int deltaX = chunkPosition.x - prevPlayerChunkCoords.x;
        //int deltaY = chunkPosition.y - prevPlayerChunkCoords.y;

        Vector2Int moveDirection = playerChunkCoords - prevPlayerChunkCoords;

        foreach (Chunk chunk in allChunks)
        {
            if (chunk.IsWithinChunkCoords(nxCoord, pxCoord, nyCoord, pyCoord))
                continue;

            int oldNxCoord = nxCoord - moveDirection.x;
            int oldNyCoord = nyCoord - moveDirection.y;

            Vector2Int localPos = new Vector2Int((chunk.ChunkX - oldNxCoord) - renderDistance, (chunk.ChunkY - oldNyCoord) - renderDistance);
            Vector2Int offset = new Vector2Int(-localPos.x, -localPos.y);

            Vector2Int newPos = new Vector2Int(chunk.ChunkX, chunk.ChunkY)
                + (offset * 2) + moveDirection;

            MoveChunk(chunk, newPos.x, newPos.y);
        }

        prevPlayerChunkCoords = playerChunkCoords;

        void MoveChunk(Chunk chunk, int newX, int newY)
        {
            Vector2Int oldPosition = new Vector2Int(chunk.ChunkX, chunk.ChunkY);
            Vector2Int newPosition = new Vector2Int(newX, newY);

            chunkMap.Remove(oldPosition);
            chunkMap.Add(newPosition, chunk);

            chunk.GenerateAt(newPosition.x, newPosition.y);
        }
    }

    public void Update(float deltaTime)
    {
        if (worldLoaded == false)
        {
            LoadingScreen();
            return;
        }

        DoAssignMeshes();

        if (player != null)
            PlayerMoved(GlobalToChunkCoords(player.transform.position));
    }

    void DoAssignMeshes()
    {
        int count = toAssignMesh.Count;

        // Set this to a number if greater than a certain value
        int maxAmount = Mathf.CeilToInt(toAssignMesh.Count / 3f);

        // Maybe this will help reduce heavy lag spikes
        maxAmount = Mathf.Min(maxAmount, 32);

        byte amount = 0;

        // Assign the meshes in the main thread.
        while (amount < maxAmount)
        {
            AssignMesh();
            amount++;
        }

        //Debug.Log($"oldCount: {count} countNow: {toAssignMesh.Count}, maxAMount: {maxAmount}, amount: {amount}");
    }

    void AssignMesh()
    {
        lock (toAssignMesh)
            if (toAssignMesh.Count != 0)
            {
                toAssignMesh[0].ChunkRenderer.AssignMesh();
                toAssignMesh.RemoveAt(0);
            }
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
        Block block = BlockLoader.I.GetBlock(chunk.Data[localCoords.x, localCoords.y, localCoords.z]);
        //print($"global coords: {x}, {y}, {z} chunk coords: {GetChunkCoordsFromGlobalCoords(new Vector3(x, y, z))} local coords outputed: {localCoords} block detected: { block.name}");
        return block;
    }
}
