using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chunk
{
    // Static variables
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public readonly ChunkRenderer ChunkRenderer;
    public readonly WorldBuilder WorldBuilder;
    public readonly int HeightLimit;
    public readonly byte Size;

    readonly Vector2 seedDependentOffset;
    //readonly Thread valuesThread;
    //readonly Thread meshThread;
    readonly int maxTerrainHeight;
    readonly byte waterLevel;
    readonly byte bedrock;
    readonly byte grass;
    readonly byte dirt;
    readonly byte stone;

    public event System.Action<Chunk> StartedGenerating;
    public event System.Action<Chunk> ValuesGenerated;
    public event System.Action<Chunk> MeshGenerated;
    public event System.Action<Chunk> MeshAssigned;

    public bool Generating { get; private set; }

    // Chunk coordinates
    public int ChunkX { get; private set; }
    public int ChunkY { get; private set; }

    public float WorldX => ChunkX * Size;
    public float WorldY => ChunkY * Size;

    // Does not count chunk borders
    public List<Vector3Int> VisibleBlocks { get; private set; }
    public List<Vector3Int> BorderAirBlocks { get; private set; }

    // We only need a Vector2 since we can assume certain axis given the border (ex: southBorder always has z == 0)
    public List<Vector2Int> NorthBorderBlocks { get; private set; }
    public List<Vector2Int> SouthBorderBlocks { get; private set; }
    public List<Vector2Int> EastBorderBlocks { get; private set; }
    public List<Vector2Int> WestBorderBlocks { get; private set; }

    byte[,] heightMap;
    byte[,,] blocks;


    public Chunk(int chunkX, int chunkY, byte size, World world, WorldBuilder worldBuilder, ChunkRenderer chunkRendererPrefab)
    {
        ChunkX = chunkX;
        ChunkY = chunkY;
        Size = size;
        WorldBuilder = worldBuilder;
        waterLevel = world.WaterLevel;
        HeightLimit = world.HeightLimit;
        maxTerrainHeight = world.MaxTerrainHeight;
        seedDependentOffset = world.NoiseOffset;

        grass = DataLibrary.I.Blocks["Grass"].Id;
        dirt = DataLibrary.I.Blocks["Dirt"].Id;
        stone = DataLibrary.I.Blocks["Stone"].Id;
        bedrock = DataLibrary.I.Blocks["Bedrock"].Id;

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
        {
            GenerateAt(queuedX, queuedY, true);
        }

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
            GenerateHeightMap();
            GenerateValues();
            GenerateStructures();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void GenerateHeightMap()
    {
        heightMap = new byte[Size, Size];

        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
                heightMap[x, y] = GetHeightAt(x, y);
    }

    void GenerateValues()
    {
        blocks = new byte[Size, HeightLimit, Size];

        VisibleBlocks = new();
        BorderAirBlocks = new();
        SouthBorderBlocks = new();
        NorthBorderBlocks = new();
        EastBorderBlocks = new();
        WestBorderBlocks = new();

        for (byte x = 0; x < Size; x++)
        {
            for (byte z = 0; z < Size; z++)
            {
                int height = heightMap[x, z];

                // DEBUG
                if (height >= HeightLimit)
                    Debug.LogError(height);

                for (byte y = 1; y < height; y++)
                {
                    //if ((y == height - 1)
                    //    //|| (x < Size - 1 && blocks[x + 1, y, z] == 0)
                    //    //|| (x > 0 && blocks[x - 1, y, z] == 0)
                    //    //|| (z > 0 && blocks[x, y, z - 1] == 0)
                    //    //|| (z < Size - 1 && blocks[x, y, z + 1] == 0)
                    //    )
                    //{
                    //    blocksToBuildFaceOn.Add(new Vector3Int(x, y, z));
                    //}

                    // If it is on the surface of the terrain
                    if (y == height - 1)
                    {
                        VisibleBlocks.Add(new Vector3Int(x, y, z));

                        // If we are under water
                        if (y <= waterLevel)
                            blocks[x, y, z] = dirt;
                        else
                            blocks[x, y, z] = grass;
                    }
                    else /*if (height - y <= 3)*/
                    {
                        //if (y > height)
                        //    blocksToBuildFaceOn.Add(new Vector3Int(x, y, z));

                        blocks[x, y, z] = dirt;
                    }

                    if (z == 0)
                        SouthBorderBlocks.Add(new Vector2Int(x, y));
                    else if (z == Size - 1)
                        NorthBorderBlocks.Add(new Vector2Int(x, y));


                    //else
                    //    blocks[x, y, z] = stone;

                    #region commented
                    //    #region tree
                    //    // Generate a Tree
                    //    //if (x > 3 && x < SIZE - 3 && z > 3 && z < SIZE - 3 && y + 5 < TERRAIN_HEIGHT_LIMIT && y > WATER_LEVEL)
                    //    //{
                    //    //    lock (ChunkLoader.random)
                    //    //    {

                    //    //        if (ChunkLoader.random.Next(0, 101) <= 1)
                    //    //        {
                    //    //            for (int lX = -2; lX < 3; lX++)
                    //    //                for (int lZ = -2; lZ < 3; lZ++)
                    //    //                    for (int lY = 0; lY < 4; lY++)
                    //    //                    {
                    //    //                        if (lY <= 1)
                    //    //                        {
                    //    //                            TryPlaceFaceBlock(x + lX, y + lY + 3, z + lZ, 10);
                    //    //                        }
                    //    //                        else if (lY == 2)
                    //    //                        {
                    //    //                            if (lX == -2 || lX == 2 || lZ == -2 || lZ == 2)
                    //    //                            {
                    //    //                                continue;
                    //    //                            }

                    //    //                            TryPlaceFaceBlock(x + lX, y + lY + 3, z + lZ, 10);
                    //    //                        }
                    //    //                        else
                    //    //                        {
                    //    //                            if ((lX == -1 && lZ == 0) || (lZ == 0 && lX == 1) || (lX == 0 && lZ == -1) || (lZ == 1 && lX == 0) || (lZ == 0 && lX == 0))
                    //    //                            {
                    //    //                                TryPlaceFaceBlock(x + lX, y + lY + 3, z + lZ, 10);
                    //    //                            }
                    //    //                        }
                    //    //                    }

                    //    //            PlaceFaceBlock(x, y + 1, z, 9);
                    //    //            PlaceFaceBlock(x, y + 2, z, 9);
                    //    //            PlaceFaceBlock(x, y + 3, z, 9);
                    //    //            PlaceFaceBlock(x, y + 4, z, 9);
                    //    //            PlaceFaceBlock(x, y + 5, z, 9);
                    //    //        }
                    //    //    }
                    //    //}
                    //    //if (BiomeHandler.I.biomes[currentBiome].cacti && x >= 1 && z >= 1 && x <= SIZE + 1 && z <= SIZE + 1 && Random.Range(0f, 101f) <= Noise(x, z, ChunkLoader.noiseOffset, 6))
                    //    //{
                    //    //    PlaceFaceBlock(x, y + 1, z, 8);
                    //    //    PlaceFaceBlock(x, y + 2, z, 8);
                    //    //    PlaceFaceBlock(x, y + 3, z, 8);
                    //    //}
                    //    #endregion
                    //}
                    //// First three layers in ground
                    //else if (y < height - 1 && y > height - BiomeHandler.I.biomes[currentBiome].subSurfaceBlockDepth)
                    //{
                    //    // Place Sub-Surface Block
                    //    blocks[x, y, z] = (BiomeHandler.I.biomes[currentBiome].subSurfaceBlock);
                    //}
                    //// First three layers in ground
                    //else
                    //{
                    //    // Place Stone
                    //    blocks[x, y, z] = 4;
                    //}
                    #endregion
                }

                // Place Water
                if (height < waterLevel)
                {
                    int depth = waterLevel - height;

                    for (int i = 0; i < depth; i++)
                    {
                        // Set to water
                        //blocks[x, waterLevel - i - 1, z] = 12;

                        //if (i == 0)
                        //{
                        //    //if (CanBeFaceBlock(x, WATER_LEVEL - 1, z))
                        //    {
                        //        waterBlocksToBuildFaceOn.Add(new Vector3Int(x, WATER_LEVEL - 1, z));
                        //    }
                        //}
                    }
                }

                // Place bedrock
                blocks[x, 0, z] = bedrock;
            }
        }
    }

    // https://www.reddit.com/r/gamedev/comments/16yyqw/how_does_minecraft_generate_structures_especially/?adlt=strict&toWww=1&redig=DC22031311094992A7DD3D3732E3A052
    void GenerateStructures()
    {
        // Generate rivers from tops of mountains to end point like a lake or ocean (polywhoreism's comment)

    }

    byte GetHeightAt(int x, int y) => (byte)(OctavePerlin(x, y, 5, .0035f, .5f) * maxTerrainHeight);

    // Reference:
    // http://adrianb.io/2014/08/09/perlinnoise.html?adlt=strict&toWww=1&redig=5489DC280FC64EBFA66408624C5F6F80
    float OctavePerlin(int x, int y, int octaves, float frequency, float persistence)
    {
        float total = 0;
        float amplitude = 1;
        float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0

        for (int i = 0; i < octaves; i++)
        {
            total += PerlinNoise(x, y, frequency, amplitude);

            maxValue += amplitude;

            // Make the following octaves have less influence
            amplitude *= persistence;
            // Give the following octaves more frequency
            frequency *= 2;
        }

        return (float)total / maxValue;
    }

    // TODO: Consider checking out fastnoise (on github) its a lot faster than the default PerlinNoise method.
    // https://github.com/lmeeng/FastNoise/blob/master/CSharp/FastNoiseLite.cs
    float PerlinNoise(float x, float y, float frequency, float amplitude)
    {
        // Amplitude = the range at which the result can be in
        // Frequency = the the period at which the data is sampled

        float noiseValue = Mathf.PerlinNoise((x + seedDependentOffset.x + WorldX) * frequency, (y + seedDependentOffset.y + WorldY) * frequency) * amplitude;
        return Mathf.Clamp01(noiseValue);
    }

    public int GetHeightMapAt(int x, int y) => heightMap[x, y];

    /// Get block at position
    public int this[int x, int y, int z]
    {
        get => blocks[x, y, z];
    }
}
