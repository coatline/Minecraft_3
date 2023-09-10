using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WorldSettings
{
    public readonly int MaxHeight;
    public readonly int TerrainHeight;
    public readonly int SeaLevel;
    public readonly int ChunkSize;

    public WorldSettings(int maxHeight, int terrainHeight, int seaLevel, int chunkSize)
    {
        MaxHeight = maxHeight;
        TerrainHeight = terrainHeight;
        SeaLevel = seaLevel;
        ChunkSize = chunkSize;
    }
}
