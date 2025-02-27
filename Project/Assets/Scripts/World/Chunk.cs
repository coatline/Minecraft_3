using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chunk
{
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public readonly ChunkRenderer ChunkRenderer;
    public readonly WorldBuilder WorldBuilder;

    //readonly Thread valuesThread;
    //readonly Thread meshThread;

    public event System.Action<Chunk> StartedGenerating;
    public event System.Action<Chunk> ValuesGenerated;
    public event System.Action<Chunk> MeshGenerated;
    public event System.Action<Chunk> MeshAssigned;

    public int Size => Data.Size;
    public readonly ChunkData Data;

    public bool Generating { get; private set; }

    // Chunk coordinates
    public int ChunkX { get; private set; }
    public int ChunkY { get; private set; }

    public float WorldX => ChunkX * Size;
    public float WorldY => ChunkY * Size;

    // Does not count chunk borders
    //public List<Vector3Int> VisibleBlocks { get; private set; }
    public List<Vector3Int> BorderAirBlocks { get; private set; }

    // We only need a Vector2 since we can assume certain axis given the border (ex: southBorder always has z == 0)
    public List<Vector2Int> NorthBorderBlocks { get; private set; }
    public List<Vector2Int> SouthBorderBlocks { get; private set; }
    public List<Vector2Int> EastBorderBlocks { get; private set; }
    public List<Vector2Int> WestBorderBlocks { get; private set; }

    public Chunk(int chunkX, int chunkY, World world, WorldBuilder worldBuilder, ChunkRenderer chunkRendererPrefab)
    {
        Data = new ChunkData(world.WorldSettings, this);
        //VisibleBlocks = new();
        ChunkX = chunkX;
        ChunkY = chunkY;
        WorldBuilder = worldBuilder;

        ChunkRenderer = Object.Instantiate(chunkRendererPrefab, new Vector3(WorldX, 0, WorldY), Quaternion.identity);
        ChunkRenderer.Setup(this);

        generateThread = new Thread(Generate);
    }

    public bool IsWithinChunkCoords(int minX, int maxX, int minY, int maxY) => !(ChunkX < minX || ChunkX > maxX || ChunkY < minY || ChunkY > maxY);

    public void AssignedMesh() => MeshAssigned?.Invoke(this);

    public void MeshComplete()
    {
        MeshGenerated.Invoke(this);

        if (queued)
            GenerateAt(queuedX, queuedY, true);
    }

    Thread generateThread;
    bool queued;
    int queuedX;
    int queuedY;

    public void GenerateAt(int chunkX, int chunkY, bool initiatingQueued = false)
    {
        if (Generating || ChunkRenderer.Generating || queued)
        {
            queued = true;
            queuedX = chunkX;
            queuedY = chunkY;
            return;
        }

        ChunkX = chunkX;
        ChunkY = chunkY;

        if (initiatingQueued == false)
        {
            generateThread.Abort();

            generateThread = new Thread(Generate);
            generateThread.Start();
        }

        // Reminder that the thread pool has a maximum of 512 threads per process
        //ThreadPool.QueueUserWorkItem(Generate);
    }

    //public void InitializeValues() => GenerateInitial(null);
    //public void InitializeMesh() => GenerateInitialMesh(null);
    public void InitializeValues() => ThreadPool.QueueUserWorkItem(GenerateInitial);
    public void InitializeMesh() => ThreadPool.QueueUserWorkItem(GenerateInitialMesh);

    void GenerateInitialMesh(object ob) => ChunkRenderer.GenerateMesh();

    void GenerateInitial(object ob)
    {
        Generating = true;

        GenerateAll();

        Generating = false;

        ValuesGenerated?.Invoke(this);
    }

    void Generate(object ob)
    {
        StartedGenerating?.Invoke(this);

        Generating = true;

        //stopwatch.Restart();

        GenerateAll();

        //stopwatch.Stop();
        //Debug.Log($"Values: {stopwatch.ElapsedMilliseconds}");

        Generating = false;

        FinishGenerating();

        ChunkRenderer.GenerateMesh();

        ValuesGenerated?.Invoke(this);
    }

    void FinishGenerating()
    {
        //Chunk northChunk = WorldBuilder.TryGetChunkAt(X, Y + 1);
        //Chunk southChunk = WorldBuilder.TryGetChunkAt(X, Y - 1);
        //Chunk eastChunk = WorldBuilder.TryGetChunkAt(X + 1, Y);
        //Chunk westChunk = WorldBuilder.TryGetChunkAt(X - 1, Y);

        //if (northChunk != null)
        //    northChunk.ChunkRenderer.UpdateBorderSouth(this);

        //if (southChunk != null)
        //    southChunk.ChunkRenderer.UpdateBorderNorth(this);

        //if (eastChunk != null)
        //    eastChunk.ChunkRenderer.UpdateBorderWest(this);

        //if (westChunk != null)
        //    westChunk.ChunkRenderer.UpdateBorderEast(this);
    }

    void GenerateAll()
    {
        try
        {
            Data.GenerateHeightMap();
            Data.GenerateValues();
            Data.GenerateStructures();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
