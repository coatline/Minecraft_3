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
    public readonly byte ChunkSize;
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


    public WorldBuilder(byte renderDistance, byte chunkSize, World world, Player playerPrefab)
    {
        RenderDistanceLength = renderDistance * 2 + 1;
        TotalChunks = RenderDistanceLength * RenderDistanceLength;

        this.renderDistance = renderDistance;
        this.playerPrefab = playerPrefab;
        this.ChunkSize = chunkSize;
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

    void LoadingScreen()
    {
        if (ChunksLoaded >= TotalChunks * 3)
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
                AssignMesh();
    }

    void CreateChunkAt(int x, int y)
    {
        // Chunk automatically generates in a separate thread
        Chunk newChunk = new Chunk(x, y, ChunkSize, world, this, chunkRendererPrefab);
        chunkMap.Add(new Vector2Int(x, y), newChunk);
        allChunks.Add(newChunk);

        // Assign the mesh outside of the thread
        newChunk.MeshGenerated += ChunkMeshGenerated;
        newChunk.StartedGenerating += ChunkStartedGenerating;

        // For loading screen
        newChunk.MeshGenerated += ChunkInitialized;
        newChunk.MeshAssigned += ChunkInitialized;
        newChunk.ValuesGenerated += ChunkValuesInitialized;
    }

    void ChunkInitialized(Chunk chunk)
    {
        Debug.Log("I did the fortnite! I really did!");
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
        lock (toAssignMesh)
            if (toAssignMesh.Contains(chunk) == false)
                toAssignMesh.Add(chunk);
    }

    Vector2Int prevPlayerChunkCoords;

    public void PlayerMoved(Vector2Int playerChunkCoords)
    {
        if (playerChunkCoords == prevPlayerChunkCoords) return;

        //// If we crossed a diagonal chunk border since the last frame, don't let it happen
        //if (prevPlayerChunkCoords.x != chunkPosition.x && chunkPosition.y != prevPlayerChunkCoords.y)
        //    chunkPosition.y = prevPlayerChunkCoords.y;
        //else
        //    chunkPosition.y = Mathf.Clamp(chunkPosition.y, prevPlayerChunkCoords.y - 1, prevPlayerChunkCoords.y + 1);

        //chunkPosition.x = Mathf.Clamp(chunkPosition.x, prevPlayerChunkCoords.x - 1, prevPlayerChunkCoords.x + 1);


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

            //print($"newPos: {newPos} old: {oldPos} localPos: {localPos} offset: {offset} ");

            //if (chunksToLoad.TryGetValue(chunk, out ChunkLoadInfo v))
            //    v.chunkCoords = newPos;
            //else
            //    chunksToLoad.Add(chunk, new ChunkLoadInfo(chunk, newPos));

            //Vector2Int toWorld = Extensions.ToWorldCoords(newPos);

            MoveChunk(chunk, newPos.x, newPos.y);
            //chunk.RegenerateAt(toWorld.x, toWorld.y);
        }

        //int xDirection = 0;
        //int yDirection = 0;

        //if (deltaX > 0)
        //{
        //    for (int x = prevNXCoord; x < prevNXCoord + deltaX; x++)
        //    {

        //    }
        //}
        //else if (deltaX < 0)
        //    xDirection = -1;

        //if (deltaY > 0)
        //{
        //    for (int y = prevNYCoord; y < prevNYCoord + deltaY; y++)
        //    {

        //    }
        //    yDirection = 1;
        //}
        //else if (deltaY < 0)
        //    yDirection = -1;



        // I may be able to switch all of these to be else ifs since two may not be able to happen at once

        //// Went North
        //if (prevPlayerChunkCoords.y < chunkPosition.y)
        //{
        //    int wentInX = 0;

        //    if (prevPlayerChunkCoords.x != chunkPosition.x)
        //        // Went East too
        //        if (prevPlayerChunkCoords.x < chunkPosition.x)
        //        {
        //            wentInX = 1;
        //        }
        //        // Went West too
        //        else
        //        {
        //            wentInX = 2;
        //        }

        //    for (int x = nxCoord; x < pxCoord; x++)
        //    {
        //        for (int y = 0; y < deltaY; y++)
        //        {
        //            MoveChunk(x, y + prevNYCoord, x, pyCoord - y);
        //        }
        //    }

        //    //for (int x = nxCoord; x <= pxCoord; x++)
        //    //{

        //    //    MoveChunk(x, nyCoord - 1, x, pyCoord);

        //    //    //// Get all of the chunks behind the player and recycle them towards the front
        //    //    //Chunk chunk = chunkMap[new Vector2Int(x, nyCoord - 1)];

        //    //    //Vector2Int newChunkCoords = new Vector2Int(x, pyCoord);

        //    //    //chunkMap.Remove(new Vector2Int(x, nyCoord - 1));
        //    //    //chunkMap.Add(newChunkCoords, chunk);

        //    //    //chunk.GenerateAt(newChunkCoords.x, newChunkCoords.y);
        //    //}
        //}

        //int xFactor = prevPlayerChunkCoords.x + chunkPosition.x;
        //int yFactor = prevPlayerChunkCoords.y + chunkPosition.y;

        //// Went North
        //if (deltaY > 0)
        //{
        //    for (int x = prevNXCoord; x <= prevPXCoord; x++)
        //    {
        //        for (int y = 0; y < deltaY; y++)
        //        {
        //            // Algebra:
        //            // newX = -1(x - prevPlayerChunkCoords.x) + chunkPosition.x
        //            // newX = -x + prevPlayerChunkCoords.x + chunkPosition.x
        //            // newX = -x + xFactor

        //            // newY = -((y + prevNYCoord) - prevPlayerChunkCoords.y) + chunkPosition.y
        //            // newY = -(y + prevNYCoord) + prevPlayerChunkCoords.y + chunkPosition.y
        //            // newY = -y - prevNYCoord + prevPlayerChunkCoords.y + chunkPosition.y

        //            int newXAlgebraWay = -x + xFactor;
        //            int newYAlgebraWay = -y - prevNYCoord + yFactor;

        //            //int localX = x - prevPlayerChunkCoords.x;
        //            //int localY = (y + prevNYCoord) - prevPlayerChunkCoords.y;

        //            //int newLocalX = -localX;
        //            //int newLocalY = -localY;

        //            //int newX = newLocalX + chunkPosition.x;
        //            //int newY = newLocalY + chunkPosition.y;

        //            MoveChunk(x, prevNYCoord + y, newXAlgebraWay, newYAlgebraWay, false);
        //        }
        //    }
        //}
        //// Went South
        //else if (deltaY < 0)
        //{
        //    // Shouldn't this be prevNXCoord and prePXCoord?
        //    for (int x = nxCoord; x <= pxCoord; x++)
        //    {
        //        // DeltaY will be negative
        //        for (int y = 0; y < -deltaY; y++)
        //        {
        //            int newXAlgebraWay = -x + xFactor;
        //            int newYAlgebraWay = -y - prevPYCoord + yFactor;

        //            MoveChunk(x, prevPYCoord - y, newXAlgebraWay, newYAlgebraWay, false);
        //        }
        //    }
        //}
        // Went West
        //if (prevPlayerChunkCoords.x > chunkPosition.x)
        //    for (int y = nyCoord; y <= pyCoord; y++)
        //    {
        //        MoveChunk(pxCoord + 1, y, nxCoord + 1, y);
        //        //Chunk chunk = chunkMap[new Vector2Int(pxCoord + 1, y)];

        //        //Vector2Int newChunkCoords = new Vector2Int(nxCoord, y);

        //        //chunkMap.Remove(new Vector2Int(pxCoord + 1, y));
        //        //chunkMap.Add(newChunkCoords, chunk);

        //        //chunk.GenerateAt(newChunkCoords.x, newChunkCoords.y);
        //    }
        //// Went East
        //else if (prevPlayerChunkCoords.x < chunkPosition.x)
        //    for (int y = nyCoord; y <= pyCoord; y++)
        //    {
        //        Chunk chunk = chunkMap[new Vector2Int(nxCoord - 1, y)];

        //        Vector2Int newChunkCoords = new Vector2Int(pxCoord, y);

        //        chunkMap.Remove(new Vector2Int(nxCoord - 1, y));
        //        chunkMap.Add(newChunkCoords, chunk);

        //        chunk.GenerateAt(newChunkCoords.x, newChunkCoords.y);
        //    }

        prevPlayerChunkCoords = playerChunkCoords;

        void MoveChunk(Chunk chunk, int newX, int newY)
        {
            Vector2Int oldPosition = new Vector2Int(chunk.ChunkX, chunk.ChunkY);
            Vector2Int newPosition = new Vector2Int(newX, newY);

            chunkMap.Remove(oldPosition);
            chunkMap.Add(newPosition, chunk);

            chunk.GenerateAt(newPosition.x, newPosition.y);

            //// Get chunk at old position
            //if (chunkMap.TryGetValue(oldPosition, out Chunk chunk))
            //{
            //    chunkMap.Remove(oldPosition);
            //    chunkMap.Add(newPosition, chunk);

            //    chunk.GenerateAt(newPosition.x, newPosition.y);
            //}
            //else
            //    // Must have been a corner piece, already moved
            //    return;
        }
    }

    void ChunkStartedGenerating(Chunk chunk)
    {
        lock (toAssignMesh)
            if (toAssignMesh.Contains(chunk))
                toAssignMesh.Remove(chunk);
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
        Block block = BlockLoader.I.GetBlock(chunk[localCoords.x, localCoords.y, localCoords.z]);
        //print($"global coords: {x}, {y}, {z} chunk coords: {GetChunkCoordsFromGlobalCoords(new Vector3(x, y, z))} local coords outputed: {localCoords} block detected: { block.name}");
        return block;
    }
}
